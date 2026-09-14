#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [ValidateSet('cosmosdb')]
    [string] $Service = 'cosmosdb',
    [string] $WorkDirectory,
    [switch] $SkipNpmInstall
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$configPath = Join-Path $repoRoot "eng/sdk-generation/services/$Service.json"
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$output = Join-Path $repoRoot $config.outputDirectory
$lock = Get-Content (Join-Path $output 'spec.lock.json') -Raw | ConvertFrom-Json
$roots = Get-Content (Join-Path $output 'roots.json') -Raw | ConvertFrom-Json
$toolRoot = Join-Path $repoRoot 'eng/sdk-generation'
if (-not $WorkDirectory) { $WorkDirectory = Join-Path $toolRoot ".work/$Service" }
$WorkDirectory = [IO.Path]::GetFullPath($WorkDirectory)

function Invoke-CommandChecked {
    param([string] $File, [string[]] $Arguments)
    Write-Host "> $File $($Arguments -join ' ')" -ForegroundColor DarkGray
    & $File @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$File failed with exit code $LASTEXITCODE" }
}

function Initialize-SparseCheckout {
    param(
        [string] $Directory,
        [string] $Repository,
        [string] $Commit,
        [string[]] $Patterns
    )

    if (-not (Test-Path (Join-Path $Directory '.git'))) {
        New-Item $Directory -ItemType Directory -Force | Out-Null
        Invoke-CommandChecked git @('-C', $Directory, 'init')
        Invoke-CommandChecked git @('-C', $Directory, 'remote', 'add', 'origin', "https://github.com/$Repository.git")
        Invoke-CommandChecked git @('-C', $Directory, 'sparse-checkout', 'init', '--no-cone')
    }
    Invoke-CommandChecked git (@('-C', $Directory, 'sparse-checkout', 'set', '--no-cone') + $Patterns)
    Invoke-CommandChecked git @('-C', $Directory, 'fetch', '--depth', '1', '--filter=blob:none', 'origin', $Commit)
    Invoke-CommandChecked git @('-C', $Directory, 'checkout', '--detach', 'FETCH_HEAD')
    $actual = (& git -C $Directory rev-parse HEAD).Trim()
    if ($actual -ne $Commit) { throw "Expected $Commit but checked out $actual in $Directory" }
}

function Initialize-Project {
    param([string] $Directory)
    New-Item (Join-Path $Directory 'src/Custom') -ItemType Directory -Force | Out-Null
    @'
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
'@ | Set-Content (Join-Path $Directory 'Directory.Build.props')
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsAotCompatible>true</IsAotCompatible>
    <DisableEnhancedAnalysis>true</DisableEnhancedAnalysis>
    <NoWarn>$(NoWarn);SCM0005</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Core" />
    <PackageReference Include="Azure.ResourceManager" />
  </ItemGroup>
</Project>
'@ | Set-Content (Join-Path $Directory 'src/Azure.ResourceManager.CosmosDB.csproj')
}

function Invoke-Generation {
    param([string] $Spec, [string] $Destination)
    Invoke-CommandChecked npm @(
        'exec', '--prefix', $toolRoot, '--no', '--', 'tsp', 'compile', $Spec,
        '--emit=@azure-typespec/http-client-csharp-mgmt',
        "--option=@azure-typespec/http-client-csharp-mgmt.emitter-output-dir=$Destination",
        '--option=@azure-typespec/http-client-csharp-mgmt.new-project=false',
        '--option=@azure-typespec/http-client-csharp-mgmt.save-inputs=true'
    )
}

function Get-ProviderSchema {
    param([string] $CodeModel)
    $model = Get-Content $CodeModel -Raw | ConvertFrom-Json -Depth 100
    foreach ($client in $model.clients) {
        foreach ($decorator in @($client.decorators)) {
            if ($decorator.name -eq 'Azure.ClientGenerator.Core.@armProviderSchema') { return $decorator.arguments }
        }
    }
    throw "ARM provider schema not found in $CodeModel"
}

