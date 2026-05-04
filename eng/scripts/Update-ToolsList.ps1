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


        # --- Common options extraction ---
        # Build a 2-level map: optionName -> jsonForm -> count
        # to find the most-used definition for each option name.
        $optionHashMap = @{} # optionName -> @{ json -> @{ count=N; definition=obj } }

        foreach ($tool in $parsed.results) {
            if (-not $tool.option) { continue }
            foreach ($opt in $tool.option) {
                $optName = $opt.name
                $optJson = $opt | ConvertTo-Json -Depth 5 -Compress

                if (-not $optionHashMap.ContainsKey($optName)) {
                    $optionHashMap[$optName] = @{}
                }
                if (-not $optionHashMap[$optName].ContainsKey($optJson)) {
                    $optionHashMap[$optName][$optJson] = @{ count = 0; definition = $opt }
                }
                $optionHashMap[$optName][$optJson].count++
            }
        }

        # For each option name, find the most common hash; if count > 10, promote to common.
        $commonOptions = [ordered]@{}
        foreach ($optName in ($optionHashMap.Keys | Sort-Object)) {
            $hashes = $optionHashMap[$optName]
            $best = $hashes.GetEnumerator() | Sort-Object { $_.Value.count } -Descending | Select-Object -First 1
            if ($best.Value.count -gt 10) {
                $commonOptions[$optName] = $best.Value.definition
            }
        }

        # Rebuild tools array: replace matching options with commonOptions reference
        $finalTools = @($parsed.results | ForEach-Object {
            $tool = $_
            $remainingOptions = @()
            $commonOptionNames = @()

            if ($tool.option -and $commonOptions.Count -gt 0) {
                foreach ($opt in $tool.option) {
                    $optName = $opt.name
                    if ($commonOptions.Contains($optName)) {
                        # Check if this option matches the common definition
                        $optJson = $opt | ConvertTo-Json -Depth 5 -Compress
                        $commonJson = $commonOptions[$optName] | ConvertTo-Json -Depth 5 -Compress
                        if ($optJson -eq $commonJson) {
                            $commonOptionNames += $optName
                        } else {
                            $remainingOptions += $opt
                        }
                    } else {
                        $remainingOptions += $opt
                    }
                }
            }

            $result = [ordered] @{
                id            = $tool.id
                name          = $tool.name
                description   = $tool.description
                command       = $tool.command
                commonOptions = $commonOptionNames
                options       = $remainingOptions
            }
            if ($null -ne $tool.metadata) {
                $result.destructive   = $tool.metadata.destructive.value
                $result.idempotent    = $tool.metadata.idempotent.value
                $result.openWorld     = $tool.metadata.openWorld.value
                $result.readOnly      = $tool.metadata.readOnly.value
                $result.secret        = $tool.metadata.secret.value
                $result.localRequired = $tool.metadata.localRequired.value
            }
            [PSCustomObject]$result            
        })

        # Build final output object
        if ($commonOptions.Count -gt 0) {
            $output = [ordered]@{
                commonOptions = $commonOptions
                tools         = $finalTools
            }
        } else {
            $output = $finalTools
        }

        $formatted = $output | ConvertTo-Json -Depth 10

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
        $commonCount = $commonOptions.Count
        Write-Host "  ✅ Updated $toolsJsonPath ($($finalTools.Count) tools, $commonCount common options)"
    }
}

if ($hasErrors) {
    Write-Host ""
    Write-Host "❌ tools.json drift detected. Please run 'eng/scripts/Update-ToolsList.ps1' and commit the changes." -ForegroundColor Red
    exit 1
}
