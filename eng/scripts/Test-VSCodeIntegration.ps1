#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $VsCodeProjectPath = 'servers/Azure.Mcp.Server/vscode'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\\', '/')
$projectPath = Join-Path $RepoRoot $VsCodeProjectPath

if (!(Test-Path $projectPath)) {
    Write-Error "VS Code project path not found: $projectPath"
}

Push-Location $projectPath
try {
    Invoke-LoggedCommand 'npm ci --omit=optional'

    # The test clears VS Code download caches, fetches the latest stable VS Code build,
    # and starts Azure MCP via npx @azure/mcp@latest before driving the UI with Playwright.
    Invoke-LoggedCommand 'npm run outerloop-test'
}
finally {
    Pop-Location
}
