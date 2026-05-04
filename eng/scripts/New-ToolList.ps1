#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Generates a tools.json metadata files for each MCP server.

.DESCRIPTION
    Builds each server and runs 'tools list --include-hidden' to produce a canonical
    tools.json file at servers/<Server>/tools.json.

    Use -Verify to check that the committed tools.json files are up-to-date without
    overwriting them (exits non-zero on drift).

.PARAMETER Verify
    When set, compares the generated output against the committed files and fails if
    any difference is detected.

.PARAMETER ServerName
    Optional server directory name (e.g., 'Azure.Mcp.Server') to limit regeneration
    to a single server. If not specified, all servers are processed.
#>

[CmdletBinding()]
param(
    [string] $ServerName
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$ServersDirectory = Join-Path $RepoRoot "servers"
$outputDirectory = Join-Path $RepoRoot ".work/tools"

New-Item -Path $outputDirectory -ItemType Directory -Force | Out-Null

# Discover servers to process
$serverDirs = Get-ChildItem -Path $ServersDirectory -Directory |
    Where-Object { Test-Path (Join-Path $_.FullName "src" "*.csproj") }

if ($ServerName) {
    $serverDirs = $serverDirs | Where-Object { $_.Name -eq $ServerName }
    if (-not $serverDirs) {
        Write-Error "Server '$ServerName' not found under $ServersDirectory"
        exit 1
    }
}

$hasErrors = $false

foreach ($serverDir in $serverDirs) {
    $projectFile = (Get-ChildItem -Path (Join-Path $serverDir.FullName "src") -Filter "*.csproj" | Select-Object -First 1).FullName
    $toolsJsonPath = Join-Path $outputDirectory "$($serverDir.Name)-tools.json"

    Write-Host "Processing $($serverDir.Name)..."

    # Run 'tools list --include-hidden' via dotnet run
    try {
        $rawOutput = dotnet run --project $projectFile --no-launch-profile -- tools list --include-hidden 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ❌ 'tools list' failed for $($serverDir.Name) (exit code $LASTEXITCODE)" -ForegroundColor Red
            $hasErrors = $true
            continue
        }

        $jsonString = ($rawOutput | Where-Object { $_ -is [string] }) -join "`n"
        $parsed = $jsonString | ConvertFrom-Json

        if ($null -eq $parsed -or $null -eq $parsed.results) {
            Write-Warning "  ❌ Invalid 'tools list' response for $($serverDir.Name)" -ForegroundColor Red
            $hasErrors = $true
            continue
        }

        # the tool list is already sorted.
        # Ensuring options are also sorted makes the output deterministic
        $output = @($parsed.results | ForEach-Object {
            $tool = $_
            
            if ($tool.option -and $tool.option.Count -gt 0) {
                $tool.option = @($tool.option | Sort-Object -Property name)
            }
            
            $tool            
        })

        $formatted = $output | ConvertTo-Json -Depth 10

    } catch {
        Write-Host "  ❌ Error processing $($serverDir.Name): $_" -ForegroundColor Red
        $hasErrors = $true
        continue
    }

    # Write the file
    $formatted | Set-Content -Path $toolsJsonPath -NoNewline
    # Ensure trailing newline
    Add-Content -Path $toolsJsonPath -Value ""
    Write-Host "  ✅ Updated $toolsJsonPath ($($output.Count) tools)"
}

if ($hasErrors) {
    Write-Host ""
    Write-Host "❌ errors encountered while updating tools.json files. Please review the output above." -ForegroundColor Red
    exit 1
}
