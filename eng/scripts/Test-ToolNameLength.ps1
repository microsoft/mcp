#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates that all tool names don't exceed the maximum length

.DESCRIPTION
    This script validates that tool name length (including root command prefix)
    doesn't exceed 48 characters.
    
    Tool name format: {root}_{area}_{resource}_{operation}
    Example: "azmcp_managedlustre_filesystem_subnetsize_validate" = 52 chars (EXCEEDS)
    
    The limit does NOT include the MCP server prefix (e.g., "AzureMCP-AllTools-").

.PARAMETER MaxLength
    Maximum allowed length for tool names including root prefix (default: 48)

.PARAMETER RootPrefix
    The root command prefix that is prepended to all tool names (default: "azmcp")
    
#>

param(
    [int]$MaxLength = 48,
    [string]$RootPrefix = "azmcp"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. "$PSScriptRoot/../common/scripts/common.ps1"

Write-Host "Validating tool name length"
Write-Host "Max length: $MaxLength characters (including root prefix)"
Write-Host "Root prefix: '${RootPrefix}_' ($($RootPrefix.Length + 1) chars)"
Write-Host ""

$serverBinPath = "$RepoRoot/servers/Azure.Mcp.Server/src/bin/Debug/net9.0"
$executableName = if ($IsWindows) { "azmcp.exe" } else { "azmcp" }
$executablePath = Join-Path $serverBinPath $executableName

# Check if the executable exists (build should have happened before this script runs)
if (-not (Test-Path $executablePath)) {
    Write-Error "azmcp executable not found at: $executablePath. Please build the solution first using 'dotnet build'."
    exit 1
}

# Get all tools using the 'tools list' command
Write-Host "Loading tools from Azure MCP Server"
$toolsJson = & $executablePath tools list | Out-String

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to list tools from Azure MCP Server (exit code: $LASTEXITCODE)"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($toolsJson)) {
    Write-Error "No output received from 'azmcp tools list' command"
    exit 1
}

try {
    $toolsResult = $toolsJson | ConvertFrom-Json
    $tools = $toolsResult.results
}
catch {
    Write-Error "Failed to parse tools JSON output: $_"
    Write-Host "Raw output:"
    Write-Host $toolsJson
    exit 1
}

if ($null -eq $tools -or $tools.Count -eq 0) {
    Write-Error "No tools found in the output"
    exit 1
}

Write-Host "Loaded $($tools.Count) tools"
Write-Host ""

# Validate tool name lengths (including root prefix)
$violations = @()
$maxToolNameLength = 0
$longestFullToolName = ""

foreach ($tool in $tools) {
    $toolName = $tool.command -replace ' ', '_'
    
    $fullToolName = "${RootPrefix}_${toolName}"
    $fullLength = $fullToolName.Length
    
    if ($fullLength -gt $maxToolNameLength) {
        $maxToolNameLength = $fullLength
        $longestFullToolName = $fullToolName
    }
    
    if ($fullLength -gt $MaxLength) {
        $violations += [PSCustomObject]@{
            ToolName = $toolName
            FullToolName = $fullToolName
            Command = $tool.command
            Length = $fullLength
            Excess = $fullLength - $MaxLength
        }
    }
}

# Report results
Write-Host "Analysis Results:"
Write-Host "Longest full tool name: $maxToolNameLength characters"
Write-Host "Tool: '$longestFullToolName'"
Write-Host ""

if ($violations.Count -eq 0) {
    Write-Host "All $($tools.Count) tool names are within the $MaxLength character limit!"
    Write-Host "(including '${RootPrefix}_' prefix)"
    exit 0
}
else {
    Write-Host "Found $($violations.Count) tool name(s) exceeding the $MaxLength character limit:"
    Write-Host ""
    
    $violations | Sort-Object -Property Length -Descending | ForEach-Object {
        Write-Host "    $($_.FullToolName)"
        Write-Host "      Command: $($_.Command)"
        Write-Host "      Length: $($_.Length) characters (exceeds by $($_.Excess))"
        Write-Host ""
    }

    Write-Host ""
    
    exit 1
}