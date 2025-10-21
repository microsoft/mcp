#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates that all tool names don't exceed the maximum length

.DESCRIPTION
    This script validates that tool name length doesn't exceed 48 characters.
    
    Tool name format: {area}_{resource}_{operation}
    Example: "managedlustre_filesystem_subnetsize_validate-length" = 50 chars (EXCEEDS)
    The limit does NOT include the MCP server prefix (e.g., "AzureMCP-AllTools-").

.PARAMETER MaxLength
    Maximum allowed length for tool names (default: 48)

.PARAMETER ServerName
    Name of the server to test. If not specified, all servers will be tested.
#>

param(
    [int]$MaxLength = 48,
    [string]$ServerName
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if ($ServerName) {
    Write-Host "Validating tool name length for $ServerName"
} else {
    Write-Host "Validating tool name length for all servers"
}
Write-Host "Max length: $MaxLength characters"
Write-Host ""

# Use the build infrastructure - New-BuildInfo.ps1 and Build-Code.ps1
$buildInfoPath = "$RepoRoot/.work/build_info.json"
$buildOutputPath = "$RepoRoot/.work/build"

# Clean up previous build artifacts
Remove-Item -Path $buildOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

# Create build metadata
& "$RepoRoot/eng/scripts/New-BuildInfo.ps1" `
    -ServerName $ServerName `
    -PublishTarget none `
    -BuildId 12345

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create build info"
    exit 1
}

# Build the server
& "$RepoRoot/eng/scripts/Build-Code.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build $ServerName"
    exit 1
}

# Read build_info.json to get server information
$buildInfo = Get-Content $buildInfoPath -Raw | ConvertFrom-Json -AsHashtable

# Get servers to test
$serversToTest = $buildInfo.servers
if (-not $serversToTest -or $serversToTest.Count -eq 0) {
    Write-Error "No servers found in build_info.json"
    exit 1
}

Write-Host "Testing $($serversToTest.Count) server(s)"
Write-Host ""

# Track overall results
$overallViolations = @()
$overallSuccess = $true
$testedServers = 0
$skippedServers = 0

foreach ($serverInfo in $serversToTest) {
    $currentServerName = $serverInfo.name
    Write-Host "=================================================="
    Write-Host "Testing: $currentServerName"
    Write-Host "=================================================="
    
    # Get the executable name and find the built platform
    $executableName = $serverInfo.cliName + $(if ($IsWindows) { ".exe" } else { "" })

    # Find the first platform that was actually built
    $builtPlatform = $serverInfo.platforms | Where-Object { 
        Test-Path "$buildOutputPath/$($_.artifactPath)" 
    } | Select-Object -First 1

    if (-not $builtPlatform) {
        Write-Warning "No built platform found for $currentServerName - skipping"
        $skippedServers++
        Write-Host ""
        continue
    }

    $executablePath = "$buildOutputPath/$($builtPlatform.artifactPath)/$executableName"

    if (-not (Test-Path $executablePath)) {
        Write-Error "Executable not found at $executablePath for $currentServerName"
        exit 1
    }

    # Try to get tools - some servers may not support 'tools list'
    Write-Host "Loading tools from $currentServerName"
    try {
        $toolsJson = & $executablePath tools list 2>&1 | Out-String

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "$currentServerName 'tools list' command failed with exit code $LASTEXITCODE (may have no tools) - skipping"
            $skippedServers++
            Write-Host ""
            continue
        }

        if ([string]::IsNullOrWhiteSpace($toolsJson)) {
            Write-Warning "No output received from '$currentServerName tools list' - skipping"
            $skippedServers++
            Write-Host ""
            continue
        }

        $toolsResult = $toolsJson | ConvertFrom-Json
        $tools = $toolsResult.results

        if ($null -eq $tools -or $tools.Count -eq 0) {
            Write-Warning "No tools found in $currentServerName - skipping"
            $skippedServers++
            Write-Host ""
            continue
        }

        Write-Host "Loaded $($tools.Count) tools"
        $testedServers++

        # Validate tool name lengths
        $violations = @()
        $maxToolNameLength = 0

        foreach ($tool in $tools) {
            $toolName = $tool.command -replace ' ', '_'
            $fullLength = $toolName.Length
            
            if ($fullLength -gt $maxToolNameLength) {
                $maxToolNameLength = $fullLength
            }
            
            if ($fullLength -gt $MaxLength) {
                $violations += [PSCustomObject]@{
                    Server = $currentServerName
                    ToolName = $toolName
                    Command = $tool.command
                    Length = $fullLength
                    Excess = $fullLength - $MaxLength
                }
            }
        }

        Write-Host "Longest tool name: $maxToolNameLength characters"

        if ($violations.Count -eq 0) {
            Write-Host "All $($tools.Count) tool names are within the $MaxLength character limit!" -ForegroundColor Green
        }
        else {
            Write-Host "Found $($violations.Count) violation(s):" -ForegroundColor Red
            $violations | ForEach-Object {
                Write-Host "  - $($_.ToolName) ($($_.Length) chars, exceeds by $($_.Excess))" -ForegroundColor Red
            }
            $overallViolations += $violations
            $overallSuccess = $false
        }
    }
    catch {
        Write-Warning "Error testing $currentServerName : $_"
        Write-Host "This server may not support tool validation - skipping"
        $skippedServers++
    }
    
    Write-Host ""
}

# Final summary
Write-Host "=================================================="
Write-Host "SUMMARY"
Write-Host "=================================================="
Write-Host "Servers tested: $testedServers"
Write-Host "Servers skipped: $skippedServers"
Write-Host "Total violations: $($overallViolations.Count)"
Write-Host ""

if ($overallViolations.Count -gt 0) {
    Write-Host "VIOLATIONS FOUND:" -ForegroundColor Red
    Write-Host ""
    
    $overallViolations | Sort-Object -Property Length -Descending | ForEach-Object {
        Write-Host "  Server: $($_.Server)"
        Write-Host "  Tool: $($_.ToolName)"
        Write-Host "  Command: $($_.Command)"
        Write-Host "  Length: $($_.Length) characters (exceeds by $($_.Excess))"
        Write-Host ""
    }
}

# Prepare return object
$result = [PSCustomObject]@{
    MaxAllowed     = $MaxLength
    ServersTested  = $testedServers
    ServersSkipped = $skippedServers
    ViolationCount = $overallViolations.Count
}

$result

if ($overallSuccess -and $testedServers -gt 0) {
    Write-Host "All tested servers passed validation!" -ForegroundColor Green
    exit 0
}
elseif ($testedServers -eq 0) {
    Write-Error "No servers were successfully tested. All $($skippedServers) server(s) were skipped."
    exit 1
}
else {
    Write-Host "Validation failed - see violations above" -ForegroundColor Red
    exit 1
}