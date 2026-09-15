#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidatePattern('^[a-z0-9][a-z0-9-]*$')] [string] $Service,
    [Parameter(Mandatory)] [string] $PackageId,
    [Parameter(Mandatory)] [string] $SdkPath,
    [Parameter(Mandatory)] [string[]] $ConsumerProject,
    [string] $ProductProject = 'servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj',
    [string] $ToolDirectory,
    [string] $OutputDirectory,
    [string] $NodeVersion
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$configPath = Join-Path $repoRoot "eng/sdk-generation/services/$Service.json"
if (-not $ToolDirectory) { $ToolDirectory = "eng/sdk-generation/environments/$Service" }
if (-not $OutputDirectory) { $OutputDirectory = "eng/generated/$PackageId" }
$toolRoot = Join-Path $repoRoot $ToolDirectory
$outputRoot = Join-Path $repoRoot $OutputDirectory
$workRoot = Join-Path $repoRoot "eng/sdk-generation/.work/$Service/initialize"

if (Test-Path $configPath) { throw "Service projection already exists: $configPath" }
if (Test-Path $outputRoot) { throw "Generated output already exists: $outputRoot" }
if (-not $NodeVersion) { $NodeVersion = (& node --version).TrimStart('v') }

$config = [ordered]@{
    packageId = $PackageId
    consumerProjects = @($ConsumerProject | Sort-Object -CaseSensitive)
    selectionPolicy = 'minimumHierarchyClosure'
    toolDirectory = $ToolDirectory
    productProject = $ProductProject
    sdkPath = $SdkPath
    outputDirectory = $OutputDirectory
}
New-Item (Split-Path $configPath -Parent) -ItemType Directory -Force | Out-Null
$config | ConvertTo-Json -Depth 10 | Set-Content $configPath

New-Item $workRoot -ItemType Directory -Force | Out-Null
$provenancePath = Join-Path $workRoot 'provenance.json'
& (Join-Path $PSScriptRoot 'Resolve-PackageProvenance.ps1') -Service $Service -OutputPath $provenancePath -WorkDirectory (Join-Path $workRoot 'provenance')
$provenance = Get-Content $provenancePath -Raw | ConvertFrom-Json -Depth 20

New-Item $toolRoot -ItemType Directory -Force | Out-Null
$packageJson = [ordered]@{
    name = "azure-mcp-$Service-sdk-generation-tools"
    private = $true
    engines = [ordered]@{ node = $NodeVersion }
    dependencies = $provenance.tooling.dependencies
    devDependencies = $provenance.tooling.devDependencies
}
$packageJson | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $toolRoot 'package.json')
$NodeVersion | Set-Content (Join-Path $toolRoot '.nvmrc')
& npm install --prefix $toolRoot --package-lock-only
if ($LASTEXITCODE -ne 0) { throw 'Unable to create the service npm lock file.' }

New-Item (Join-Path $outputRoot 'src/Shared') -ItemType Directory -Force | Out-Null
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>$PackageId</AssemblyName>
    <RootNamespace>$PackageId</RootNamespace>
    <IsAotCompatible>true</IsAotCompatible>
    <NoWarn>`$(NoWarn);SCM0005</NoWarn>
  </PropertyGroup>
  <PropertyGroup Condition="'`$(Configuration)' == 'Release'">
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Core" />
    <PackageReference Include="Azure.ResourceManager" />
  </ItemGroup>
</Project>
"@ | Set-Content (Join-Path $outputRoot "$PackageId.csproj")
@{ operations = @() } | ConvertTo-Json | Set-Content (Join-Path $outputRoot 'roots.json')
@{ selectionPolicy = 'minimumHierarchyClosure'; directRootCount = 0; resources = @(); nonResourceOperations = @() } |
    ConvertTo-Json -Depth 10 | Set-Content (Join-Path $outputRoot 'expanded-operations.json')
@() | ConvertTo-Json | Set-Content (Join-Path $outputRoot 'expected-hierarchy.json')

