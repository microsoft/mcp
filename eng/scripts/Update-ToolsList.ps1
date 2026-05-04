#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Regenerates the tools.json metadata files for each MCP server.

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
    [switch] $Verify,
    [string] $ServerName
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$ServersDirectory = Join-Path $RepoRoot "servers"

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
    $toolsJsonPath = Join-Path $serverDir.FullName "tools.json"

    Write-Host "Processing $($serverDir.Name)..."

    # Run 'tools list --include-hidden' via dotnet run
    try {
        $rawOutput = dotnet run --project $projectFile --no-launch-profile -- tools list --include-hidden 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "  'tools list' failed for $($serverDir.Name) (exit code $LASTEXITCODE) - skipping"
            continue
        }

        $jsonString = ($rawOutput | Where-Object { $_ -is [string] }) -join "`n"
        $parsed = $jsonString | ConvertFrom-Json

        if ($null -eq $parsed -or $null -eq $parsed.results) {
            Write-Warning "  No results returned for $($serverDir.Name) - skipping"
            continue
        }

        # Extract just the results array and flatten metadata to booleans
        $toolsArray = @($parsed.results | ForEach-Object {
            $tool = [ordered]@{
                id          = $_.id
                name        = $_.name
                description = $_.description
                command     = $_.command
                option      = $_.option
            }
            if ($_.metadata) {
                $tool.destructive   = [bool]$_.metadata.destructive.value
                $tool.idempotent    = [bool]$_.metadata.idempotent.value
                $tool.openWorld     = [bool]$_.metadata.openWorld.value
                $tool.readOnly      = [bool]$_.metadata.readOnly.value
                $tool.secret        = [bool]$_.metadata.secret.value
                $tool.localRequired = [bool]$_.metadata.localRequired.value
            }
            [PSCustomObject]$tool
        })
        $formatted = $toolsArray | ConvertTo-Json -Depth 10

    } catch {
        Write-Warning "  Error processing $($serverDir.Name): $_"
        continue
    }

    if ($Verify) {
        # Compare against committed file
        if (-not (Test-Path $toolsJsonPath)) {
            Write-Host "  ❌ tools.json does not exist. Run 'eng/scripts/Update-ToolsList.ps1' to generate it." -ForegroundColor Red
            $hasErrors = $true
            continue
        }

        $committed = Get-Content $toolsJsonPath -Raw
        if ($committed.TrimEnd() -ne $formatted.TrimEnd()) {
            Write-Host "  ❌ tools.json is out of date. Run 'eng/scripts/Update-ToolsList.ps1' to regenerate." -ForegroundColor Red
            $hasErrors = $true
        } else {
            Write-Host "  ✅ tools.json is up to date."
        }
    } else {
        # Write the file
        $formatted | Set-Content -Path $toolsJsonPath -NoNewline
        # Ensure trailing newline
        Add-Content -Path $toolsJsonPath -Value ""
        Write-Host "  ✅ Updated $toolsJsonPath ($($toolsArray.Count) tools)"
    }
}

if ($hasErrors) {
    Write-Host ""
    Write-Host "❌ tools.json drift detected. Please run 'eng/scripts/Update-ToolsList.ps1' and commit the changes." -ForegroundColor Red
    exit 1
}
