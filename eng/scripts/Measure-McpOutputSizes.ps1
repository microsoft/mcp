#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Builds and runs the MCP output size measurement test, then summarizes the results.

.DESCRIPTION
    Runs the full measurement workflow end to end:

    1. Builds the Azure.Mcp.Server.Tests project (which also builds the azmcp server).
    2. Runs the McpOutputSizeTests measurement test, which starts the MCP server over
       stdio in both consolidated and namespace modes and measures the initialize
       greeting, tools/list discovery, and learn-mode responses for every tool.
    3. Runs Summarize-McpOutputSizes.ps1 to produce console, JSON, and Markdown
       summaries, extract readable description text, and split each top tool's inner
       commands into individual files.

    The measurement test is opt-in and normally skipped, so this script sets
    MCP_OUTPUT_SIZE_ENABLED for the duration of the run.

.PARAMETER OutputDirectory
    Directory for the measurement report and its artifacts.
    Defaults to `<repo-root>/TestResults`.

.PARAMETER Configuration
    The build configuration to use. Defaults to `Debug`.

.PARAMETER SkipBuild
    Skip the build step and use the existing test binaries.

.PARAMETER Clean
    Remove the output directory before running so stale artifacts are not mixed with
    the new results.

.PARAMETER LearnResponseThresholdUtf8Bytes
    Include every learn response over this UTF-8 byte threshold in the summary.
    Defaults to 45000.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1

    Builds, measures, and summarizes using default paths.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -SkipBuild -Clean

    Reuses the existing build, clears previous results, then measures and summarizes.
#>

[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [string]$Configuration = 'Debug',
    [switch]$SkipBuild,
    [switch]$Clean,
    [int]$LearnResponseThresholdUtf8Bytes = 45000
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$repoRoot = $RepoRoot.Path

$testProject = Join-Path $repoRoot 'servers/Azure.Mcp.Server/tests/Azure.Mcp.Server.Tests'

if (!$OutputDirectory) {
    $OutputDirectory = Join-Path $repoRoot 'TestResults'
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)

if ($Clean -and (Test-Path -LiteralPath $OutputDirectory)) {
    Write-Host "Removing existing output directory $OutputDirectory"
    Remove-Item -LiteralPath $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$reportPath = Join-Path $OutputDirectory 'mcp-output-size.json'

if ($SkipBuild) {
    Write-Host "Skipping build."
} else {
    Write-Host "Building $testProject ($Configuration)..."
    dotnet build $testProject --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

$testExecutable = Join-Path $testProject "bin/$Configuration/net10.0/Azure.Mcp.Server.Tests$(if ($IsWindows) { '.exe' } else { '' })"
if (!(Test-Path -LiteralPath $testExecutable -PathType Leaf)) {
    Write-Error "Test executable not found at $testExecutable. Run without -SkipBuild to build it first."
    exit 1
}

Write-Host "Running MCP output size measurement test..."
$previousEnabled = $env:MCP_OUTPUT_SIZE_ENABLED
$previousReport = $env:MCP_OUTPUT_SIZE_REPORT
try {
    $env:MCP_OUTPUT_SIZE_ENABLED = 'true'
    $env:MCP_OUTPUT_SIZE_REPORT = $reportPath

    & $testExecutable --filter-class Azure.Mcp.Server.Tests.Infrastructure.McpOutputSizeTests --no-progress
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Measurement test failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
} finally {
    $env:MCP_OUTPUT_SIZE_ENABLED = $previousEnabled
    $env:MCP_OUTPUT_SIZE_REPORT = $previousReport
}

if (!(Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    Write-Error "Measurement report was not produced at $reportPath."
    exit 1
}

Write-Host "Summarizing results..."
& "$PSScriptRoot/Summarize-McpOutputSizes.ps1" `
    -InputPath $reportPath `
    -OutputPath (Join-Path $OutputDirectory 'mcp-output-size-summary.json') `
    -MarkdownPath (Join-Path $OutputDirectory 'mcp-output-size-summary.md') `
    -LearnResponseThresholdUtf8Bytes $LearnResponseThresholdUtf8Bytes

Write-Host ""
Write-Host "Done. Results are in $OutputDirectory"
