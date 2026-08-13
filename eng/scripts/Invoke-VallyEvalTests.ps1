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
    `<repo-root>/.work/vally/evals`.

.PARAMETER BuildInfoPath
    Path to the build_info.json file produced by New-BuildInfo.ps1. Defaults to
    `<repo-root>/.work/build_info.json`.

.PARAMETER OutputPath
    Optional path for Vally output. Defaults to `<repo-root>/.work/vally/vally-results`.

.PARAMETER NumberOfRuns
    The number of times to run each eval spec. Defaults to 1.

.PARAMETER IsDebug
    When specified, adds `--verbose` to the `vally eval` invocation for
    additional diagnostic output.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1

    Runs Vally using default paths derived from the repository root.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -BuildInfoPath '.work/custom_build_info.json' -IsDebug

    Runs Vally with a custom build info file and verbose output enabled.
#>

param(
    [string]$WorkDirectory,
    [string]$BuildInfoPath,
    [string]$EvalsDirectory,
    [string]$OutputPath,
    [int]$NumberOfRuns = 1,
    [switch]$IsDebug
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$WorkDirectory) {
    $WorkDirectory = $RepoRoot
}

$workArtifactsDirectory = Join-Path $WorkDirectory ".work"
$vallyArtifactsDirectory = Join-Path $workArtifactsDirectory "vally"

if (!$EvalsDirectory) {
    $EvalsDirectory = Join-Path $vallyArtifactsDirectory "evals"
}

if (!(Test-Path $EvalsDirectory)) {
    Write-Warning "Evals directory not found at $EvalsDirectory. Please run VallyEvaluator to generate eval.yaml files first."
    exit 1
}

if (!$OutputPath) {
    $OutputPath = Join-Path $vallyArtifactsDirectory "vally-results"
}

if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# build_info.json is initialized with all buildable platforms
if (!$BuildInfoPath) {
    $BuildInfoPath = Join-Path $workArtifactsDirectory "build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    Write-Error "Build info file not found at $BuildInfoPath. Please run New-BuildInfo.ps1 first."
    exit 1
}

$environment = "";
if ($IsWindows) {
    $environment = "windows"
} elseif ($IsLinux) {
    $environment = "linux"
} else {
    Write-Error "Unsupported platform. This script only supports Windows, Linux, and macOS."
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

$expression = "vally eval --work-dir '$WorkDirectory' --output-dir '$OutputPath' --runs $NumberOfRuns --param ENVIRONMENT=$environment"

if ($IsDebug) {
    $expression += " --verbose"
}

$expression += " $commandArg"

Write-Host "Running command: $expression"
Invoke-Expression $expression