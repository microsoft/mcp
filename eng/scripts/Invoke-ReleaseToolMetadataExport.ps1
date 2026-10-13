#Requires -Version 7

<#
.SYNOPSIS
Builds ToolMetadataExporter and runs it against a server executable from the build output.

.DESCRIPTION
Resolves a server and platform from build_info.json, locates the server executable under the
platform's artifact path in the binaries directory, and runs ToolMetadataExporter against it. The server must
implement `server info` and `tools list`. Template.Mcp.Server releases run in dry-run mode (no Kusto ingestion).

.PARAMETER BuildInfoPath
Path to the pipeline build_info.json artifact.

.PARAMETER BinariesDirectory
Root directory of the downloaded binaries artifact (for example, binaries_windows_x64).

.PARAMETER ServerName
Server name in build_info.json.

.PARAMETER ArtifactDirectory
Directory where generated tool-change files are staged for pipeline artifact publishing.

.PARAMETER RuntimeIdentifier
Platform name in build_info.json (for example, windows-x64).
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
Write-Host "Locating the binary '$($server.cliName)' in '$packageDirectory'."
$matching = Get-ChildItem -LiteralPath $packageDirectory -Filter "$($server.cliName)$($platformInformation.extension)" -File

if ($matching.Count -eq 0) {
    throw "No binary '$($server.cliName)' was found in '$packageDirectory'."
}
if ($matching.Count -gt 1) {
    throw "Expected one $RuntimeIdentifier binary '$($server.cliName)' in '$packageDirectory', but found $($matching.Count)."
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

$exporterArguments = @('--AzmcpExe', $executablePath)
if ($ServerName -eq 'Template.Mcp.Server') {
    Write-Host "Template server release: running exporter in dry-run mode."
    $exporterArguments += @('--IsDryRun', 'true')
}

$exporterExitCode = 0
Push-Location (Split-Path $projectPath -Parent)
try {
    & dotnet run --project $projectPath --configuration Release --no-build --no-launch-profile -- @exporterArguments
    $exporterExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($exporterExitCode -ne 0) {
    throw "ToolMetadataExporter failed with exit code $exporterExitCode."
}

Write-Host "##[section]Tool metadata export completed successfully"
