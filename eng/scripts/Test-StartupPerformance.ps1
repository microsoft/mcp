#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Measures end-to-end startup performance of the Azure MCP Server across four scenarios:
      1. CLI cold start         : azmcp tools list  (process spawn → exit)
      2. MCP stdio default      : azmcp server start → initialize → tools/list response
      3. MCP stdio namespace    : azmcp server start --mode namespace → tools/list response
      4. MCP stdio all-tools    : azmcp server start --mode all → tools/list response

    For each MCP scenario the script also tokenizes the tools/list response using the
    GPT-4o o200k_base encoding (via Microsoft.ML.Tokenizers) to give exact token budgets.

.PARAMETER Executable
    Path to the azmcp executable. Defaults to the Debug build in the repo.

.PARAMETER Runs
    Number of timing samples per scenario. Default: 5.

.PARAMETER OutputPath
    Optional path to write the JSON results file.
    If omitted, results are written to .perf-results/startup-e2e.json under the repo root.

.PARAMETER BaselinePath
    Optional path to a JSON baseline file (previously produced by this script).
    When provided, the script compares results against the baseline and fails if any
    median exceeds BaselineThreshold.

.PARAMETER BaselineThreshold
    Maximum allowed regression ratio (default 1.20 = 20% slower than baseline).
    Only used when BaselinePath is specified.

.EXAMPLE
    # Default run against the Debug build
    ./eng/scripts/Test-StartupPerformance.ps1

.EXAMPLE
    # Run against a Release build with more samples
    ./eng/scripts/Test-StartupPerformance.ps1 -Executable ./servers/Azure.Mcp.Server/src/bin/Release/net10.0/azmcp.exe -Runs 10

.EXAMPLE
    # Compare against a saved baseline and fail on regression
    ./eng/scripts/Test-StartupPerformance.ps1 -BaselinePath eng/perf-baseline.json
#>

