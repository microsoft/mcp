#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Appends a startup-performance run to a persistent trend file and regenerates a
    Markdown trend report.

.DESCRIPTION
    Reads the results JSON produced by Test-StartupPerformance.ps1 and appends a compact,
    one-line-per-run record to a JSON Lines (.jsonl) trend file. It then regenerates a
    Markdown table summarizing the most recent runs so reviewers can eyeball the trend
    (issue #3118: "Baseline results and trend report").

    The JSONL file is the durable, machine-readable history; the Markdown file is a
    human-readable rendering of the tail. Both are safe to commit as build artifacts.

.PARAMETER ResultsPath
    Path to the results JSON written by Test-StartupPerformance.ps1.

.PARAMETER TrendPath
    Path to the JSONL trend file to append to. Created if it does not exist.
    Defaults to .perf-results/startup-trend.jsonl under the repo root.

.PARAMETER MarkdownPath
    Path to the Markdown report to (re)write. Defaults to .perf-results/startup-trend.md.

.PARAMETER MaxRows
    Maximum number of most-recent rows to render in the Markdown table (default 20).
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath,
    [string] $TrendPath,
    [string] $MarkdownPath,
    [int]    $MaxRows = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')

$ScriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { $PWD.Path }
$RepoRoot  = (Get-Item (Join-Path $ScriptDir '../..')).FullName

if (-not $TrendPath)    { $TrendPath    = Join-Path $RepoRoot '.perf-results/startup-trend.jsonl' }
if (-not $MarkdownPath) { $MarkdownPath = Join-Path $RepoRoot '.perf-results/startup-trend.md' }

$TrendPath    = Resolve-PerfOutputPath -Path $TrendPath -BaseDirectory $RepoRoot
$MarkdownPath = Resolve-PerfOutputPath -Path $MarkdownPath -BaseDirectory $RepoRoot

$results = Get-Content $ResultsPath | ConvertFrom-Json
$s       = $results.scenarios

# Compact record: one flat object per run so the JSONL stays diff-friendly.
$record = [ordered]@{
    timestamp                     = $results.timestamp
    commit                        = $results.commit
    runs                          = $results.runs
    cli_cold_ms                   = (Get-MemberValue (Get-MemberValue $s 'cli_cold_start_ms') 'median')
    cli_p95_ms                    = (Get-MemberValue (Get-MemberValue $s 'cli_cold_start_ms') 'p95')
    stdio_default_median_ms       = (Get-MemberValue (Get-MemberValue $s 'mcp_stdio_to_tools_list_ms') 'median')
    stdio_default_p95_ms          = (Get-MemberValue (Get-MemberValue $s 'mcp_stdio_to_tools_list_ms') 'p95')
    all_median_ms                 = (Get-MemberValue (Get-MemberValue $s 'mcp_all_mode_startup_ms') 'median')
    all_p95_ms                    = (Get-MemberValue (Get-MemberValue $s 'mcp_all_mode_startup_ms') 'p95')
    http_default_median_ms        = (Get-MemberValue (Get-MemberValue $s 'mcp_http_default_startup_ms') 'median')
    http_readiness_median_ms      = (Get-MemberValue (Get-MemberValue $s 'mcp_http_readiness_ms') 'median')
    default_tool_count            = (Get-MemberValue (Get-MemberValue $s 'tools_list_payload') 'tool_count')
    default_full_schema_tokens    = (Get-MemberValue (Get-MemberValue $s 'tools_list_payload') 'full_schema_tokens_exact')
    all_tool_count                = (Get-MemberValue (Get-MemberValue $s 'all_mode_tools_list_payload') 'tool_count')
    all_full_schema_tokens        = (Get-MemberValue (Get-MemberValue $s 'all_mode_tools_list_payload') 'full_schema_tokens_exact')
    default_full_schema_serialize_ms = (Get-MemberValue (Get-MemberValue $s 'tools_list_payload') 'full_schema_serialize_ms')
}

# Append as a single compact JSON line.
$recordJson = $record | ConvertTo-Json -Depth 5 -Compress
Add-Content -Path $TrendPath -Value $recordJson -Encoding utf8
Write-Host "Appended trend record to: $TrendPath"

# Re-render the Markdown tail from the full JSONL history.
$allRecords = Get-Content $TrendPath | Where-Object { $_.Trim() } | ForEach-Object { $_ | ConvertFrom-Json }
$recent     = @($allRecords | Select-Object -Last $MaxRows)

$sb = [System.Text.StringBuilder]::new()
$null = $sb.AppendLine('# Azure MCP Server startup performance trend')
$null = $sb.AppendLine()
$null = $sb.AppendLine("_Last updated: $(Get-Date -Format 'o'). Showing the $($recent.Count) most recent of $($allRecords.Count) runs._")
$null = $sb.AppendLine()
$null = $sb.AppendLine('| Commit | CLI cold (ms) | stdio p50/p95 (ms) | all p50/p95 (ms) | HTTP p50 (ms) | HTTP ready (ms) | default tools | default tokens | all tokens |')
$null = $sb.AppendLine('|--------|--------------:|-------------------:|-----------------:|--------------:|----------------:|--------------:|---------------:|-----------:|')
foreach ($r in $recent) {
    $null = $sb.AppendLine(("| {0} | {1} | {2}/{3} | {4}/{5} | {6} | {7} | {8} | {9} | {10} |" -f `
        (Get-MemberValue $r 'commit'),
        (Get-MemberValue $r 'cli_cold_ms'),
        (Get-MemberValue $r 'stdio_default_median_ms'), (Get-MemberValue $r 'stdio_default_p95_ms'),
        (Get-MemberValue $r 'all_median_ms'), (Get-MemberValue $r 'all_p95_ms'),
        (Get-MemberValue $r 'http_default_median_ms'),
        (Get-MemberValue $r 'http_readiness_median_ms'),
        (Get-MemberValue $r 'default_tool_count'),
        (Get-MemberValue $r 'default_full_schema_tokens'),
        (Get-MemberValue $r 'all_full_schema_tokens')))
}

$sb.ToString() | Out-File -FilePath $MarkdownPath -Encoding utf8 -Force
Write-Host "Wrote Markdown trend report to: $MarkdownPath"
