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

    $tempName = ".temp.$([guid]::NewGuid())"
    $slnFile = "$tempName.slnx"

    if ($Verify) {
        Write-Host "Verifying solution file for server: $serverName" -ForegroundColor Cyan
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
        dotnet new sln -n $tempName --format slnx

        Write-Host "Adding server projects and dependencies to solution" -ForegroundColor Cyan
        $serverProjects = Get-ChildItem -Path "$serverDirectory/src" -Filter "*.csproj" | Sort-Object -Property FullName
        dotnet sln $slnFile add $serverProjects

        $projects = dotnet sln $slnFile list | Where-Object { $_ -like "*.csproj" } | ForEach-Object { Resolve-Path $_ } | Sort-Object

        Write-Host "Adding tests to solution" -ForegroundColor Cyan

        $testProjects = @()
        foreach ($project in $projects) {
            $projectDirectory = Split-Path -Parent $project
            $projectArea = Split-Path -Parent $projectDirectory

            if($serverName -ne 'Azure.Mcp.Server' -and $projectArea -like "*Azure.Mcp.Core*") {
                # Because of the Azure.Mcp.Core.Tests -> Azure.Mcp.Server -> All Azure Tools dependency chain, when
                # we're not building the Azure.Mcp.Server solution, avoid adding the Azure.Mcp.Core.Tests project
                continue
            }
            $testProjects += Get-ChildItem -Path "$projectArea/tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
        }

        if ($testProjects) {
            $testProjects = $testProjects | Sort-Object -Property FullName -Unique
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
        if ($Verify) {
            $originalFile = "$serverDirectory/$serverName.slnx"
            if (-not (Test-Path $originalFile)) {
                Write-Host "❌ $serverName.slnx does not exist."
                $script:stale = $true
                $script:staleFiles += "$serverName.slnx"
            } elseif ((Normalize-Solution (Get-Content $originalFile -Raw)) -ne (Normalize-Solution $contents)) {
                Write-Host "❌ $serverName.slnx is out of date."
                $script:stale = $true
                $script:staleFiles += "$serverName.slnx"
            } else {
                Write-Host "✅ $serverName.slnx is up-to-date."
            }
        } else {
            Set-Content -Path $targetFile -Value $contents -Encoding utf8 -NoNewline -Force
        }
    }
    finally {
        if (Test-Path $slnFile) {    
            Remove-Item -Path $slnFile -Force -ErrorAction SilentlyContinue
        }
    }

    Write-Host "Solution update complete for server: $serverName" -ForegroundColor Green
}

function Update-RootSolution {
    Write-Host "Updating root solution" -ForegroundColor Cyan

    $slnFile = "Microsoft.Mcp.slnx"

    if ($Verify) {
        Write-Host "Verifying root solution file" -ForegroundColor Cyan
    } else {
        Write-Host "Removing existing root solution files" -ForegroundColor Cyan
        Remove-Item -Path "$RepoRoot/Microsoft.Mcp.sln" -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$RepoRoot/Microsoft.Mcp.slnx" -Force -ErrorAction SilentlyContinue
        $targetFile = $slnFile
    }

    $tempRootName = ".temp.root.$([guid]::NewGuid())"
    $tempSlnFile = "$tempRootName.slnx"
    Remove-Item -Path $tempSlnFile -Force -ErrorAction SilentlyContinue

    try {
        Write-Host "Creating new root solution file" -ForegroundColor Cyan
        dotnet new sln -n $tempRootName --format slnx

        $allProjects = Get-ChildItem -Path $RepoRoot -Filter "*.csproj" -Recurse | Sort-Object -Property FullName
        Write-Host "Adding all projects to root solution" -ForegroundColor Cyan
        dotnet sln $tempSlnFile add $allProjects

        if ($Verify) {
            $contents = Get-Content $tempSlnFile -Raw
            if (-not (Test-Path $slnFile)) {
                Write-Host "❌ $slnFile does not exist."
                $script:stale = $true
                $script:staleFiles += $slnFile
            } elseif ((Normalize-Solution (Get-Content $slnFile -Raw)) -ne (Normalize-Solution $contents)) {
                Write-Host "❌ $slnFile is out of date."
                $script:stale = $true
                $script:staleFiles += $slnFile
            } else {
                Write-Host "✅ $slnFile is up-to-date."
            }
        } else {
            $contents = Get-Content $tempSlnFile -Raw
            Set-Content -Path $targetFile -Value $contents -Encoding utf8 -NoNewline -Force
        }
    }
    finally {
        if (Test-Path $tempSlnFile) {
            Remove-Item -Path $tempSlnFile -Force -ErrorAction SilentlyContinue
        }    
    }

    Write-Host "Root solution update complete." -ForegroundColor Green
}

function Normalize-Solution($solution) {
    if ($null -eq $solution) {
        return ""
    }
 
    # Normalize all newline styles to LF
    $solution = $solution -replace "`r`n?|`n", "`n"
 
    $lines = $solution.Split("`n") | ForEach-Object { $_.TrimEnd() }
    return ($lines -join "`n").TrimEnd("`n")
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

        if ($ServerNames -and -not $serverDirectories) {
            Write-Host "❌ No matching server directories found for: $($ServerNames -join ', ')" -ForegroundColor Red
            $script:stale = $true
            $script:staleFiles += "No matching servers: $($ServerNames -join ', ')"
        }

        foreach ($serverDir in $serverDirectories) {
            Update-Solution -ServerDirectory $serverDir
        }
    }

    if (-not ($All -or $ServerNames -or $Root)) {
        Write-Host "No update targets specified. Use -All, -ServerNames, or -Root." -ForegroundColor Yellow
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
