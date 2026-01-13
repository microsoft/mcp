#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string[]] $ServerNames,
    [switch] $Flat
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$ErrorActionPreference = "Stop"

function Update-Solution {
    param (
        [string] $serverDirectory
    )

    Write-Host "Updating solution for server directory: $($serverDirectory)"
    $serverName = Split-Path -Leaf $serverDirectory

    $slnFile = "$serverName.slnx"

    Write-Host "Removing existing solution files" -ForegroundColor Cyan
    Remove-Item -Path "$serverDirectory/$serverName.sln" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$serverDirectory/$serverName.slnx" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue

    # we're creating the solution file in the repo root so it auto creates the repo folder structure in the solution
    Write-Host "Creating new solution file: $slnFile" -ForegroundColor Cyan
    dotnet new sln -n $serverName --format slnx

    Write-Host "Adding server projects and dependencies to solution" -ForegroundColor Cyan
    $serverProjects = Get-ChildItem -Path "$serverDirectory/src" -Filter "*.csproj" | Sort-Object -Property FullName
    foreach ($project in $serverProjects) {
        dotnet sln $slnFile add $project
    }

    $projects = dotnet sln $slnFile list | Where-Object { $_ -like "*.csproj" } | ForEach-Object { Resolve-Path $_ }

    Write-Host "Adding tests to solution" -ForegroundColor Cyan
    foreach ($project in $projects) {
        $projectDirectory = Split-Path -Parent $project
        $projectArea = Split-Path -Parent $projectDirectory

        if($serverName -ne 'Azure.Mcp.Server' -and $projectArea -like "*Azure.Mcp.Core*") {
            # Because of the Azure.Mcp.Core.UnitTests -> Azure.Mcp.Server -> All Azure Tools dependency chain, when
            # we're not building the Azure.Mcp.Server solution, avoid adding the Azure.Mcp.Core.UnitTests project
            continue
        }
        $testProjects = Get-ChildItem -Path "$projectArea/tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
        foreach ($testProject in $testProjects) {
            dotnet sln $slnFile add $testProject
        }
    }

    # When moving the solution file into the server directory, we need to update the project paths
    $contents = Get-Content $slnFile -Raw
    $pathMatches = ($contents | Select-String -Pattern ' Path="([^"]+)"' -AllMatches).Matches
    foreach($match in $pathMatches) {
        $fullPath = "$RepoRoot/$($match.Groups[1].Value)"
        $serverRelativePath = Resolve-Path $fullPath -Relative -RelativeBasePath $serverDirectory
        $contents = $contents.Replace($match.Value, " Path=`"$($serverRelativePath.Replace('\', '/'))`"")
    }
    Set-Content -Path "$serverDirectory/$slnFile" -Value $contents -Force
    Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue
    Write-Host "Solution update complete for server: $serverName" -ForegroundColor Green  
}

$originalLocation = Get-Location
try {
    Set-Location $RepoRoot

    $serverDirectories = Get-ChildItem -Path "$RepoRoot/servers" -Directory
    $serverFilters = $ServerNames | ForEach-Object { "*$_*" }
    if ($serverFilters) {
        $serverDirectories = $serverDirectories | Where-Object {
            foreach($filter in $serverFilters) {
                if ($_.Name -like $filter) {
                    return $true
                }
            }
            return $false
        }
    }

    foreach ($serverDir in $serverDirectories) {
        Update-Solution -ServerDirectory $serverDir
    }
}
finally {
    Remove-Item "$RepoRoot/*.slnx" -Force -ErrorAction SilentlyContinue
    Set-Location $originalLocation
}
