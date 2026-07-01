#!/bin/env pwsh
#Requires -Version 7

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# build_info.json is initialized with all buildable platforms, native and not
# Trim the platform lists to only built platforms
$buildInfoPath = "$RepoRoot/.work/build_info.json"

if (!(Test-Path $buildInfoPath)) {
    Write-Error "Build info file not found at $buildInfoPath. Please run New-BuildInfo.ps1 first."
    exit 1
}

$buildInfo = Get-Content $buildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$results = [System.Collections.ArrayList]::new()

foreach ($path in $buildInfo.pathsToTest) {
    if ([string]::IsNullOrEmpty($path.testResourcesPath)) {
        continue
    }

    $evalPath = Join-Path $RepoRoot $path.testResourcesPath "eval.yaml"
    if (Test-Path $evalPath) {
        $results.Add($evalPath) | Out-Null
    }
}

$results | ForEach-Object { Write-Output $_ }
