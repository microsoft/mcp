#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Discovers and runs the vally experiments for the Azure MCP tools.

.DESCRIPTION
This is a wrapper around the vally CLI (https://microsoft.github.io/vally) that:

  1. Builds the Azure.Mcp.Server project (unless -SkipBuild is passed) so the
     freshly compiled `azmcp` executable is available.
  2. Prepends the build output directory to PATH so the `command: azmcp` entry in
     each eval spec resolves to your local build (vally does not interpolate
     environment variables inside eval specs).
  3. Discovers experiments laid out by area and tool (see below). For each area it
     optionally runs a pre-evaluation provisioning script to create the Azure
     resources the experiments expect, and always runs a post-evaluation teardown
     script afterwards (even when a run or provisioning fails), so resources are
     not leaked.
  4. For each discovered tool, runs its experiment (<tool>.experiment.yaml) via
     `vally experiment run`. The experiment executes the shared eval spec as
     several variants for comparison:
       - namespace    - WITH the Azure MCP server in its default `namespace` mode,
       - consolidated - WITH the Azure MCP server in `consolidated` mode, and
       - baseline     - WITHOUT the Azure MCP server.
     `baseline` is the control; `namespace` and `consolidated` are the server
     CANDIDATES. The delta between each candidate and the baseline isolates the
     Azure MCP server's contribution in that mode.
     Each experiment is run -Iterations time(s) (default 1); the results summary
     reports the outcome of every iteration, followed by a consolidated,
     by-tool summary that folds all iterations together and names the
     best-performing candidate variant (and its aggregated metrics) per tool.

Layout / naming convention (discovered automatically):

    tests/Vally/
      <area>/                          # e.g. eventhubs
        <tool>.experiment.yaml         # experiment (required)
        <tool>.eval.yaml               # shared eval spec the experiment runs
        New-*Resources.ps1             # per-area provisioning (optional)
        Remove-*Resources.ps1          # per-area teardown     (optional)

The tool name is the experiment file name without the '.experiment.yaml' suffix
(e.g. 'eventhub-get'). Use -Area / -Tool to filter what runs, or -ExperimentSpec
to run a single experiment.

Vally must be installed and on your PATH. See
https://microsoft.github.io/vally/get-started/install/.

All variants are graded on the same task *outcome*, so their verdicts are
directly comparable; the baseline (no Azure MCP server) is EXPECTED TO FAIL when
the tool is required. Provide a real subscription (via `az login`) so each
candidate can return real data and the comparison is meaningful. The script's exit
code is non-zero if any experiment fails to run or a candidate stimulus fails;
baseline failures are expected and do not affect it.

Every invocation of this script writes ALL of its output under one fresh,
timestamped run directory: '<OutputDir>/<run-timestamp>/<area>/<tool>/...'. That
keeps everything a single invocation produces - every discovered area/tool, and
every -Iterations repeat of each - together and unambiguous, rather than
scattered across independent per-tool timestamps with no shared "this is one
run" grouping.

Pass -ReportOnly to skip building, provisioning, and running vally entirely, and
instead regenerate the summary from the newest run directory already saved under
-OutputDir - the script prints which run directory it read. Pass -ReportFrom
<run-directory> to report from a SPECIFIC run directory instead (e.g. an older
run, or one copied/archived elsewhere); -ReportFrom implies -ReportOnly. Both are
fast, offline, and free - handy for re-examining or debugging a prior run's
results, or for iterating on this script's own post-processing/reporting logic
without re-running the (slow, Azure-touching) evaluations.

.PARAMETER ExperimentSpec
Optional path to a single experiment spec (<tool>.experiment.yaml) to run instead
of discovering them. Cannot be combined with -Area or -Tool.

.PARAMETER Iterations
Number of times to run each discovered experiment (default 1). Every iteration is
a full `vally experiment run` (baseline + every server candidate) written to its
own timestamped directory, and the results summary reports each iteration's outcome
plus an aggregate pass count. Useful for surfacing non-deterministic (flaky)
results. Must be 1 or greater.

.PARAMETER OutputDir
Parent directory under which each invocation of this script writes a fresh,
timestamped run directory (e.g. '2026-07-29T18-19-30-496Z'). Every area/tool this
invocation discovers goes to an '<run-timestamp>/<area>/<tool>' subdirectory,
under which `vally experiment run` creates one further timestamped folder per
-Iterations repeat, each with one subfolder per variant (baseline, namespace,
consolidated). Defaults to ./.vally-results next to this script.

.PARAMETER PreEvalScript
Optional path to a provisioning script run BEFORE the evaluations (e.g.
./eventhubs/New-EventHubsResources.ps1). Use it to create the Azure resources the
eval prompts reference. -ResourceGroup and -Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `New-*Resources.ps1` script next to the eval spec and uses that.

.PARAMETER PostEvalScript
Optional path to a teardown script run AFTER the evaluations (e.g.
./eventhubs/Remove-EventHubsResources.ps1). It runs in a finally block, so it
executes even if the eval or the pre-eval provisioning fails. -ResourceGroup and
-Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `Remove-*Resources.ps1` script next to the eval spec and uses
that.

.PARAMETER Area
Optional filter: only run experiments under the named area subdirectory (e.g.
'eventhubs'). Matches the immediate subfolder name under this script's directory.
Repeatable. When omitted, all areas are discovered.

.PARAMETER Tool
Optional filter: only run experiments whose tool name matches (e.g.
'eventhub-get'). The tool name is the experiment file name without the
'.experiment.yaml' suffix. Supports wildcards (e.g. 'eventhub-*'). Repeatable.
When omitted, all tools are run.

.PARAMETER SkipProvisioning
Do not run (or auto-discover) any pre/post provisioning scripts. Use this when the
Azure resources already exist and should be left untouched.

.PARAMETER ResourceGroup
Optional resource group name forwarded to the pre/post provisioning scripts.

.PARAMETER Subscription
Optional Azure subscription (id or name) forwarded to the pre/post provisioning
scripts.

.PARAMETER SkipBuild
Skip building the server. Use this when azmcp is already built and on PATH.

.PARAMETER ReportOnly
Skip building, provisioning, and running vally; instead reconstruct the results
summary from artifacts already saved under -OutputDir. Without -ReportFrom, reads
the newest run directory under -OutputDir (the script prints which one). The
-Area/-Tool filters and -Iterations still apply: -Iterations selects how many of
the newest timestamped iteration folder(s) per tool, WITHIN that one run
directory, to report (default 1). Because the vally exit code is not persisted in
the artifacts, the summary relies on the per-stimulus verdicts in each run's
results.jsonl; the process still exits non-zero if a candidate stimulus failed or
an effectiveness regression is detected.

.PARAMETER ReportFrom
Optional path to a SPECIFIC run directory to report from (its
'<area>/<tool>/<timestamp>' layout must match what a single invocation of this
script produces under -OutputDir) instead of auto-discovering the newest one.
Implies -ReportOnly. Use it to re-report an older run, or an archived/copied run
directory.

.PARAMETER Verbose
Enables this script's own verbose logging (standard PowerShell common parameter).
Note: `vally experiment run` has no --verbose option (only `vally eval` does), so
full agent output is not available through the experiment runner.

.EXAMPLE
# Discover and run every <tool>.experiment.yaml under every area subfolder,
# provisioning + tearing down each area as needed.
./Invoke-VallyEval.ps1 -Subscription <subscription-id>

.EXAMPLE
# Only the Event Hubs area
./Invoke-VallyEval.ps1 -Area eventhubs -Subscription <subscription-id>

.EXAMPLE
# Only a specific tool (wildcards allowed)
./Invoke-VallyEval.ps1 -Tool eventhub-get -Verbose

.EXAMPLE
# Resources already exist - don't provision or tear down
./Invoke-VallyEval.ps1 -SkipProvisioning

.EXAMPLE
# Run one explicit experiment
./Invoke-VallyEval.ps1 -ExperimentSpec ./eventhubs/eventhub-get.experiment.yaml -SkipProvisioning

.EXAMPLE
# Run every experiment three times and report each iteration's outcome
./Invoke-VallyEval.ps1 -Iterations 3

.EXAMPLE
# Re-print the summary from the last run's saved artifacts - no build, no Azure
./Invoke-VallyEval.ps1 -ReportOnly

.EXAMPLE
# Report all three iterations of a prior run for a single tool
./Invoke-VallyEval.ps1 -ReportOnly -Tool eventhub-get -Iterations 3

.EXAMPLE
# Report from a specific, older run directory instead of the newest one
./Invoke-VallyEval.ps1 -ReportFrom ./.vally-results/2026-07-29T18-19-30-496Z
#>

[CmdletBinding()]
param(
    [string[]] $Area,
    [string[]] $Tool,
    [string] $ExperimentSpec,
    [ValidateRange(1, [int]::MaxValue)]
    [int] $Iterations = 1,
    [string] $OutputDir = (Join-Path $PSScriptRoot '.vally-results'),
    [string] $PreEvalScript,
    [string] $PostEvalScript,
    [string] $ResourceGroup,
    [string] $Subscription,
    [switch] $SkipBuild,
    [switch] $SkipProvisioning,
    [switch] $ReportOnly,
    [string] $ReportFrom
)

$ErrorActionPreference = 'Stop'

# -ReportFrom names a specific run directory to report from and implies
# -ReportOnly; it does NOT replace -OutputDir (which may contain many run
# directories) - see the "Resolve the run directory" section below.
if ($ReportFrom) { $ReportOnly = $true }

# vally emits UTF-8 (box-drawing rules, check marks, etc.). If the console is on a
# legacy OEM code page (e.g. 437), those bytes are decoded wrong and show up as
# mojibake like '?��'/'?��'. Force UTF-8 (BOM-less, so redirected/CI output stays
# clean) so vally's output is captured and displayed correctly.
$OutputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

# Repo root is four levels up: servers/Azure.Mcp.Server/tests/Vally -> repo root
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..' '..')
$ServerProject = Join-Path $RepoRoot 'servers' 'Azure.Mcp.Server' 'src' 'Azure.Mcp.Server.csproj'

function Write-Info($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Yellow }

# --- Resolve the run directory ------------------------------------------------
# Every invocation of this script groups ALL of its output under one top-level,
# timestamped run directory: <OutputDir>/<run-timestamp>/<area>/<tool>/... This
# keeps everything a single invocation produces - every discovered area/tool,
# and every -Iterations repeat of each - together and unambiguous, rather than
# scattered across independent per-tool timestamps with no shared "this is one
# run" grouping.
#
# -ReportOnly (without -ReportFrom) reports from the newest such run directory
# found under -OutputDir. -ReportFrom names a SPECIFIC run directory to report
# from instead (e.g. an older run, or one copied/archived elsewhere).
$RunDirTimestampPattern = '^\d{4}-\d{2}-\d{2}T'
if ($ReportOnly) {
    if ($ReportFrom) {
        $resolvedRunDir = Resolve-Path -Path $ReportFrom -ErrorAction SilentlyContinue
        if (-not $resolvedRunDir) { throw "-ReportFrom run directory not found: $ReportFrom" }
        $RunDir = $resolvedRunDir.Path
    }
    else {
        if (-not (Test-Path -LiteralPath $OutputDir)) {
            throw "No run artifacts found under '$OutputDir'. Run without -ReportOnly first, or pass -ReportFrom <run-directory>."
        }
        $newestRunDir = Get-ChildItem -Path $OutputDir -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match $RunDirTimestampPattern } |
        Sort-Object Name -Descending |
        Select-Object -First 1
        if (-not $newestRunDir) {
            throw "No timestamped run directories found under '$OutputDir'. Run without -ReportOnly first, or pass -ReportFrom <run-directory>."
        }
        $RunDir = $newestRunDir.FullName
    }
    Write-Info "Report-only: reporting from run directory: $RunDir"
}
else {
    # A fresh run directory for this invocation, named with a sortable UTC
    # timestamp matching vally's own style (e.g. 2026-07-29T18-19-30-496Z) so it
    # sorts chronologically alongside vally's own per-iteration timestamp
    # folders nested beneath each tool's directory.
    $RunDir = Join-Path $OutputDir ([DateTime]::UtcNow.ToString('yyyy-MM-ddTHH-mm-ss-fffZ'))
    Write-Info "Run directory for this invocation: $RunDir"
}

