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
    scenario exceeds the tiered budgets from issue #3118 (p50/p95 +10%, p99 +20%,
    discovery scaling efficiency +15%; changes above +5% are reported as warnings).

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
    [int]     $Runs = 10,
    [string]  $OutputPath,
    [string]  $BaselinePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ---------------------------------------------------------------------------
# Resolve repo root and executable path
# ---------------------------------------------------------------------------
$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { $PWD.Path }
$RepoRoot = (Get-Item (Join-Path $ScriptDir '../..')).FullName

# Shared, unit-tested helpers (Get-TimingStats, Resolve-PerfOutputPath).
. (Join-Path $ScriptDir 'StartupPerformance.Common.ps1')

if (-not $Executable) {
    $Executable = Join-Path $RepoRoot 'servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp.exe'
    if ($IsLinux -or $IsMacOS) {
        $Executable = Join-Path $RepoRoot 'servers/Azure.Mcp.Server/src/bin/Debug/net10.0/azmcp'
    }
}

if (-not (Test-Path $Executable)) {
    Write-Error "azmcp executable not found at: $Executable`nBuild first: dotnet build servers/Azure.Mcp.Server/src"
}

# Resolve to a full path so child transports (e.g. the stdio SDK client) don't
# fail on a leading './' that Windows cmd cannot parse.
$Executable = (Resolve-Path $Executable).Path

