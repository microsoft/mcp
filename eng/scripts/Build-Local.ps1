#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $ServerName,
    [switch] $Trimmed,
    [switch] $NoSelfContained,
    [switch] $NoUsePaths,
    [switch] $AllPlatforms,
    [switch] $VerifyNpx,
    [switch] $DebugBuild,
    [switch] $BuildNative
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$buildOutputPath = "$RepoRoot/.work/build"
$packageOutputPath = "$RepoRoot/.work/package"

$prereleaseLabel = 'alpha'
$prereleaseNumber = [int]::Parse((Get-Date -UFormat %s))
$versionSuffix = "-$prereleaseLabel.$prereleaseNumber"

function Build($os, $arch) {
    & "$RepoRoot/eng/scripts/Build-Code.ps1" `
        -ServerName $ServerName `
        -VersionSuffix $versionSuffix `
        -OperatingSystem $os `
        -Architecture $arch `
        -SelfContained:(!$NoSelfContained) `
        -Trimmed:$Trimmed `
        -OutputPath $buildOutputPath `
        -DebugBuild:$DebugBuild `
        -BuildNative:$BuildNative
}

Remove-Item -Path $buildOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
Remove-Item -Path $packageOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

if($AllPlatforms) {
    Build -os linux -arch x64
    Build -os windows -arch x64
    Build -os windows -arch arm64
    Build -os macos -arch x64
    Build -os macos -arch arm64
}
else {
    $runtime = $([System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier)
    $parts = $runtime.Split('-')
    $os = $parts[0]
    $arch = $parts[1]

    if($os -eq 'win') {
        $os = 'windows'
    } elseif($os -eq 'osx') {
        $os = 'macos'
    }

    Build -os $os -arch $arch
}

& "$RepoRoot/eng/scripts/Pack-Npm.ps1" `
    -ArtifactsPath $buildOutputPath `
    -UsePaths:(!$NoUsePaths) `
    -OutputPath $packageOutputPath

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