# Make $RunDir absolute (rooted against our original working directory) rather
# than relative. Invoke-VallyExperiment temporarily changes directory before
# invoking `vally`, and a relative -OutputDir/-ReportFrom would otherwise
# resolve against the wrong directory once that happens.
$RunDir = [System.IO.Path]::GetFullPath($RunDir, (Get-Location).ProviderPath)

# Report-only mode reconstructs the summary from saved artifacts, so it needs
# neither the vally CLI, a build, nor the azmcp binary on PATH. Skip all of that
# setup and go straight to discovery + reporting.
if (-not $ReportOnly) {
    # 1. Verify the vally CLI is available.
    $vally = Get-Command 'vally' -ErrorAction SilentlyContinue
    if (-not $vally) {
        throw "The 'vally' CLI was not found on PATH. Install it first: https://microsoft.github.io/vally/get-started/install/"
    }
    Write-Info "Using vally: $($vally.Source)"

    # 2. Build the server so a current azmcp binary exists.
    $exeName = $IsWindows ? 'azmcp.exe' : 'azmcp'
    if (-not $SkipBuild) {
        Write-Info "Building $ServerProject ..."
        dotnet build $ServerProject -c Debug | Write-Host
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }
    }

    # 3. Locate the freshly built azmcp executable and prepend it to PATH.
    $serverBinDir = Join-Path (Split-Path $ServerProject -Parent) 'bin'
    $azmcp = Get-ChildItem -Path $serverBinDir -Recurse -Filter $exeName -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

    if (-not $azmcp) {
        throw "Could not find '$exeName' under '$serverBinDir'. Build the server first (omit -SkipBuild)."
    }

    $azmcpDir = Split-Path $azmcp.FullName -Parent
    Write-Info "Using azmcp: $($azmcp.FullName)"
    $env:PATH = "$azmcpDir$([IO.Path]::PathSeparator)$env:PATH"
}

# 4. Run the experiment(s).
#
# `vally experiment run` executes the shared eval spec once per variant
# (baseline + every server candidate) and writes each variant's results under a
# single timestamped run directory it creates beneath --output-dir:
#
#   <RunOutputDir>/<timestamp>/
#     report.md                      # combined markdown report
#     <variant>/results.jsonl        # trial-result records (one per stimulus)
#     <variant>/run-summary.jsonl    # resolved config per variant
#
# Returns a result object with the vally exit code and the timestamped run
# directory (so the caller can read each variant's results.jsonl).
function Invoke-VallyExperiment {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $ExperimentFile,
        [Parameter(Mandatory)] [string] $RunOutputDir
    )

    $vallyArgs = @(
        'experiment', 'run', $ExperimentFile
        '--output-dir', $RunOutputDir
    )
    # NOTE: `vally experiment run` accepts only --variant/--output-dir/--workers/
    # --dry-run - it has NO --verbose option (that flag exists only on `vally eval`),
    # so we must not forward one here or vally exits with 'unknown option --verbose'.

    # vally creates a fresh timestamped subdirectory per run. Snapshot the
    # existing ones so we can identify the new one afterwards (more robust than
    # parsing stdout, and independent of clock formatting).
    $before = @{}
    if (Test-Path -LiteralPath $RunOutputDir) {
        Get-ChildItem -Path $RunOutputDir -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { $before[$_.FullName] = $true }
    }

    Write-Info "[$Label] Running: vally $($vallyArgs -join ' ')"
    # `vally experiment run` has no --workdir/--cwd option, so the process (and
    # every per-trial agent sandbox it spawns via the copilot-sdk executor)
    # simply inherits our current directory. That runtime spills large tool
    # outputs to a 'session-state/temp/' folder resolved relative to that
    # inherited cwd - NOT scoped per-experiment or under -OutputDir. Without
    # pinning it, that scratch folder lands wherever the caller happened to
    # invoke this script from (e.g. the repo root), accumulating stale files
    # across unrelated runs over time. Pin it to this script's own directory,
    # right alongside .vally-results, so it is at least deterministic and
    # confined to the Vally test tree rather than leaking into the repo root.
    Push-Location $PSScriptRoot
    try {
        # Pipe vally's output to the host so it is displayed but NOT emitted on
        # this function's output stream. Without Out-Host, vally's stdout is
        # bundled with the exit code into the caller's variable, turning it
        # into an array; the later ($exit -eq 0) checks then misread a passing
        # run (exit 0) as FAIL.
        & vally @vallyArgs | Out-Host
        $exit = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    # Identify the timestamped run directory vally just created.
    $runDir = $null
    if (Test-Path -LiteralPath $RunOutputDir) {
        $runDir = Get-ChildItem -Path $RunOutputDir -Directory -ErrorAction SilentlyContinue |
        Where-Object { -not $before.ContainsKey($_.FullName) } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
        if (-not $runDir) {
            # Fallback: newest directory (e.g. a rerun that reused an mtime).
            $runDir = Get-ChildItem -Path $RunOutputDir -Directory -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
        }
    }

    return [pscustomobject]@{
        Exit   = $exit
        RunDir = $runDir ? $runDir.FullName : $null
    }
}

