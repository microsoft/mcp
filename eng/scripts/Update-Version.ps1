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
    if ($ServerName -eq 'Fabric.Mcp.Server') {
        # Fabric MCP Server follows a GA-only, minor-increment versioning strategy.
        # Bump the minor segment and reset patch so the next version is a stable GA
        # release (e.g. 1.1.0 -> 1.2.0). The shared IncrementAndSetToPrerelease helper
        # always produces a prerelease for non-zero major versions, so a direct minor
        # bump is used here instead.
        $nextVersion.Minor++
        $nextVersion.Patch = 0
    }
    else {
        $nextVersion.IncrementAndSetToPrerelease('patch')
    }
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
  Write-Host "> Update-ChangeLog.ps1 -Version '$Version' -ChangelogPath '$changeLogPath' -Unreleased `$True"
  & "$RepoRoot/eng/common/scripts/Update-ChangeLog.ps1" -Version $Version `
  -ChangelogPath $changeLogPath -Unreleased $True
}
else {
  Write-Host "> Update-ChangeLog.ps1 -Version '$Version' -ChangelogPath '$changeLogPath' -Unreleased `$False -ReplaceLatestEntryTitle `$$ReplaceLatestEntryTitle -ReleaseDate '$ReleaseDate'"
  & "$RepoRoot/eng/common/scripts/Update-ChangeLog.ps1" -Version $Version `
  -ChangelogPath $changeLogPath -Unreleased $False `
  -ReplaceLatestEntryTitle $ReplaceLatestEntryTitle -ReleaseDate $ReleaseDate
}
