#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Gates the command-dispatch / transport overhead results against a saved baseline (#3119).

.DESCRIPTION
    Reads the results JSON produced by Measure-DispatchOverhead.ps1 and a baseline JSON of the
    same shape, then applies the tiered budgets from issue #3119:
      * p50 and p95 of each transport/operation may not regress by more than FailP50P95 (+10%)
      * p99 may not regress by more than FailP99 (+20%)
      * any change above Warn (+5%), including transport overhead, is reported as a warning
    Fails (throws) if any budget is exceeded. Does not re-run any measurements.

.PARAMETER ResultsPath
    Path to the results JSON written by Measure-DispatchOverhead.ps1.

.PARAMETER BaselinePath
    Path to the baseline JSON to compare against.

.PARAMETER FailP50P95
    Maximum allowed p50/p95 regression ratio (default 1.10 = +10%).

.PARAMETER FailP99
    Maximum allowed p99 regression ratio (default 1.20 = +20%).

.PARAMETER Warn
    Ratio above which a change is reported as a warning (default 1.05 = +5%).
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [Parameter(Mandatory)][string] $BaselinePath,
    [double] $FailP50P95 = 1.10,
    [double] $FailP99    = 1.20,
    [double] $Warn       = 1.05
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'DispatchPerformance.Common.ps1')

$results  = Get-Content $ResultsPath | ConvertFrom-Json
$baseline = Get-Content $BaselinePath | ConvertFrom-Json

$gate = Invoke-DispatchRegressionGate -ResultScenarios $results.scenarios `
                                      -BaselineScenarios $baseline.scenarios `
                                      -FailP50P95 $FailP50P95 -FailP99 $FailP99 -Warn $Warn

if ($gate.Warnings.Count -gt 0) {
    Write-Warning "Dispatch warnings (>$([math]::Round(($Warn - 1) * 100))%): $($gate.Warnings -join ', ')"
}

if ($gate.Failures.Count -gt 0) {
    Write-Error "Dispatch regression detected in: $($gate.Failures -join ', ')"
}
else {
    Write-Host "  All dispatch budget checks passed."
}