# Milliseconds -> "27.4s" for compact display.
function Format-Ms {
    param([int] $Ms)
    return '{0:0.0}s' -f ($Ms / 1000.0)
}

# Formats a candidate-vs-baseline delta (lower is better) as e.g. "+12343 (+28%)"
# or "-2.8s (-9%)". A negative value means the candidate used less - i.e. better.
# -Decimals formats the raw (non-seconds) delta with that many decimal places -
# used for AI Credits, which are typically fractional.
function Format-Delta {
    param(
        [Parameter(Mandatory)] [double] $Candidate,
        [Parameter(Mandatory)] [double] $Baseline,
        [switch] $AsSeconds,
        [int] $Decimals = 0
    )
    $delta = $Candidate - $Baseline
    $sign = ($delta -gt 0) ? '+' : ''
    $body = $AsSeconds ? ('{0}{1:0.0}s' -f $sign, ($delta / 1000.0)) : ('{0}{1}' -f $sign, [math]::Round($delta, $Decimals))
    if ($Baseline -ne 0) {
        $pct = [math]::Round(($delta / [double] $Baseline) * 100)
        $body += ' ({0}{1}%)' -f (($pct -gt 0) ? '+' : ''), $pct
    }
    return $body
}

# Coerces a trajectory metric (tokens, turns, wall time) into a single integer.
# vally normally emits a scalar per metric, but a MULTI-TRIAL trajectory - e.g.
# when the eval spec sets `defaults.runs > 1` to average over several trials -
# yields an ARRAY of trial trajectories, so `trajectory.metrics.<field>` comes
# back as an array of values instead of one number. Casting that array straight
# to [int] throws ("Cannot convert System.Object[] ... to System.Int32"), so sum
# the numeric values (the natural aggregate for these lower-is-better metrics)
# and return one int. Non-numeric or missing values contribute nothing.
function ConvertTo-MetricInt {
    param($Value)

    $sum = 0.0
    foreach ($item in @($Value)) {
        if ($null -eq $item) { continue }
        $parsed = 0.0
        if ([double]::TryParse(
                [string] $item,
                [System.Globalization.NumberStyles]::Any,
                [System.Globalization.CultureInfo]::InvariantCulture,
                [ref] $parsed)) {
            $sum += $parsed
        }
    }
    return [int] $sum
}

# nano-aiu -> AI Credits. vally reports token usage cost in "nano-aiu" units
# (metrics.tokenUsage.cost.{provider,unit,amount}); 1 AI Credit is 1e9 nano-aiu.
$script:NanoAiuPerCredit = 1000000000.0

# Coerces vally's token-usage cost (metrics.tokenUsage.cost.amount, in
# nano-aiu) into AI Credits, summing across trials the same way
# ConvertTo-MetricInt does for the other metrics. Cost is omitted by vally
# whenever the source didn't report a complete, consistently-provider/unit
# cost, so missing/non-numeric values simply contribute nothing.
function ConvertTo-AiCredits {
    param($Value)

    $sum = 0.0
    foreach ($item in @($Value)) {
        if ($null -eq $item) { continue }
        $parsed = 0.0
        if ([double]::TryParse(
                [string] $item,
                [System.Globalization.NumberStyles]::Any,
                [System.Globalization.CultureInfo]::InvariantCulture,
                [ref] $parsed)) {
            $sum += $parsed
        }
    }
    return $sum / $script:NanoAiuPerCredit
}

# Reads vally's machine-readable results.jsonl for a completed run and returns an
# ordered hashtable keyed by stimulus name, each value carrying the per-stimulus
# verdict and the efficiency metrics the comparison needs (tokens, turns, wall
# time, AI credits). Returns $null when no results file is present.
function Get-VallyStimulusResults {
    param([Parameter(Mandatory)] [string] $RunOutputDir)

    if (-not (Test-Path -LiteralPath $RunOutputDir)) { return $null }

    # vally writes each run to a timestamped subdirectory; pick the newest one that
    # actually contains a results.jsonl.
    $resultsFile = Get-ChildItem -Path $RunOutputDir -Recurse -Filter 'results.jsonl' -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
    if (-not $resultsFile) { return $null }

    $byStimulus = [ordered]@{}
    foreach ($line in Get-Content -LiteralPath $resultsFile.FullName) {
        if (-not $line) { continue }
        try { $record = $line | ConvertFrom-Json } catch { continue }
        if ($record.type -ne 'trial-result') { continue }

        # A single-trial run has one trajectory object; a multi-trial run
        # (defaults.runs > 1) has an array of them. Accessing `.metrics` on either
        # gives us the metric container(s); ConvertTo-MetricInt collapses arrays.
        $metrics = $record.trajectory.metrics
        $failedGraders = @()
        $failureEvidence = $null
        if ($record.gradeResult -and $record.gradeResult.details) {
            $failedGraderRecords = @($record.gradeResult.details | Where-Object { $_ -and -not $_.passed })
            if ($failedGraderRecords.Count -gt 0) {
                $failedGraders = @($failedGraderRecords | ForEach-Object { [string] $_.name })
                $failureEvidence = [string] $failedGraderRecords[0].evidence
            }
        }

        # Fallback when grader-level details are unavailable.
        if (-not $failureEvidence -and $record.gradeResult) {
            $failureEvidence = [string] $record.gradeResult.evidence
        }

        if ($failureEvidence -and $failureEvidence.Length -gt 220) {
            $failureEvidence = $failureEvidence.Substring(0, 220) + '...'
        }

        $tokenCount = ConvertTo-MetricInt $metrics.tokenUsage.totalTokens
        $turnCount = ConvertTo-MetricInt $metrics.turnCount
        $wallTimeMs = ConvertTo-MetricInt $metrics.wallTimeMs
        # metrics.tokenUsage.cost.amount is reported in nano-aiu; convert to AI
        # Credits. Omitted (and so $null / not-a-number) whenever vally could not
        # establish a complete, consistently-provider/unit cost for the trial.
        $aiCredits = ConvertTo-AiCredits $metrics.tokenUsage.cost.amount

        $byStimulus[[string] $record.stimulus] = [pscustomobject]@{
            Stimulus        = [string] $record.stimulus
            Passed          = [bool] $record.gradeResult.passed
            TotalTokens     = $tokenCount
            TurnCount       = $turnCount
            WallTimeMs      = $wallTimeMs
            AiCredits       = $aiCredits
            FailedGraders   = $failedGraders
            FailureEvidence = $failureEvidence
        }
    }
    return $byStimulus
}

# Classifies a single stimulus by comparing a candidate (WITH the Azure MCP
# server, in a given mode) against the baseline (WITHOUT it). This encodes the
# effectiveness criteria: the server is VALUABLE when it enables an outcome the
# baseline could not achieve; it is a REGRESSION when the baseline succeeds
# without it but the candidate fails; when this candidate AND the baseline both
# pass, the tool was not required for the outcome and efficiency (tokens/turns/
# wall time/AI credits, lower is better) decides; INCONCLUSIVE when neither
# succeeds. Returns 'PASS (baseline also passed)' rather than a bare 'BOTH
# PASS' because each candidate variant (e.g. namespace, consolidated) is
# reported against the shared baseline individually - with 3+ variants on
# screen at once, "BOTH" alone doesn't say which two are meant.
function Get-EffectivenessCategory {
    param($Candidate, $Baseline)

    if (-not $Candidate) { return 'NO DATA' }
    if (-not $Baseline) { return $Candidate.Passed ? 'PASS (no baseline)' : 'FAIL (no baseline)' }
    if ($Candidate.Passed -and -not $Baseline.Passed) { return 'VALUABLE' }
    if (-not $Candidate.Passed -and $Baseline.Passed) { return 'REGRESSION' }
    if (-not $Candidate.Passed -and -not $Baseline.Passed) { return 'INCONCLUSIVE' }
    return 'PASS (baseline also passed)'
}

