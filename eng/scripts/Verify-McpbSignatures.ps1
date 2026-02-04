#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Verifies signatures on MCPB files.

.DESCRIPTION
    This script verifies the signatures on all MCPB files in a directory
    using the mcpb CLI tool. It provides detailed output about each file
    and summarizes the verification results.

.PARAMETER ArtifactsPath
    Path to the directory containing signed MCPB files.

.PARAMETER FailOnError
    If set, the script will exit with an error code if any verification fails.
    Default is false because mcpb verify may return non-zero if the certificate
    chain is not in the trust store (expected for Microsoft-signed packages).

.EXAMPLE
    ./Verify-McpbSignatures.ps1 -ArtifactsPath "./signed"

.EXAMPLE
    ./Verify-McpbSignatures.ps1 -ArtifactsPath "./signed" -FailOnError
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $ArtifactsPath,

    [switch] $FailOnError
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

if (!(Test-Path $ArtifactsPath)) {
    LogError "MCPB directory not found: $ArtifactsPath"
    exit 1
}

# Ensure MCPB CLI is installed
if (-not (Get-Command mcpb -ErrorAction SilentlyContinue)) {
    Write-Host "Installing MCPB CLI..."
    Invoke-LoggedCommand 'dotnet tool install --global Mcpb.Cli'
}

LogInfo "Verifying signed MCPB files..."

$mcpbFiles = Get-ChildItem -Path $ArtifactsPath -Filter "*.mcpb" -Recurse

if ($mcpbFiles.Count -eq 0) {
    LogError "No .mcpb files found in $ArtifactsPath"
    exit 1
}

$passedCount = 0
$warningCount = 0

foreach ($mcpb in $mcpbFiles) {
    LogInfo "`n=== Verifying: $($mcpb.Name) ==="
    
    # Show bundle info
    & mcpb info $mcpb.FullName
    
    # Verify signature
    & mcpb verify $mcpb.FullName
    
    if ($LASTEXITCODE -eq 0) {
        LogInfo "✓ $($mcpb.Name) - Signature verified"
        $passedCount++
    } else {
        LogWarning "✗ $($mcpb.Name) - Verification returned exit code $LASTEXITCODE"
        # Note: mcpb verify may return non-zero if certificate chain not in trust store
        # This is acceptable for Microsoft-signed packages
        $warningCount++
    }
}

LogInfo "`n=== Verification Summary ==="
LogInfo "  Passed: $passedCount"
LogInfo "  Warnings: $warningCount"
LogInfo "  Total: $($mcpbFiles.Count)"

if ($FailOnError -and $warningCount -gt 0) {
    LogError "Some MCPB files failed verification"
    exit 1
}

LogInfo "`nVerification complete"
