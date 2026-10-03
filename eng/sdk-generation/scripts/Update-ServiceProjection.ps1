#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9][a-z0-9-]*$')]
    [string] $Service,
    [string] $WorkDirectory,
    [switch] $SkipNpmInstall
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$configPath = Join-Path $repoRoot "eng/sdk-generation/services/$Service.json"
$config = Get-Content $configPath -Raw | ConvertFrom-Json
$output = Join-Path $repoRoot $config.outputDirectory
$lock = Get-Content (Join-Path $output 'spec.lock.json') -Raw | ConvertFrom-Json
if (-not $config.toolDirectory) { throw "toolDirectory is required in $configPath" }
$toolRoot = Join-Path $repoRoot $config.toolDirectory
if (-not $WorkDirectory) { $WorkDirectory = Join-Path $repoRoot "eng/sdk-generation/.work/$Service" }
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
    param([string] $Directory, [string] $ProjectFileName)
    New-Item (Join-Path $Directory 'src/Custom') -ItemType Directory -Force | Out-Null
    @'
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
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
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Core" />
    <PackageReference Include="Azure.ResourceManager" />
  </ItemGroup>
</Project>
'@ | Set-Content (Join-Path $Directory "src/$ProjectFileName")
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

function Get-ContentHash {
    param([string[]] $Files, [string[]] $Directories = @())
    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($file in $Files) {
        $resolved = (Resolve-Path $file).Path
        $hash = (Get-FileHash $resolved -Algorithm SHA256).Hash.ToLowerInvariant()
        [void] $lines.Add("file|$([IO.Path]::GetFileName($resolved))|$hash")
    }
    foreach ($directory in $Directories) {
        $resolvedDirectory = (Resolve-Path $directory).Path
        foreach ($file in Get-ChildItem $resolvedDirectory -File -Recurse | Where-Object {
            $_.FullName -notmatch '[\\/](bin|obj)[\\/]' -and $_.Name -ne 'cache-manifest.json'
        } | Sort-Object FullName) {
            $relative = [IO.Path]::GetRelativePath($resolvedDirectory, $file.FullName).Replace('\', '/')
            $hash = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
            [void] $lines.Add("tree|$relative|$hash")
        }
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

New-Item $WorkDirectory -ItemType Directory -Force | Out-Null
$consumerPath = Join-Path $WorkDirectory 'package-consumers.json'
$projectFileName = if ($config.projectFileName) { [string] $config.projectFileName } else { "$($config.packageId).csproj" }
$emitterProjectFileName = "$($config.packageId).csproj"
$generatedProject = Join-Path $output $projectFileName
& (Join-Path $PSScriptRoot 'Resolve-PackageConsumers.ps1') `
    -PackageId $config.packageId `
    -GeneratedProject $generatedProject `
    -RepositoryRoot $repoRoot `
    -OutputPath $consumerPath `
    -ProductionOnly
$actualConsumers = @(Get-Content $consumerPath -Raw | ConvertFrom-Json)
$expectedConsumers = @($config.consumerProjects | Sort-Object -CaseSensitive)
$actualConsumers = @($actualConsumers | Sort-Object -CaseSensitive)
if (($actualConsumers -join "`n") -cne ($expectedConsumers -join "`n")) {
    throw "Package consumer set differs from service configuration. Expected [$($expectedConsumers -join ', ')]; actual [$($actualConsumers -join ', ')]."
}

$nodeVersion = (& node --version).TrimStart('v')
if ($nodeVersion -ne $lock.tooling.node) {
    throw "Node $($lock.tooling.node) is required; found $nodeVersion."
}
$packageLockHash = (Get-FileHash (Join-Path $toolRoot 'package-lock.json') -Algorithm SHA256).Hash.ToLowerInvariant()
if ($packageLockHash -ne $lock.tooling.packageLockSha256) { throw 'package-lock.json does not match spec.lock.json.' }
if (-not $SkipNpmInstall) { Invoke-CommandChecked npm @('ci', '--prefix', $toolRoot) }

New-Item $WorkDirectory -ItemType Directory -Force | Out-Null
$resolvedProvenancePath = Join-Path $WorkDirectory 'resolved-provenance.json'
& (Join-Path $PSScriptRoot 'Resolve-PackageProvenance.ps1') -Service $Service -OutputPath $resolvedProvenancePath
$resolved = Get-Content $resolvedProvenancePath -Raw | ConvertFrom-Json
$comparisons = @(
    [pscustomobject]@{ Name = 'package ID'; Pinned = [string] $lock.package.id; Resolved = [string] $resolved.package.id }
    [pscustomobject]@{ Name = 'package version'; Pinned = [string] $lock.package.version; Resolved = [string] $resolved.package.version }
    [pscustomobject]@{ Name = 'SDK tag'; Pinned = [string] $lock.azureSdkForNet.tag; Resolved = [string] $resolved.azureSdkForNet.tag }
    [pscustomobject]@{ Name = 'SDK commit'; Pinned = [string] $lock.azureSdkForNet.commit; Resolved = [string] $resolved.azureSdkForNet.commit }
    [pscustomobject]@{ Name = 'spec repository'; Pinned = [string] $lock.azureRestApiSpecs.repository; Resolved = [string] $resolved.azureRestApiSpecs.repository }
    [pscustomobject]@{ Name = 'spec commit'; Pinned = [string] $lock.azureRestApiSpecs.commit; Resolved = [string] $resolved.azureRestApiSpecs.commit }
    [pscustomobject]@{ Name = 'spec directory'; Pinned = [string] $lock.azureRestApiSpecs.directory; Resolved = [string] $resolved.azureRestApiSpecs.directory }
    [pscustomobject]@{ Name = 'emitter version'; Pinned = [string] $lock.tooling.emitter; Resolved = [string] $resolved.tooling.emitter }
    [pscustomobject]@{ Name = 'compiler version'; Pinned = [string] $lock.tooling.compiler; Resolved = [string] $resolved.tooling.compiler }
)
foreach ($comparison in $comparisons) {
    if (-not [string]::Equals($comparison.Pinned, $comparison.Resolved, [StringComparison]::Ordinal)) {
        throw "Pinned $($comparison.Name) '$($comparison.Pinned)' differs from package provenance '$($comparison.Resolved)'."
    }
}
$toolPackage = Get-Content (Join-Path $toolRoot 'package.json') -Raw | ConvertFrom-Json
foreach ($section in @('dependencies', 'devDependencies')) {
    $localProperties = @($toolPackage.$section.PSObject.Properties)
    $resolvedProperties = @($resolved.tooling.$section.PSObject.Properties)
    if ($localProperties.Count -ne $resolvedProperties.Count) { throw "$section differs from the package's emitter manifest." }
    foreach ($property in $resolvedProperties) {
        $local = $toolPackage.$section.PSObject.Properties[$property.Name]
        if ($null -eq $local -or -not [string]::Equals([string] $local.Value, [string] $property.Value, [StringComparison]::Ordinal)) {
            throw "$section dependency '$($property.Name)' differs from the package's emitter manifest."
        }
    }
}

$specRepo = Join-Path $WorkDirectory 'azure-rest-api-specs'
$specPath = $lock.azureRestApiSpecs.directory
$specPatterns = [System.Collections.Generic.List[string]]::new()
[void] $specPatterns.Add("/$specPath/*.tsp")
[void] $specPatterns.Add("/$specPath/tspconfig.yaml")
foreach ($additionalDirectory in @($lock.azureRestApiSpecs.additionalDirectories)) {
    [void] $specPatterns.Add("/$additionalDirectory/**")
}
Initialize-SparseCheckout $specRepo $lock.azureRestApiSpecs.repository $lock.azureRestApiSpecs.commit $specPatterns.ToArray()
$sourceSpec = Join-Path $specRepo $specPath
if (-not (Test-Path (Join-Path $sourceSpec 'client.tsp'))) { throw "client.tsp is missing from sparse checkout $sourceSpec" }

$sdkRepo = Join-Path $WorkDirectory 'azure-sdk-for-net'
$sdkServicePath = "$($config.sdkPath)/src"
Initialize-SparseCheckout $sdkRepo $lock.azureSdkForNet.repository $lock.azureSdkForNet.commit @(
    "/$sdkServicePath/Custom/**/*.cs",
    "/$sdkServicePath/Properties/*.cs"
)

$full = Join-Path $WorkDirectory 'full'
$fullIdentity = Get-ContentHash -Files @(
    (Join-Path $output 'spec.lock.json'),
    (Join-Path $toolRoot 'package-lock.json'),
    $configPath,
    (Join-Path $repoRoot 'Directory.Packages.props'),
    $PSCommandPath
) -Directories @((Join-Path $output 'src/Shared'))
$fullCache = Join-Path $WorkDirectory "cache/full/$fullIdentity"
$cacheManifestPath = Join-Path $fullCache 'cache-manifest.json'
$cacheHit = $false
if (Test-Path $cacheManifestPath) {
    $cacheManifest = Get-Content $cacheManifestPath -Raw | ConvertFrom-Json
    if ($cacheManifest.identity -ceq $fullIdentity -and
        (Test-Path (Join-Path $fullCache 'tspCodeModel.json')) -and
        (Test-Path (Join-Path $fullCache 'src/Generated'))) {
        $actualCacheHash = Get-ContentHash -Files @() -Directories @($fullCache)
        $cacheHit = $actualCacheHash -ceq [string] $cacheManifest.contentHash
    }
    if (-not $cacheHit) {
        Write-Warning "Discarding corrupt full-generation cache entry: $fullCache"
        Remove-Item $fullCache -Recurse -Force
    }
}

Remove-Item $full -Recurse -Force -ErrorAction SilentlyContinue
if ($cacheHit) {
    Copy-Item $fullCache $full -Recurse
    Remove-Item (Join-Path $full 'cache-manifest.json') -Force
    Write-Host "Reused full-generation cache: $fullIdentity" -ForegroundColor Green
}
else {
    Initialize-Project $full $emitterProjectFileName
    Copy-Item (Join-Path $output 'src/Shared') (Join-Path $full 'src/Shared') -Recurse
    Copy-Item (Join-Path $sdkRepo "$sdkServicePath/Custom/*") (Join-Path $full 'src/Custom') -Recurse
    if (Test-Path (Join-Path $sdkRepo "$sdkServicePath/Properties")) {
        Copy-Item (Join-Path $sdkRepo "$sdkServicePath/Properties") (Join-Path $full 'src/Properties') -Recurse
    }
    Invoke-Generation (Join-Path $sourceSpec 'client.tsp') $full
    Remove-Item (Join-Path $full 'src/bin') -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $full 'src/obj') -Recurse -Force -ErrorAction SilentlyContinue
    $cacheParent = Split-Path $fullCache -Parent
    New-Item $cacheParent -ItemType Directory -Force | Out-Null
    $cacheTemp = "$fullCache.tmp"
    Remove-Item $cacheTemp -Recurse -Force -ErrorAction SilentlyContinue
    Copy-Item $full $cacheTemp -Recurse
    $contentHash = Get-ContentHash -Files @() -Directories @($cacheTemp)
    [ordered]@{ identity = $fullIdentity; contentHash = $contentHash } | ConvertTo-Json | Set-Content (Join-Path $cacheTemp 'cache-manifest.json')
    Move-Item $cacheTemp $fullCache
    Write-Host "Stored full-generation cache: $fullIdentity" -ForegroundColor Green
}
Invoke-CommandChecked dotnet @('build', (Join-Path $full "src/$emitterProjectFileName"), '/p:NuGetAudit=false')

$fullCodeModel = Join-Path $full 'tspCodeModel.json'
$expandedManifestPath = Join-Path $WorkDirectory 'expanded-operations.json'
& (Join-Path $PSScriptRoot 'Resolve-MinimumHierarchyClosure.ps1') `
    -CodeModel $fullCodeModel `
    -Roots (Join-Path $output 'roots.json') `
    -OutputPath $expandedManifestPath
$expandedManifest = Get-Content $expandedManifestPath -Raw | ConvertFrom-Json -Depth 100
$selectedResourceTypes = @($expandedManifest.resources.resourceType)
$selectedOperationIds = @($expandedManifest.resources.operations.methodId) + @($expandedManifest.nonResourceOperations.methodId)
if ($selectedResourceTypes.Count -eq 0 -or $selectedOperationIds.Count -eq 0) { throw 'Minimum hierarchy closure is empty.' }

$scopedSpec = Join-Path $WorkDirectory 'scoped-spec'
& (Join-Path $PSScriptRoot 'New-ScopedSpec.ps1') `
    -SourceDirectory $sourceSpec `
    -CodeModel $fullCodeModel `
    -Closure $expandedManifestPath `
    -OutputDirectory $scopedSpec
$clientTsp = Join-Path $scopedSpec 'client.tsp'

$projected = Join-Path $WorkDirectory 'projected'
Remove-Item $projected -Recurse -Force -ErrorAction SilentlyContinue
Initialize-Project $projected $emitterProjectFileName
Copy-Item (Join-Path $output 'src/Shared') (Join-Path $projected 'src/Shared') -Recurse
Invoke-Generation $clientTsp $projected
Invoke-CommandChecked dotnet @('build', (Join-Path $projected "src/$emitterProjectFileName"), '/p:NuGetAudit=false')

$projectedSchema = Get-ProviderSchema (Join-Path $projected 'tspCodeModel.json')
if (@($projectedSchema.resources).Count -ne $selectedResourceTypes.Count) { throw 'Projected resource set contains missing or additional resources.' }
$actualIds = @(@($projectedSchema.resources.methods.methodId) + @($projectedSchema.nonResourceMethods.methodId) | Sort-Object -CaseSensitive)
$expectedIds = @($selectedOperationIds | Sort-Object -CaseSensitive)
if (($actualIds -join "`n") -cne ($expectedIds -join "`n")) { throw 'Projected operation set differs from the selected closure.' }

& (Join-Path $PSScriptRoot 'Test-ResourceHierarchy.ps1') `
    -FullCodeModel $fullCodeModel `
    -FullGeneratedDirectory (Join-Path $full 'src/Generated') `
    -ProjectedCodeModel (Join-Path $projected 'tspCodeModel.json') `
    -ProjectedGeneratedDirectory (Join-Path $projected 'src/Generated') `
    -SelectedResourceType $selectedResourceTypes

$expectedHierarchyPath = Join-Path $WorkDirectory 'expected-hierarchy.json'
& (Join-Path $PSScriptRoot 'Export-SelectedHierarchy.ps1') `
    -CodeModel $fullCodeModel `
    -GeneratedDirectory (Join-Path $full 'src/Generated') `
    -Closure $expandedManifestPath `
    -OutputPath $expectedHierarchyPath

Remove-Item (Join-Path $output 'src/Generated') -Recurse -Force
Copy-Item (Join-Path $projected 'src/Generated') (Join-Path $output 'src/Generated') -Recurse
Copy-Item $expandedManifestPath (Join-Path $output 'expanded-operations.json') -Force
Copy-Item $expectedHierarchyPath (Join-Path $output 'expected-hierarchy.json') -Force

Invoke-CommandChecked dotnet @('build', $generatedProject, '/p:NuGetAudit=false')
foreach ($consumer in $expectedConsumers) {
    Invoke-CommandChecked dotnet @('build', (Join-Path $repoRoot $consumer), '/p:NuGetAudit=false')
}
if ($config.productProject) {
    $packageOutput = & dotnet list (Join-Path $repoRoot $config.productProject) package --include-transitive
    if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect the product package graph.' }
    if ($packageOutput -match "(?m)^\s*>\s+$([regex]::Escape([string] $config.packageId))\s") {
        throw "Released package remains in the product dependency graph: $($config.packageId)"
    }
}

$fullSchema = Get-ProviderSchema $fullCodeModel
$fullOperationCount = @($fullSchema.resources.methods).Count + @($fullSchema.nonResourceMethods).Count
Write-Host "Projection updated: $($projectedSchema.resources.Count) resource(s), $($actualIds.Count) retained, $($fullOperationCount - $actualIds.Count) excluded operation(s)." -ForegroundColor Green
