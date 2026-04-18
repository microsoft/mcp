#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string[]] $ServerNames,
    [switch] $Root,
    [switch] $All,
    [switch] $Verify
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

function Update-Solution {
    param (
        [string] $serverDirectory
    )

    Write-Host "Updating solution for server directory: $($serverDirectory)"
    $serverName = Split-Path -Leaf $serverDirectory

    $slnFile = ".temp.slnx"

    if ($Verify) {
        Write-Host "Verifying solution file for server: $serverName" -ForegroundColor Cyan
        $targetFile = "$serverDirectory/$serverName.slnx.verify"
    } else {
        Write-Host "Removing existing solution files" -ForegroundColor Cyan
        Remove-Item -Path "$serverDirectory/$serverName.sln" -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$serverDirectory/$serverName.slnx" -Force -ErrorAction SilentlyContinue
        $targetFile = "$serverDirectory/$serverName.slnx"
    }
    
    Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue

    try {
        # we're creating the solution file in the repo root so it auto creates the repo folder structure in the solution
        Write-Host "Creating new solution file: $slnFile" -ForegroundColor Cyan
        dotnet new sln -n ".temp" --format slnx

        Write-Host "Adding server projects and dependencies to solution" -ForegroundColor Cyan
        $serverProjects = Get-ChildItem -Path "$serverDirectory/src" -Filter "*.csproj" | Sort-Object -Property FullName
        dotnet sln $slnFile add $serverProjects

        $projects = dotnet sln $slnFile list | Where-Object { $_ -like "*.csproj" } | ForEach-Object { Resolve-Path $_ }

        Write-Host "Adding tests to solution" -ForegroundColor Cyan

        $testProjects = @()
        foreach ($project in $projects) {
            $projectDirectory = Split-Path -Parent $project
            $projectArea = Split-Path -Parent $projectDirectory

            if($serverName -ne 'Azure.Mcp.Server' -and $projectArea -like "*Azure.Mcp.Core*") {
                # Because of the Azure.Mcp.Core.UnitTests -> Azure.Mcp.Server -> All Azure Tools dependency chain, when
                # we're not building the Azure.Mcp.Server solution, avoid adding the Azure.Mcp.Core.UnitTests project
                continue
            }
            $testProjects += Get-ChildItem -Path "$projectArea/tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
        }

        if ($testProjects) {
            dotnet sln $slnFile add $testProjects
        }

        # When moving the solution file into the server directory, we need to update the project paths
        $contents = Get-Content $slnFile -Raw
        $pathMatches = ($contents | Select-String -Pattern ' Path="([^"]+)"' -AllMatches).Matches
        foreach($match in $pathMatches) {
            $fullPath = "$RepoRoot/$($match.Groups[1].Value)"
            $serverRelativePath = Resolve-Path $fullPath -Relative -RelativeBasePath $serverDirectory
            $contents = $contents.Replace($match.Value, " Path=`"$($serverRelativePath.Replace('\', '/'))`"")
        }
        Set-Content -Path $targetFile -Value $contents -NoNewline -Force

        # After writing, if Verify, compare and clean up
        if ($Verify) {
            $originalFile = "$serverDirectory/$serverName.slnx"
            if (-not (Test-Path $originalFile)) {
                Write-Host "❌ $serverName.slnx does not exist."
                $script:stale = $true
                $script:staleFiles += "$serverName.slnx"
            } elseif ((Get-Content $originalFile -Raw) -ne $contents) {
                Write-Host "❌ $serverName.slnx is out of date."
                $script:stale = $true
                $script:staleFiles += "$serverName.slnx"
            } else {
                Write-Host "✅ $serverName.slnx is up-to-date."
            }
            Remove-Item -Path $targetFile -Force -ErrorAction SilentlyContinue
        }
    }
    finally {
        Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Solution update complete for server: $serverName" -ForegroundColor Green
}

function Update-RootSolution {
    Write-Host "Updating root solution" -ForegroundColor Cyan

    $slnFile = "Microsoft.Mcp.slnx"

    if ($Verify) {
        Write-Host "Verifying root solution file" -ForegroundColor Cyan
        $targetFile = "Microsoft.Mcp.slnx.verify"
    } else {
        Write-Host "Removing existing root solution files" -ForegroundColor Cyan
        Remove-Item -Path "$RepoRoot/Microsoft.Mcp.sln" -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$RepoRoot/Microsoft.Mcp.slnx" -Force -ErrorAction SilentlyContinue
        $targetFile = $slnFile
    }

    $tempSlnFile = ".temp.root.slnx"
    Remove-Item -Path $tempSlnFile -Force -ErrorAction SilentlyContinue

    try {
        Write-Host "Creating new root solution file" -ForegroundColor Cyan
        dotnet new sln -n ".temp.root" --format slnx

        $allProjects = Get-ChildItem -Path $RepoRoot -Filter "*.csproj" -Recurse
        Write-Host "Adding all projects to root solution" -ForegroundColor Cyan
        dotnet sln $tempSlnFile add $allProjects

        if ($Verify) {
            $contents = Get-Content $tempSlnFile -Raw
            if (-not (Test-Path $slnFile)) {
                Write-Host "❌ $slnFile does not exist."
                $script:stale = $true
                $script:staleFiles += $slnFile
            } elseif ((Get-Content $slnFile -Raw) -ne $contents) {
                Write-Host "❌ $slnFile is out of date."
                $script:stale = $true
                $script:staleFiles += $slnFile
            } else {
                Write-Host "✅ $slnFile is up-to-date."
            }
        } else {
            Move-Item -Path $tempSlnFile -Destination $targetFile -Force
        }
    }
    finally {
        Remove-Item -Path $tempSlnFile -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Root solution update complete." -ForegroundColor Green
}

$originalLocation = Get-Location
$script:stale = $false
$script:staleFiles = @()
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

if ($Verify -and $script:stale) {
    Write-Host ""
    Write-Host "❌ The following solution files need updating:" -ForegroundColor Red
    $script:staleFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "Please run './eng/scripts/Update-Solution.ps1 -All' and commit the changes."
    exit 1
}
