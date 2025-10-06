#!/bin/env pwsh
#Requires -Version 7

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$projectPropertiesScript = "$RepoRoot/eng/scripts/Get-ProjectProperties.ps1"

function Validate-Nuget-Packages {
    param (
        [string] $ServerName,
        [string] $ArtifactsPath
    )

    $hasFailures = $false
    $wrapperDirs = Get-ChildItem -Path $ArtifactsPath -Directory -Recurse | Where-Object { $_.Name -eq "wrapper" }
    $serverProjectProperties = & "$projectPropertiesScript" -ProjectName "$ServerName.csproj"
    foreach ($wrapperDir in $wrapperDirs) {
        $platformDir = Join-Path $wrapperDir.Parent.FullName "platform"
        if (Test-Path $platformDir) {
            Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
            Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"

            dnx $serverProjectProperties.PackageId -y --source $platformDir --prerelease -- azmcp tools list
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Server tools list command completed successfully."
            } else {
                Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                $hasFailures = $true
            }
        }
    }
    return $hasFailures
}

function Validate-Npm-Packages {
    param (
        [string] $ArtifactsPath,
        [string] $TargetOs,
        [string] $TargetArch,
        [string] $WorkingDirectory
    )

    Push-Location $WorkingDirectory

    $hasFailures = $false
    $wrapperDirs = Get-ChildItem -Path $ArtifactsPath -Directory -Recurse | Where-Object { $_.Name -eq "wrapper" }
    foreach ($wrapperDir in $wrapperDirs) {
        $platformDir = Join-Path $wrapperDir.Parent.FullName "platform"
        if (Test-Path $platformDir) {
            Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
            Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"

            $mainPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp-*.tgz" | Where-Object { $_.Name -notmatch '-(linux|darwin|win32)-' }
            $platformPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp-$TargetOs-$TargetArch-*.tgz"
            if ($mainPackage) { 
                npm install $mainPackage.FullName
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "npm install of $($mainPackage.FullName) failed with exit code $LASTEXITCODE"
                    $hasFailures = $true
                    continue
                }
            }
            if ($platformPackage) { 
                npm install $platformPackage.FullName
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "npm install of $($platformPackage.FullName) failed with exit code $LASTEXITCODE"
                    $hasFailures = $true
                    continue
                }
            }
            npx azmcp tools list
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Server tools list command completed successfully."
            } else {
                Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                $hasFailures = $true
            }
        }
    }
    return $hasFailures
}