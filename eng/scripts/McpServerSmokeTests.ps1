#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $BuildInfoPath,
    [string] $ArtifactsDirectory,
    [string] $ServerName,
    [string] $TargetOs,
    [string] $TargetArch,
    [switch] $CI
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$exitCode = 0
$isPipelineRun = $CI -or $env:TF_BUILD -eq "true"
$ignoreMissingArtifacts = -not $isPipelineRun

$tempDirectory = "$RepoRoot/.work/temp"

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$ArtifactsDirectory) {
    $ArtifactsDirectory = "$RepoRoot/.work"
}

if (!$TargetOs) {
    $osPlatform = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
    $TargetOs = switch -Regex ($osPlatform) {
        "Windows" { "windows" }
        "Linux"   { "linux" }
        "Darwin"  { "macOs" }
        default { LogError "Unknown OS Platform: $osPlatform"; $exitCode = 1}
    }
}

if (!$TargetArch) {
    $archPlatform = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    $TargetArch = switch ($archPlatform) {
        "X64" { "x64" }
        "Arm64" { "arm64" }
        default { LogError "Unknown Architecture Platform: $archPlatform"; $exitCode = 1}
    }
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if (!(Test-Path $ArtifactsDirectory)) {
    LogError "Artifacts directory $ArtifactsDirectory does not exist."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    Write-Host "Exiting with code $exitCode"
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

function Test-NugetPackages {
    param (
        [hashtable] $Server
    )

    Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Path $tempDirectory | Out-Null

    $artifactsPath = "$ArtifactsDirectory/packages_nuget_signed/$($Server.artifactPath)"
    if( -not (Test-Path $artifactsPath) ) {
        $message = "Artifacts path $artifactsPath does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            return $true
        } else {
            LogError $message
            return $false
        }
    }

    Copy-Item -Path (Join-Path $artifactsPath '*') -Destination $tempDirectory -Recurse -Force

    Write-Host "Copied from $artifactsPath to $tempDirectory"
    Write-Host "Validating NuGet package for server $($Server.name)"

    $output = dnx $server.dnxPackageId -y --source $tempDirectory --prerelease -- $server.dnxToolCommandName tools list
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Server tools list command completed successfully for server $($Server.name)."
    } else {
        Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
        Write-Host $output
        return $false
    }

    return $true
}

function Test-NpmPackages {
    param (
        [hashtable] $Server
    )

    switch ($TargetOs) {
        "linux" { $artifactOs = "linux" }
        "macOs"   { $artifactOs = "darwin" }
        "windows"   { $artifactOs = "win32" }
        default { throw "Unknown TargetOs: $TargetOs" }
    }

    Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Path $tempDirectory | Out-Null

    $artifactsPath = "$ArtifactsDirectory/packages_npm/$($Server.artifactPath)"
    if( -not (Test-Path $artifactsPath) ) {
        $message = "Artifacts path $artifactsPath does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            return $true
        } else {
            LogError $message
            return $false
        }
    }

    $mainPackage = Get-ChildItem -Path "$artifactsPath/wrapper" -Filter "*.tgz" | Where-Object { $_.Name -notmatch '-(linux|darwin|win32)-' } | Select-Object -First 1
    $platformPackage = Get-ChildItem -Path "$artifactsPath/platform" -Filter "*-$artifactOs-$TargetArch-*.tgz" | Select-Object -First 1

    $originalLocation = Get-Location
    Set-Location $tempDirectory
    try {
        Write-Host "Installing Platform Package: $($platformPackage.FullName)"
        npm install $platformPackage.FullName | Out-Null

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to install platform package $($platformPackage.FullName) with exit code $LASTEXITCODE"
            return $false
        }

        Write-Host "Installing Wrapper Package: $($mainPackage.FullName)"
        npm install $mainPackage.FullName | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to install platform package $($platformPackage.FullName) with exit code $LASTEXITCODE"
            return $false
        }

        $output = npx --no $server.cliName tools list
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Server tools list command completed successfully for $($server.name)"
        } else {
            Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
            Write-Host $output
            return $false
        }
    } finally {
        Set-Location $originalLocation
    }

    return $true
}

$servers = $buildInfo.servers
if ($ServerName) {
    $servers = $servers | Where-Object { $_.name -eq $ServerName }
}

foreach($server in $servers) {
    Write-Host "Validating packages for server $($server.name) version $($server.version) for OS $TargetOs and Arch $TargetArch"

    $nugetValid = Test-NugetPackages -Server $server
    $npmValid = Test-NpmPackages -Server $server

    if (!$nugetValid -or !$npmValid) {
        $exitCode = 1
    }
}

exit $exitCode