param(
    [string]  $Executable,
    [int]     $Runs = 5,
    [string]  $OutputPath,
    [string]  $BaselinePath,
    [double]  $BaselineThreshold = 1.20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Resolve repo root and executable path
# ---------------------------------------------------------------------------
$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { $PWD.Path }
$RepoRoot = (Get-Item (Join-Path $ScriptDir '../..')).FullName

if (-not $Executable) {
    $Executable = Join-Path $RepoRoot 'servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp.exe'
    if ($IsLinux -or $IsMacOS) {
        $Executable = Join-Path $RepoRoot 'servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp'
    }
}

if (-not (Test-Path $Executable)) {
    Write-Error "azmcp executable not found at: $Executable`nBuild first: dotnet build servers/Azure.Mcp.Server/src"
}

$OutputPath = if ($OutputPath) { $OutputPath } else { Join-Path $RepoRoot '.perf-results/startup-e2e.json' }
$null = New-Item -ItemType Directory -Force -Path (Split-Path $OutputPath)

# ---------------------------------------------------------------------------
# Resolve benchmark exe path (Release build preferred, Debug as fallback)
# ---------------------------------------------------------------------------
$BenchmarkExeSuffix = if ($IsWindows) { '.exe' } else { '' }
$BenchmarkExeBase   = Join-Path $RepoRoot 'servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks/bin'
$BenchmarkExe       = Join-Path $BenchmarkExeBase "Release/net10.0/Azure.Mcp.Server.PerformanceBenchmarks$BenchmarkExeSuffix"
if (-not (Test-Path $BenchmarkExe)) {
    $BenchmarkExe = Join-Path $BenchmarkExeBase "Debug/net10.0/Azure.Mcp.Server.PerformanceBenchmarks$BenchmarkExeSuffix"
}
if (-not (Test-Path $BenchmarkExe)) {
    Write-Error "Benchmark exe not found. Build first: dotnet build -c Release servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks"
}

Write-Host "azmcp executable : $Executable"
Write-Host "Benchmark exe    : $BenchmarkExe"
Write-Host "Samples per scenario : $Runs"
Write-Host ""

# ---------------------------------------------------------------------------
# Helper: measure-cli-coldstart
# Time from OS process creation to process exit for a single CLI invocation.
# ---------------------------------------------------------------------------
function Measure-CliColdStart {
    param([string] $ExePath)

    $psi = [System.Diagnostics.ProcessStartInfo]@{
        FileName               = $ExePath
        Arguments              = 'tools list'
        RedirectStandardOutput = $true
        RedirectStandardError  = $true
        UseShellExecute        = $false
        CreateNoWindow         = $true
    }

    $sw   = [System.Diagnostics.Stopwatch]::StartNew()
    $proc = [System.Diagnostics.Process]::Start($psi)
    # Drain both streams asynchronously to prevent buffer deadlocks
    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()
    $proc.WaitForExit()
    $sw.Stop()
    $null = $stdoutTask.Result
    $null = $stderrTask.Result
    $proc.Dispose()
    return $sw.ElapsedMilliseconds
}

# ---------------------------------------------------------------------------
# Helper: invoke-benchmark-mcp-startup
# Calls the benchmark binary with --mcp-startup so the official MCP C# SDK
# performs the initialize + tools/list handshake and reports timing.
# The stopwatch inside the binary starts before McpClient.CreateAsync (which
# spawns the server process) and stops after ListToolsAsync completes.
# Returns the parsed JSON result object:
#   { elapsed_ms, tool_count,
#     name_description: { bytes, exact_tokens_gpt4o_o200k, approx_tokens_bytes_div_4 },
#     full_schema:       { bytes, exact_tokens_gpt4o_o200k, approx_tokens_bytes_div_4 } }
# ---------------------------------------------------------------------------
function Invoke-BenchmarkMcpStartup {
    param(
        [string]   $BenchmarkExe,
        [string]   $ExePath,
        [string[]] $ServerArgTokens
    )

    $output  = & $BenchmarkExe --mcp-startup $ExePath @ServerArgTokens 2>$null
    $jsonLine = @($output) | Where-Object { $_ -match '^\s*\{' } | Select-Object -Last 1
    return $jsonLine | ConvertFrom-Json
}

# ---------------------------------------------------------------------------
# Helper: compute first/average/median timing stats from a sample array
# ---------------------------------------------------------------------------
function Get-TimingStats {
    param([long[]] $Samples)
    return [ordered]@{
        first   = $Samples[0]
        average = [math]::Round(($Samples | Measure-Object -Average).Average, 1)
        median  = ($Samples | Sort-Object)[[math]::Floor($Samples.Count / 2)]
        all     = $Samples
    }
}

# ---------------------------------------------------------------------------
# Helper: print a tools/list payload summary to the console
# ---------------------------------------------------------------------------
function Write-PayloadStats {
    param([string] $Label, $Payload)
    Write-Host "=== tools/list payload ($Label) ==="
    Write-Host ("  Tool count                                  : {0}" -f $Payload.tool_count)
    Write-Host ("  Name+description bytes                      : {0:N0}" -f $Payload.name_description_bytes)
    Write-Host ("  Name+description exact tokens (GPT-4o)      : {0:N0}" -f $Payload.name_description_tokens_exact)
    Write-Host ("  Full schema bytes (incl. inputSchema)       : {0:N0}" -f $Payload.full_schema_bytes)
    Write-Host ("  Full schema exact tokens (GPT-4o) [LLM cost]: {0:N0}" -f $Payload.full_schema_tokens_exact)
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Helper: run one MCP stdio scenario (timing loop + stats + token measurement)
# Uses the MCP C# SDK (via the benchmark binary) for accurate protocol timing.
# Returns [ordered]@{ stats = ...; payload = ... }
# ---------------------------------------------------------------------------
function Invoke-McpScenario {
    param(
        [string] $Label,
        [string] $ServerArgs,
        [string] $ExePath,
        [int]    $Runs,
        [string] $BenchmarkExe
    )
    Write-Host "=== $Label ==="
    $ms              = @()
    $lastBenchResult = $null
    $serverArgTokens = $ServerArgs -split '\s+'
    for ($i = 1; $i -le $Runs; $i++) {
        $benchResult = Invoke-BenchmarkMcpStartup -BenchmarkExe $BenchmarkExe `
                           -ExePath $ExePath -ServerArgTokens $serverArgTokens
        $ms += [long]$benchResult.elapsed_ms
        $lastBenchResult = $benchResult
        Write-Host ("  Run {0}: {1} ms" -f $i, $benchResult.elapsed_ms)
    }
    $stats = Get-TimingStats -Samples $ms
    Write-Host ("  → first={0} ms  average={1} ms  median={2} ms" -f $stats.first, $stats.average, $stats.median)
    Write-Host ""
    $payload = [ordered]@{
        tool_count                    = [int]$lastBenchResult.tool_count
        name_description_bytes        = [int]$lastBenchResult.name_description.bytes
        name_description_tokens_exact = [int]$lastBenchResult.name_description.exact_tokens_gpt4o_o200k
        full_schema_bytes             = [int]$lastBenchResult.full_schema.bytes
        full_schema_tokens_exact      = [int]$lastBenchResult.full_schema.exact_tokens_gpt4o_o200k
    }
    Write-PayloadStats -Label $Label -Payload $payload
    return [ordered]@{ stats = $stats; payload = $payload }
}

# ---------------------------------------------------------------------------
# Scenario 1 – CLI cold start
# ---------------------------------------------------------------------------
Write-Host "=== Scenario 1: CLI cold start (azmcp tools list) ==="
$cliMs = @()
for ($i = 1; $i -le $Runs; $i++) {
    $ms = Measure-CliColdStart -ExePath $Executable
    $cliMs += $ms
    Write-Host ("  Run {0}: {1} ms" -f $i, $ms)
}

$cli = Get-TimingStats -Samples $cliMs
Write-Host ("  → first={0} ms  average={1} ms  median={2} ms" -f $cli.first, $cli.average, $cli.median)
Write-Host ""

# ---------------------------------------------------------------------------
# Scenarios 2-4 – MCP stdio startup across three server modes
# Timing is performed by the benchmark binary using the official MCP C# SDK
# (McpClient.CreateAsync + ListToolsAsync via StdioClientTransport).
# ---------------------------------------------------------------------------
$s2 = Invoke-McpScenario -Label 'Scenario 2: MCP stdio startup (default mode)' `
                          -ServerArgs 'server start' -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

$s3 = Invoke-McpScenario -Label 'Scenario 3: MCP stdio startup (--mode namespace)' `
                          -ServerArgs 'server start --mode namespace' -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

$s4 = Invoke-McpScenario -Label 'Scenario 4: MCP stdio startup (--mode all)' `
                          -ServerArgs 'server start --mode all' -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

# ---------------------------------------------------------------------------
# Write results JSON
# ---------------------------------------------------------------------------
$gitCommit = (& git -C $RepoRoot rev-parse --short HEAD 2>$null) ?? 'unknown'

$results = [ordered]@{
    timestamp                   = (Get-Date -Format 'o')
    commit                      = $gitCommit
    executable                  = $Executable
    runs                        = $Runs
    scenarios                   = [ordered]@{
        cli_cold_start_ms                 = $cli
        mcp_stdio_to_tools_list_ms        = $s2.stats
        tools_list_payload                = $s2.payload
        mcp_namespace_mode_startup_ms     = $s3.stats
        namespace_mode_tools_list_payload = $s3.payload
        mcp_all_mode_startup_ms           = $s4.stats
        all_mode_tools_list_payload       = $s4.payload
    }
}

$resultsJson = $results | ConvertTo-Json -Depth 10
$resultsJson | Out-File -FilePath $OutputPath -Encoding utf8 -Force
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
        Write-Host "=== Regression check (threshold: +$([math]::Round(($BaselineThreshold - 1) * 100))%) ==="

        $baseline   = Get-Content $BaselinePath | ConvertFrom-Json
        $failures   = @()

        $checks = @(
            @{ Name = 'cli_cold_start_ms (median)';           Current = $cli.median;       Baseline = $baseline.scenarios.cli_cold_start_ms.median }
            @{ Name = 'mcp_stdio_default (median)';           Current = $s2.stats.median;  Baseline = $baseline.scenarios.mcp_stdio_to_tools_list_ms.median }
            @{ Name = 'mcp_namespace_mode (median)';          Current = $s3.stats.median;  Baseline = $baseline.scenarios.mcp_namespace_mode_startup_ms.median }
            @{ Name = 'mcp_all_mode (median)';                Current = $s4.stats.median;  Baseline = $baseline.scenarios.mcp_all_mode_startup_ms.median }
        )

        foreach ($check in $checks) {
            $limit  = [math]::Round($check.Baseline * $BaselineThreshold)
            $status = if ($check.Current -le $limit) { 'PASS' } else { $failures += $check.Name; 'FAIL' }
            Write-Host ("  [{0}] {1}: current={2} ms  baseline={3} ms  limit={4} ms" -f
                $status, $check.Name, $check.Current, $check.Baseline, $limit)
        }

        if ($failures.Count -gt 0) {
            Write-Error "Startup regression detected in: $($failures -join ', ')"
        }
        else {
            Write-Host "  All checks passed."
        }
    }
}

# ---------------------------------------------------------------------------
# Emit Azure DevOps pipeline variables (no-op outside AzDO)
# ---------------------------------------------------------------------------
Write-Host "##vso[task.setvariable variable=CliColdStartMedianMs]$($cli.median)"
Write-Host "##vso[task.setvariable variable=McpStdioStartupMedianMs]$($s2.stats.median)"
Write-Host "##vso[task.setvariable variable=ToolsListToolCount]$($s2.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=ToolsListNameDescTokensGpt4o]$($s2.payload.name_description_tokens_exact)"
Write-Host "##vso[task.setvariable variable=ToolsListFullSchemaTokensGpt4o]$($s2.payload.full_schema_tokens_exact)"
Write-Host "##vso[task.setvariable variable=McpNamespaceModeStartupMedianMs]$($s3.stats.median)"
Write-Host "##vso[task.setvariable variable=NamespaceModeToolsListToolCount]$($s3.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=NamespaceModeToolsListFullSchemaTokensGpt4o]$($s3.payload.full_schema_tokens_exact)"
Write-Host "##vso[task.setvariable variable=AllModeStartupMedianMs]$($s4.stats.median)"
Write-Host "##vso[task.setvariable variable=AllModeToolsListToolCount]$($s4.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=AllModeToolsListFullSchemaTokensGpt4o]$($s4.payload.full_schema_tokens_exact)"
Write-Host "##vso[build.addbuildtag]perf-tracked"
