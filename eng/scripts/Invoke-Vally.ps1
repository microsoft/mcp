#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Runs Vally evaluations against Azure MCP tool command specs.

.DESCRIPTION
    Collects eval.yaml specification files from test resource paths listed in the
    `build_info.json` file, as well as from an optional evals directory, then
    invokes the `vally eval` command against all discovered specifications.

    Run New-BuildInfo.ps1 first to generate the required build info file before
    calling this script.

.PARAMETER WorkDirectory
    The working directory passed to `vally`. Defaults to the repository root.

.PARAMETER EvalsDirectory
    Directory containing additional eval.yaml files to include. Defaults to
    `<repo-root>/.work/evals`.

.PARAMETER BuildInfoPath
    Path to the build_info.json file produced by New-BuildInfo.ps1. Defaults to
    `<repo-root>/.work/build_info.json`.

.PARAMETER OutputPath
    Optional path for Vally output. Currently passed through for future use.

.PARAMETER IsDebug
    When specified, adds `--verbose` to the `vally eval` invocation for
    additional diagnostic output.

.EXAMPLE
    ./eng/scripts/Invoke-Vally.ps1

    Runs Vally using default paths derived from the repository root.

.EXAMPLE
    ./eng/scripts/Invoke-Vally.ps1 -BuildInfoPath '.work/custom_build_info.json' -IsDebug

    Runs Vally with a custom build info file and verbose output enabled.
#>

param(
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