# When both a candidate and the baseline pass, decides which is more efficient
# using the four lower-is-better metrics, returning a short human-readable
# judgment. CandidateLabel/BaselineLabel identify which side is which in the
# returned text (e.g. "namespace"/"consolidated" or "candidate"/"baseline"),
# since callers compare different pairings (a variant vs the shared baseline,
# or one variant vs another) and a generic "candidate"/"baseline" wording would
# be ambiguous in the latter case.
function Get-EfficiencyJudgment {
    param($Candidate, $Baseline, [string] $CandidateLabel = 'candidate', [string] $BaselineLabel = 'baseline')

    $wins = 0
    $losses = 0
    foreach ($pair in @(
            , @($Candidate.TotalTokens, $Baseline.TotalTokens)
            , @($Candidate.TurnCount, $Baseline.TurnCount)
            , @($Candidate.WallTimeMs, $Baseline.WallTimeMs)
            , @($Candidate.AiCredits, $Baseline.AiCredits))) {
        if ($pair[0] -lt $pair[1]) { $wins++ }
        elseif ($pair[0] -gt $pair[1]) { $losses++ }
    }
    if ($wins -gt 0 -and $losses -eq 0) { return "$CandidateLabel more efficient than $BaselineLabel (better on $wins/4 metrics)" }
    if ($losses -gt 0 -and $wins -eq 0) { return "$CandidateLabel less efficient than $BaselineLabel (worse on $losses/4 metrics)" }
    if ($wins -eq 0 -and $losses -eq 0) { return 'equivalent efficiency' }
    return "mixed ($CandidateLabel better on $wins, worse on $losses)"
}

# Picks the console color that reflects an effectiveness category / judgment.
function Get-VerdictColor {
    param([string] $Category, [string] $Judgment)
    switch -Wildcard ($Category) {
        'VALUABLE' { return 'Green' }
        'REGRESSION' { return 'Red' }
        'INCONCLUSIVE' { return 'Red' }
        'PASS (baseline also passed)' {
            if ($Judgment -like '* more efficient*' -or $Judgment -like 'equivalent*') { return 'Green' }
            return 'Yellow'
        }
        'PASS*' { return 'Green' }
        'FAIL*' { return 'Red' }
        default { return 'Gray' }
    }
}

