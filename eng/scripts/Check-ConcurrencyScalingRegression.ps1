#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Gates the concurrency / throughput / scalability results against a saved baseline (#3122).

.DESCRIPTION
    Reads the results JSON produced by Measure-ConcurrencyScaling.ps1 and a baseline JSON of the
    same shape, then applies the tiered budgets from issue #3122 per concurrency level:
      * throughput may not decrease by more than ThroughputDropPct (default 10%)
      * concurrency scaling efficiency may not degrade by more than ScalingDropPct (default 15%)
      * p95 latency may not degrade by more than LatencyP95RisePct (default 15%)
      * p99 latency may not regress by more than LatencyP99RisePct (default 20%)
      * timeout/server-error rate may not increase by more than ErrorRatePpLimit (default 0.5 points)
      * any concurrent tool-count mismatch is a hard failure (state leakage / thread safety)
    Fails (throws) if any budget is exceeded. Does not re-run any measurements.

.PARAMETER ResultsPath
    Path to the results JSON written by Measure-ConcurrencyScaling.ps1.

.PARAMETER BaselinePath
    Path to the baseline JSON to compare against.
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [Parameter(Mandatory)][string] $BaselinePath,
    [double] $ThroughputDropPct = 0.10,
    [double] $ScalingDropPct    = 0.15,
    [double] $LatencyP95RisePct = 0.15,
    [double] $LatencyP99RisePct = 0.20,
    [double] $ErrorRatePpLimit  = 0.5,
    [double] $WarnPct           = 0.05
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'ConcurrencyPerformance.Common.ps1')

$results  = Get-Content $ResultsPath | ConvertFrom-Json
$baseline = Get-Content $BaselinePath | ConvertFrom-Json

$gate = Invoke-ConcurrencyRegressionGate -ResultLevels $results.levels -BaselineLevels $baseline.levels `
    -ThroughputDropPct $ThroughputDropPct -ScalingDropPct $ScalingDropPct `
    -LatencyP95RisePct $LatencyP95RisePct -LatencyP99RisePct $LatencyP99RisePct `
    -ErrorRatePpLimit $ErrorRatePpLimit -WarnPct $WarnPct

if ($gate.Warnings.Count -gt 0) {
    Write-Warning "Concurrency warnings: $($gate.Warnings -join ', ')"
}

if ($gate.Failures.Count -gt 0) {
    Write-Error "Concurrency regression detected in: $($gate.Failures -join ', ')"
}
else {
    Write-Host "  All concurrency budget checks passed."
}
