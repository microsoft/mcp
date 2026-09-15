#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Gates the startup performance results against a saved baseline.

.DESCRIPTION
    Reads the results JSON produced by Test-StartupPerformance.ps1 and a baseline JSON
    of the same shape, then applies the tiered budgets from issue #3118:
      * p50 and p95 may not regress by more than FailP50P95 (default +10%)
      * p99 may not regress by more than FailP99 (default +20%)
      * discovery scaling efficiency may not degrade by more than ScalingDegrade (+15%)
      * any change above Warn (default +5%) is reported as a warning
    Fails (throws) if any budget is exceeded. Does not re-run any benchmarks.

.PARAMETER ResultsPath
    Path to the results JSON written by Test-StartupPerformance.ps1.

.PARAMETER BaselinePath
    Path to the baseline JSON to compare against.

.PARAMETER FailP50P95
    Maximum allowed p50/p95 regression ratio (default 1.10 = +10%).

.PARAMETER FailP99
    Maximum allowed p99 regression ratio (default 1.20 = +20%).

.PARAMETER ScalingDegrade
    Maximum allowed discovery-scaling-efficiency degradation ratio (default 1.15 = +15%).

.PARAMETER Warn
    Ratio above which a change is reported as a warning (default 1.05 = +5%).
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [Parameter(Mandatory)][string] $BaselinePath,
    [double] $FailP50P95     = 1.10,
    [double] $FailP99        = 1.20,
    [double] $ScalingDegrade = 1.15,
    [double] $Warn           = 1.05
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')

$results  = Get-Content $ResultsPath | ConvertFrom-Json
$baseline = Get-Content $BaselinePath | ConvertFrom-Json

$gate = Invoke-StartupRegressionGate -ResultScenarios $results.scenarios `
                                     -BaselineScenarios $baseline.scenarios `
                                     -FailP50P95 $FailP50P95 -FailP99 $FailP99 `
                                     -ScalingDegrade $ScalingDegrade -Warn $Warn

if ($gate.Warnings.Count -gt 0) {
    Write-Warning "Performance warnings (>$([math]::Round(($Warn - 1) * 100))%): $($gate.Warnings -join ', ')"
}

if ($gate.Failures.Count -gt 0) {
    Write-Error "Regression detected in: $($gate.Failures -join ', ')"
}
else {
    Write-Host "  All budget checks passed."
}
