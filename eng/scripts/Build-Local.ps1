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
    [switch] $DebugBuild,
    [switch] $IncludeNative
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$buildOutputPath = "$RepoRoot/.work/build"
$packageOutputPath = "$RepoRoot/.work/package_npm"

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
    -DebugBuild:$DebugBuild `
    -AllPlatforms:$AllPlatforms

if ($IncludeNative) {
    & "$RepoRoot/eng/scripts/Build-Code.ps1" `
        -DebugBuild:$DebugBuild `
        -AllPlatforms:$AllPlatforms `
        -BuildNative
}

& "$RepoRoot/eng/scripts/Pack-Npm.ps1" -UsePaths:(!$NoUsePaths)

if ($VerifyNpx) {
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
