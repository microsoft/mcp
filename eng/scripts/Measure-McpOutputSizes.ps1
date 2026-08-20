#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Builds and runs the MCP output size measurement test, then summarizes the results.

.DESCRIPTION
    Runs the full measurement workflow end to end:

    1. Builds the Azure.Mcp.Server project and the standalone McpOutputSizeMeasurer tool.
    2. Runs McpOutputSizeMeasurer, which starts the MCP server over stdio in both
       consolidated and namespace modes and measures the initialize greeting,
       tools/list discovery, and learn-mode responses (including inner commands) for
       every tool.
    3. Runs Summarize-McpOutputSizes.ps1 to produce console, JSON, and Markdown
       summaries, extract readable description text, and split each top tool's inner
       commands into individual files.

.PARAMETER OutputDirectory
    Directory for the measurement report and its artifacts.
    Defaults to `<repo-root>/TestResults`.

.PARAMETER Configuration
    The build configuration to use. Defaults to `Debug`.

.PARAMETER SkipBuild
    Skip the build step and use the existing binaries. Implied (and not required) when
    -ServerExecutable is supplied, since that server binary isn't built by this script.

.PARAMETER Clean
    Remove the output directory before running so stale artifacts are not mixed with
    the new results.

.PARAMETER LearnResponseThresholdUtf8Bytes
    Include every learn response over this UTF-8 byte threshold in the summary.
    Defaults to 45000.

.PARAMETER ServerExecutable
    Path to an already-built or published azmcp server executable to measure, instead of
    building and using the local servers/Azure.Mcp.Server/src project. Use this to measure
    a previously released version of the server (e.g. a binary extracted from a release
    asset or installed via a package manager) so its output sizes can be diffed against
    the current source tree. When supplied, the local server project is not built or
    resolved; only the McpOutputSizeMeasurer tool is still built (unless -SkipBuild is
    also passed).

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1

    Builds, measures, and summarizes using default paths.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -SkipBuild -Clean

    Reuses the existing build, clears previous results, then measures and summarizes.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -ServerExecutable C:\releases\azmcp-1.2.3\azmcp.exe -OutputDirectory TestResults/released-1.2.3

    Measures a previously released azmcp build (e.g. downloaded/extracted from a GitHub
    release) instead of the local source tree, writing results to a separate directory so
    they can be compared against a current-source run.
#>

[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [string]$Configuration = 'Debug',
    [switch]$SkipBuild,
    [switch]$Clean,
    [int]$LearnResponseThresholdUtf8Bytes = 45000,
    [string]$ServerExecutable
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$repoRoot = $RepoRoot.Path

$serverProject = Join-Path $repoRoot 'servers/Azure.Mcp.Server/src'
$measurerProject = Join-Path $repoRoot 'eng/tools/McpOutputSizeMeasurer/src'

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

if ($ServerExecutable) {
    $ServerExecutable = [IO.Path]::GetFullPath($ServerExecutable)
    if (!(Test-Path -LiteralPath $ServerExecutable -PathType Leaf)) {
        Write-Error "Server executable not found at $ServerExecutable."
        exit 1
    }
}

if ($SkipBuild) {
    Write-Host "Skipping build."
} else {
    if ($ServerExecutable) {
        Write-Host "Using pre-built server executable $ServerExecutable; skipping local server build."
    } else {
        Write-Host "Building $serverProject ($Configuration)..."
        dotnet build $serverProject --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    }

    Write-Host "Building $measurerProject ($Configuration)..."
    dotnet build $measurerProject --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

if ($ServerExecutable) {
    $serverExecutable = $ServerExecutable
} else {
    $serverExecutable = Join-Path $serverProject "bin/$Configuration/net10.0/azmcp$(if ($IsWindows) { '.exe' } else { '' })"
    if (!(Test-Path -LiteralPath $serverExecutable -PathType Leaf)) {
        Write-Error "Server executable not found at $serverExecutable. Run without -SkipBuild to build it first."
        exit 1
    }
}

$measurerExecutable = Join-Path $measurerProject "bin/$Configuration/net10.0/McpOutputSizeMeasurer$(if ($IsWindows) { '.exe' } else { '' })"
if (!(Test-Path -LiteralPath $measurerExecutable -PathType Leaf)) {
    Write-Error "Measurer executable not found at $measurerExecutable. Run without -SkipBuild to build it first."
    exit 1
}

Write-Host "Running MCP output size measurement..."
& $measurerExecutable --executable $serverExecutable --report $reportPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Measurement failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
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