# Reports one candidate variant (WITH the Azure MCP server in a given mode)
# against the shared baseline, writing the candidate's top-line verdict, any
# failed-stimulus detail, and the per-stimulus effectiveness categories.
# Returns whether the candidate should fail the run (a candidate stimulus
# failed or an effectiveness REGRESSION was detected) and the candidate's
# PASS/FAIL verdict.
function Write-CandidateComparison {
    param(
        [Parameter(Mandatory)] [string] $CandidateName,
        $CandidateStimuli,
        $BaselineStimuli
    )

    $failure = $false

    if (-not $CandidateStimuli) {
        Write-Warn ("  [{0}] (no results.jsonl found - skipping effectiveness comparison)" -f $CandidateName)
        return [pscustomobject]@{ Failure = $true; Verdict = 'NO DATA' }
    }

    # Top-line status: a candidate passes when every one of its stimuli passed.
    $candidatePassed = @($CandidateStimuli.Values | Where-Object { -not $_.Passed }).Count -eq 0
    $verdict = $candidatePassed ? 'PASS' : 'FAIL'
    if (-not $candidatePassed) { $failure = $true }

    Write-Info ("  [{0}] {1}" -f $CandidateName, $verdict)

    if (-not $candidatePassed) {
        $failedRecords = @($CandidateStimuli.Values | Where-Object { -not $_.Passed })
        $failedStimuli = @($failedRecords | ForEach-Object { $_.Stimulus })
        if ($failedStimuli.Count -gt 0) {
            Write-Info ("    {0} failed stimuli: {1}" -f $CandidateName, ($failedStimuli -join ', '))
            foreach ($failed in $failedRecords) {
                $graderPart = ($failed.FailedGraders -and $failed.FailedGraders.Count -gt 0) ? ($failed.FailedGraders -join ', ') : 'unknown grader'
                $evidencePart = $failed.FailureEvidence ? $failed.FailureEvidence : 'no grader evidence was emitted in results.jsonl'
                Write-Info ("      - {0}: {1} - {2}" -f $failed.Stimulus, $graderPart, $evidencePart)
            }
        }
    }

    foreach ($stimulus in $CandidateStimuli.Keys) {
        $c = $CandidateStimuli[$stimulus]
        $b = $BaselineStimuli ? $BaselineStimuli[$stimulus] : $null
        $category = Get-EffectivenessCategory -Candidate $c -Baseline $b

        $detail = ''
        $judgment = $null
        if ($category -eq 'PASS (baseline also passed)') {
            $judgment = Get-EfficiencyJudgment -Candidate $c -Baseline $b -CandidateLabel $CandidateName -BaselineLabel 'baseline'
            $detail = " - $judgment; " +
            ("tokens {0} vs {1} ({2}), turns {3} vs {4} ({5}), wall {6} vs {7} ({8}), credits {9} vs {10} ({11})" -f `
                $c.TotalTokens, $b.TotalTokens, (Format-Delta -Candidate $c.TotalTokens -Baseline $b.TotalTokens),
            $c.TurnCount, $b.TurnCount, (Format-Delta -Candidate $c.TurnCount -Baseline $b.TurnCount),
            (Format-Ms $c.WallTimeMs), (Format-Ms $b.WallTimeMs), (Format-Delta -Candidate $c.WallTimeMs -Baseline $b.WallTimeMs -AsSeconds),
            ('{0:0.00}' -f $c.AiCredits), ('{0:0.00}' -f $b.AiCredits), (Format-Delta -Candidate $c.AiCredits -Baseline $b.AiCredits -Decimals 2))
            # Both passing means the outcome did not require the server - surface it,
            # but it does not fail the run.
            if ($judgment -like '* less efficient*' -or $judgment -like 'mixed*') {
                $detail += ' [review: server did not improve efficiency]'
            }
        }
        elseif ($category -eq 'VALUABLE') {
            $detail = ' - the Azure MCP server enabled an outcome the baseline could not achieve'
        }
        elseif ($category -eq 'REGRESSION') {
            $detail = ' - baseline succeeded WITHOUT the server but the candidate FAILED'
            $failure = $true
        }
        elseif ($category -eq 'INCONCLUSIVE') {
            $detail = ' - neither the candidate nor baseline achieved the outcome'
        }

        $color = Get-VerdictColor -Category $category -Judgment $judgment
        Write-Host ("    - {0,-30} {1}{2}" -f $stimulus, $category, $detail) -ForegroundColor $color
    }

    return [pscustomobject]@{ Failure = $failure; Verdict = $verdict }
}

# Reports a single experiment iteration: the shared baseline verdict followed by
# every candidate (server mode) variant compared against it. Returns an object
# describing whether the iteration should fail the overall run (the experiment
# failed to produce results, a candidate stimulus failed, or an effectiveness
# REGRESSION was detected), the per-candidate verdicts, and the baseline verdict
# so callers can aggregate outcomes across iterations.
function Write-ExperimentIterationReport {
    param(
        [Parameter(Mandatory)] [string] $Name,
        $ExperimentExit,
        [string] $BaselineDir,
        [Parameter(Mandatory)] $CandidateDirs
    )

    $failure = $false
    $candidateVerdicts = [ordered]@{}

    # Per-stimulus results for the baseline and each candidate variant from
    # vally's machine-readable output.
    $baselineStimuli = $BaselineDir ? (Get-VallyStimulusResults -RunOutputDir $BaselineDir) : $null

    $candidateStimuli = [ordered]@{}
    foreach ($candidateName in $CandidateDirs.Keys) {
        $candidateStimuli[$candidateName] = Get-VallyStimulusResults -RunOutputDir $CandidateDirs[$candidateName]
    }
    $anyCandidateResults = @($candidateStimuli.Values | Where-Object { $_ }).Count -gt 0

    # `vally experiment run` can return non-zero when one variant has grader
    # failures (for us, baseline failures are expected). Treat it as a hard run
    # failure only when no usable variant results were produced.
    if ($ExperimentExit -ne 0) {
        if ($anyCandidateResults -or $baselineStimuli) {
            Write-Warn ("{0,-32} vally exit {1} (non-zero due to failed graders in one or more variants; see per-stimulus verdicts below)" -f $Name, $ExperimentExit)
        }
        else {
            Write-Warn ("{0,-32} EXPERIMENT FAILED (vally exit {1}; no variant results found)" -f $Name, $ExperimentExit)
            $failure = $true
        }
    }

    if (-not $anyCandidateResults) {
        Write-Warn ("{0,-32} (no candidate results.jsonl found - skipping effectiveness comparison)" -f $Name)
        return [pscustomobject]@{
            Failure           = $true
            CandidateVerdicts = $candidateVerdicts
            BaselineVerdict   = $null
        }
    }

    # The shared baseline verdict every candidate is compared against.
    $baselineVerdict = $null
    $baselinePassed = $true
    if ($baselineStimuli) {
        $baselinePassed = @($baselineStimuli.Values | Where-Object { -not $_.Passed }).Count -eq 0
        $baselineVerdict = $baselinePassed ? 'PASS' : 'FAIL'
        Write-Info ("{0,-32} baseline {1}" -f $Name, $baselineVerdict)
        if (-not $baselinePassed) {
            $failedBaselineStimuli = @($baselineStimuli.Values | Where-Object { -not $_.Passed } | ForEach-Object { $_.Stimulus })
            if ($failedBaselineStimuli.Count -gt 0) {
                Write-Info ("  baseline failed stimuli: {0}" -f ($failedBaselineStimuli -join ', '))
            }
        }
    }
    else {
        Write-Info ("{0,-32} (no baseline)" -f $Name)
    }

    foreach ($candidateName in $candidateStimuli.Keys) {
        $outcome = Write-CandidateComparison `
            -CandidateName $candidateName `
            -CandidateStimuli $candidateStimuli[$candidateName] `
            -BaselineStimuli $baselineStimuli
        $candidateVerdicts[$candidateName] = $outcome.Verdict
        if ($outcome.Failure) { $failure = $true }
    }

    Write-NamespaceVsConsolidatedComparison `
        -CandidateStimuli $candidateStimuli `
        -BaselineStimuli $baselineStimuli

    return [pscustomobject]@{
        Failure           = $failure
        CandidateVerdicts = $candidateVerdicts
        BaselineVerdict   = $baselineVerdict
    }
}

# When BOTH the 'namespace' and 'consolidated' candidates are individually
# VALUABLE for a given stimulus (i.e. each enabled an outcome the baseline
# could not achieve), the two candidates' PASS/FAIL against the baseline tells
# us nothing about which server MODE is preferable. In that case, additionally
# report the relative difference between the two candidates themselves (lower
# is better) across the same four efficiency metrics used for candidate-vs-
# baseline comparisons, so both server modes' results stay visible alongside
# the head-to-head delta. Silently does nothing when either variant is
# missing, has no results for a stimulus, or was not VALUABLE for it.
function Write-NamespaceVsConsolidatedComparison {
    param(
        [Parameter(Mandatory)] $CandidateStimuli,
        $BaselineStimuli
    )

    $namespaceStimuli = $CandidateStimuli['namespace']
    $consolidatedStimuli = $CandidateStimuli['consolidated']
    if (-not $namespaceStimuli -or -not $consolidatedStimuli) { return }

    $stimuli = @($namespaceStimuli.Keys | Where-Object { $consolidatedStimuli.Contains($_) })
    foreach ($stimulus in $stimuli) {
        $ns = $namespaceStimuli[$stimulus]
        $co = $consolidatedStimuli[$stimulus]
        $baseline = $BaselineStimuli ? $BaselineStimuli[$stimulus] : $null

        $nsCategory = Get-EffectivenessCategory -Candidate $ns -Baseline $baseline
        $coCategory = Get-EffectivenessCategory -Candidate $co -Baseline $baseline
        if ($nsCategory -ne 'VALUABLE' -or $coCategory -ne 'VALUABLE') { continue }

        # Both modes are individually valuable - report each mode's result plus
        # the relative (namespace-vs-consolidated) difference, lower is better.
        $judgment = Get-EfficiencyJudgment -Candidate $ns -Baseline $co -CandidateLabel 'namespace' -BaselineLabel 'consolidated'
        $detail = "namespace vs consolidated - $judgment; " +
        ("tokens {0} vs {1} ({2}), turns {3} vs {4} ({5}), wall {6} vs {7} ({8}), credits {9} vs {10} ({11})" -f `
            $ns.TotalTokens, $co.TotalTokens, (Format-Delta -Candidate $ns.TotalTokens -Baseline $co.TotalTokens),
        $ns.TurnCount, $co.TurnCount, (Format-Delta -Candidate $ns.TurnCount -Baseline $co.TurnCount),
        (Format-Ms $ns.WallTimeMs), (Format-Ms $co.WallTimeMs), (Format-Delta -Candidate $ns.WallTimeMs -Baseline $co.WallTimeMs -AsSeconds),
        ('{0:0.00}' -f $ns.AiCredits), ('{0:0.00}' -f $co.AiCredits), (Format-Delta -Candidate $ns.AiCredits -Baseline $co.AiCredits -Decimals 2))

        Write-Host ("    - {0,-30} NAMESPACE+CONSOLIDATED BOTH VALUABLE - {1}" -f $stimulus, $detail) -ForegroundColor Cyan
    }
}

# Invokes a pre/post provisioning script, forwarding -ResourceGroup / -Subscription
# only when the target script declares those parameters.
function Invoke-ProvisioningScript {
    param(
        [Parameter(Mandatory)] [string] $Label,
        [Parameter(Mandatory)] [string] $Path
    )

    $resolved = Resolve-Path -Path $Path -ErrorAction SilentlyContinue
    if (-not $resolved) {
        throw "$Label script not found: $Path"
    }

    $scriptArgs = @{}
    $scriptParams = (Get-Command $resolved.Path).Parameters
    if ($ResourceGroup -and $scriptParams.ContainsKey('ResourceGroup')) { $scriptArgs['ResourceGroup'] = $ResourceGroup }
    if ($Subscription -and $scriptParams.ContainsKey('Subscription')) { $scriptArgs['Subscription'] = $Subscription }

    Write-Info "[$Label] Running: $($resolved.Path)"
    $global:LASTEXITCODE = 0
    & $resolved.Path @scriptArgs
    if ($LASTEXITCODE) {
        throw "$Label script failed with exit code $LASTEXITCODE."
    }
}

# The experiment file name suffix. The tool name is the file name with this
# suffix removed (e.g. 'eventhub-get.experiment.yaml' -> 'eventhub-get').
$ExperimentSuffix = '.experiment.yaml'

# Resolves the pre/post provisioning scripts for an area directory: an explicit
# override (if given) wins, otherwise auto-discover by convention.
function Resolve-AreaProvisioning {
    param([Parameter(Mandatory)] [string] $AreaDir)

    $pre = $null
    $post = $null
    if ($SkipProvisioning) {
        return [pscustomobject]@{ Pre = $null; Post = $null }
    }

    if ($PreEvalScript) {
        $pre = $PreEvalScript
    }
    else {
        $auto = Get-ChildItem -Path $AreaDir -Filter 'New-*Resources.ps1' -File -ErrorAction SilentlyContinue |
        Sort-Object Name | Select-Object -First 1
        if ($auto) { $pre = $auto.FullName }
    }

    if ($PostEvalScript) {
        $post = $PostEvalScript
    }
    else {
        $auto = Get-ChildItem -Path $AreaDir -Filter 'Remove-*Resources.ps1' -File -ErrorAction SilentlyContinue |
        Sort-Object Name | Select-Object -First 1
        if ($auto) { $post = $auto.FullName }
    }

    return [pscustomobject]@{ Pre = $pre; Post = $post }
}

# The variant subfolder name vally uses for the control (server-less) variant.
$BaselineVariantName = 'baseline'

# Splits a completed run's per-variant subfolders into the baseline (control) and
# the candidate variants (every other subfolder - e.g. 'namespace',
# 'consolidated'). `vally experiment run` writes one subfolder per variant named
# after its `variants:` key. Candidate discovery is dynamic so new server-mode
# variants added to an experiment spec are picked up without script changes.
# Returns a null BaselineDir and an empty CandidateDirs map when the run
# directory is missing.
function Get-RunVariantDirs {
    param([string] $RunDir)

    $baselineDir = $null
    $candidateDirs = [ordered]@{}

    if ($RunDir -and (Test-Path -LiteralPath $RunDir)) {
        $subdirs = Get-ChildItem -Path $RunDir -Directory -ErrorAction SilentlyContinue | Sort-Object Name
        foreach ($d in $subdirs) {
            if ($d.Name -ieq $BaselineVariantName) { $baselineDir = $d.FullName }
            else { $candidateDirs[$d.Name] = $d.FullName }
        }
    }

    return [pscustomobject]@{
        BaselineDir   = $baselineDir
        CandidateDirs = $candidateDirs
    }
}

# Runs each discovered experiment -Iterations time(s), provisioning + tearing down
# per area, and returns the list of per-experiment result descriptors the summary
# consumes. Each descriptor carries Name, ProvisioningFailed, and an Iterations
# list of { Iteration, ExperimentExit, BaselineDir, CandidateDirs }.
function Get-ExperimentResultsFromRuns {
    param([Parameter(Mandatory)] $Evals)

    $results = [System.Collections.Generic.List[object]]::new()

    foreach ($areaGroup in ($Evals | Group-Object AreaDir)) {
        $areaDir = $areaGroup.Name
        $areaName = Split-Path $areaDir -Leaf
        $provisioning = Resolve-AreaProvisioning -AreaDir $areaDir
        $provisioned = $true

        try {
            # Provision once for the whole area. A provisioning failure fails the run:
            # record it, skip this area's evals (the resources aren't there), but still
            # run teardown below to clean up anything partially created.
            if ($provisioning.Pre) {
                try {
                    Invoke-ProvisioningScript -Label "[$areaName] pre-eval provisioning" -Path $provisioning.Pre
                }
                catch {
                    $provisioned = $false
                    Write-Warn "[$areaName] pre-eval provisioning FAILED: $($_.Exception.Message)"
                    Write-Warn "[$areaName] Skipping this area's experiments."
                    foreach ($eval in $areaGroup.Group) {
                        $results.Add([pscustomobject]@{
                                Name               = "$($eval.Area)/$($eval.Tool)"
                                ProvisioningFailed = $true
                                Iterations         = @()
                            })
                    }
                }
            }
            else {
                Write-Info "[$areaName] No pre-eval provisioning script (resources assumed to already exist)."
            }

            if ($provisioned) {
                foreach ($eval in $areaGroup.Group) {
                    $label = "$($eval.Area)/$($eval.Tool)"
                    $runRoot = Join-Path (Join-Path $RunDir $eval.Area) $eval.Tool

                    # Run the experiment -Iterations time(s). Each run produces every
                    # variant (baseline + the server candidates) under its own
                    # timestamped directory; record every iteration so the summary
                    # can report each one's outcome.
                    $iterationResults = [System.Collections.Generic.List[object]]::new()
                    for ($i = 1; $i -le $Iterations; $i++) {
                        $iterationLabel = ($Iterations -gt 1) ? "$label (iteration $i/$Iterations)" : $label

                        $run = Invoke-VallyExperiment `
                            -Label $iterationLabel `
                            -ExperimentFile $eval.Experiment `
                            -RunOutputDir $runRoot

                        # Each variant's results.jsonl lives in <run>/<variant>/. Split
                        # the run's subfolders into the baseline (control) and the
                        # candidate (server mode) variants; the candidate names match
                        # the non-baseline `variants:` keys in the experiment spec.
                        $variantDirs = Get-RunVariantDirs -RunDir $run.RunDir

                        $iterationResults.Add([pscustomobject]@{
                                Iteration      = $i
                                ExperimentExit = $run.Exit
                                BaselineDir    = $variantDirs.BaselineDir
                                CandidateDirs  = $variantDirs.CandidateDirs
                            })
                    }

                    $results.Add([pscustomobject]@{
                            Name               = $label
                            ProvisioningFailed = $false
                            Iterations         = $iterationResults
                        })
                }
            }
        }
        finally {
            # Tear down the area even if provisioning or an eval threw.
            if ($provisioning.Post) {
                try {
                    Invoke-ProvisioningScript -Label "[$areaName] post-eval teardown" -Path $provisioning.Post
                }
                catch {
                    Write-Warn "[$areaName] post-eval teardown failed: $($_.Exception.Message)"
                    Write-Warn 'Resources may still exist. The DeleteAfter tag ensures they are cleaned up later.'
                }
            }
        }
    }

    return $results
}

