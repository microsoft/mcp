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
    Defaults to `<repo-root>/.work/mcp-output-size`.

.PARAMETER Configuration
    The build configuration to use. Defaults to `Debug`.

.PARAMETER SkipBuild
    Skip the build step and use the existing binaries. Implied (and not required) when
    -ServerExecutable or -ReleaseTag is supplied, since that server binary isn't built by
    this script.

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
    also passed). Mutually exclusive with -ReleaseTag.

.PARAMETER ReleaseTag
    A GitHub release tag from the microsoft/mcp repository (e.g.
    `Azure.Mcp.Server-3.0.0-beta.36`) whose azmcp server asset should be downloaded and
    measured, instead of building the local source tree. The matching platform zip
    (`Azure.Mcp.Server-<os>-<arch>.zip`) is downloaded to
    `<OutputDirectory>/release-download/<ReleaseTag>` and extracted before measurement.
    Mutually exclusive with -ServerExecutable.

.PARAMETER GitHubRepository
    The `owner/repo` used to resolve -ReleaseTag. Defaults to `microsoft/mcp`. Only used
    when -ReleaseTag is supplied.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1

    Builds, measures, and summarizes using default paths.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -SkipBuild -Clean

    Reuses the existing build, clears previous results, then measures and summarizes.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -ServerExecutable C:\releases\azmcp-1.2.3\azmcp.exe -OutputDirectory .work/mcp-output-size/released-1.2.3

    Measures a previously released azmcp build (e.g. downloaded/extracted from a GitHub
    release) instead of the local source tree, writing results to a separate directory so
    they can be compared against a current-source run.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -ReleaseTag Azure.Mcp.Server-3.0.0-beta.36 -OutputDirectory .work/mcp-output-size/beta.36

    Downloads the azmcp server asset for the given release tag from GitHub, measures it,
    and writes results to a separate directory so they can be compared against a
    current-source run.
#>

[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [string]$Configuration = 'Debug',
    [switch]$SkipBuild,
    [switch]$Clean,
    [int]$LearnResponseThresholdUtf8Bytes = 45000,
    [string]$ServerExecutable,
    [string]$ReleaseTag,
    [string]$GitHubRepository = 'microsoft/mcp'
)

if ($ServerExecutable -and $ReleaseTag) {
    Write-Error "-ServerExecutable and -ReleaseTag are mutually exclusive."
    exit 1
}

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$repoRoot = $RepoRoot.Path

$serverProject = Join-Path $repoRoot 'servers/Azure.Mcp.Server/src'
$measurerProject = Join-Path $repoRoot 'eng/tools/McpOutputSizeMeasurer/src'

if (!$OutputDirectory) {
    $OutputDirectory = Join-Path $repoRoot '.work/mcp-output-size'
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

if ($ReleaseTag) {
    # Map the current platform to the asset name pattern used by Pack-Zip.ps1 /
    # New-BuildInfo.ps1, e.g. Azure.Mcp.Server-win-x64.zip, Azure.Mcp.Server-linux-arm64.zip,
    # Azure.Mcp.Server-osx-arm64.zip.
    if ($IsWindows) {
        $releaseOs = 'win'
    } elseif ($IsMacOS) {
        $releaseOs = 'osx'
    } elseif ($IsLinux) {
        $releaseOs = 'linux'
    } else {
        Write-Error "Unable to determine current OS for release asset selection."
        exit 1
    }

    $arch = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
    $releaseArch = switch ($arch) {
        'X64' { 'x64' }
        'Arm64' { 'arm64' }
        default {
            Write-Error "Unsupported process architecture '$arch' for release asset selection."
            exit 1
        }
    }

    $assetName = "Azure.Mcp.Server-$releaseOs-$releaseArch.zip"
    $downloadUrl = "https://github.com/$GitHubRepository/releases/download/$ReleaseTag/$assetName"

    $releaseDownloadDir = Join-Path $OutputDirectory "release-download/$ReleaseTag"
    $releaseExtractDir = Join-Path $releaseDownloadDir 'extracted'
    $releaseZipPath = Join-Path $releaseDownloadDir $assetName

    New-Item -ItemType Directory -Path $releaseDownloadDir -Force | Out-Null

    Write-Host "Downloading $assetName from release $ReleaseTag ($GitHubRepository)..."
    Write-Host "  $downloadUrl"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $releaseZipPath

    Write-Host "Extracting $releaseZipPath..."
    if (Test-Path -LiteralPath $releaseExtractDir) {
        Remove-Item -LiteralPath $releaseExtractDir -Recurse -Force
    }
    Expand-Archive -Path $releaseZipPath -DestinationPath $releaseExtractDir -Force

    $ServerExecutable = Join-Path $releaseExtractDir "azmcp$(if ($IsWindows) { '.exe' } else { '' })"
    if (!(Test-Path -LiteralPath $ServerExecutable -PathType Leaf)) {
        Write-Error "azmcp executable not found in downloaded release asset at $ServerExecutable."
        exit 1
    }
    if (!$IsWindows) {
        chmod +x $ServerExecutable
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
