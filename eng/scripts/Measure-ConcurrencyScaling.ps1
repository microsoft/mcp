#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Measures Azure MCP Server throughput, latency, and scaling under a sweep of concurrency
    levels (issue #3122).

.DESCRIPTION
    Runs the performance-benchmark harness in --mcp-concurrency mode, which launches the server
    over HTTP and, at each concurrency level, drives a pool of parallel MCP clients issuing
    tools/list for a fixed window. For each level it records throughput, latency p50/p95/p99,
    error/timeout rate, concurrency scaling efficiency, and a state-isolation signal (responses
    whose tool count does not match the expected count). When a baseline is supplied the results
    are gated against the tiered budgets from issue #3122 (throughput within 10%, p95 & scaling
    within 15%, p99 within 20%, error-rate increase within 0.5 points, no state leakage).

.PARAMETER Executable
    Path to the azmcp server executable. Defaults to the Release build.

.PARAMETER BenchmarkExe
    Path to the performance-benchmark executable. Defaults to the Release build.

.PARAMETER Levels
    Concurrency levels to sweep.

.PARAMETER DurationSeconds
    Timed load window per level.

.PARAMETER WarmupSeconds
    Warmup window per level (not counted).

.PARAMETER TimeoutMs
    Per-request timeout; a timeout counts as an error.

.PARAMETER OutputPath
    Path to write the JSON results. Defaults to .perf-results/concurrency-scaling.json.

.PARAMETER BaselinePath
    Optional baseline JSON (previously produced by this script) to gate against.

.EXAMPLE
    ./eng/scripts/Measure-ConcurrencyScaling.ps1 -Levels 1,2,4,8,16 -DurationSeconds 10
#>

param(
    [string] $Executable = './servers/Azure.Mcp.Server/src/bin/Release/net10.0/azmcp.exe',
    [string] $BenchmarkExe = './servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks/bin/Release/net10.0/Azure.Mcp.Server.PerformanceBenchmarks.exe',
    [int[]]  $Levels = @(1, 2, 4, 8, 16),
    [int]    $DurationSeconds = 5,
    [int]    $WarmupSeconds = 1,
    [int]    $TimeoutMs = 10000,
    [string] $OutputPath = '.perf-results/concurrency-scaling.json',
    [string] $BaselinePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot
$RepoRoot = (Get-Item (Join-Path $ScriptDir '../..')).FullName
. (Join-Path $ScriptDir 'ConcurrencyPerformance.Common.ps1')

foreach ($path in @($Executable, $BenchmarkExe)) {
    if (-not (Test-Path $path)) {
        throw "Required executable not found: $path  (build the solution in Release first)."
    }
}
$Executable = (Resolve-Path $Executable).Path
$BenchmarkExe = (Resolve-Path $BenchmarkExe).Path
$OutputPath = Resolve-PerfOutputPath -Path $OutputPath

$env:PERF_CONC_LEVELS = ($Levels -join ',')
$env:PERF_CONC_DURATION_SECONDS = $DurationSeconds
$env:PERF_CONC_WARMUP_SECONDS = $WarmupSeconds
$env:PERF_CONC_TIMEOUT_MS = $TimeoutMs

Write-Host "=== Concurrency scaling: levels [$($Levels -join ', ')], ${DurationSeconds}s per level ==="

# The harness sweeps every level in a single invocation; size the timeout to the whole sweep
# (plus headroom) so a hung run fails fast rather than consuming the entire job.
$concurrencyTimeoutSeconds = 180 + $Levels.Count * ($DurationSeconds + $WarmupSeconds) * 3
$data = Invoke-PerfHarnessJson -FilePath $BenchmarkExe `
            -ArgumentList @('--mcp-concurrency', $Executable, 'server', 'start') -TimeoutSeconds $concurrencyTimeoutSeconds

foreach ($level in $data.levels) {
    Write-Host ("  c{0,-3} throughput={1,8} rps  p50={2,7} ms  p95={3,7} ms  p99={4,7} ms  err={5}%  scaling={6}  mismatch={7}" -f `
        $level.concurrency, $level.throughput_rps, $level.latency_ms.p50, $level.latency_ms.p95, `
        $level.latency_ms.p99, $level.error_rate_pct, $level.scaling_efficiency, $level.tool_count_mismatches)
}
Write-Host ""

$commit = try { (& git -C $RepoRoot rev-parse --short HEAD 2>$null).Trim() } catch { '' }

$results = [ordered]@{
    timestamp           = (Get-Date).ToUniversalTime().ToString('o')
    commit              = $commit
    environment         = (Get-PerfRunMetadata -RepoRoot $RepoRoot)
    duration_seconds    = $DurationSeconds
    expected_tool_count = $data.expected_tool_count
    levels              = $data.levels
}

$results | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutputPath -Encoding utf8 -Force
Write-Host "Results written to: $OutputPath"

# ---------------------------------------------------------------------------
# Optional: regression check against a baseline file
# ---------------------------------------------------------------------------
if ($BaselinePath) {
    if (-not (Test-Path $BaselinePath)) {
        Write-Warning "Baseline file not found: $BaselinePath  (skipping regression check)"
    }
    else {
        Write-Host ""
        Write-Host "=== Concurrency regression check (throughput +10%, p95/scaling +15%, p99 +20%, error +0.5pp) ==="

        $baseline = Get-Content $BaselinePath | ConvertFrom-Json
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $results.levels -BaselineLevels $baseline.levels

        if ($gate.Warnings.Count -gt 0) {
            Write-Warning "Concurrency warnings (>5%): $($gate.Warnings -join ', ')"
        }

        if ($gate.Failures.Count -gt 0) {
            Write-Error "Concurrency regression detected in: $($gate.Failures -join ', ')"
        }
        else {
            Write-Host "  All concurrency budget checks passed."
        }
    }
}

# ---------------------------------------------------------------------------
# Emit Azure DevOps pipeline variables (no-op outside AzDO)
# ---------------------------------------------------------------------------
$peak = $data.levels | Sort-Object throughput_rps -Descending | Select-Object -First 1
if ($peak) {
    Write-Host "##vso[task.setvariable variable=ConcurrencyPeakThroughputRps]$($peak.throughput_rps)"
    Write-Host "##vso[task.setvariable variable=ConcurrencyPeakAtLevel]$($peak.concurrency)"
}
$maxLevel = $data.levels | Sort-Object concurrency -Descending | Select-Object -First 1
if ($maxLevel) {
    Write-Host "##vso[task.setvariable variable=ConcurrencyMaxLevelScalingEfficiency]$($maxLevel.scaling_efficiency)"
}
