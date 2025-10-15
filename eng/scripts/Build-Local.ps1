#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $ServerName,
    [switch] $NoTrimmed,
    [switch] $NoSelfContained,
    [switch] $NoUsePaths,
    [switch] $AllPlatforms,
    [switch] $VerifyNpx,
    [switch] $ReleaseBuild,
    [switch] $IncludeNative
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$buildOutputPath = "$RepoRoot/.work/build"
$packageOutputPath = "$RepoRoot/.work/packages_npm"

Remove-Item -Path $buildOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
Remove-Item -Path $packageOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

& "$RepoRoot/eng/scripts/New-BuildInfo.ps1" `
    -ServerName $ServerName `
    -PublishTarget none `
    -BuildId 12345 `
    -IncludeNative:$IncludeNative

& "$RepoRoot/eng/scripts/Build-Code.ps1" `
    -SelfContained:(!$NoSelfContained) `
    -Trimmed:(!$NoTrimmed) `
    -ReleaseBuild:$ReleaseBuild `
    -AllPlatforms:$AllPlatforms

if ($LastExitCode -ne 0) {
    exit $LastExitCode
}

if ($IncludeNative) {
    & "$RepoRoot/eng/scripts/Build-Code.ps1" `
        -ReleaseBuild:$ReleaseBuild `
        -AllPlatforms:$AllPlatforms `
        -Native
}

# build_info.json is initialized with all buildable platforms, native and not
# Trim the platform lists to only built platforms
$buildInfoPath = "$RepoRoot/.work/build_info.json"
$buildInfo = Get-Content $buildInfoPath -Raw | ConvertFrom-Json -AsHashtable

foreach($server in $buildInfo.servers) {
    $built = @()
    foreach($platform in $server.platforms) {
        if (Test-Path "$buildOutputPath/$($platform.artifactPath)") {
            $built += $platform
        }
    }
    $server.platforms = $built
}

$buildInfo | ConvertTo-Json -Depth 10 | Set-Content -Path $buildInfoPath

& "$RepoRoot/eng/scripts/Pack-Npm.ps1" -UsePaths:(!$NoUsePaths)

if ($VerifyNpx -and !$NoUsePaths) {
    $tgzFiles = Get-ChildItem -Path $packageOutputPath -Filter '*.tgz'
    | Where-Object { $_.Directory.Name -eq 'wrapper' }

    foreach($tgzFile in $tgzFiles) {
        Push-Location -Path $RepoRoot
        try {
            Invoke-LoggedCommand "npx -y clear-npx-cache"
            Invoke-LoggedCommand "npx -y `"file://$tgzFile`" tools list"
        }
        finally {
            Pop-Location
        }
    }
}
