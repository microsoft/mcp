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

.PARAMETER EvalPathFilter
    One or more wildcard patterns applied to each discovered eval.yaml path. When
    specified, only matching eval specifications are run.

.PARAMETER ToolDiscoveryMode
    Selects the Azure MCP Server discovery mode. TwoStep preserves the existing
    behavior; ThreeStep starts the server with --three-step-tool-discovery.

.PARAMETER IsDebug
    When specified, adds `--verbose` to the `vally eval` invocation for
    additional diagnostic output.

.PARAMETER SkipAgentsInstructions
    When specified, skips overwriting `AGENTS.md` in the work directory with the
    Vally evaluation instructions from `eng/tools/VallyEvaluator/src/Resources/eval.instructions.md`.
    By default this script temporarily replaces `AGENTS.md` (restoring it afterward)
    so local runs match the behavior of the `vally-eval.yml` CI workflow, which
    performs the same substitution. Without these instructions, the evaluated agent
    does not know to treat placeholders as synthetic test data or skip
    `subscription_list`/`az login`, which commonly causes spurious local failures
    that do not occur in CI.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1

    Runs Vally using default paths derived from the repository root.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -BuildInfoPath '.work/custom_build_info.json' -IsDebug

    Runs Vally with a custom build info file and verbose output enabled.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -ToolDiscoveryMode ThreeStep

    Runs Vally against the three-step Azure MCP Server discovery environment.

.EXAMPLE
    ./eng/scripts/Invoke-VallyEvalTests.ps1 -EvalPathFilter '*AppConfig*', '*Storage*'

    Runs only eval specifications whose paths match either wildcard pattern.
#>

param(
    [string]$WorkDirectory,
    [string]$BuildInfoPath,
    [string]$EvalsDirectory,
    [string]$OutputPath,
    [int]$NumberOfRuns = 1,
    [string[]]$EvalPathFilter,
    [ValidateSet("TwoStep", "ThreeStep")]
    [string]$ToolDiscoveryMode = "TwoStep",
    [switch]$IsDebug,
    [switch]$SkipAgentsInstructions
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
    $outputDirectoryName = if ($ToolDiscoveryMode -eq "ThreeStep") {
        "vally-results-three-step"
    } else {
        "vally-results"
    }
    $OutputPath = Join-Path $vallyArtifactsDirectory $outputDirectoryName
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

if ($ToolDiscoveryMode -eq "ThreeStep") {
    $environment += "-three-step"
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$evalPaths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$commandArg = ""

foreach ($path in $buildInfo.pathsToTest) {
    if ([string]::IsNullOrEmpty($path.testResourcesPath)) {
        continue
    }

    $evalPath = Join-Path $RepoRoot $path.testResourcesPath "eval.yaml"
    if (Test-Path $evalPath) {
        [void]$evalPaths.Add((Resolve-Path -LiteralPath $evalPath).Path)
    }
}

Write-Host "Getting eval paths from VallyEvaluator"
Get-ChildItem -Path $EvalsDirectory -Filter "eval.yaml" -Recurse | ForEach-Object {
    [void]$evalPaths.Add($_.FullName)
}

$results = @($evalPaths | Sort-Object)
if ($EvalPathFilter.Count -gt 0) {
    $results = @(
        $results | Where-Object {
            $evalPath = $_
            $EvalPathFilter | Where-Object { $evalPath -like $_ }
        })
}

if ($results.Count -eq 0) {
    $filterDescription = if ($EvalPathFilter.Count -gt 0) {
        " matching filters: $($EvalPathFilter -join ', ')"
    } else {
        ""
    }
    Write-Error "No eval.yaml files were found$filterDescription."
    exit 1
}

Write-Host "Running $($results.Count) eval specification(s)."
$results | ForEach-Object { $commandArg += "--eval-spec '$($_)' " }

if ([string]::IsNullOrEmpty($commandArg)) {
    Write-Host "No eval.yaml files found to execute vally from."
    exit 0
}

$expression = "vally eval --work-dir '$WorkDirectory' --output-dir '$OutputPath' --runs $NumberOfRuns --param ENVIRONMENT=$environment"

if ($IsDebug) {
    $expression += " --verbose"
}

$expression += " $commandArg"

# The evaluated agent needs the Vally-specific behavioral instructions (synthetic
# subscription id, treat placeholders as supplied test values, never call
# subscription_list/az login, etc.) or it will legitimately ask for clarification
# on placeholder prompts and fail tool-call graders. The vally-eval.yml CI
# workflow achieves this by force-copying eval.instructions.md over AGENTS.md
# before running; replicate that here so local runs match CI behavior, and
# restore the original AGENTS.md (or remove it if none existed) afterward.
$agentsPath = Join-Path $WorkDirectory "AGENTS.md"
$instructionsPath = Join-Path $RepoRoot "eng/tools/VallyEvaluator/src/Resources/eval.instructions.md"
$agentsBackupPath = $null

if (!$SkipAgentsInstructions) {
    if (!(Test-Path $instructionsPath)) {
        Write-Error "Vally eval instructions file not found at $instructionsPath."
        exit 1
    }

    if (Test-Path $agentsPath) {
        $agentsBackupPath = Join-Path ([System.IO.Path]::GetTempPath()) "AGENTS.md.vally-backup-$([Guid]::NewGuid())"
        Copy-Item -LiteralPath $agentsPath -Destination $agentsBackupPath -Force
    }

    Write-Host "Temporarily replacing $agentsPath with Vally evaluation instructions."
    Copy-Item -LiteralPath $instructionsPath -Destination $agentsPath -Force
}

try {
    Write-Host "Running command: $expression"
    Invoke-Expression $expression
}
finally {
    if (!$SkipAgentsInstructions) {
        if ($agentsBackupPath) {
            Move-Item -LiteralPath $agentsBackupPath -Destination $agentsPath -Force
        }
        elseif (Test-Path $agentsPath) {
            Remove-Item -LiteralPath $agentsPath -Force
        }
    }
}
