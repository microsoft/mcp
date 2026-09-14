#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Side-effect-free helpers for the resource-usage baseline gate (issue #3121).

.DESCRIPTION
    Dot-source this file to reuse the pure functions in Measure-ResourceUsage.ps1 and
    Check-ResourceUsageRegression.ps1 and to unit-test them without launching a server.
    Reuses the generic accessor Get-MemberValue from StartupPerformance.Common.ps1.
#>

. (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')

# ---------------------------------------------------------------------------
# Returns the member names of a hashtable/ordered dictionary or a PSCustomObject
# (as produced by ConvertFrom-Json), so callers can enumerate scenario maps
# regardless of how the results JSON was materialized.
# ---------------------------------------------------------------------------
function Get-MemberNames {
    [CmdletBinding()]
    param($Object)

    if ($null -eq $Object) { return @() }
    if ($Object -is [System.Collections.IDictionary]) { return @($Object.Keys) }
    return @($Object.PSObject.Properties.Name)
}

# ---------------------------------------------------------------------------
# Tiered resource-regression gate shared by Measure-ResourceUsage.ps1 and
# Check-ResourceUsageRegression.ps1. For each server-mode scenario present in
# both the results and the baseline it applies the acceptance-criteria budgets
# from issue #3121:
#   * steady-state CPU and memory may not increase by more than 10% (FailSteady = 1.10)
#   * peak memory may not increase by more than 15%                 (FailPeak   = 1.15)
#   * any change above 5% is reported as a warning                  (Warn       = 1.05)
#   * a soak phase flagged with unbounded_growth is a hard failure  (memory/handle leak)
#
# Returns [ordered]@{ Failures = @(...); Warnings = @(...) } and writes a
# human-readable line per check via Write-Host.
# ---------------------------------------------------------------------------
function Invoke-ResourceRegressionGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $ResultScenarios,
        [Parameter(Mandatory)] $BaselineScenarios,
        [double] $FailSteady = 1.10,
        [double] $FailPeak   = 1.15,
        [double] $Warn       = 1.05
    )

    $failures = @()
    $warnings = @()

    foreach ($mode in (Get-MemberNames $ResultScenarios)) {
        $r = Get-MemberValue $ResultScenarios $mode
        $b = Get-MemberValue $BaselineScenarios $mode
        if ($null -eq $b) {
            Write-Host ("  [SKIP] {0}: absent in baseline" -f $mode)
            continue
        }

        $rSteady = Get-MemberValue (Get-MemberValue $r 'phases') 'steady_state'
        $bSteady = Get-MemberValue (Get-MemberValue $b 'phases') 'steady_state'

        $checks = @(
            @{ Label = 'steady_working_set_mb'; Unit = 'MB'
               Cur = Get-MemberValue (Get-MemberValue $rSteady 'working_set_mb') 'mean'
               Base = Get-MemberValue (Get-MemberValue $bSteady 'working_set_mb') 'mean'
               Limit = $FailSteady }
            @{ Label = 'steady_cpu_percent'; Unit = '%'
               Cur = Get-MemberValue $rSteady 'cpu_percent'
               Base = Get-MemberValue $bSteady 'cpu_percent'
               Limit = $FailSteady }
            @{ Label = 'peak_working_set_mb'; Unit = 'MB'
               Cur = Get-MemberValue (Get-MemberValue $r 'peak') 'working_set_mb'
               Base = Get-MemberValue (Get-MemberValue $b 'peak') 'working_set_mb'
               Limit = $FailPeak }
        )

        foreach ($c in $checks) {
            if ($null -eq $c.Cur -or $null -eq $c.Base) {
                Write-Host ("  [SKIP] {0} {1}: value absent" -f $mode, $c.Label)
                continue
            }

            $ratio = if ($c.Base -gt 0) { $c.Cur / $c.Base } else { 1 }
            $label = "$mode $($c.Label)"
            if ($ratio -gt $c.Limit) {
                $failures += $label
                $status = 'FAIL'
            }
            elseif ($ratio -gt $Warn) {
                $warnings += $label
                $status = 'WARN'
            }
            else {
                $status = 'PASS'
            }
            Write-Host ("  [{0}] {1}: current={2}{3}  baseline={4}{3}  (+{5}%)" -f `
                $status, $label, $c.Cur, $c.Unit, $c.Base, [math]::Round(($ratio - 1) * 100, 1))
        }

        # Leak detection: a soak phase flagged unbounded is a hard failure regardless of baseline.
        $soak = Get-MemberValue $r 'soak'
        if ($null -ne $soak) {
            $unbounded = Get-MemberValue $soak 'unbounded_growth'
            if ($unbounded) {
                $failures += "$mode soak_unbounded_growth"
                Write-Host ("  [FAIL] {0} soak_unbounded_growth: working_set_slope={1} MB/min (R2={2}), handle_slope={3}/min (R2={4})" -f `
                    $mode, (Get-MemberValue $soak 'working_set_slope_mb_per_min'), (Get-MemberValue $soak 'working_set_r2'), `
                    (Get-MemberValue $soak 'handle_slope_per_min'), (Get-MemberValue $soak 'handle_r2'))
            }
            else {
                Write-Host ("  [PASS] {0} soak: no unbounded growth" -f $mode)
            }
        }
    }

    return [ordered]@{ Failures = $failures; Warnings = $warnings }
}
