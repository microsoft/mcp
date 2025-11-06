#!/bin/env pwsh
#Requires -Version 7
[CmdletBinding(DefaultParameterSetName='default')]
param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [Parameter(Mandatory=$true, ParameterSetName='Release')]
    [string] $Version,
    [Parameter(Mandatory=$true, ParameterSetName='Release')]
    [string] $ReleaseDate,
    [Parameter(ParameterSetName='Release')]
    [boolean] $ReplaceLatestEntryTitle=$true
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$projectFile = "$RepoRoot/servers/$ServerName/src/$ServerName.csproj"
$changeLogPath = "$RepoRoot/servers/$ServerName/CHANGELOG.md"
$serverJsonPath = "$RepoRoot/servers/$ServerName/server.json"

if(!(Test-Path $projectFile)) {
    Write-Error "Project file $projectFile does not exist."
    exit 1
}

$project = [xml](Get-Content $projectFile)
$currentVersion = $project.Project.PropertyGroup.Version | Select-Object -First 1

$autoVersion = $false
if (!$Version) {
    # get the number of commits since the last tag
    $nextVersion = [AzureEngSemanticVersion]::new($currentVersion)
    $nextVersion.IncrementAndSetToPrerelease('patch')
    $Version = $nextVersion.ToString()
    $autoVersion = $true
}

Write-Host "Current Version: $currentVersion"
Write-Host "New Version: $Version"
Write-Host "Updating project file $projectFile"

$projectText = Get-Content $projectFile -Raw
$projectText = $projectText -replace "<Version>$([Regex]::Escape($currentVersion))</Version>", "<Version>$Version</Version>"
$projectText | Set-Content $projectFile -Force -NoNewLine

if ($autoVersion) {
  & "$RepoRoot/eng/common/scripts/Update-ChangeLog.ps1" -Version $Version `
  -ChangelogPath $changeLogPath -Unreleased $True
}
else {
  & "$RepoRoot/eng/common/scripts/Update-ChangeLog.ps1" -Version $Version `
  -ChangelogPath $changeLogPath -Unreleased $False `
  -ReplaceLatestEntryTitle $ReplaceLatestEntryTitle -ReleaseDate $ReleaseDate
}

Write-Host "Updating server.json file $serverJsonPath"

$jsonContent = Get-Content -Path $serverJsonPath -Raw
$jsonObject = $jsonContent | ConvertFrom-Json

# Update top-level version field
if ($jsonObject.PSObject.Properties.Name -contains "version") {
    Write-Debug "Updating top-level version: $($jsonObject.version) => $Version"
    $jsonObject.version = $Version
}

# Update version fields in packages array
if ($jsonObject.PSObject.Properties.Name -contains "packages" -and $jsonObject.packages -is [Array]) {
    Write-Debug "Processing packages array..."

    for ($i = 0; $i -lt $jsonObject.packages.Count; $i++) {
        $package = $jsonObject.packages[$i]
        
        if ($package -is [PSCustomObject] -and $package.PSObject.Properties.Name -contains "version") {
            $packageName = if ($package.PSObject.Properties.Name -contains "identifier") { $package.identifier } else { $null }

            if ($packageName) {
                Write-Debug "  Updating version in $packageName`: $($package.version) → $Version"
                $package.version = $Version
            } else {
                Write-Debug "  Package at index $i has no identifier."
            }

        }
    }
}
  
# Convert back to JSON with proper formatting
$updatedJson = $jsonObject | ConvertTo-Json -Depth 10
$updatedJson | Set-Content -Path $serverJsonPath -Force -NoNewLine