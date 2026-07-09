#!/bin/env pwsh
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
     `vally experiment run`. The experiment executes the shared eval spec as two
     variants for comparison:
       - treatment - WITH the Azure MCP server, and
       - baseline  - WITHOUT the Azure MCP server.
     The delta between them isolates the Azure MCP server's contribution.
     Each experiment is run -Iterations time(s) (default 1); the results summary
     reports the outcome of every iteration.

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

Both variants are graded on the same task *outcome*, so their verdicts are
directly comparable; the baseline (no Azure MCP server) is EXPECTED TO FAIL when
the tool is required. Provide a real subscription (via `az login`) so the
treatment can return real data and the comparison is meaningful. The script's exit
code is non-zero if any experiment fails to run or a treatment stimulus fails;
baseline failures are expected and do not affect it.

Pass -ReportOnly (or -ReportFrom <dir>) to skip building, provisioning, and
running vally entirely, and instead regenerate the summary from run artifacts a
previous run already saved under -OutputDir (or -ReportFrom). This is fast,
offline, and free - handy for re-examining or debugging a prior run's results.

.PARAMETER ExperimentSpec
Optional path to a single experiment spec (<tool>.experiment.yaml) to run instead
of discovering them. Cannot be combined with -Area or -Tool.

.PARAMETER Iterations
Number of times to run each discovered experiment (default 1). Every iteration is
a full `vally experiment run` (baseline + treatment) written to its own
timestamped directory, and the results summary reports each iteration's outcome
plus an aggregate pass count. Useful for surfacing non-deterministic (flaky)
results. Must be 1 or greater.

.PARAMETER OutputDir
Parent directory where vally writes its run artifacts. Each experiment's runs go
to an '<area>/<tool>' subdirectory, under which `vally experiment run` creates a
timestamped folder with one subfolder per variant (baseline, treatment). Defaults
to ./.vally-results next to this script.

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

.PARAMETER PreEvalScript
Optional path to a provisioning script run BEFORE the experiments of an area (e.g.
./eventhubs/New-EventHubsResources.ps1). Use it to create the Azure resources the
eval prompts reference. -ResourceGroup and -Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `New-*Resources.ps1` script in each area directory and uses that.

.PARAMETER PostEvalScript
Optional path to a teardown script run AFTER an area's experiments (e.g.
./eventhubs/Remove-EventHubsResources.ps1). It runs in a finally block, so it
executes even if the run or the pre-eval provisioning fails. -ResourceGroup and
-Subscription are forwarded to it.

If not specified (and -SkipProvisioning is not set), the runner auto-discovers a
convention-named `Remove-*Resources.ps1` script in each area directory and uses
that.

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
summary from artifacts a previous run saved under -OutputDir (or -ReportFrom). The
-Area/-Tool filters and -Iterations still apply: -Iterations selects how many of
the newest timestamped run(s) per tool to report (default 1). Because the vally
exit code is not persisted in the artifacts, the summary relies on the per-stimulus
verdicts in each run's results.jsonl; the process still exits non-zero if a
treatment stimulus failed or an effectiveness regression is detected.

.PARAMETER ReportFrom
Optional path to a results tree to report from (its '<area>/<tool>/<timestamp>'
layout must match what a run produces). Implies -ReportOnly. Use it to re-report an
archived or copied set of artifacts without overwriting -OutputDir.

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

# -ReportFrom names an alternate artifacts tree to report from, and implies
# -ReportOnly. Both are convenience switches for regenerating the summary from a
# previous run without building, provisioning, or invoking vally.
if ($ReportFrom) {
    $ReportOnly = $true
    $OutputDir = $ReportFrom
}

# vally emits UTF-8 (box-drawing rules, check marks, etc.). If the console is on a
# legacy OEM code page (e.g. 437), those bytes are decoded wrong and show up as
# mojibake like '?��'/'?��'. Force UTF-8 (BOM-less, so redirected/CI output stays
# clean) so vally's output is captured and displayed correctly.
$OutputEncoding = [System.Text.UTF8Encoding]::new()
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()

# Repo root is five levels up: servers/Azure.Mcp.Server/tests/Vally -> repo root
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..' '..' '..' '..')
$ServerProject = Join-Path $RepoRoot 'servers' 'Azure.Mcp.Server' 'src' 'Azure.Mcp.Server.csproj'

