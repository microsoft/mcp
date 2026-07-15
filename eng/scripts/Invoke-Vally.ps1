#!/bin/env pwsh
#Requires -Version 7

param(
    # Common Parameters
    [string]$WorkDirectory,
    [string]$EvalsDirectory,
    [string]$BuildInfoPath,
    [string]$OutputPath,
    [switch]$IsDebug
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$WorkDirectory) {
    $WorkDirectory = $RepoRoot
}

if (!$EvalsDirectory) {
    $EvalsDirectory = "$RepoRoot/.work/evals"
}

# build_info.json is initialized with all buildable platforms
if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    Write-Error "Build info file not found at $BuildInfoPath. Please run New-BuildInfo.ps1 first."
    exit 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$results = [System.Collections.ArrayList]::new()
$commandArg = ""

foreach ($path in $buildInfo.pathsToTest) {
    if ([string]::IsNullOrEmpty($path.testResourcesPath)) {
        continue
    }

    $evalPath = Join-Path $RepoRoot $path.testResourcesPath "eval.yaml"
    if (Test-Path $evalPath) {
        $results.Add($evalPath) | Out-Null
    }
}

$results | ForEach-Object { $commandArg += "--eval-spec '$($_)' " }

Write-Host "Getting eval paths from VallyEvaluator"
$(Get-ChildItem "$EvalsDirectory/**/eval.yaml") | ForEach-Object { $commandArg += "--eval-spec '$($_.FullName)' " }

$expression = "vally eval --work-dir '$WorkDirectory' $commandArg"

if ($IsDebug) {
    $expression += " --verbose"
}

Write-Host "Running command: $expression"
Invoke-Expression $expression