$OutputPath = if ($OutputPath) { $OutputPath } else { Join-Path $RepoRoot '.perf-results/startup-e2e.json' }
$OutputPath = Resolve-PerfOutputPath -Path $OutputPath -BaseDirectory $RepoRoot

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

    $timeoutMs = 30000
    $sw        = [System.Diagnostics.Stopwatch]::StartNew()
    $proc      = [System.Diagnostics.Process]::Start($psi)
    try {
        # Drain both streams asynchronously to prevent buffer deadlocks
        $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
        $stderrTask = $proc.StandardError.ReadToEndAsync()

        if (-not $proc.WaitForExit($timeoutMs)) {
            $sw.Stop()
            try   { $proc.Kill($true) }
            catch { if (-not $proc.HasExited) { throw } }
            $null          = $proc.WaitForExit(5000)
            $stderr        = $stderrTask.GetAwaiter().GetResult()
            $stderrMessage = if ([string]::IsNullOrWhiteSpace($stderr)) { '<no stderr>' } else { $stderr.Trim() }
            throw "CLI cold start timed out after $timeoutMs ms for '$ExePath tools list'. stderr: $stderrMessage"
        }

        $sw.Stop()
        $null   = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()

        if ($proc.ExitCode -ne 0) {
            $stderrMessage = if ([string]::IsNullOrWhiteSpace($stderr)) { '<no stderr>' } else { $stderr.Trim() }
            throw "CLI cold start failed with exit code $($proc.ExitCode) for '$ExePath tools list'. stderr: $stderrMessage"
        }

        return $sw.ElapsedMilliseconds
    }
    finally {
        if ($null -ne $proc) { $proc.Dispose() }
    }
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
        [string[]] $ServerArgTokens,
        [int]      $TimeoutSeconds = 60,
        # '--mcp-startup' (stdio) or '--mcp-startup-http' (HTTP transport).
        [string]   $BenchmarkMode = '--mcp-startup'
    )

    $processStartInfo                      = [System.Diagnostics.ProcessStartInfo]::new()
    $processStartInfo.FileName             = $BenchmarkExe
    $processStartInfo.RedirectStandardOutput = $true
    $processStartInfo.RedirectStandardError  = $true
    $processStartInfo.UseShellExecute      = $false
    $processStartInfo.CreateNoWindow       = $true
    $null = $processStartInfo.ArgumentList.Add($BenchmarkMode)
    $null = $processStartInfo.ArgumentList.Add($ExePath)
    foreach ($serverArgToken in $ServerArgTokens) {
        $null = $processStartInfo.ArgumentList.Add($serverArgToken)
    }

    $process           = [System.Diagnostics.Process]::new()
    $process.StartInfo = $processStartInfo
    try {
        if (-not $process.Start()) {
            throw "Failed to start benchmark process '$BenchmarkExe'."
        }

        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()

        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            try   { $process.Kill($true) }
            catch { if (-not $process.HasExited) { throw } }
            $null          = $process.WaitForExit()
            $stderr        = $stderrTask.GetAwaiter().GetResult()
            $stderrMessage = if ([string]::IsNullOrWhiteSpace($stderr)) { '<no stderr>' } else { $stderr.Trim() }
            throw "Benchmark process '$BenchmarkExe' timed out after $TimeoutSeconds seconds. Stderr: $stderrMessage"
        }

        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()

        if ($process.ExitCode -ne 0) {
            $stderrMessage = if ([string]::IsNullOrWhiteSpace($stderr)) { '<no stderr>' } else { $stderr.Trim() }
            throw "Benchmark process '$BenchmarkExe' exited with code $($process.ExitCode). Stderr: $stderrMessage"
        }

        $output   = @($stdout -split "`r?`n")
        $jsonLine = $output | Where-Object { $_ -match '^\s*\{' } | Select-Object -Last 1
        if ([string]::IsNullOrWhiteSpace($jsonLine)) {
            $stderrMessage = if ([string]::IsNullOrWhiteSpace($stderr)) { '<no stderr>' } else { $stderr.Trim() }
            throw "Benchmark process '$BenchmarkExe' did not emit a JSON result. Stderr: $stderrMessage"
        }

        return $jsonLine | ConvertFrom-Json
    }
    finally {
        $process.Dispose()
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
    Write-Host ("  Full schema serialization time (ms)         : {0}" -f $Payload.full_schema_serialize_ms)
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Helper: run one MCP scenario (timing loop + cold/warm split + percentiles +
# token/serialization measurement). Works for both stdio and HTTP transports
# via the $BenchmarkMode switch. Uses the MCP C# SDK (via the benchmark binary)
# for protocol-correct timing.
# Returns [ordered]@{ stats = ...; coldWarm = ...; payload = ... }
# ---------------------------------------------------------------------------
function Invoke-McpScenario {
    param(
        [string]   $Label,
        [string[]] $ServerArgs,
        [string]   $ExePath,
        [int]      $Runs,
        [string]   $BenchmarkExe,
        [string]   $BenchmarkMode = '--mcp-startup',
        [int]      $TimeoutSeconds = 60
    )
    Write-Host "=== $Label ==="
    $ms              = @()
    $readinessMs     = @()
    $lastBenchResult = $null
    for ($i = 1; $i -le $Runs; $i++) {
        $benchResult = Invoke-BenchmarkMcpStartup -BenchmarkExe $BenchmarkExe `
                           -ExePath $ExePath -ServerArgTokens $ServerArgs `
                           -BenchmarkMode $BenchmarkMode -TimeoutSeconds $TimeoutSeconds
        $ms += [long]$benchResult.elapsed_ms
        if ($benchResult.PSObject.Properties.Name -contains 'readiness_ms') {
            $readinessMs += [long]$benchResult.readiness_ms
        }
        $lastBenchResult = $benchResult
        Write-Host ("  Run {0}: {1} ms" -f $i, $benchResult.elapsed_ms)
    }
    $stats    = Get-TimingStats -Samples $ms
    $coldWarm = Split-ColdWarm -Samples $ms
    Write-Host ("  → cold={0} ms  warm-median={1} ms  p95={2} ms  p99={3} ms" -f `
        $coldWarm.cold_ms, $coldWarm.warm.median, $stats.p95, $stats.p99)
    Write-Host ""
    $payload = [ordered]@{
        tool_count                    = [int]$lastBenchResult.tool_count
        name_description_bytes        = [int]$lastBenchResult.name_description.bytes
        name_description_tokens_exact = [int]$lastBenchResult.name_description.exact_tokens_gpt4o_o200k
        full_schema_bytes             = [int]$lastBenchResult.full_schema.bytes
        full_schema_tokens_exact      = [int]$lastBenchResult.full_schema.exact_tokens_gpt4o_o200k
        full_schema_serialize_ms      = [double]$lastBenchResult.full_schema.serialize_ms
    }
    Write-PayloadStats -Label $Label -Payload $payload
    $result = [ordered]@{ stats = $stats; coldWarm = $coldWarm; payload = $payload }
    if ($readinessMs.Count -gt 0) {
        $result.readiness = Get-TimingStats -Samples $readinessMs
    }
    return $result
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

$cli         = Get-TimingStats -Samples $cliMs
$cliColdWarm = Split-ColdWarm -Samples $cliMs
Write-Host ("  → cold={0} ms  warm-median={1} ms  p95={2} ms  p99={3} ms" -f `
    $cliColdWarm.cold_ms, $cliColdWarm.warm.median, $cli.p95, $cli.p99)
Write-Host ""

# ---------------------------------------------------------------------------
# Scenarios 2-4 – MCP stdio startup across three server modes
# Timing is performed by the benchmark binary using the official MCP C# SDK
# (McpClient.CreateAsync + ListToolsAsync via StdioClientTransport).
# ---------------------------------------------------------------------------
$s2 = Invoke-McpScenario -Label 'Scenario 2: MCP stdio startup (default mode)' `
                          -ServerArgs @('server', 'start') -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

$s3 = Invoke-McpScenario -Label 'Scenario 3: MCP stdio startup (--mode namespace)' `
                          -ServerArgs @('server', 'start', '--mode', 'namespace') -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

$s4 = Invoke-McpScenario -Label 'Scenario 4: MCP stdio startup (--mode all)' `
                          -ServerArgs @('server', 'start', '--mode', 'all') -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe

# ---------------------------------------------------------------------------
# Scenario 5 – MCP remote HTTP startup (default mode)
# The benchmark binary spawns the server with --transport http
# --dangerously-disable-http-incoming-auth on a free loopback port, polls
# readiness, then times the initialize + tools/list handshake over
# HttpClientTransport. readiness_ms captures server-ready time separately.
# HTTP startup includes web-host build + Kestrel bind, so it needs a longer
# per-run timeout than stdio.
# ---------------------------------------------------------------------------
$s5 = Invoke-McpScenario -Label 'Scenario 5: MCP remote HTTP startup (default mode)' `
                          -ServerArgs @('server', 'start') -ExePath $Executable -Runs $Runs `
                          -BenchmarkExe $BenchmarkExe -BenchmarkMode '--mcp-startup-http' `
                          -TimeoutSeconds 90

# ---------------------------------------------------------------------------
# Write results JSON
# ---------------------------------------------------------------------------
$gitCommit = (& git -C $RepoRoot rev-parse --short HEAD 2>$null) ?? 'unknown'

$results = [ordered]@{
    timestamp                   = (Get-Date -Format 'o')
    commit                      = $gitCommit
    executable                  = $Executable
    runs                        = $Runs
    environment                 = (Get-PerfRunMetadata)
    scenarios                   = [ordered]@{
        cli_cold_start_ms                 = $cli
        cli_cold_warm                     = $cliColdWarm
        mcp_stdio_to_tools_list_ms        = $s2.stats
        mcp_stdio_cold_warm               = $s2.coldWarm
        tools_list_payload                = $s2.payload
        mcp_namespace_mode_startup_ms     = $s3.stats
        mcp_namespace_cold_warm           = $s3.coldWarm
        namespace_mode_tools_list_payload = $s3.payload
        mcp_all_mode_startup_ms           = $s4.stats
        mcp_all_cold_warm                 = $s4.coldWarm
        all_mode_tools_list_payload       = $s4.payload
        mcp_http_default_startup_ms       = $s5.stats
        mcp_http_cold_warm                = $s5.coldWarm
        mcp_http_readiness_ms             = $s5.readiness
        http_default_tools_list_payload   = $s5.payload
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
        Write-Host "=== Regression check (p50/p95 +10%, p99 +20%, scaling +15%, warn +5%) ==="

        $baseline = Get-Content $BaselinePath | ConvertFrom-Json
        $gate     = Invoke-StartupRegressionGate -ResultScenarios $results.scenarios `
                                                 -BaselineScenarios $baseline.scenarios

        if ($gate.Warnings.Count -gt 0) {
            Write-Warning "Startup performance warnings (>5%): $($gate.Warnings -join ', ')"
        }

        if ($gate.Failures.Count -gt 0) {
            Write-Error "Startup regression detected in: $($gate.Failures -join ', ')"
        }
        else {
            Write-Host "  All budget checks passed."
        }
    }
}

# ---------------------------------------------------------------------------
# Emit Azure DevOps pipeline variables (no-op outside AzDO)
# ---------------------------------------------------------------------------
Write-Host "##vso[task.setvariable variable=CliColdStartMedianMs]$($cli.median)"
Write-Host "##vso[task.setvariable variable=CliColdStartColdMs]$($cliColdWarm.cold_ms)"
Write-Host "##vso[task.setvariable variable=CliColdStartWarmMedianMs]$($cliColdWarm.warm.median)"
Write-Host "##vso[task.setvariable variable=CliColdStartP95Ms]$($cli.p95)"
Write-Host "##vso[task.setvariable variable=CliColdStartP99Ms]$($cli.p99)"
Write-Host "##vso[task.setvariable variable=McpStdioStartupMedianMs]$($s2.stats.median)"
Write-Host "##vso[task.setvariable variable=McpStdioStartupP95Ms]$($s2.stats.p95)"
Write-Host "##vso[task.setvariable variable=McpStdioStartupP99Ms]$($s2.stats.p99)"
Write-Host "##vso[task.setvariable variable=ToolsListToolCount]$($s2.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=ToolsListNameDescTokensGpt4o]$($s2.payload.name_description_tokens_exact)"
Write-Host "##vso[task.setvariable variable=ToolsListFullSchemaTokensGpt4o]$($s2.payload.full_schema_tokens_exact)"
Write-Host "##vso[task.setvariable variable=ToolsListFullSchemaSerializeMs]$($s2.payload.full_schema_serialize_ms)"
Write-Host "##vso[task.setvariable variable=McpNamespaceModeStartupMedianMs]$($s3.stats.median)"
Write-Host "##vso[task.setvariable variable=NamespaceModeToolsListToolCount]$($s3.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=NamespaceModeToolsListFullSchemaTokensGpt4o]$($s3.payload.full_schema_tokens_exact)"
Write-Host "##vso[task.setvariable variable=AllModeStartupMedianMs]$($s4.stats.median)"
Write-Host "##vso[task.setvariable variable=AllModeStartupP95Ms]$($s4.stats.p95)"
Write-Host "##vso[task.setvariable variable=AllModeStartupP99Ms]$($s4.stats.p99)"
Write-Host "##vso[task.setvariable variable=AllModeToolsListToolCount]$($s4.payload.tool_count)"
Write-Host "##vso[task.setvariable variable=AllModeToolsListFullSchemaTokensGpt4o]$($s4.payload.full_schema_tokens_exact)"
Write-Host "##vso[task.setvariable variable=McpHttpDefaultStartupMedianMs]$($s5.stats.median)"
Write-Host "##vso[task.setvariable variable=McpHttpDefaultStartupP95Ms]$($s5.stats.p95)"
Write-Host "##vso[task.setvariable variable=McpHttpReadinessMedianMs]$(if ($s5.Contains('readiness')) { $s5.readiness.median } else { 'n/a' })"
Write-Host "##vso[build.addbuildtag]perf-tracked"
