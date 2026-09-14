#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Side-effect-free helpers for the concurrency / throughput / scalability gate (issue #3122).

.DESCRIPTION
    Dot-source this file to reuse the pure functions in Measure-ConcurrencyScaling.ps1 and
    Check-ConcurrencyScalingRegression.ps1 and to unit-test them without launching a server.
    Reuses Get-MemberValue from StartupPerformance.Common.ps1.
#>

. (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')

# ---------------------------------------------------------------------------
# Evaluates one budget. Direction 'higher' means bigger is better (throughput,
# scaling efficiency) and reports the fraction dropped; 'lower' means smaller is
# better (latency) and reports the fraction risen. Returns a status + change.
# ---------------------------------------------------------------------------
function Test-ConcurrencyBudget {
    [CmdletBinding()]
    param(
        [double] $Current,
        [double] $Baseline,
        [double] $FailFraction,
        [double] $WarnFraction,
        [ValidateSet('higher', 'lower')] [string] $Direction
    )

    if ($Baseline -le 0) {
        return @{ Status = 'SKIP'; Change = 0 }
    }

    $change = if ($Direction -eq 'higher') { 1 - ($Current / $Baseline) } else { ($Current / $Baseline) - 1 }
    $status = if ($change -gt $FailFraction) { 'FAIL' } elseif ($change -gt $WarnFraction) { 'WARN' } else { 'PASS' }
    return @{ Status = $status; Change = $change }
}

# ---------------------------------------------------------------------------
# Tiered concurrency-regression gate shared by Measure-ConcurrencyScaling.ps1 and
# Check-ConcurrencyScalingRegression.ps1. For each concurrency level present in both
# results and baseline it applies the acceptance-criteria budgets from issue #3122:
#   * throughput may not decrease by more than ThroughputDropPct (default 10%)
#   * concurrency scaling efficiency may not degrade by more than ScalingDropPct (default 15%)
#   * p95 latency may not degrade by more than LatencyP95RisePct (default 15%)
#   * p99 latency may not regress by more than LatencyP99RisePct (default 20%)
#   * timeout/server-error rate may not increase by more than ErrorRatePpLimit (default 0.5 points)
#   * any concurrent tool-count mismatch is a hard failure (state leakage / thread safety)
#   * any change above WarnPct (default 5%) is reported as a warning
#
# Returns [ordered]@{ Failures = @(...); Warnings = @(...) } and writes a line per check.
# ---------------------------------------------------------------------------
function Invoke-ConcurrencyRegressionGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $ResultLevels,
        [Parameter(Mandatory)] $BaselineLevels,
        [double] $ThroughputDropPct  = 0.10,
        [double] $ScalingDropPct     = 0.15,
        [double] $LatencyP95RisePct  = 0.15,
        [double] $LatencyP99RisePct  = 0.20,
        [double] $ErrorRatePpLimit   = 0.5,
        [double] $WarnPct            = 0.05
    )

    $failures = @()
    $warnings = @()
    $evaluated = 0

    $baselineByConcurrency = @{}
    foreach ($bl in $BaselineLevels) {
        $baselineByConcurrency[[string](Get-MemberValue $bl 'concurrency')] = $bl
    }

    $seenConcurrency = @{}

    foreach ($level in $ResultLevels) {
        $concurrency = Get-MemberValue $level 'concurrency'
        $seenConcurrency[[string]$concurrency] = $true

        # State leakage / thread safety — a hard failure regardless of the baseline, so it is
        # evaluated before the baseline-presence check below can skip this level.
        $mismatches = Get-MemberValue $level 'tool_count_mismatches'
        if ($mismatches -and $mismatches -gt 0) {
            $failures += "c$concurrency state_leakage"
            Write-Host ("  [FAIL] c{0} tool_count_mismatches={1} (state leakage / thread-safety failure)" -f $concurrency, $mismatches)
        }

        $baseline = $baselineByConcurrency[[string]$concurrency]
        if ($null -eq $baseline) {
            Write-Host ("  [SKIP] concurrency={0}: absent in baseline (relative budgets)" -f $concurrency)
            continue
        }

        $curLatency  = Get-MemberValue $level 'latency_ms'
        $baseLatency = Get-MemberValue $baseline 'latency_ms'

        $checks = @(
            @{ Label = 'throughput_rps';     Cur = (Get-MemberValue $level 'throughput_rps');     Base = (Get-MemberValue $baseline 'throughput_rps');     Fail = $ThroughputDropPct; Dir = 'higher'; Unit = ' rps' }
            @{ Label = 'scaling_efficiency'; Cur = (Get-MemberValue $level 'scaling_efficiency'); Base = (Get-MemberValue $baseline 'scaling_efficiency'); Fail = $ScalingDropPct;    Dir = 'higher'; Unit = '' }
            @{ Label = 'latency_p95';        Cur = (Get-MemberValue $curLatency 'p95');           Base = (Get-MemberValue $baseLatency 'p95');            Fail = $LatencyP95RisePct; Dir = 'lower';  Unit = ' ms' }
            @{ Label = 'latency_p99';        Cur = (Get-MemberValue $curLatency 'p99');           Base = (Get-MemberValue $baseLatency 'p99');            Fail = $LatencyP99RisePct; Dir = 'lower';  Unit = ' ms' }
        )

        foreach ($check in $checks) {
            if ($null -eq $check.Base) {
                continue
            }
            if ($null -eq $check.Cur) {
                $failures += "c$concurrency $($check.Label) (missing in results)"
                Write-Host ("  [FAIL] c{0} {1}: expected by baseline but absent in results" -f $concurrency, $check.Label)
                continue
            }
            $evaluated++
            $result = Test-ConcurrencyBudget -Current $check.Cur -Baseline $check.Base `
                -FailFraction $check.Fail -WarnFraction $WarnPct -Direction $check.Dir
            $label = "c$concurrency $($check.Label)"
            switch ($result.Status) {
                'FAIL' { $failures += $label }
                'WARN' { $warnings += $label }
            }
            Write-Host ("  [{0}] {1}: current={2}{3}  baseline={4}{3}  ({5}{6}%)" -f `
                $result.Status, $label, $check.Cur, $check.Unit, $check.Base, `
                $(if ($result.Change -ge 0) { '+' } else { '' }), [math]::Round($result.Change * 100, 1))
        }

        # Error/timeout rate — an absolute percentage-point budget.
        $curError  = Get-MemberValue $level 'error_rate_pct'
        $baseError = Get-MemberValue $baseline 'error_rate_pct'
        if ($null -ne $baseError) {
            $label = "c$concurrency error_rate_pct"
            if ($null -eq $curError) {
                $failures += "$label (missing in results)"
                Write-Host ("  [FAIL] {0}: expected by baseline but absent in results" -f $label)
            }
            else {
                $evaluated++
                $delta = $curError - $baseError
                if ($delta -gt $ErrorRatePpLimit) {
                    $failures += $label
                    $status = 'FAIL'
                }
                else {
                    $status = 'PASS'
                }
                Write-Host ("  [{0}] {1}: current={2}%  baseline={3}%  (+{4} pp)" -f `
                    $status, $label, $curError, $baseError, [math]::Round($delta, 3))
            }
        }
    }

    # Baseline levels the results no longer cover are treated as an expected-but-missing metric.
    foreach ($blKey in $baselineByConcurrency.Keys) {
        if (-not $seenConcurrency.ContainsKey($blKey)) {
            $failures += "c$blKey missing_in_results"
            Write-Host ("  [FAIL] concurrency={0}: expected by baseline but absent in results" -f $blKey)
        }
    }

    if ($evaluated -eq 0) {
        $failures += 'no_comparable_metrics'
        Write-Host "  [FAIL] no comparable concurrency metrics between results and baseline (incompatible or empty baseline)"
    }

    return [ordered]@{ Failures = $failures; Warnings = $warnings }
}
