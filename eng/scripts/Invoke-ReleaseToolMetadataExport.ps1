#Requires -Version 7

<#
.SYNOPSIS
Builds ToolMetadataExporter and runs it against an executable from a signed NuGet package.

.DESCRIPTION
Resolves a server from build_info.json, extracts its platform NuGet package, locates the packaged
executable through DotnetToolSettings.xml, and runs ToolMetadataExporter against it.

.PARAMETER BuildInfoPath
Path to the pipeline build_info.json artifact.

.PARAMETER BinariesDirectory
Root directory of the downloaded binaries_signed artifact.

.PARAMETER ServerName
Server name in build_info.json.

.PARAMETER ArtifactDirectory
Directory where generated tool-change files are staged for pipeline artifact publishing.

.PARAMETER RuntimeIdentifier
Runtime-specific package and tools directory to use.
#>
param(
    [Parameter(Mandatory)]
    [string] $BuildInfoPath,

    [Parameter(Mandatory)]
    [string] $BinariesDirectory,

    [Parameter(Mandatory)]
    [string] $ServerName,

    [Parameter(Mandatory)]
    [string] $ArtifactDirectory,

    [Parameter(Mandatory)]
    [string] $RuntimeIdentifier
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

Write-Host "##[section]Starting tool metadata export for '$ServerName' ($RuntimeIdentifier)"
Write-Host "Loading build information from '$BuildInfoPath'."
$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json
$server = $buildInfo.servers |
    Where-Object { $_.name -eq $ServerName } |
    Select-Object -First 1
if (-not $server) {
    throw "Server '$ServerName' was not found in '$BuildInfoPath'."
}

$platformInformation = $server.platforms |
    Where-Object { $_.name -eq $RuntimeIdentifier } |
    Select-Object -First 1
if (-not $platformInformation) {
    throw "Platform information for runtime identifier '$RuntimeIdentifier' was not found in build info for server '$ServerName'."
}

$packageDirectory = Join-Path $BinariesDirectory $platformInformation.artifactPath
Write-Host "Locating the signed binary '$($server.cliName)' in '$packageDirectory'."
$matching = Get-ChildItem -LiteralPath $packageDirectory -Filter "$($server.cliName)$($platformInformation.extension)" -File

if ($matching.Count -eq 0) {
    throw "No signed binary '$($server.cliName)' was found in '$packageDirectory'."
}
if ($matching.Count -gt 1) {
    throw "Expected one $RuntimeIdentifier signed binary '$($server.cliName)' in '$packageDirectory', but found $($matching.Count)."
}

$executablePath = $matching[0].FullName
Write-Host "Resolved server executable '$executablePath'."

$projectPath = Join-Path $RepoRoot 'eng/tools/ToolMetadataExporter/src/ToolMetadataExporter.csproj'
Write-Host "##[section]Building ToolMetadataExporter"

if (!$(Test-Path $projectPath)) {
    throw "ToolMetadataExporter project not found at '$projectPath'."
}

& dotnet build $projectPath --configuration Release

if ($LASTEXITCODE -ne 0) {
    throw "ToolMetadataExporter build failed with exit code $LASTEXITCODE."
}

$env:AppConfig__WorkDirectory = [System.IO.Path]::GetFullPath($ArtifactDirectory)
Write-Host "Exporter output directory: '$env:AppConfig__WorkDirectory'."

$exporterExitCode = 0
Push-Location (Split-Path $projectPath -Parent)
try {
    & dotnet run --project $projectPath --configuration Release --no-build --no-launch-profile -- --AzmcpExe $executablePath
    $exporterExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($exporterExitCode -ne 0) {
    throw "ToolMetadataExporter failed with exit code $exporterExitCode."
}

Write-Host "##[section]Tool metadata export completed successfully"
