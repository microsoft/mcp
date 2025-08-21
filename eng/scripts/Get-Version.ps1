#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $PrereleaseLabel,
    [int] $PrereleaseNumber,
    [string] $VariableName,
    [string] $ServerName
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

function GetVersion($server) {
    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$server.csproj"
    $version = [AzureEngSemanticVersion]::new($properties.Version)

    if ($PrereleaseLabel) {
        $version.PrereleaseLabel = $PrereleaseLabel
        $version.PrereleaseNumber = $PrereleaseNumber
    }

    return $version.ToString()
}



if ($ServerName) {
    $version = GetVersion $ServerName

    if ($VariableName) {
        # If $ServerName is specified, set the version variable for the specific server
        Write-Host "##vso[task.setvariable variable=$VariableName]$version"
    }

    $version
} else {
    $serverNames = Get-ChildItem "$RepoRoot/servers" -Directory | Select-Object -ExpandProperty Name

    $versions = @()

    foreach ($server in $serverNames) {
        $version = GetVersion $server
        $versions += @{ Name = $server; Version = $version }
    }

    $versions | Select-Object Name, Version
}
