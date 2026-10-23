#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Builds the Azure MCP Server performance-benchmark project in Release for the perf pipeline.

.DESCRIPTION
    Kept as a script (rather than inline pipeline YAML) so CI behavior can be reproduced locally.

    NU1902 (the NuGet audit warning) is already non-fatal via WarningsNotAsErrors in
    Directory.Build.props. A command-line -p:NoWarn is deliberately NOT passed here: a global
    NoWarn property overrides every project's own <NoWarn> (for example the Insights project's
    MCP9005 sampling-deprecation suppression), which breaks a clean build.

.PARAMETER Project
    Path to the performance-benchmark csproj to build.
#>
param(
    [string] $Project = 'servers/Azure.Mcp.Server/perf/Azure.Mcp.Server.PerformanceBenchmarks/Azure.Mcp.Server.PerformanceBenchmarks.csproj'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

dotnet build $Project -c Release -consoleLoggerParameters:NoSummary
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