function Write-Info($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[vally-eval] $Message" -ForegroundColor Yellow }

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
# (baseline + treatment) and writes each variant's results under a single
# timestamped run directory it creates beneath --output-dir:
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
    # Pipe vally's output to the host so it is displayed but NOT emitted on this
    # function's output stream. Without Out-Host, vally's stdout is bundled with
    # the exit code into the caller's variable, turning it into an array; the later
    # ($exit -eq 0) checks then misread a passing run (exit 0) as FAIL.
    & vally @vallyArgs | Out-Host
    $exit = $LASTEXITCODE

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

# Formats a treatment-vs-baseline delta (lower is better) as e.g. "+12343 (+28%)"
# or "-2.8s (-9%)". A negative value means the treatment used less - i.e. better.
function Format-Delta {
    param(
        [Parameter(Mandatory)] [int] $Treatment,
        [Parameter(Mandatory)] [int] $Baseline,
        [switch] $AsSeconds
    )
    $delta = $Treatment - $Baseline
    $sign = ($delta -gt 0) ? '+' : ''
    $body = $AsSeconds ? ('{0}{1:0.0}s' -f $sign, ($delta / 1000.0)) : ('{0}{1}' -f $sign, $delta)
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

# Reads vally's machine-readable results.jsonl for a completed run and returns an
# ordered hashtable keyed by stimulus name, each value carrying the per-stimulus
# verdict and the efficiency metrics the comparison needs (tokens, turns, wall
# time). Returns $null when no results file is present.
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

        $byStimulus[[string] $record.stimulus] = [pscustomobject]@{
            Stimulus        = [string] $record.stimulus
            Passed          = [bool] $record.gradeResult.passed
            TotalTokens     = $tokenCount
            TurnCount       = $turnCount
            WallTimeMs      = $wallTimeMs
            FailedGraders   = $failedGraders
            FailureEvidence = $failureEvidence
        }
    }
    return $byStimulus
}

# Classifies a single stimulus by comparing the treatment (WITH the Azure MCP
# server) against the baseline (WITHOUT it). This encodes the effectiveness
# criteria: the server is VALUABLE when it enables an outcome the baseline could
# not achieve; it is a REGRESSION when the baseline succeeds without it but the
# treatment fails; when BOTH PASS the tool was not required for the outcome and
# efficiency (tokens/turns/wall time, lower is better) decides; INCONCLUSIVE when
# neither succeeds.
function Get-EffectivenessCategory {
    param($Treatment, $Baseline)

    if (-not $Treatment) { return 'NO DATA' }
    if (-not $Baseline) { return $Treatment.Passed ? 'PASS (no baseline)' : 'FAIL (no baseline)' }
    if ($Treatment.Passed -and -not $Baseline.Passed) { return 'VALUABLE' }
    if (-not $Treatment.Passed -and $Baseline.Passed) { return 'REGRESSION' }
    if (-not $Treatment.Passed -and -not $Baseline.Passed) { return 'INCONCLUSIVE' }
    return 'BOTH PASS'
}

# When both treatment and baseline pass, decides which is more efficient using the
# three lower-is-better metrics, returning a short human-readable judgment.
function Get-EfficiencyJudgment {
    param($Treatment, $Baseline)

    $wins = 0
    $losses = 0
    foreach ($pair in @(
            , @($Treatment.TotalTokens, $Baseline.TotalTokens)
            , @($Treatment.TurnCount, $Baseline.TurnCount)
            , @($Treatment.WallTimeMs, $Baseline.WallTimeMs))) {
        if ($pair[0] -lt $pair[1]) { $wins++ }
        elseif ($pair[0] -gt $pair[1]) { $losses++ }
    }
    if ($wins -gt 0 -and $losses -eq 0) { return "treatment more efficient (better on $wins/3 metrics)" }
    if ($losses -gt 0 -and $wins -eq 0) { return "treatment less efficient (worse on $losses/3 metrics)" }
    if ($wins -eq 0 -and $losses -eq 0) { return 'equivalent efficiency' }
    return "mixed (treatment better on $wins, worse on $losses)"
}

# Picks the console color that reflects an effectiveness category / judgment.
function Get-VerdictColor {
    param([string] $Category, [string] $Judgment)
    switch -Wildcard ($Category) {
        'VALUABLE' { return 'Green' }
        'REGRESSION' { return 'Red' }
        'INCONCLUSIVE' { return 'Red' }
        'BOTH PASS' {
            if ($Judgment -like 'treatment more efficient*' -or $Judgment -like 'equivalent*') { return 'Green' }
            return 'Yellow'
        }
        'PASS*' { return 'Green' }
        'FAIL*' { return 'Red' }
        default { return 'Gray' }
    }
}

