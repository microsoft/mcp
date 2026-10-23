#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Gates the resource-usage results against a saved baseline (issue #3121).

.DESCRIPTION
    Reads the results JSON produced by Measure-ResourceUsage.ps1 and a baseline JSON of the
    same shape, then applies the tiered resource budgets from issue #3121:
      * steady-state CPU and memory may not regress by more than FailSteady (default +10%)
      * peak memory may not regress by more than FailPeak (default +15%)
      * a soak phase flagged with unbounded growth is a hard failure (memory/handle leak)
      * any change above Warn (default +5%) is reported as a warning
    Fails (throws) if any budget is exceeded. Does not re-run any measurements.

.PARAMETER ResultsPath
    Path to the results JSON written by Measure-ResourceUsage.ps1.

.PARAMETER BaselinePath
    Path to the baseline JSON to compare against.

.PARAMETER FailSteady
    Maximum allowed steady-state CPU/memory regression ratio (default 1.10 = +10%).

.PARAMETER FailPeak
    Maximum allowed peak-memory regression ratio (default 1.15 = +15%).

.PARAMETER Warn
    Ratio above which a change is reported as a warning (default 1.05 = +5%).
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [Parameter(Mandatory)][string] $BaselinePath,
    [double] $FailSteady = 1.10,
    [double] $FailPeak   = 1.15,
    [double] $Warn       = 1.05
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ResourcePerformance.Common.ps1')

$results  = Get-Content $ResultsPath | ConvertFrom-Json
$baseline = Get-Content $BaselinePath | ConvertFrom-Json

$gate = Invoke-ResourceRegressionGate -ResultScenarios $results.scenarios `
                                      -BaselineScenarios $baseline.scenarios `
                                      -FailSteady $FailSteady -FailPeak $FailPeak -Warn $Warn

if ($gate.Warnings.Count -gt 0) {
    Write-Warning "Resource warnings (>$([math]::Round(($Warn - 1) * 100))%): $($gate.Warnings -join ', ')"
}

if ($gate.Failures.Count -gt 0) {
    Write-Error "Resource regression detected in: $($gate.Failures -join ', ')"
}
else {
    Write-Host "  All resource budget checks passed."
}
