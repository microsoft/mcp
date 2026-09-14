#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Measures Azure MCP Server command-dispatch and transport overhead over stdio and HTTP
    (issue #3119) by timing many repetitions of ping and tools/list on a live server.

.DESCRIPTION
    For each transport this runs the performance-benchmark harness in --mcp-dispatch mode,
    which times ping (the transport/protocol floor) and tools/list (a real request with
    server-side serialization) across many iterations. Per-transport p50/p95/p99 are computed
    with the shared percentile helpers, the HTTP-minus-stdio ping delta isolates transport
    overhead, and (when a baseline is supplied) results are gated against the tiered budgets
    from issue #3119 (p50/p95 within 10%, p99 within 20%, transport change above 5% reported).

.PARAMETER Executable
    Path to the azmcp server executable. Defaults to the Release build.

.PARAMETER BenchmarkExe
    Path to the performance-benchmark executable. Defaults to the Release build.

.PARAMETER Iterations
    Timed samples per operation per transport.

.PARAMETER Warmup
    Warmup calls (not timed) per operation.

.PARAMETER OutputPath
    Path to write the JSON results. Defaults to .perf-results/dispatch-overhead.json.

.PARAMETER BaselinePath
    Optional baseline JSON (previously produced by this script) to gate against.

.EXAMPLE
    ./eng/scripts/Measure-DispatchOverhead.ps1

.EXAMPLE
    ./eng/scripts/Measure-DispatchOverhead.ps1 -Iterations 500 -BaselinePath eng/dispatch-baseline.json
#>

param(
    [string] $Executable = './servers/Azure.Mcp.Server/src/bin/Release/net10.0/azmcp.exe',
    [string] $BenchmarkExe = './servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks/bin/Release/net10.0/Azure.Mcp.Server.PerformanceBenchmarks.exe',
    [int]    $Iterations = 200,
    [int]    $Warmup = 20,
    [string] $OutputPath = '.perf-results/dispatch-overhead.json',
    [string] $BaselinePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir 'DispatchPerformance.Common.ps1')

foreach ($path in @($Executable, $BenchmarkExe)) {
    if (-not (Test-Path $path)) {
        throw "Required executable not found: $path  (build the solution in Release first)."
    }
}
$Executable = (Resolve-Path $Executable).Path
$BenchmarkExe = (Resolve-Path $BenchmarkExe).Path
$OutputPath = Resolve-PerfOutputPath -Path $OutputPath

$env:PERF_DISPATCH_ITERATIONS = $Iterations
$env:PERF_DISPATCH_WARMUP = $Warmup

function Get-DispatchStats {
    param([double[]] $Samples)
    $s = Get-TimingStats -Samples $Samples
    return [ordered]@{
        average = $s.average
        median  = $s.median
        p50     = $s.p50
        p95     = $s.p95
        p99     = $s.p99
        count   = $Samples.Count
    }
}

$scenarios = [ordered]@{}

foreach ($transport in @('stdio', 'http')) {
    Write-Host "=== Dispatch overhead: $transport ($Iterations iterations) ==="
    $mode = "--mcp-dispatch-$transport"

    $stdout = & $BenchmarkExe $mode $Executable server start 2>$null
    $jsonLine = $stdout | Where-Object { $_ -match '^\s*\{' } | Select-Object -Last 1
    if (-not $jsonLine) {
        throw "No JSON emitted by the dispatch harness for transport '$transport'. Raw output: $stdout"
    }

    $data = $jsonLine | ConvertFrom-Json
    $toolsList = Get-DispatchStats -Samples ([double[]]$data.tools_list_ms)

    $scenario = [ordered]@{ tools_list = $toolsList }
    if ($data.PSObject.Properties.Name -contains 'readiness_ms' -and $null -ne $data.readiness_ms) {
        $scenario.readiness_ms = $data.readiness_ms
    }
    $scenarios[$transport] = $scenario

    Write-Host ("  tools/list: p50={0} ms  p95={1} ms  p99={2} ms" -f $toolsList.p50, $toolsList.p95, $toolsList.p99)
    Write-Host ""
}

$overhead = Get-TransportOverhead $scenarios
if ($null -ne $overhead) {
    Write-Host ("=== Transport overhead (http - stdio tools/list p50): {0} ms ===" -f [math]::Round($overhead, 4))
    Write-Host ""
}

$commit = try { (& git rev-parse --short HEAD 2>$null).Trim() } catch { '' }

$results = [ordered]@{
    timestamp                        = (Get-Date).ToUniversalTime().ToString('o')
    commit                           = $commit
    environment                      = (Get-PerfRunMetadata)
    iterations                       = $Iterations
    warmup                           = $Warmup
    transport_overhead_tools_list_p50 = $overhead
    scenarios                        = $scenarios
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
        Write-Host "=== Dispatch regression check (p50/p95 +10%, p99 +20%, transport warn +5%) ==="

        $baseline = Get-Content $BaselinePath | ConvertFrom-Json
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $results.scenarios `
                                              -BaselineScenarios $baseline.scenarios

        if ($gate.Warnings.Count -gt 0) {
            Write-Warning "Dispatch warnings (>5%): $($gate.Warnings -join ', ')"
        }

        if ($gate.Failures.Count -gt 0) {
            Write-Error "Dispatch regression detected in: $($gate.Failures -join ', ')"
        }
        else {
            Write-Host "  All dispatch budget checks passed."
        }
    }
}

# ---------------------------------------------------------------------------
# Emit Azure DevOps pipeline variables (no-op outside AzDO)
# ---------------------------------------------------------------------------
foreach ($transport in @('stdio', 'http')) {
    $s = $scenarios[$transport]
    $t = (Get-Culture).TextInfo.ToTitleCase($transport)
    Write-Host "##vso[task.setvariable variable=Dispatch${t}ToolsListP50Ms]$($s.tools_list.p50)"
}
if ($null -ne $overhead) {
    Write-Host "##vso[task.setvariable variable=DispatchTransportOverheadToolsListP50Ms]$overhead"
}