# Reports a single experiment iteration's treatment-vs-baseline comparison,
# writing the per-stimulus effectiveness verdicts. Returns an object describing
# whether the iteration should fail the overall run (the experiment failed to
# produce results, a treatment stimulus failed, or an effectiveness REGRESSION
# was detected) along with the treatment/baseline verdicts so callers can
# aggregate outcomes across iterations.
function Write-ExperimentIterationReport {
    param(
        [Parameter(Mandatory)] [string] $Name,
        $ExperimentExit,
        [string] $TreatmentDir,
        [string] $BaselineDir
    )

    $failure = $false
    $treatmentVerdict = 'NO DATA'
    $baselineVerdict = $null

    # Per-stimulus results for each variant from vally's machine-readable output.
    $treatmentStimuli = Get-VallyStimulusResults -RunOutputDir $TreatmentDir
    $baselineStimuli = $BaselineDir ? (Get-VallyStimulusResults -RunOutputDir $BaselineDir) : $null

    # `vally experiment run` can return non-zero when one variant has grader
    # failures (for us, baseline failures are expected). Treat it as a hard run
    # failure only when no usable variant results were produced.
    if ($ExperimentExit -ne 0) {
        if ($treatmentStimuli -or $baselineStimuli) {
            Write-Warn ("{0,-32} vally exit {1} (non-zero due to failed graders in one or more variants; see per-stimulus verdicts below)" -f $Name, $ExperimentExit)
        }
        else {
            Write-Warn ("{0,-32} EXPERIMENT FAILED (vally exit {1}; no variant results found)" -f $Name, $ExperimentExit)
            $failure = $true
        }
    }

    if (-not $treatmentStimuli) {
        Write-Warn ("{0,-32} (no treatment results.jsonl found - skipping effectiveness comparison)" -f $Name)
        return [pscustomobject]@{
            Failure          = $true
            TreatmentVerdict = $treatmentVerdict
            BaselineVerdict  = $baselineVerdict
        }
    }

    # Top-line status: treatment/baseline pass = every stimulus in that variant passed.
    $treatmentPassed = @($treatmentStimuli.Values | Where-Object { -not $_.Passed }).Count -eq 0
    $treatmentVerdict = $treatmentPassed ? 'PASS' : 'FAIL'
    if (-not $treatmentPassed) { $failure = $true }

    if (-not $baselineStimuli) {
        Write-Info ("{0,-32} treatment {1} (no baseline)" -f $Name, $treatmentVerdict)
    }
    else {
        $baselinePassed = @($baselineStimuli.Values | Where-Object { -not $_.Passed }).Count -eq 0
        $baselineVerdict = $baselinePassed ? 'PASS' : 'FAIL'
        Write-Info ("{0,-32} treatment {1} | baseline {2}" -f $Name, $treatmentVerdict, $baselineVerdict)

        if (-not $baselinePassed) {
            $failedBaselineStimuli = @($baselineStimuli.Values | Where-Object { -not $_.Passed } | ForEach-Object { $_.Stimulus })
            if ($failedBaselineStimuli.Count -gt 0) {
                Write-Info ("  baseline failed stimuli: {0}" -f ($failedBaselineStimuli -join ', '))
            }
        }
    }

    if (-not $treatmentPassed) {
        $failedTreatmentRecords = @($treatmentStimuli.Values | Where-Object { -not $_.Passed })
        $failedTreatmentStimuli = @($failedTreatmentRecords | ForEach-Object { $_.Stimulus })
        if ($failedTreatmentStimuli.Count -gt 0) {
            Write-Info ("  treatment failed stimuli: {0}" -f ($failedTreatmentStimuli -join ', '))
            foreach ($failed in $failedTreatmentRecords) {
                $graderPart = ($failed.FailedGraders -and $failed.FailedGraders.Count -gt 0) ? ($failed.FailedGraders -join ', ') : 'unknown grader'
                $evidencePart = $failed.FailureEvidence ? $failed.FailureEvidence : 'no grader evidence was emitted in results.jsonl'
                Write-Info ("    - {0}: {1} - {2}" -f $failed.Stimulus, $graderPart, $evidencePart)
            }
        }
    }

    foreach ($stimulus in $treatmentStimuli.Keys) {
        $t = $treatmentStimuli[$stimulus]
        $b = $baselineStimuli ? $baselineStimuli[$stimulus] : $null
        $category = Get-EffectivenessCategory -Treatment $t -Baseline $b

        $detail = ''
        if ($category -eq 'BOTH PASS') {
            $judgment = Get-EfficiencyJudgment -Treatment $t -Baseline $b
            $detail = " - $judgment; " +
            ("tokens {0} vs {1} ({2}), turns {3} vs {4} ({5}), wall {6} vs {7} ({8})" -f `
                $t.TotalTokens, $b.TotalTokens, (Format-Delta -Treatment $t.TotalTokens -Baseline $b.TotalTokens),
            $t.TurnCount, $b.TurnCount, (Format-Delta -Treatment $t.TurnCount -Baseline $b.TurnCount),
            (Format-Ms $t.WallTimeMs), (Format-Ms $b.WallTimeMs), (Format-Delta -Treatment $t.WallTimeMs -Baseline $b.WallTimeMs -AsSeconds))
            # Both passing means the outcome did not require the server - surface it,
            # but it does not fail the run.
            if ($judgment -like 'treatment less efficient*' -or $judgment -like 'mixed*') {
                $detail += ' [review: server did not improve efficiency]'
            }
        }
        elseif ($category -eq 'VALUABLE') {
            $detail = ' - the Azure MCP server enabled an outcome the baseline could not achieve'
        }
        elseif ($category -eq 'REGRESSION') {
            $detail = ' - baseline succeeded WITHOUT the server but the treatment FAILED'
            $failure = $true
        }
        elseif ($category -eq 'INCONCLUSIVE') {
            $detail = ' - neither treatment nor baseline achieved the outcome'
        }

        $color = Get-VerdictColor -Category $category -Judgment $judgment
        Write-Host ("  - {0,-30} {1}{2}" -f $stimulus, $category, $detail) -ForegroundColor $color
        # Reset $judgment so it does not leak into the next stimulus' coloring.
        $judgment = $null
    }

    return [pscustomobject]@{
        Failure          = $failure
        TreatmentVerdict = $treatmentVerdict
        BaselineVerdict  = $baselineVerdict
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

# Runs each discovered experiment -Iterations time(s), provisioning + tearing down
# per area, and returns the list of per-experiment result descriptors the summary
# consumes. Each descriptor carries Name, ProvisioningFailed, and an Iterations
# list of { Iteration, ExperimentExit, TreatmentDir, BaselineDir }.
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
                    $runRoot = Join-Path (Join-Path $OutputDir $eval.Area) $eval.Tool

                    # Run the experiment -Iterations time(s). Each run produces both
                    # variants (baseline + treatment) under its own timestamped
                    # directory; record every iteration so the summary can report each
                    # one's outcome.
                    $iterationResults = [System.Collections.Generic.List[object]]::new()
                    for ($i = 1; $i -le $Iterations; $i++) {
                        $iterationLabel = ($Iterations -gt 1) ? "$label (iteration $i/$Iterations)" : $label

                        $run = Invoke-VallyExperiment `
                            -Label $iterationLabel `
                            -ExperimentFile $eval.Experiment `
                            -RunOutputDir $runRoot

                        # Each variant's results.jsonl lives in <run>/<variant>/. These
                        # variant names must match the `variants:` keys in the experiment
                        # spec (baseline + treatment).
                        $treatmentDir = $run.RunDir ? (Join-Path $run.RunDir 'treatment') : $null
                        $baselineDir = $run.RunDir ? (Join-Path $run.RunDir 'baseline') : $null

                        $iterationResults.Add([pscustomobject]@{
                                Iteration      = $i
                                ExperimentExit = $run.Exit
                                TreatmentDir   = $treatmentDir
                                BaselineDir    = $baselineDir
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
# previous run already saved under -OutputDir, WITHOUT building, provisioning, or
# invoking vally. For each discovered experiment it takes the newest -Iterations
# timestamped run directory(ies) under <OutputDir>/<area>/<tool>/ and points at
# their treatment/baseline subfolders. The vally exit code is not persisted in the
# artifacts, so ExperimentExit is reported as 0 and the summary relies on the
# per-stimulus verdicts recorded in each run's results.jsonl.
function Get-ExperimentResultsFromArtifacts {
    param([Parameter(Mandatory)] $Evals)

    $results = [System.Collections.Generic.List[object]]::new()

    foreach ($eval in $Evals) {
        $label = "$($eval.Area)/$($eval.Tool)"
        $runRoot = Join-Path (Join-Path $OutputDir $eval.Area) $eval.Tool

        if (-not (Test-Path -LiteralPath $runRoot)) {
            Write-Warn "[$label] No artifacts under '$runRoot' (nothing to report)."
            continue
        }

        # vally names each run directory with a sortable UTC timestamp (e.g.
        # 2026-07-09T00-05-49-540Z). Match only those so stray non-run folders
        # (e.g. a hand-run 'treatment'/'baseline') are ignored, then take the newest
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
        foreach ($runDir in $runDirs) {
            $i++
            $iterationResults.Add([pscustomobject]@{
                    Iteration      = $i
                    ExperimentExit = 0
                    TreatmentDir   = (Join-Path $runDir.FullName 'treatment')
                    BaselineDir    = (Join-Path $runDir.FullName 'baseline')
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
# Azure MCP server's effectiveness, per stimulus, by comparing the two experiment
# variants - treatment (WITH the server) against baseline (WITHOUT it):
#
#   * baseline FAIL + treatment PASS -> VALUABLE      (the server enabled the outcome)
#   * baseline PASS + treatment FAIL -> REGRESSION    (the server hurt the outcome)
#   * both PASS                      -> the tool was not required; efficiency decides,
#                                        using tokens / turns / wall time (lower is better)
#   * neither PASS                   -> INCONCLUSIVE
#
# A result fails the run only when the experiment itself failed to execute, a
# treatment stimulus failed, or an effectiveness REGRESSION is detected. Baseline
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
        $treatmentPassCount = 0

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
                -TreatmentDir $iteration.TreatmentDir `
                -BaselineDir $iteration.BaselineDir

            if ($outcome.Failure) { $anyFailure = $true }
            if ($outcome.TreatmentVerdict -eq 'PASS') { $treatmentPassCount++ }
        }

        # Aggregate the treatment outcomes across iterations so flaky results (some
        # iterations pass, some fail) are visible at a glance.
        if ($iterationCount -gt 1) {
            $aggregateColor = ($treatmentPassCount -eq $iterationCount) ? 'Green' : (($treatmentPassCount -eq 0) ? 'Red' : 'Yellow')
            Write-Host ("{0,-32} treatment passed {1}/{2} iteration(s)" -f $r.Name, $treatmentPassCount, $iterationCount) -ForegroundColor $aggregateColor
        }
    }
    Write-Info "Artifacts under: $OutputDir"

    return $anyFailure
}

# --- Discover the experiments to run -----------------------------------------
# An experiment (<tool>.experiment.yaml) runs a shared eval spec as a baseline
# variant (no Azure MCP server) and a treatment variant (with it). Experiments
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
    Write-Info ("Report-only: reading the newest {0} run(s) per experiment from $OutputDir." -f $Iterations)
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
        throw "No run artifacts found under '$OutputDir' for the requested experiments. Run without -ReportOnly first, or pass -ReportFrom <dir>."
    }
}
else {
    $results = Get-ExperimentResultsFromRuns -Evals $evals
}

# --- Report the comparison ---------------------------------------------------
# The goal is to measure the Azure MCP server's effectiveness, per stimulus, by
# comparing the two experiment variants - treatment (WITH the server) against
# baseline (WITHOUT it):
#
#   * baseline FAIL + treatment PASS -> VALUABLE      (the server enabled the outcome)
#   * baseline PASS + treatment FAIL -> REGRESSION    (the server hurt the outcome)
#   * both PASS                      -> the tool was not required; efficiency decides,
#                                        using tokens / turns / wall time (lower is better)
#   * neither PASS                   -> INCONCLUSIVE
#
# A run is only considered failed (non-zero exit) when the experiment itself
# failed to execute, a treatment stimulus failed, or an effectiveness REGRESSION
# is detected. Baseline outcome failures are expected and do not fail the run.
# Returns $true if any such failure was detected across all experiments.
$anyFailure = Write-ResultsSummary -Results $results

# Non-zero if any experiment failed to run, any treatment stimulus failed, any
# area's provisioning failed, or an effectiveness regression (baseline PASS while
# treatment FAIL) was detected; baseline outcome failures on their own are
# expected and do not affect the exit code.
exit ($anyFailure ? 1 : 0)
