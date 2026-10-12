#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Measures the Azure MCP Server's CPU, memory, and handle baselines across server modes
    and concurrency levels (issue #3121).

.DESCRIPTION
    For each requested server mode this script launches the performance-benchmark harness
    in --mcp-resource mode, which starts the server over HTTP and samples process resource
    counters across the startup, idle, discovery, steady-state, and optional soak phases.
    Results for every mode are collected into a single JSON document and, when a baseline is
    supplied, gated against the tiered resource budgets from issue #3121 (steady-state CPU
    and memory within 10%, peak memory within 15%, no unbounded soak growth).

.PARAMETER Executable
    Path to the azmcp server executable. Defaults to the Release build.

.PARAMETER BenchmarkExe
    Path to the performance-benchmark executable. Defaults to the Release build.

.PARAMETER Modes
    Server modes to measure. Valid values: default, namespace, all.

.PARAMETER Concurrency
    Number of concurrent load clients driving steady-state and soak load.

.PARAMETER IdleSeconds
    Idle observation window (no client connected) in seconds.

.PARAMETER SteadySeconds
    Steady-state load duration in seconds.

.PARAMETER SoakSeconds
    Sustained soak duration in seconds. 0 disables the soak/leak-detection phase.

.PARAMETER SampleMs
    Resource sampling interval in milliseconds.

.PARAMETER OutputPath
    Path to write the JSON results. Defaults to .perf-results/resource-usage.json.

.PARAMETER BaselinePath
    Optional baseline JSON (previously produced by this script) to gate against.

.EXAMPLE
    ./eng/scripts/Measure-ResourceUsage.ps1

.EXAMPLE
    ./eng/scripts/Measure-ResourceUsage.ps1 -Modes default,all -Concurrency 4 -SoakSeconds 120
#>