$sdkRepo = Join-Path $workRoot 'provenance/azure-sdk-for-net'
$sharedManifest = Get-Content (Join-Path $repoRoot 'eng/sdk-generation/shared-source-files.json') -Raw | ConvertFrom-Json
foreach ($entry in $sharedManifest.files) {
    $content = & git -C $sdkRepo show "$($provenance.azureSdkForNet.tag):$($entry.source)" 2>$null
    if ($LASTEXITCODE -ne 0) { throw "Required shared source is missing at the package release: $($entry.source)" }
    $target = Join-Path $outputRoot "src/Shared/$($entry.target)"
    New-Item (Split-Path $target -Parent) -ItemType Directory -Force | Out-Null
    $content | Set-Content $target
}

[xml] $packages = Get-Content (Join-Path $repoRoot 'Directory.Packages.props') -Raw
function Get-PackageVersion([string] $Id) {
    $nodes = @($packages.Project.ItemGroup.PackageVersion | Where-Object { $_.Include -ceq $Id })
    if ($nodes.Count -ne 1) { throw "Expected one PackageVersion for '$Id'." }
    return [string] $nodes[0].Version
}

$specRepo = Join-Path $workRoot 'azure-rest-api-specs'
New-Item $specRepo -ItemType Directory -Force | Out-Null
& git -C $specRepo init | Out-Null
& git -C $specRepo remote add origin "https://github.com/$($provenance.azureRestApiSpecs.repository).git"
& git -C $specRepo fetch --depth 1 --filter=blob:none origin $provenance.azureRestApiSpecs.commit
if ($LASTEXITCODE -ne 0) { throw 'Unable to fetch the pinned specification commit.' }
$tspConfigPath = "$($provenance.azureRestApiSpecs.directory)/tspconfig.yaml"
$tspConfig = & git -C $specRepo show "FETCH_HEAD:$tspConfigPath"
if ($LASTEXITCODE -ne 0) { throw "Unable to read $tspConfigPath" }
$inCSharp = $false
$apiVersion = $null
foreach ($line in $tspConfig) {
    if ($line -match '^\s{2}"?@azure-typespec/http-client-csharp-mgmt"?:\s*$') { $inCSharp = $true; continue }
    if ($inCSharp -and $line -match '^\s{2}\S') { break }
    if ($inCSharp -and $line -match '^\s+api-version:\s*"?([^"\s]+)"?') { $apiVersion = $Matches[1]; break }
}
if (-not $apiVersion) { throw "Unable to resolve the C# api-version from $tspConfigPath" }

$packageLockHash = (Get-FileHash (Join-Path $toolRoot 'package-lock.json') -Algorithm SHA256).Hash.ToLowerInvariant()
$lock = [ordered]@{
    package = $provenance.package
    azureSdkForNet = $provenance.azureSdkForNet
    azureRestApiSpecs = $provenance.azureRestApiSpecs
    tooling = [ordered]@{
        node = $NodeVersion
        packageLockSha256 = $packageLockHash
        emitter = [string] $provenance.tooling.emitter
        compiler = [string] $provenance.tooling.compiler
        apiVersion = $apiVersion
    }
    runtimePackages = [ordered]@{
        'Azure.Core' = Get-PackageVersion 'Azure.Core'
        'Azure.Identity' = Get-PackageVersion 'Azure.Identity'
        'Azure.ResourceManager' = Get-PackageVersion 'Azure.ResourceManager'
    }
    source = [ordered]@{
        sharedSource = [ordered]@{
            repository = 'Azure/azure-sdk-for-net'
            commit = [string] $provenance.azureSdkForNet.commit
            manifest = 'eng/sdk-generation/shared-source-files.json'
        }
        serviceCustomizations = @()
    }
}
$lock | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $outputRoot 'spec.lock.json')

Write-Host "Initialized projection '$Service'. Review roots.json, add the generated project reference to every configured consumer, remove their released package references, then run Update-ServiceProjection.ps1." -ForegroundColor Green
