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
        Write-Host "Verifying package: $($wrapperDir.Parent.Name)"
        if (Test-Path $platformDir) {
            Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
            Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"
            Write-Host "Validating NuGet package for server $ServerName"

            $dnxOutput = dnx $serverProjectProperties.PackageId -y --source $platformDir --prerelease -- azmcp tools list
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Server tools list command completed successfully for server $($wrapperDir.Parent.Name)."
            } else {
                Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                Write-Host $dnxOutput
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
    try {
        $wrapperDirs = Get-ChildItem -Path $ArtifactsPath -Directory -Recurse | Where-Object { $_.Name -eq "wrapper" }
        foreach ($wrapperDir in $wrapperDirs) {
            $platformDir = Join-Path $wrapperDir.Parent.FullName "platform"
             Write-Host "Verifying package: $($wrapperDir.Parent.Name)"
            if (Test-Path $platformDir) {
                Copy-Item -Path (Join-Path $wrapperDir.FullName '*') -Destination $platformDir -Recurse -Force
                Write-Host "Copied from $($wrapperDir.FullName) to $platformDir"

                $mainPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp-*.tgz" | Where-Object { $_.Name -notmatch '-(linux|darwin|win32)-' }
                $platformPackage = Get-ChildItem -Path $platformDir -Filter "azure-mcp-$TargetOs-$TargetArch-*.tgz"
                if ($platformPackage) { npm install $platformPackage.FullName }
                if ($mainPackage) { npm install $mainPackage.FullName }
                $npmOutput = npx azmcp tools list
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Server tools list command completed successfully for $($wrapperDir.Parent.Name)"
                } else {
                    Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
                    Write-Host $npmOutput
                    $hasFailures = $true
                }
            }
        }
    } finally {
        Pop-Location
    }
    return $hasFailures
}

$nugetHasFailures = Validate-Nuget-Packages -ServerName "${{ parameters.ServerName }}" -ArtifactsPath "$(Pipeline.Workspace)/packages_nuget_signed"
$npmHasFailures = Validate-Npm-Packages -ArtifactsPath "$(Pipeline.Workspace)/packages_npm" -TargetOs "${{ parameters.OSName }}" -TargetArch "$(Architecture)" -WorkingDirectory "$(Agent.TempDirectory)"

Write-Host "NuGet package validation has Failures: $nugetHasFailures"
Write-Host "NPM package validation has Failures : $npmHasFailures"

if ($nugetHasFailures -or $npmHasFailures) {
    exit 1
}