# Reconstructs the same per-experiment result descriptors from artifacts a
# previous run already saved under the resolved -RunDir (the newest run under
# -OutputDir, or the specific run named by -ReportFrom), WITHOUT building,
# provisioning, or invoking vally. For each discovered experiment it takes the
# newest -Iterations timestamped run directory(ies) under
# <RunDir>/<area>/<tool>/ and splits each into its baseline and candidate
# (server mode) variant subfolders. The vally exit code is not persisted in the
# artifacts, so ExperimentExit is reported as 0 and the summary relies on the
# per-stimulus verdicts recorded in each run's results.jsonl.
function Get-ExperimentResultsFromArtifacts {
    param([Parameter(Mandatory)] $Evals)

    $results = [System.Collections.Generic.List[object]]::new()

    foreach ($eval in $Evals) {
        $label = "$($eval.Area)/$($eval.Tool)"
        $runRoot = Join-Path (Join-Path $RunDir $eval.Area) $eval.Tool

        if (-not (Test-Path -LiteralPath $runRoot)) {
            Write-Warn "[$label] No artifacts under '$runRoot' (nothing to report)."
            continue
        }

        # vally names each run directory with a sortable UTC timestamp (e.g.
        # 2026-07-09T00-05-49-540Z). Match only those so stray non-run folders
        # (e.g. a hand-run variant folder) are ignored, then take the newest
        # -Iterations of them - oldest-first so iteration numbers read chronologically.
        $timestampPattern = '^\d{4}-\d{2}-\d{2}T'
        $runDirs = @(Get-ChildItem -Path $runRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match $timestampPattern } |
            Sort-Object Name -Descending |
            Select-Object -First $Iterations |
            Sort-Object Name)

        if ($runDirs.Count -eq 0) {
            Write-Warn "[$label] No timestamped run directories under '$runRoot' (nothing to report)."
            continue
        }

        $iterationResults = [System.Collections.Generic.List[object]]::new()
        $i = 0
        # NOTE: do not name this loop variable $runDir/$RunDir - PowerShell
        # variable names are case-insensitive, so that would silently clobber
        # the script-scope $RunDir used above (and by later iterations of the
        # outer $Evals loop) to compute $runRoot for each tool.
        foreach ($iterationRunDir in $runDirs) {
            $i++
            $variantDirs = Get-RunVariantDirs -RunDir $iterationRunDir.FullName
            $iterationResults.Add([pscustomobject]@{
                    Iteration      = $i
                    ExperimentExit = 0
                    BaselineDir    = $variantDirs.BaselineDir
                    CandidateDirs  = $variantDirs.CandidateDirs
                })
        }

        $results.Add([pscustomobject]@{
                Name               = $label
                ProvisioningFailed = $false
                Iterations         = $iterationResults
            })
    }

    return $results
}