param(
    [string]   $Executable = './servers/Azure.Mcp.Server/src/bin/Release/net10.0/azmcp.exe',
    [string]   $BenchmarkExe = './servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks/bin/Release/net10.0/Azure.Mcp.Server.PerformanceBenchmarks.exe',
    [ValidateSet('default', 'namespace', 'all')]
    [string[]] $Modes = @('default', 'all'),
    [int]      $Concurrency = 1,
    [int]      $IdleSeconds = 3,
    [int]      $SteadySeconds = 10,
    [int]      $SoakSeconds = 0,
    [int]      $SampleMs = 100,
    [string]   $OutputPath = '.perf-results/resource-usage.json',
    [string]   $BaselinePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot
$RepoRoot = (Get-Item (Join-Path $ScriptDir '../..')).FullName
. (Join-Path $ScriptDir 'ResourcePerformance.Common.ps1')

foreach ($path in @($Executable, $BenchmarkExe)) {
    if (-not (Test-Path $path)) {
        throw "Required executable not found: $path  (build the solution in Release first)."
    }
}
$Executable = (Resolve-Path $Executable).Path
$BenchmarkExe = (Resolve-Path $BenchmarkExe).Path
$OutputPath = Resolve-PerfOutputPath -Path $OutputPath

function Get-ModeServerArgs {
    param([string] $Mode)
    switch ($Mode) {
        'default'   { return @('server', 'start') }
        'namespace' { return @('server', 'start', '--mode', 'namespace') }
        'all'       { return @('server', 'start', '--mode', 'all') }
        default     { throw "Unknown mode '$Mode'." }
    }
}

# Propagate the run parameters to the harness via environment variables.
$env:PERF_CONCURRENCY   = $Concurrency
$env:PERF_IDLE_SECONDS  = $IdleSeconds
$env:PERF_STEADY_SECONDS = $SteadySeconds
$env:PERF_SOAK_SECONDS  = $SoakSeconds
$env:PERF_SAMPLE_MS     = $SampleMs

# Per-mode timeout covers idle + steady + soak (plus startup/headroom) so a hung harness
# fails fast instead of consuming the whole job.
$resourceTimeoutSeconds = 180 + ($IdleSeconds + $SteadySeconds + $SoakSeconds) * 3

$scenarios = [ordered]@{}

foreach ($mode in $Modes) {
    $serverArgs = Get-ModeServerArgs -Mode $mode
    Write-Host "=== Resource usage: $mode mode (concurrency=$Concurrency, steady=${SteadySeconds}s, soak=${SoakSeconds}s) ==="

    $data = Invoke-PerfHarnessJson -FilePath $BenchmarkExe `
                -ArgumentList (@('--mcp-resource', $Executable) + $serverArgs) -TimeoutSeconds $resourceTimeoutSeconds
    $scenarios[$mode] = $data

    $steady = $data.phases.steady_state
    Write-Host ("  tools={0}  steady: ws-mean={1} MB  ws-max={2} MB  cpu={3}%  {4} req ({5} rps)" -f `
        $data.tool_count, $steady.working_set_mb.mean, $steady.working_set_mb.max, `
        $steady.cpu_percent, $steady.requests, $steady.throughput_rps)
    Write-Host ("  peak: ws={0} MB  private={1} MB  handles={2}   growth idle->steady: ws={3} MB  handles={4}" -f `
        $data.peak.working_set_mb, $data.peak.private_mb, $data.peak.handles, `
        $data.growth.idle_to_steady_working_set_mb, $data.growth.idle_to_steady_handles)
    if ($null -ne $data.soak) {
        Write-Host ("  soak: {0} req over {1}s  ws-slope={2} MB/min (R2={3})  handle-slope={4}/min  unbounded={5}" -f `
            $data.soak.requests, $data.soak.duration_s, $data.soak.working_set_slope_mb_per_min, `
            $data.soak.working_set_r2, $data.soak.handle_slope_per_min, $data.soak.unbounded_growth)
    }
    Write-Host ""
}

$commit = try { (& git -C $RepoRoot rev-parse --short HEAD 2>$null).Trim() } catch { '' }

$results = [ordered]@{
    timestamp     = (Get-Date).ToUniversalTime().ToString('o')
    commit        = $commit
    environment   = (Get-PerfRunMetadata -RepoRoot $RepoRoot)
    concurrency   = $Concurrency
    idle_seconds  = $IdleSeconds
    steady_seconds = $SteadySeconds
    soak_seconds  = $SoakSeconds
    sample_ms     = $SampleMs
    scenarios     = $scenarios
}

$results | ConvertTo-Json -Depth 12 | Out-File -FilePath $OutputPath -Encoding utf8 -Force
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
        Write-Host "=== Resource regression check (steady CPU/mem +10%, peak mem +15%, no soak leak) ==="

        $baseline = Get-Content $BaselinePath | ConvertFrom-Json
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $results.scenarios `
                                              -BaselineScenarios $baseline.scenarios

        if ($gate.Warnings.Count -gt 0) {
            Write-Warning "Resource warnings (>5%): $($gate.Warnings -join ', ')"
        }

        if ($gate.Failures.Count -gt 0) {
            Write-Error "Resource regression detected in: $($gate.Failures -join ', ')"
        }
        else {
            Write-Host "  All resource budget checks passed."
        }
    }
}

# ---------------------------------------------------------------------------
# Emit Azure DevOps pipeline variables (no-op outside AzDO)
# ---------------------------------------------------------------------------
foreach ($mode in $Modes) {
    $data = $scenarios[$mode]
    $steady = $data.phases.steady_state
    $modeVar = (Get-Culture).TextInfo.ToTitleCase($mode)
    Write-Host "##vso[task.setvariable variable=Resource${modeVar}SteadyWorkingSetMb]$($steady.working_set_mb.mean)"
    Write-Host "##vso[task.setvariable variable=Resource${modeVar}SteadyCpuPercent]$($steady.cpu_percent)"
    Write-Host "##vso[task.setvariable variable=Resource${modeVar}PeakWorkingSetMb]$($data.peak.working_set_mb)"
}
