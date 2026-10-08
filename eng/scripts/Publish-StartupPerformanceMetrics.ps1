#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Emits the startup performance metrics from a results JSON file.

.DESCRIPTION
    Reads the results JSON produced by Test-StartupPerformance.ps1 and logs each tracked
    metric. The metrics are currently written as pipeline log warnings so they surface in
    the build timeline; replace the Write-Host sink with a real telemetry destination
    (Azure Monitor, Application Insights, etc.) to persist them.

.PARAMETER ResultsPath
    Path to the results JSON written by Test-StartupPerformance.ps1.
#>

param(
    [Parameter(Mandatory)][string] $ResultsPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$results = Get-Content $ResultsPath | ConvertFrom-Json
$s = $results.scenarios

$metrics = @(
    @{ name = 'startup.cli.cold.median_ms';      value = $s.cli_cold_start_ms.median }
    @{ name = 'startup.cli.cold.p95_ms';         value = $s.cli_cold_start_ms.p95 }
    @{ name = 'startup.mcp.default.median_ms';   value = $s.mcp_stdio_to_tools_list_ms.median }
    @{ name = 'startup.mcp.default.p95_ms';      value = $s.mcp_stdio_to_tools_list_ms.p95 }
    @{ name = 'startup.mcp.default.p99_ms';      value = $s.mcp_stdio_to_tools_list_ms.p99 }
    @{ name = 'startup.mcp.namespace.median_ms'; value = $s.mcp_namespace_mode_startup_ms.median }
    @{ name = 'startup.mcp.all.median_ms';       value = $s.mcp_all_mode_startup_ms.median }
    @{ name = 'startup.mcp.all.p95_ms';          value = $s.mcp_all_mode_startup_ms.p95 }
    @{ name = 'startup.mcp.all.p99_ms';          value = $s.mcp_all_mode_startup_ms.p99 }
    @{ name = 'startup.mcp.http.median_ms';      value = $s.mcp_http_default_startup_ms.median }
    @{ name = 'startup.mcp.http.p95_ms';         value = $s.mcp_http_default_startup_ms.p95 }
    @{ name = 'startup.mcp.http.readiness_median_ms'; value = $s.mcp_http_readiness_ms.median }
    @{ name = 'serialize.default.full_schema_ms'; value = $s.tools_list_payload.full_schema_serialize_ms }
    @{ name = 'serialize.all.full_schema_ms';     value = $s.all_mode_tools_list_payload.full_schema_serialize_ms }
    @{ name = 'serialize.http.full_schema_ms';    value = $s.http_default_tools_list_payload.full_schema_serialize_ms }
    @{ name = 'tokens.default.full_schema_gpt4o';   value = $s.tools_list_payload.full_schema_tokens_exact }
    @{ name = 'tokens.namespace.full_schema_gpt4o'; value = $s.namespace_mode_tools_list_payload.full_schema_tokens_exact }
    @{ name = 'tokens.all.full_schema_gpt4o';       value = $s.all_mode_tools_list_payload.full_schema_tokens_exact }
    @{ name = 'tools.default.count';             value = $s.tools_list_payload.tool_count }
    @{ name = 'tools.all.count';                 value = $s.all_mode_tools_list_payload.tool_count }
)

foreach ($m in $metrics) {
    Write-Host ("##vso[task.logissue type=warning]METRIC: {0} = {1}" -f $m.name, $m.value)
    # Replace the log statement above with your preferred telemetry sink, e.g.:
    #   az monitor metrics alert create ...
    #   Invoke-RestMethod -Uri $AppInsightsEndpoint -Body @{ name=$m.name; value=$m.value } ...
}
Write-Host "Metrics logged. Wire up a telemetry sink to persist them."