# Prints the effectiveness comparison for every gathered experiment result and
# returns $true if any of them should fail the run. The goal is to measure the
# Azure MCP server's effectiveness, per stimulus, by comparing each candidate
# (WITH the server, in a given mode) against the baseline (WITHOUT it):
#
#   * baseline FAIL + candidate PASS -> VALUABLE      (the server enabled the outcome)
#   * baseline PASS + candidate FAIL -> REGRESSION    (the server hurt the outcome)
#   * both PASS                      -> the tool was not required; efficiency decides,
#                                        using tokens / turns / wall time / AI credits
#                                        (lower is better)
#   * neither PASS                   -> INCONCLUSIVE
#
# A result fails the run only when the experiment itself failed to execute, a
# candidate stimulus failed, or an effectiveness REGRESSION is detected. Baseline
# outcome failures are expected and do not fail the run.
function Write-ResultsSummary {
    param([Parameter(Mandatory)] $Results)

    Write-Host ''
    Write-Info '======================= Results ======================='
    $anyFailure = $false
    foreach ($r in $Results) {
        if ($r.ProvisioningFailed) {
            Write-Warn ("{0,-32} PROVISIONING FAILED (experiment skipped)" -f $r.Name)
            $anyFailure = $true
            continue
        }

        $iterations = @($r.Iterations)
        $iterationCount = $iterations.Count
        # Per-candidate pass counts across iterations (candidate name -> count).
        $candidatePassCounts = [ordered]@{}

        foreach ($iteration in $iterations) {
            # When an experiment runs more than once, header each iteration so its
            # per-stimulus verdicts below are unambiguous.
            if ($iterationCount -gt 1) {
                Write-Host ''
                Write-Info ("{0} - iteration {1}/{2}" -f $r.Name, $iteration.Iteration, $iterationCount)
            }

            $outcome = Write-ExperimentIterationReport `
                -Name $r.Name `
                -ExperimentExit $iteration.ExperimentExit `
                -BaselineDir $iteration.BaselineDir `
                -CandidateDirs $iteration.CandidateDirs

            if ($outcome.Failure) { $anyFailure = $true }
            foreach ($candidateName in $outcome.CandidateVerdicts.Keys) {
                if (-not $candidatePassCounts.Contains($candidateName)) { $candidatePassCounts[$candidateName] = 0 }
                if ($outcome.CandidateVerdicts[$candidateName] -eq 'PASS') { $candidatePassCounts[$candidateName]++ }
            }
        }

        # Aggregate each candidate's outcomes across iterations so flaky results
        # (some iterations pass, some fail) are visible at a glance.
        if ($iterationCount -gt 1) {
            foreach ($candidateName in $candidatePassCounts.Keys) {
                $passCount = $candidatePassCounts[$candidateName]
                $aggregateColor = ($passCount -eq $iterationCount) ? 'Green' : (($passCount -eq 0) ? 'Red' : 'Yellow')
                Write-Host ("{0,-32} {1} passed {2}/{3} iteration(s)" -f $r.Name, $candidateName, $passCount, $iterationCount) -ForegroundColor $aggregateColor
            }
        }
    }
    Write-Info "Run directory: $RunDir"

    return $anyFailure
}

# Aggregates one variant's (baseline or a named candidate) per-stimulus metrics
# across every iteration of a single tool's results, collapsing all
# iteration/stimulus trials for that variant into one set of summary statistics.
# This is what lets the consolidated report account for multiple -Iterations:
# each trial (one stimulus in one iteration) contributes equally to the
# average, and PassRate is the fraction of all those trials that passed.
# Returns $null when the variant has no results in any iteration (e.g. an
# experiment that only ran a subset of variants, or missing artifacts).
function Get-VariantAggregateMetrics {
    param(
        [Parameter(Mandatory)] $IterationsList,
        [Parameter(Mandatory)] [string] $VariantName
    )

    $totalTokens = 0.0
    $totalTurns = 0.0
    $totalWallMs = 0.0
    $totalCredits = 0.0
    $passCount = 0
    $trialCount = 0

    foreach ($iteration in $IterationsList) {
        $dir = ($VariantName -ieq $BaselineVariantName) ? $iteration.BaselineDir : $iteration.CandidateDirs[$VariantName]
        if (-not $dir) { continue }

        $stimuli = Get-VallyStimulusResults -RunOutputDir $dir
        if (-not $stimuli) { continue }

        foreach ($stimulusResult in $stimuli.Values) {
            $trialCount++
            if ($stimulusResult.Passed) { $passCount++ }
            $totalTokens += $stimulusResult.TotalTokens
            $totalTurns += $stimulusResult.TurnCount
            $totalWallMs += $stimulusResult.WallTimeMs
            $totalCredits += $stimulusResult.AiCredits
        }
    }

    if ($trialCount -eq 0) { return $null }

    return [pscustomobject]@{
        Variant       = $VariantName
        TrialCount    = $trialCount
        PassCount     = $passCount
        PassRate      = $passCount / [double] $trialCount
        AvgTokens     = $totalTokens / $trialCount
        AvgTurnCount  = $totalTurns / $trialCount
        AvgWallTimeMs = $totalWallMs / $trialCount
        AvgAiCredits  = $totalCredits / $trialCount
    }
}

# Returns every variant name (the shared 'baseline' plus each candidate, e.g.
# 'namespace'/'consolidated') that appears in ANY iteration of a tool's results,
# in first-seen order. Iterations can in principle vary in which variants
# produced results (e.g. a candidate crashed in one iteration), so this unions
# across all of them rather than trusting only the first iteration.
function Get-AllVariantNames {
    param([Parameter(Mandatory)] $IterationsList)

    $names = [System.Collections.Generic.List[string]]::new()
    $seen = @{}
    foreach ($iteration in $IterationsList) {
        if ($iteration.BaselineDir -and -not $seen.ContainsKey($BaselineVariantName)) {
            $names.Add($BaselineVariantName)
            $seen[$BaselineVariantName] = $true
        }
        foreach ($candidateName in $iteration.CandidateDirs.Keys) {
            if (-not $seen.ContainsKey($candidateName)) {
                $names.Add($candidateName)
                $seen[$candidateName] = $true
            }
        }
    }
    return $names
}

# Picks the best-performing entry from a list of Get-VariantAggregateMetrics
# results: highest PassRate wins first (a variant that achieves the outcome
# more often is "better" regardless of efficiency), and ties are broken by the
# lower-is-better efficiency metrics in order (tokens, then turns, then wall
# time, then AI credits) - the same metric priority Get-EfficiencyJudgment uses
# for candidate-vs-baseline comparisons.
function Get-BestVariantMetrics {
    param([Parameter(Mandatory)] $MetricsList)

    return @($MetricsList) |
    Sort-Object -Property `
    @{ Expression = 'PassRate'; Descending = $true }, `
        'AvgTokens', 'AvgTurnCount', 'AvgWallTimeMs', 'AvgAiCredits' |
    Select-Object -First 1
}

