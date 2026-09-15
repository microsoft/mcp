#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Service,
    [string] $OutputPath,
    [string] $WorkDirectory
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$configPath = Join-Path $repoRoot "eng/sdk-generation/services/$Service.json"
if (-not (Test-Path $configPath)) { throw "Service configuration not found: $configPath" }
$config = Get-Content $configPath -Raw | ConvertFrom-Json

[xml] $packages = Get-Content (Join-Path $repoRoot 'Directory.Packages.props') -Raw
$packageNodes = @($packages.Project.ItemGroup.PackageVersion | Where-Object { $_.Include -ceq $config.packageId })
if ($packageNodes.Count -ne 1) { throw "Expected one PackageVersion for '$($config.packageId)', found $($packageNodes.Count)." }
$packageVersion = [string] $packageNodes[0].Version
if (-not $packageVersion) { throw "Package version is empty for '$($config.packageId)'." }

if ($config.packageVersion -and $config.packageVersion -cne $packageVersion) {
    throw "Service configuration version '$($config.packageVersion)' differs from Directory.Packages.props version '$packageVersion'. Projection never changes package versions."
}
if (-not $config.sdkPath) { throw "sdkPath is required in $configPath" }

$tag = "$($config.packageId)_$packageVersion"
if (-not $WorkDirectory) { $WorkDirectory = Join-Path $repoRoot "eng/sdk-generation/.work/$Service/provenance" }
$WorkDirectory = [IO.Path]::GetFullPath($WorkDirectory)
$sdkRepo = Join-Path $WorkDirectory 'azure-sdk-for-net'
if (-not (Test-Path (Join-Path $sdkRepo '.git'))) {
    New-Item $sdkRepo -ItemType Directory -Force | Out-Null
    & git -C $sdkRepo init | Out-Null
    & git -C $sdkRepo remote add origin https://github.com/Azure/azure-sdk-for-net.git
}
& git -C $sdkRepo fetch --force --depth 1 origin "refs/tags/$tag`:refs/tags/$tag"
if ($LASTEXITCODE -ne 0) { throw "Unable to fetch azure-sdk-for-net tag '$tag'." }
$sdkCommit = (& git -C $sdkRepo rev-list -n 1 $tag).Trim()
if (-not $sdkCommit) { throw "Unable to resolve commit for '$tag'." }

$tspLocationPath = "$($config.sdkPath)/tsp-location.yaml"
$tspLocation = & git -C $sdkRepo show "$tag`:$tspLocationPath" 2>$null
if ($LASTEXITCODE -ne 0 -or -not $tspLocation) {
    throw "Package '$($config.packageId)' $packageVersion is not TypeSpec-generated: $tspLocationPath is missing at $tag."
}

$values = @{}
$additionalDirectories = [System.Collections.Generic.List[string]]::new()
$readingAdditional = $false
foreach ($line in $tspLocation) {
    if ($line -match '^([A-Za-z]+):\s*(.*)$') {
        $key = $Matches[1]
        $value = $Matches[2].Trim().Trim("'`"")
        $values[$key] = $value
        $readingAdditional = $key -eq 'additionalDirectories'
        continue
    }
    if ($readingAdditional -and $line -match '^\s*-\s*(.+)$') {
        [void] $additionalDirectories.Add($Matches[1].Trim().Trim("'`""))
    }
}
foreach ($required in @('directory', 'commit', 'repo', 'emitterPackageJsonPath')) {
    if (-not $values[$required]) { throw "Missing '$required' in $tspLocationPath at $tag." }
}

$emitterManifestText = & git -C $sdkRepo show "$tag`:$($values.emitterPackageJsonPath)"
if ($LASTEXITCODE -ne 0) { throw "Emitter manifest '$($values.emitterPackageJsonPath)' is missing at $tag." }
$emitterManifest = $emitterManifestText | ConvertFrom-Json
$emitterVersion = [string] $emitterManifest.dependencies.'@azure-typespec/http-client-csharp-mgmt'
$compilerVersion = [string] $emitterManifest.devDependencies.'@typespec/compiler'
if (-not $emitterVersion -or -not $compilerVersion) { throw 'Emitter manifest does not pin the management emitter and TypeSpec compiler.' }

$result = [ordered]@{
    package = [ordered]@{
        id = [string] $config.packageId
        version = $packageVersion
    }
    azureSdkForNet = [ordered]@{
        repository = 'Azure/azure-sdk-for-net'
        tag = $tag
        commit = $sdkCommit
        sdkPath = [string] $config.sdkPath
        emitterPackageJsonPath = [string] $values.emitterPackageJsonPath
    }
    azureRestApiSpecs = [ordered]@{
        repository = [string] $values.repo
        commit = [string] $values.commit
        directory = [string] $values.directory
        additionalDirectories = $additionalDirectories.ToArray()
    }
    tooling = [ordered]@{
        emitter = $emitterVersion
        compiler = $compilerVersion
    }
}

$json = $result | ConvertTo-Json -Depth 10
if ($OutputPath) {
    $json | Set-Content $OutputPath
    Write-Host "Wrote package provenance: $OutputPath" -ForegroundColor Green
}
else {
    $json
}
