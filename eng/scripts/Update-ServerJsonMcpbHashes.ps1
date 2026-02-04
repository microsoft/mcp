#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Updates the server.json file with SHA256 hashes for signed MCPB packages.

.DESCRIPTION
    This script computes SHA256 hashes for signed MCPB files and updates the
    corresponding fileSha256 fields in server.json. This is required for
    publishing to the MCP Registry, which validates file integrity.

.PARAMETER ServerName
    Name of the server (e.g., Azure.Mcp.Server).

.PARAMETER McpbPath
    Path to the directory containing signed MCPB files.

.PARAMETER ServerJsonPath
    Path to the server.json file to update.

.PARAMETER OutputPath
    Optional path for the updated server.json. Defaults to updating in place.

.EXAMPLE
    ./Update-ServerJsonMcpbHashes.ps1 -ServerName "Azure.Mcp.Server" `
        -McpbPath "./packages_mcpb_signed/Azure.Mcp.Server" `
        -ServerJsonPath "./build_info/Azure.Mcp.Server/server.json"
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $ServerName,

    [Parameter(Mandatory = $true)]
    [string] $McpbPath,

    [Parameter(Mandatory = $true)]
    [string] $ServerJsonPath,

    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

if (!(Test-Path $ServerJsonPath)) {
    LogError "Server JSON file not found: $ServerJsonPath"
    exit 1
}

if (!(Test-Path $McpbPath)) {
    LogError "MCPB directory not found: $McpbPath"
    exit 1
}

# Map platform names to their placeholder suffixes
$platformToPlaceholder = @{
    'win-x64' = 'WinX64'
    'linux-x64' = 'LinuxX64'
    'osx-x64' = 'OsxX64'
    'osx-arm64' = 'OsxArm64'
}

# Find all MCPB files and compute their SHA256 hashes
$mcpbFiles = Get-ChildItem -Path $McpbPath -Filter "*.mcpb" -Recurse
$hashes = @{}

foreach ($mcpb in $mcpbFiles) {
    # Extract platform from filename (e.g., "Azure.Mcp.Server-win-x64.mcpb" -> "win-x64")
    if ($mcpb.BaseName -match "^$([regex]::Escape($ServerName))-(.+)$") {
        $platform = $Matches[1]
        
        # Compute SHA256 hash
        $hash = (Get-FileHash -Path $mcpb.FullName -Algorithm SHA256).Hash.ToLower()
        $hashes[$platform] = $hash
        
        LogInfo "  $($mcpb.Name): $hash"
    } else {
        LogWarning "Could not extract platform from MCPB filename: $($mcpb.Name)"
    }
}

if ($hashes.Count -eq 0) {
    LogError "No MCPB files found in $McpbPath"
    exit 1
}

# Read and update server.json
$serverJson = Get-Content -Path $ServerJsonPath -Raw | ConvertFrom-Json -AsHashtable

$updatedCount = 0
foreach ($package in $serverJson.packages) {
    if ($package.registryType -eq 'mcpb') {
        # Find the platform from the identifier URL (e.g., "-win-x64.mcpb")
        foreach ($platform in $platformToPlaceholder.Keys) {
            if ($package.identifier -like "*-$platform.mcpb*") {
                if ($hashes.ContainsKey($platform)) {
                    $package.fileSha256 = $hashes[$platform]
                    $updatedCount++
                    LogInfo "Updated SHA256 for $platform package"
                } else {
                    LogWarning "No hash found for platform $platform"
                }
                break
            }
        }
    }
}

if ($updatedCount -eq 0) {
    LogWarning "No MCPB package hashes were updated in server.json"
}

# Write updated server.json
$outputFile = if ($OutputPath) { $OutputPath } else { $ServerJsonPath }
$updatedJson = $serverJson | ConvertTo-Json -Depth 10

New-Item -ItemType File -Force -Path $outputFile | Out-Null
Set-Content -Path $outputFile -Value $updatedJson -Encoding UTF8 -NoNewLine

LogInfo "Updated server.json written to $outputFile ($updatedCount MCPB hashes updated)"
