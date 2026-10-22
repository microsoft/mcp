#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $VsCodeProjectPath = 'servers/Azure.Mcp.Server/vscode',
    [string] $ServerExecutablePath,
    [string] $ArtifactsPath
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$projectPath = Join-Path $RepoRoot $VsCodeProjectPath

if (!(Test-Path $projectPath)) {
    Write-Error "VS Code project path not found: $projectPath"
}

$exeName = if ($IsWindows) { 'azmcp.exe' } else { 'azmcp' }

# 1. Check explicit parameter or existing environment variable
if (-not $ServerExecutablePath -and $env:MCP_SERVER_EXECUTABLE) {
    $ServerExecutablePath = $env:MCP_SERVER_EXECUTABLE
}

# 2. Check pipeline artifacts directory if running in CI or if ArtifactsPath was passed
if (-not $ServerExecutablePath) {
    $searchDir = if ($ArtifactsPath) { $ArtifactsPath } elseif ($env:PIPELINE_WORKSPACE) { $env:PIPELINE_WORKSPACE } else { $null }
    if ($searchDir -and (Test-Path -Path $searchDir -PathType Container)) {
        # Pin to the windows-x64 artifact explicitly. The pipeline downloads all
        # build artifacts (including windows-arm64), so a broad search would
        # non-deterministically pick the wrong architecture.
        $candidates = @(Get-ChildItem -Path $searchDir -Recurse -File -Include @($exeName) |
            Where-Object { $_.FullName -like '*Azure.Mcp.Server*windows-x64*' })
        if ($candidates.Count -eq 0) {
            Write-Error "MCP server executable not found under '$searchDir'. Expected a file matching '*Azure.Mcp.Server*windows-x64*$exeName'. Verify that the pipeline artifact was downloaded correctly."
        } elseif ($candidates.Count -gt 1) {
            $paths = ($candidates | ForEach-Object { $_.FullName }) -join "`n  "
            Write-Error "Ambiguous MCP server executable: $($candidates.Count) matches found under '$searchDir':`n  $paths`nRefine the search path or provide -ServerExecutablePath explicitly."
        } else {
            $ServerExecutablePath = $candidates[0].FullName
        }
    }
}

# 3. Check standard local build output paths
if (-not $ServerExecutablePath) {
    $localCandidates = @(
        "$RepoRoot/.work/build/Azure.Mcp.Server/windows-x64/$exeName",
        "$RepoRoot/.work/build/Azure.Mcp.Server/linux-x64/$exeName",
        "$RepoRoot/.work/build/Azure.Mcp.Server/macos-x64/$exeName",
        "$RepoRoot/.work/build/Azure.Mcp.Server/macos-arm64/$exeName",
        "$RepoRoot/servers/Azure.Mcp.Server/src/bin/Release/net10.0/$exeName",
        "$RepoRoot/servers/Azure.Mcp.Server/src/bin/Debug/net10.0/$exeName"
    )

    foreach ($candidate in $localCandidates) {
        if (Test-Path -Path $candidate -PathType Leaf) {
            $ServerExecutablePath = $candidate
            break
        }
    }
}

if (-not $ServerExecutablePath -or -not (Test-Path -Path $ServerExecutablePath -PathType Leaf)) {
    Write-Error "MCP server executable not found. Please provide -ServerExecutablePath, set `$env:MCP_SERVER_EXECUTABLE, or build Azure.Mcp.Server before running integration tests."
}

$ServerExecutablePath = (Resolve-Path $ServerExecutablePath).Path.Replace('\', '/')
Write-Host "Using MCP server executable: $ServerExecutablePath" -ForegroundColor Green
$env:MCP_SERVER_EXECUTABLE = $ServerExecutablePath

Push-Location $projectPath
try {
    Invoke-LoggedCommand 'npm ci --omit=optional'

    # Run the process lifecycle unit/regression tests before starting the full VS Code scenario.
    Invoke-LoggedCommand 'node --test src/test/outerloop/mcpProcessLifecycle.test.js'

    # The test clears VS Code download caches, fetches the test VS Code build,
    # and starts the built Azure MCP server via $env:MCP_SERVER_EXECUTABLE before driving the UI with Playwright.
    Invoke-LoggedCommand 'npm run outerloop-test'
}
finally {
    Pop-Location
}