# Formats one variant's aggregated metrics as a compact one-line summary, e.g.
# "namespace       pass 27/28 (96%) | avg tokens 118,432, avg turns 6.5, avg wall 34.2s, avg credits 21.07".
function Format-VariantMetricsLine {
    param([Parameter(Mandatory)] $Metrics)

    $passPct = [math]::Round($Metrics.PassRate * 100)
    return ("pass {0}/{1} ({2}%) | avg tokens {3}, avg turns {4:0.0}, avg wall {5}, avg credits {6:0.00}" -f `
            $Metrics.PassCount, $Metrics.TrialCount, $passPct,
        ('{0:N0}' -f $Metrics.AvgTokens), $Metrics.AvgTurnCount, (Format-Ms $Metrics.AvgWallTimeMs), $Metrics.AvgAiCredits)
}

# Prints the consolidated, by-tool report: one line per tool naming the
# best-performing variant - including the shared baseline, which is eligible
# to win "best" like any other variant since a server-less control that
# matches or beats every candidate's pass rate while using far fewer
# tokens/turns/wall time/credits genuinely is the best-performing option for
# that tool - and its aggregated metrics, followed by every other variant so
# the full comparison stays visible. All metrics are aggregated across every
# iteration and stimulus recorded for that tool, so a multi-iteration run
# (-Iterations > 1) is correctly folded into one set of per-variant statistics
# rather than reported per iteration.
function Write-ConsolidatedSummary {
    param([Parameter(Mandatory)] $Results)

    Write-Host ''
    Write-Info '================= Consolidated Summary (by tool) ================='

    foreach ($r in $Results) {
        if ($r.ProvisioningFailed) {
            Write-Warn ("{0,-32} PROVISIONING FAILED - excluded from consolidated summary" -f $r.Name)
            continue
        }

        $iterations = @($r.Iterations)
        if ($iterations.Count -eq 0) {
            Write-Warn ("{0,-32} (no iterations recorded - excluded from consolidated summary)" -f $r.Name)
            continue
        }

        $variantNames = Get-AllVariantNames -IterationsList $iterations
        $metricsByVariant = [ordered]@{}
        foreach ($variantName in $variantNames) {
            $metrics = Get-VariantAggregateMetrics -IterationsList $iterations -VariantName $variantName
            if ($metrics) { $metricsByVariant[$variantName] = $metrics }
        }

        if ($metricsByVariant.Count -eq 0) {
            Write-Warn ("{0,-32} (no variant results found - excluded from consolidated summary)" -f $r.Name)
            continue
        }

        $iterationNote = $iterations.Count -gt 1 ? (" ({0} iterations)" -f $iterations.Count) : ''
        Write-Host ("{0,-32}{1}" -f $r.Name, $iterationNote) -ForegroundColor Cyan

        $allMetrics = @($metricsByVariant.Values)
        $best = Get-BestVariantMetrics -MetricsList $allMetrics

        # Beyond naming a winner, call out the value-add verdict this
        # comparison implies: if baseline (no MCP server) is the best
        # variant, the MCP tool(s) added no value for this operation -
        # baseline already matches or beats every candidate's pass rate at a
        # fraction of the cost. If a candidate wins instead, that only
        # happens when baseline couldn't match its pass rate, so the
        # meaningful question becomes which MCP tool variant is best - not
        # whether the tool helps at all.
        $verdict = ($best.Variant -ieq $BaselineVariantName) `
            ? '[no added value - baseline matches/beats every MCP tool variant]' `
            : '[baseline could not match this outcome - best MCP tool variant shown]'
        Write-Host ("  best: {0,-14} {1} {2}" -f $best.Variant, (Format-VariantMetricsLine -Metrics $best), $verdict) -ForegroundColor Green

        foreach ($variantName in ($allMetrics | Where-Object { $_.Variant -ne $best.Variant } | ForEach-Object { $_.Variant })) {
            $m = $metricsByVariant[$variantName]
            $color = ($variantName -ieq $BaselineVariantName) ? 'DarkGray' : 'Gray'
            Write-Host ("  {0,-19} {1}" -f $variantName, (Format-VariantMetricsLine -Metrics $m)) -ForegroundColor $color
        }
    }
}

# --- Discover the experiments to run -----------------------------------------
# An experiment (<tool>.experiment.yaml) runs a shared eval spec as a baseline
# variant (no Azure MCP server) and one or more server candidate variants
# (namespace + consolidated modes). Experiments
# are grouped by their area (the immediate subdirectory of this script) so
# provisioning runs once per area.
if ($ExperimentSpec -and ($Area -or $Tool)) {
    throw '-ExperimentSpec cannot be combined with -Area or -Tool.'
}
if ($ReportOnly) {
    # Report-only never provisions or runs, so provisioning switches don't apply.
    if ($PreEvalScript -or $PostEvalScript -or $SkipProvisioning -or $SkipBuild) {
        Write-Warn '-ReportOnly ignores -PreEvalScript/-PostEvalScript/-SkipProvisioning/-SkipBuild.'
    }
}
elseif ($SkipProvisioning -and ($PreEvalScript -or $PostEvalScript)) {
    Write-Warn '-SkipProvisioning was set; ignoring -PreEvalScript/-PostEvalScript.'
    $PreEvalScript = $null
    $PostEvalScript = $null
}

$experimentFiles = [System.Collections.Generic.List[System.IO.FileInfo]]::new()

if ($ExperimentSpec) {
    $resolvedSpec = Resolve-Path -Path $ExperimentSpec -ErrorAction SilentlyContinue
    if (-not $resolvedSpec) { throw "Experiment spec not found: $ExperimentSpec" }
    $experimentFiles.Add((Get-Item $resolvedSpec.Path))
}
else {
    # Every *.experiment.yaml under any subfolder.
    $candidates = Get-ChildItem -Path $PSScriptRoot -Recurse -File -Filter "*$ExperimentSuffix" -ErrorAction SilentlyContinue |
    Sort-Object FullName

    foreach ($c in $candidates) {
        $areaName = Split-Path (Split-Path $c.FullName -Parent) -Leaf
        $toolName = $c.Name.Substring(0, $c.Name.Length - $ExperimentSuffix.Length)

        if ($Area -and -not ($Area | Where-Object { $areaName -like $_ })) { continue }
        if ($Tool -and -not ($Tool | Where-Object { $toolName -like $_ })) { continue }

        $experimentFiles.Add($c)
    }
}

if ($experimentFiles.Count -eq 0) {
    throw "No experiments matched (Area='$($Area -join ',')', Tool='$($Tool -join ',')'). Expected files named '<tool>$ExperimentSuffix'."
}

# Build the ordered list of experiment descriptors and group them by area directory.
$evals = foreach ($spec in $experimentFiles) {
    $areaDir = Split-Path $spec.FullName -Parent
    $areaName = Split-Path $areaDir -Leaf
    $toolName = $spec.Name.Substring(0, $spec.Name.Length - $ExperimentSuffix.Length)

    [pscustomobject]@{
        Area       = $areaName
        AreaDir    = $areaDir
        Tool       = $toolName
        Experiment = $spec.FullName
    }
}

Write-Info ("Discovered {0} experiment(s): {1}" -f $evals.Count, (($evals | ForEach-Object { "$($_.Area)/$($_.Tool)" }) -join ', '))
if ($ReportOnly) {
    Write-Info ("Report-only: reading the newest {0} iteration(s) per experiment from $RunDir." -f $Iterations)
}
elseif ($Iterations -gt 1) {
    Write-Info "Running each experiment $Iterations times."
}

# --- Gather results ----------------------------------------------------------
# In report-only mode reconstruct results from saved artifacts; otherwise run the
# experiments (build/provision/run/teardown already happened above). Both paths
# yield the same descriptor shape so the summary is identical.
if ($ReportOnly) {
    $results = Get-ExperimentResultsFromArtifacts -Evals $evals
    if (@($results).Count -eq 0) {
        throw "No run artifacts found under '$RunDir' for the requested experiments. Run without -ReportOnly first, or pass -ReportFrom <run-directory>."
    }
}
else {
    $results = Get-ExperimentResultsFromRuns -Evals $evals
}

# --- Report the comparison ---------------------------------------------------
# The goal is to measure the Azure MCP server's effectiveness, per stimulus, by
# comparing each candidate (WITH the server, in a given mode) against the
# baseline (WITHOUT it):
#
#   * baseline FAIL + candidate PASS -> VALUABLE      (the server enabled the outcome)
#   * baseline PASS + candidate FAIL -> REGRESSION    (the server hurt the outcome)
#   * both PASS                      -> the tool was not required; efficiency decides,
#                                        using tokens / turns / wall time / AI credits
#                                        (lower is better)
#   * neither PASS                   -> INCONCLUSIVE
#
# A run is only considered failed (non-zero exit) when the experiment itself
# failed to execute, a candidate stimulus failed, or an effectiveness REGRESSION
# is detected. Baseline outcome failures are expected and do not fail the run.
# Returns $true if any such failure was detected across all experiments.
$anyFailure = Write-ResultsSummary -Results $results

# --- Consolidated, by-tool summary -------------------------------------------
# On top of the per-iteration comparison above, print one aggregated line per
# tool naming its best-performing candidate variant and that variant's metrics,
# with metrics folded across every iteration and stimulus (not just the latest
# one), so a multi-iteration run is represented by one clear verdict per tool.
Write-ConsolidatedSummary -Results $results

# Non-zero if any experiment failed to run, any candidate stimulus failed, any
# area's provisioning failed, or an effectiveness regression (baseline PASS while
# a candidate FAILs) was detected; baseline outcome failures on their own are
# expected and do not affect the exit code.
exit ($anyFailure ? 1 : 0)
