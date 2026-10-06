#Requires -Version 7

<#
.SYNOPSIS
Builds ToolMetadataExporter and runs it against an executable from a signed NuGet package.

.DESCRIPTION
Resolves a server from build_info.json, extracts its platform NuGet package, locates the packaged
executable through DotnetToolSettings.xml, and runs ToolMetadataExporter against it.

.PARAMETER BuildInfoPath
Path to the pipeline build_info.json artifact.

.PARAMETER PackagesDirectory
Root directory of the downloaded packages_nuget_signed artifact.

.PARAMETER ServerName
Server name in build_info.json.

.PARAMETER ExtractDirectory
Temporary directory used to expand the NuGet package.

.PARAMETER OutputDirectory
Directory where ToolMetadataExporter writes generated JSON files.

.PARAMETER ArtifactDirectory
Directory where generated tool-change files are staged for pipeline artifact publishing.

.PARAMETER RuntimeIdentifier
Runtime-specific package and tools directory to use.
#>
param(
    [Parameter(Mandatory)]
    [string] $BuildInfoPath,

    [Parameter(Mandatory)]
    [string] $PackagesDirectory,

    [Parameter(Mandatory)]
    [string] $ServerName,

    [Parameter(Mandatory)]
    [string] $ExtractDirectory,

    [Parameter(Mandatory)]
    [string] $OutputDirectory,

    [Parameter(Mandatory)]
    [string] $ArtifactDirectory,

    [string] $RuntimeIdentifier = 'win-x64'
)

$ErrorActionPreference = 'Stop'

Write-Host "##[section]Starting tool metadata export for '$ServerName' ($RuntimeIdentifier)"
Write-Host "Loading build information from '$BuildInfoPath'."
$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json
$server = $buildInfo.servers |
    Where-Object { $_.name -eq $ServerName } |
    Select-Object -First 1
if (-not $server) {
    throw "Server '$ServerName' was not found in '$BuildInfoPath'."
}
Write-Host "Resolved server version '$($server.version)' and package '$($server.dnxPackageId)'."

$packageDirectory = Join-Path $PackagesDirectory $server.artifactPath 'platform'
Write-Host "Locating the signed package in '$packageDirectory'."
$packages = @(
    Get-ChildItem $packageDirectory -Filter "$($server.dnxPackageId).$RuntimeIdentifier.*.nupkg" |
        Where-Object { $_.Name -notlike '*.symbols.nupkg' }
)
if ($packages.Count -ne 1) {
    throw "Expected one $RuntimeIdentifier NuGet package in '$packageDirectory', but found $($packages.Count)."
}
Write-Host "Using package '$($packages[0].FullName)'."

Write-Host "Preparing extraction directory '$ExtractDirectory'."
if (Test-Path $ExtractDirectory) {
    Remove-Item $ExtractDirectory -Recurse -Force
}
New-Item -Path $ExtractDirectory -ItemType Directory | Out-Null

$archivePath = Join-Path $ExtractDirectory 'package.zip'
Write-Host "Expanding the signed package."
Copy-Item $packages[0].FullName -Destination $archivePath
Expand-Archive $archivePath -DestinationPath $ExtractDirectory

$toolDirectory = Join-Path $ExtractDirectory "tools/any/$RuntimeIdentifier"
$toolSettingsPath = Join-Path $toolDirectory 'DotnetToolSettings.xml'
Write-Host "Resolving the packaged executable from '$toolSettingsPath'."
[xml] $toolSettings = Get-Content $toolSettingsPath -Raw
$executablePath = Join-Path $toolDirectory $toolSettings.DotNetCliTool.Commands.Command.EntryPoint
if (-not (Test-Path $executablePath)) {
    throw "NuGet package executable was not found at '$executablePath'."
}
Write-Host "Resolved packaged executable '$executablePath'."

$projectPath = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot '..\tools\ToolMetadataExporter\src\ToolMetadataExporter.csproj'))
Write-Host "##[section]Building ToolMetadataExporter"
Write-Host "Project: '$projectPath'."
& dotnet build $projectPath --configuration Release --nologo
if ($LASTEXITCODE -ne 0) {
    throw "ToolMetadataExporter build failed with exit code $LASTEXITCODE."
}
Write-Host "ToolMetadataExporter build completed."

Write-Host "##[section]Running ToolMetadataExporter"
foreach ($directory in @($OutputDirectory, $ArtifactDirectory)) {
    if (Test-Path $directory) {
        Remove-Item $directory -Recurse -Force
    }
    New-Item -Path $directory -ItemType Directory | Out-Null
}

$env:AppConfig__WorkDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
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

$changeFiles = @(Get-ChildItem $OutputDirectory -Filter '*_tool_changes_*.json' -File)
if ($changeFiles.Count -gt 0) {
    Write-Host "Staging $($changeFiles.Count) tool-change file(s) in '$ArtifactDirectory'."
    $changeFiles | Copy-Item -Destination $ArtifactDirectory
    Write-Host "##vso[task.setvariable variable=ShouldPublishToolMetadataChanges]true"
}
else {
    Write-Host "No tool-change files were generated."
    Write-Host "##vso[task.setvariable variable=ShouldPublishToolMetadataChanges]false"
}

if ($exporterExitCode -ne 0) {
    throw "ToolMetadataExporter failed with exit code $exporterExitCode."
}

Write-Host "##[section]Tool metadata export completed successfully"
