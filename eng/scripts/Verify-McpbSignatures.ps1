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
    If set, the script will exit with an error code if any verification fails,
    including trust chain warnings. Without this flag, the script will still fail
    on genuinely invalid signatures but will tolerate trust chain issues (expected
    for Microsoft-signed packages when the certificate chain is not in the trust store).

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
    LogInfo "Installing MCPB CLI..."
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
$failedCount = 0

foreach ($mcpb in $mcpbFiles) {
    LogInfo "`n=== Verifying: $($mcpb.Name) ==="
    
    # Show bundle info
    & mcpb info $mcpb.FullName
    
    # Verify signature and capture output for classification
    $verifyOutput = & mcpb verify $mcpb.FullName 2>&1 | Out-String
    $verifyExitCode = $LASTEXITCODE

    LogInfo $verifyOutput
    
    if ($verifyExitCode -eq 0) {
        LogInfo "✓ $($mcpb.Name) - Signature verified"
        $passedCount++
    } else {
        # Distinguish untrusted certificate chain (expected in CI for Microsoft-signed
        # packages) from genuinely invalid or corrupt signatures.
        $isTrustChainIssue = $verifyOutput -match '(?i)(untrusted|chain|trust|certificate.*not found|root.*not.*trusted|certificate.*expired)'
        
        if ($isTrustChainIssue) {
            LogWarning "✗ $($mcpb.Name) - Certificate chain not trusted (exit code $verifyExitCode). This is expected for Microsoft-signed packages in CI."
            $warningCount++
        } else {
            LogError "✗ $($mcpb.Name) - Signature verification failed (exit code $verifyExitCode)"
            $failedCount++
        }
    }
}

LogInfo "`n=== Verification Summary ==="
LogInfo "  Passed: $passedCount"
LogInfo "  Warnings (trust chain): $warningCount"
LogInfo "  Failed (invalid signature): $failedCount"
LogInfo "  Total: $($mcpbFiles.Count)"

# Always fail on genuinely invalid signatures regardless of -FailOnError
if ($failedCount -gt 0) {
    LogError "Some MCPB files have invalid signatures"
    exit 1
}

# Trust chain warnings are only fatal when -FailOnError is set
if ($FailOnError -and $warningCount -gt 0) {
    LogError "Some MCPB files had trust chain warnings and -FailOnError is set"
    exit 1
}

LogInfo "`nVerification complete"