$nodeVersion = (& node --version).TrimStart('v')
if ($nodeVersion -ne $lock.tooling.node) {
    throw "Node $($lock.tooling.node) is required; found $nodeVersion."
}
$packageLockHash = (Get-FileHash (Join-Path $toolRoot 'package-lock.json') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($packageLockHash -ne $lock.tooling.packageLockSha256) { throw 'package-lock.json does not match spec.lock.json.' }
if (-not $SkipNpmInstall) { Invoke-CommandChecked npm @('ci', '--prefix', $toolRoot) }

New-Item $WorkDirectory -ItemType Directory -Force | Out-Null
$specRepo = Join-Path $WorkDirectory 'azure-rest-api-specs'
$specPath = $lock.azureRestApiSpecs.directory
Initialize-SparseCheckout $specRepo $lock.azureRestApiSpecs.repository $lock.azureRestApiSpecs.commit @(
    "/$specPath/*.tsp",
    "/$specPath/tspconfig.yaml"
)
$sourceSpec = Join-Path $specRepo $specPath
if (-not (Test-Path (Join-Path $sourceSpec 'client.tsp'))) { throw "client.tsp is missing from sparse checkout $sourceSpec" }

$sdkRepo = Join-Path $WorkDirectory 'azure-sdk-for-net'
$sdkServicePath = 'sdk/cosmosdb/Azure.ResourceManager.CosmosDB/src'
Initialize-SparseCheckout $sdkRepo $lock.azureSdkForNet.repository $lock.azureSdkForNet.commit @(
    "/$sdkServicePath/Custom/**/*.cs",
    "/$sdkServicePath/Properties/*.cs"
)

$full = Join-Path $WorkDirectory 'full'
Remove-Item $full -Recurse -Force -ErrorAction SilentlyContinue
Initialize-Project $full
Copy-Item (Join-Path $output 'src/Shared') (Join-Path $full 'src/Shared') -Recurse
Copy-Item (Join-Path $sdkRepo "$sdkServicePath/Custom/*") (Join-Path $full 'src/Custom') -Recurse
if (Test-Path (Join-Path $sdkRepo "$sdkServicePath/Properties")) {
    Copy-Item (Join-Path $sdkRepo "$sdkServicePath/Properties") (Join-Path $full 'src/Properties') -Recurse
}
Invoke-Generation (Join-Path $sourceSpec 'client.tsp') $full
Invoke-CommandChecked dotnet @('build', (Join-Path $full 'src/Azure.ResourceManager.CosmosDB.csproj'), '/p:NuGetAudit=false')

$fullSchema = Get-ProviderSchema (Join-Path $full 'tspCodeModel.json')
$rootIds = @($roots.operations.methodId)
$selected = New-Object 'System.Collections.Generic.HashSet[string]'
$selectedResources = New-Object 'System.Collections.Generic.List[object]'
foreach ($resource in $fullSchema.resources) {
    $ids = @($resource.methods.methodId)
    if (@($ids | Where-Object { $rootIds -contains $_ }).Count -gt 0) {
        [void] $selectedResources.Add($resource)
        foreach ($id in $ids) { [void] $selected.Add([string] $id) }
    }
}
foreach ($id in $rootIds) {
    if (-not $selected.Contains([string] $id)) { throw "Root operation was not resolved to a resource: $id" }
}
if ($selectedResources.Count -ne 1) { throw "Cosmos POC expected one selected resource, found $($selectedResources.Count)." }

$expandedManifest = [ordered]@{
    selectionPolicy = [string] $config.selectionPolicy
    resources = @($selectedResources | ForEach-Object {
        [ordered]@{
            resourceType = [string] $_.resourceType
            reason = 'Owns a direct MCP root operation'
            operations = @($_.methods | ForEach-Object {
                [ordered]@{
                    methodId = [string] $_.methodId
                    kind = [string] $_.kind
                    operationPath = [string] $_.operationPath
                    reason = 'Complete selected resource'
                }
            })
        }
    })
    nonResourceOperations = @()
}
$expandedManifestPath = Join-Path $WorkDirectory 'expanded-operations.json'
$expandedManifest | ConvertTo-Json -Depth 10 | Set-Content $expandedManifestPath

$allOperations = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($resource in $fullSchema.resources) { foreach ($id in $resource.methods.methodId) { [void] $allOperations.Add([string] $id) } }
foreach ($method in $fullSchema.nonResourceMethods) { [void] $allOperations.Add([string] $method.methodId) }
$excluded = @($allOperations | Where-Object { -not $selected.Contains($_) } | Sort-Object)

$scopedSpec = Join-Path $WorkDirectory 'scoped-spec'
Remove-Item $scopedSpec -Recurse -Force -ErrorAction SilentlyContinue
New-Item $scopedSpec -ItemType Directory | Out-Null
Copy-Item (Join-Path $sourceSpec '*.tsp') $scopedSpec
Copy-Item (Join-Path $sourceSpec 'tspconfig.yaml') $scopedSpec
$clientTsp = Join-Path $scopedSpec 'client.tsp'
Add-Content $clientTsp "`n`n// BEGIN AZURE MCP GENERATED OPERATION SCOPES"
foreach ($id in $excluded) { Add-Content $clientTsp ('@@scope({0}, "!csharp");' -f $id) }
Add-Content $clientTsp '// END AZURE MCP GENERATED OPERATION SCOPES'

$projected = Join-Path $WorkDirectory 'projected'
Remove-Item $projected -Recurse -Force -ErrorAction SilentlyContinue
Initialize-Project $projected
Copy-Item (Join-Path $output 'src/Shared') (Join-Path $projected 'src/Shared') -Recurse
Invoke-Generation $clientTsp $projected
Invoke-CommandChecked dotnet @('build', (Join-Path $projected 'src/Azure.ResourceManager.CosmosDB.csproj'), '/p:NuGetAudit=false')

$projectedSchema = Get-ProviderSchema (Join-Path $projected 'tspCodeModel.json')
if (@($projectedSchema.resources).Count -ne $selectedResources.Count) { throw 'Projected resource set contains missing or additional resources.' }
$actualIds = @($projectedSchema.resources.methods.methodId | Sort-Object)
$expectedIds = @($selected | Sort-Object)
if (($actualIds -join "`n") -cne ($expectedIds -join "`n")) { throw 'Projected operation set differs from the selected closure.' }

& (Join-Path $PSScriptRoot 'Test-ResourceHierarchy.ps1') `
    -FullCodeModel (Join-Path $full 'tspCodeModel.json') `
    -FullGeneratedDirectory (Join-Path $full 'src/Generated') `
    -ProjectedCodeModel (Join-Path $projected 'tspCodeModel.json') `
    -ProjectedGeneratedDirectory (Join-Path $projected 'src/Generated') `
    -SelectedResourceType @($selectedResources.resourceType)
if ($LASTEXITCODE -ne 0) { throw 'Strict hierarchy validation failed.' }

Remove-Item (Join-Path $output 'src/Generated') -Recurse -Force
Copy-Item (Join-Path $projected 'src/Generated') (Join-Path $output 'src/Generated') -Recurse
Copy-Item $expandedManifestPath (Join-Path $output 'expanded-operations.json') -Force
Write-Host "Projection updated: $($projectedSchema.resources.Count) resource(s), $($actualIds.Count) operation(s), $($excluded.Count) excluded operation(s)." -ForegroundColor Green
