#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Publishes signed MCPB files to a GitHub Release.

.DESCRIPTION
    This script uploads signed MCPB files to an existing GitHub Release.
    The release must already exist (typically created by the TagRepository job).

.PARAMETER ServerName
    Name of the server (e.g., Azure.Mcp.Server).

.PARAMETER McpbPath
    Path to the directory containing signed MCPB files.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and version information.

.EXAMPLE
    ./Publish-McpbToGitHub.ps1 -ServerName "Azure.Mcp.Server" `
        -McpbPath "./packages_mcpb_signed" `
        -BuildInfoPath "./build_info/build_info.json"
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $ServerName,

    [Parameter(Mandatory = $true)]
    [string] $McpbPath,

    [Parameter(Mandatory = $true)]
    [string] $BuildInfoPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file not found: $BuildInfoPath"
    exit 1
}

if (!(Test-Path $McpbPath)) {
    LogError "MCPB directory not found: $McpbPath"
    exit 1
}

# Verify gh CLI is available
$ghCmd = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghCmd) {
    LogError "GitHub CLI (gh) not found. Install from: https://cli.github.com/"
    exit 1
}

# Load build info
$buildInfo = Get-Content -Raw -Path $BuildInfoPath | ConvertFrom-Json

$server = $buildInfo.servers | Where-Object { $_.name -eq $ServerName } | Select-Object -First 1
if (-not $server) {
    LogError "Server '$ServerName' not found in build info."
    exit 1
}

$version = $server.version
$tag = $server.releaseTag

LogInfo "Server: $ServerName"
LogInfo "Version: $version"
LogInfo "Release Tag: $tag"

# Find MCPB files for this server
$serverMcpbPath = Join-Path $McpbPath $ServerName
if (-not (Test-Path $serverMcpbPath)) {
    LogError "No MCPB files found for $ServerName at $serverMcpbPath"
    exit 1
}

$mcpbFiles = Get-ChildItem -Path $serverMcpbPath -Filter "*.mcpb" -Recurse

if ($mcpbFiles.Count -eq 0) {
    LogError "No .mcpb files found in $serverMcpbPath"
    exit 1
}

LogInfo "`nUploading $($mcpbFiles.Count) MCPB file(s) to release $tag..."

$uploadedCount = 0
$failedCount = 0

foreach ($mcpb in $mcpbFiles) {
    LogInfo "  Uploading: $($mcpb.Name) ($($mcpb.Length) bytes)"
    
    & gh release upload $tag $mcpb.FullName --clobber
    
    if ($LASTEXITCODE -ne 0) {
        LogError "Failed to upload $($mcpb.Name)"
        $failedCount++
    } else {
        $uploadedCount++
    }
}

LogInfo "`nUpload Summary:"
LogInfo "  Uploaded: $uploadedCount"
LogInfo "  Failed: $failedCount"

if ($failedCount -gt 0) {
    LogError "Some MCPB files failed to upload"
    exit 1
}

LogInfo "`n✓ All MCPB files uploaded successfully"
