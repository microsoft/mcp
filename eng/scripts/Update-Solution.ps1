#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string[]] $ServerNames,
    [switch] $Root,
    [switch] $All
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$ErrorActionPreference = "Stop"

function Update-Solution {
    param (
        [string] $serverDirectory
    )

    Write-Host "Updating solution for server directory: $($serverDirectory)"
    $serverName = Split-Path -Leaf $serverDirectory

    $slnFile = ".temp.slnx"

    Write-Host "Removing existing solution files" -ForegroundColor Cyan
    Remove-Item -Path "$serverDirectory/$serverName.sln" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$serverDirectory/$serverName.slnx" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue

    # we're creating the solution file in the repo root so it auto creates the repo folder structure in the solution
    Write-Host "Creating new solution file: $slnFile" -ForegroundColor Cyan
    dotnet new sln -n ".temp" --format slnx

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

        # Skip core unit tests that have dependencies on different servers
        # For example, Azure.Mcp.Core.UnitTests depends on Azure.Mcp.Server
        if ($projectArea -like "*Core*") {
            $testProjects = Get-ChildItem -Path "$projectArea/tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
            foreach ($testProject in $testProjects) {
                $testProjectContent = Get-Content $testProject.FullName -Raw
                # Check if this test project references a different server than the one we're building
                $referencesOtherServer = $false
                $allServers = Get-ChildItem -Path "$RepoRoot/servers" -Directory | ForEach-Object { $_.Name }
                foreach ($otherServer in $allServers) {
                    if ($otherServer -ne $serverName -and $testProjectContent -like "*$otherServer*") {
                        $referencesOtherServer = $true
                        break
                    }
                }
                if (-not $referencesOtherServer) {
                    dotnet sln $slnFile add $testProject
                }
            }
        } else {
            # For non-core projects, add all tests normally
            $testProjects = Get-ChildItem -Path "$projectArea/tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
            foreach ($testProject in $testProjects) {
                dotnet sln $slnFile add $testProject
            }
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
    Set-Content -Path "$serverDirectory/$serverName.slnx" -Value $contents -NoNewline -Force
    Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue
    Write-Host "Solution update complete for server: $serverName" -ForegroundColor Green  
}

function Update-RootSolution {
    Write-Host "Updating root solution" -ForegroundColor Cyan

    $slnFile = "Microsoft.Mcp.slnx"

    Write-Host "Removing existing root solution files" -ForegroundColor Cyan
    Remove-Item -Path "$RepoRoot/Microsoft.Mcp.sln" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$RepoRoot/Microsoft.Mcp.slnx" -Force -ErrorAction SilentlyContinue
    Write-Host "Creating new root solution file: $slnFile" -ForegroundColor Cyan
    dotnet new sln -n "Microsoft.Mcp" --format slnx

    $allProjects = Get-ChildItem -Path $RepoRoot -Filter "*.csproj" -Recurse
    Write-Host "Adding all projects to root solution" -ForegroundColor Cyan
    foreach ($project in $allProjects) {
        dotnet sln $slnFile add $project
    }   

    Write-Host "Root solution update complete." -ForegroundColor Green  
}

$originalLocation = Get-Location
try {
    Set-Location $RepoRoot

    if($All -or $Root) {
        Update-RootSolution
    }

    if($All -or $ServerNames) {
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
}
finally {
    Set-Location $originalLocation
}
