#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Gates the startup performance results against a saved baseline.

.DESCRIPTION
    Reads the results JSON produced by Test-StartupPerformance.ps1 and a baseline JSON
    of the same shape, then fails (throws) if any tracked median exceeds the baseline
    median multiplied by Threshold. Does not re-run any benchmarks.

.PARAMETER ResultsPath
    Path to the results JSON written by Test-StartupPerformance.ps1.

.PARAMETER BaselinePath
    Path to the baseline JSON to compare against.

.PARAMETER Threshold
    Maximum allowed regression ratio (default 1.20 = 20% slower than baseline).
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [Parameter(Mandatory)][string] $BaselinePath,
    [double] $Threshold = 1.20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$results = Get-Content $ResultsPath | ConvertFrom-Json
$baseline = Get-Content $BaselinePath | ConvertFrom-Json
$s = $results.scenarios
$b = $baseline.scenarios

$failures = @()
$checks = @(
    @{ Name = 'cli_cold_start_ms (median)';   Current = $s.cli_cold_start_ms.median;             Baseline = $b.cli_cold_start_ms.median }
    @{ Name = 'mcp_stdio_default (median)';    Current = $s.mcp_stdio_to_tools_list_ms.median;    Baseline = $b.mcp_stdio_to_tools_list_ms.median }
    @{ Name = 'mcp_namespace_mode (median)';   Current = $s.mcp_namespace_mode_startup_ms.median; Baseline = $b.mcp_namespace_mode_startup_ms.median }
    @{ Name = 'mcp_all_mode (median)';         Current = $s.mcp_all_mode_startup_ms.median;       Baseline = $b.mcp_all_mode_startup_ms.median }
)

foreach ($c in $checks) {
    $limit = [math]::Round($c.Baseline * $Threshold)
    $status = if ($c.Current -le $limit) { 'PASS' } else { $failures += $c.Name; 'FAIL' }
    Write-Host ("  [{0}] {1}: current={2}ms  baseline={3}ms  limit={4}ms" -f $status, $c.Name, $c.Current, $c.Baseline, $limit)
}

if ($failures.Count -gt 0) {
    Write-Error "Regression detected in: $($failures -join ', ')"
}
else {
    Write-Host "  All checks passed."
}
