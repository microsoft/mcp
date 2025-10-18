#!/bin/env pwsh
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

.EXAMPLE
    ./ToolNameLength.ps1

.EXAMPLE
    ./ToolNameLength.ps1 -MaxLength 50
#>

param(
    [int]$MaxLength = 48,
    [string]$RootPrefix = "azmcp"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. "$PSScriptRoot/../common/scripts/common.ps1"

Write-Host "Validating tool name lengths-"
Write-Host "Max length: $MaxLength characters (including root prefix)"
Write-Host "Root prefix: '${RootPrefix}_' ($($RootPrefix.Length + 1) chars)"
Write-Host ""

$serverBinPath = "$RepoRoot/servers/Azure.Mcp.Server/src/bin/Debug/net9.0"
$executableName = if ($IsWindows) { "azmcp.exe" } else { "azmcp" }
$executablePath = Join-Path $serverBinPath $executableName

# Build the server if not already built
if (-not (Test-Path $executablePath)) {
    Write-Host "Building Azure MCP Server..."
    Push-Location "$RepoRoot/servers/Azure.Mcp.Server/src"
    try {
        dotnet build --configuration Debug --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build Azure MCP Server"
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# Get all tools using the 'tools list' command
Write-Host "Loading tools from Azure MCP Server"
$toolsJson = & $executablePath tools list 2>&1 | Out-String

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to list tools from Azure MCP Server"
    Write-Host $toolsJson
    exit 1
}

try {
    $toolsResult = $toolsJson | ConvertFrom-Json
    $tools = $toolsResult.results
}
catch {
    Write-Host "Failed to parse tools JSON output"
    Write-Host $toolsJson
    exit 1
}

Write-Host "Loaded $($tools.Count) tools"
Write-Host ""

# Validate tool name lengths (including root prefix)
$violations = @()
$maxToolNameLength = 0
$longestFullToolName = ""

foreach ($tool in $tools) {
    # The 'command' field contains the full path with spaces (e.g., "subscription list")
    # Convert spaces to underscores to get the tokenized name
    $toolName = $tool.command -replace ' ', '_'
    
    # Build full tool name with root prefix: azmcp_{toolName}
    $fullToolName = "${RootPrefix}_${toolName}"
    Write-Host "Checking tool: $fullToolName"
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