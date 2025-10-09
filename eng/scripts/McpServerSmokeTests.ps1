#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ServerName,
    [string] $ArtifactsDirectory,
    [string] $TargetOs,
    [string] $TargetArch,
    [string] $WorkingDirectory
)

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
        Write-Host "Verifying package: $($wrapperDir.Parent.Name)"
        if (Test-Path $platformDir) {
            Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
            Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"
            Write-Host "Validating NuGet package for server $ServerName"

            $output = dnx $serverProjectProperties.PackageId -y --source $platformDir --prerelease -- azmcp tools list
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Server tools list command completed successfully for server $($wrapperDir.Parent.Name)."
            } else {
                Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                Write-Host $output
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

    switch ($TargetOs) {
        "linux" { $artifactOs = "linux" }
        "macOs"   { $artifactOs = "darwin" }
        "windows"   { $artifactOs = "win32" }
        default { throw "Unknown TargetOs: $TargetOs" }
    }

    Push-Location $WorkingDirectory
    $hasFailures = $false
    try {
        $wrapperDirs = Get-ChildItem -Path $ArtifactsPath -Directory -Recurse | Where-Object { $_.Name -eq "wrapper" }
        foreach ($wrapperDir in $wrapperDirs) {
            $platformDir = Join-Path $wrapperDir.Parent.FullName "platform"
            Write-Host "Verifying package: $($wrapperDir.Parent.Name)"
            if (Test-Path $platformDir) {
                Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
                Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"

                $mainPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp-*.tgz" | Where-Object { $_.Name -notmatch '-(linux|darwin|win32)-' } | Select-Object -First 1
                $platformPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp*-$artifactOs-$TargetArch-*.tgz" | Select-Object -First 1

                if ($mainPackage -and $platformPackage -and (Test-Path $mainPackage.FullName) -and (Test-Path $platformPackage.FullName)) {
                    Write-Host "Installing Platform Package: $($platformPackage.FullName)"
                    npm install $platformPackage.FullName | Out-Null
                    
                    Write-Host "Installing Wrapper Package: $($mainPackage.FullName)"
                    npm install $mainPackage.FullName | Out-Null

                    $output = npx azmcp tools list
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "Server tools list command completed successfully for $($wrapperDir.Parent.Name)"
                    } else {
                        Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                        Write-Host $output
                        $hasFailures = $true
                    }
                }
                else {
                    Write-Host "Either main package or platform package is missing. Skipping tools list command."
                }
            }
        }
    } finally {
        Pop-Location
    }
    return $hasFailures
}

$nugetHasFailures = Validate-Nuget-Packages -ServerName $ServerName -ArtifactsPath "$ArtifactsDirectory/packages_nuget_signed"
$npmHasFailures = Validate-Npm-Packages -ArtifactsPath "$ArtifactsDirectory/packages_npm" -TargetOs $TargetOs -TargetArch $TargetArch -WorkingDirectory $WorkingDirectory

if ($nugetHasFailures -or $npmHasFailures) {
    exit 1
}