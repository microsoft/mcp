#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Signs an MCPB file using ESRP detached PKCS#7 signing or an existing .p7s file.

.DESCRIPTION
    This script enables signing of MCPB files using either:
    1. An existing .p7s signature file (for pipeline integration with ESRP)
    2. Self-signed mode using the mcpb CLI (for local development/testing)

    When using ESRP in the pipeline:
    1. ESRP's Pkcs7DetachedSign operation creates a .p7s file
    2. This script converts that .p7s to MCPB's embedded signature format
    3. The signature is appended to the MCPB file

    The MCPB signature format (per https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md):
    
    [Original MCPB ZIP content]
    MCPB_SIG_V1
    [4-byte little-endian length prefix]
    [DER-encoded PKCS#7 signature]
    MCPB_SIG_END

    This script assumes the 'mcpb' CLI is installed and available in PATH. The 'mcpb' CLI can be
    installed using the command: 'dotnet tool install --global Mcpb.Cli'.

.PARAMETER McpbFile
    Path to the unsigned .mcpb file.

.PARAMETER OutputFile
    Path for the signed output file. Defaults to replacing the input file (in-place signing).

.PARAMETER SignatureFile
    Path to an existing .p7s signature file from ESRP. If provided, skips signing
    and directly applies this signature.

.PARAMETER SelfSigned
    Use mcpb's built-in self-signed certificate for local testing.
    This is NOT suitable for production use.

.PARAMETER SkipVerification
    Skip signature verification after signing. Not recommended for production.

.EXAMPLE
    # Apply an existing ESRP signature
    ./Sign-McpbWithEsrp.ps1 -McpbFile ./server.mcpb -SignatureFile ./signature.p7s

.EXAMPLE
    # Self-signed for local testing
    ./Sign-McpbWithEsrp.ps1 -McpbFile ./server.mcpb -SelfSigned

.EXAMPLE
    # Apply signature with custom output path
    ./Sign-McpbWithEsrp.ps1 -McpbFile ./unsigned.mcpb -SignatureFile ./sig.p7s -OutputFile ./signed.mcpb
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $McpbFile,

    [string] $OutputFile,

    [string] $SignatureFile,

    [switch] $SelfSigned,

    [switch] $SkipVerification
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

# Validate inputs
if (-not (Test-Path $McpbFile)) {
    throw "MCPB file not found: $McpbFile"
}

if (-not $SelfSigned -and -not $SignatureFile) {
    throw "Either -SignatureFile or -SelfSigned must be specified."
}

if ($SelfSigned -and $SignatureFile) {
    throw "Cannot use both -SignatureFile and -SelfSigned. Choose one."
}

# Default output to input file (in-place)
if (-not $OutputFile) {
    $OutputFile = $McpbFile
}

$mcpbFileName = Split-Path $McpbFile -Leaf

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Signing MCPB: $mcpbFileName" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($SelfSigned) {
    # Use mcpb CLI's built-in self-signed mode
    Write-Host "Using self-signed certificate (for testing only)..." -ForegroundColor Yellow
    
    # If output is different from input, copy first
    if ($OutputFile -ne $McpbFile) {
        Copy-Item $McpbFile $OutputFile -Force
    }
    
    & mcpb sign $OutputFile --self-signed
    if ($LASTEXITCODE -ne 0) {
        throw "mcpb sign --self-signed failed for $mcpbFileName"
    }
    
    Write-Host "Self-signed signature applied." -ForegroundColor Green
}
else {
    # Apply existing .p7s signature from ESRP
    if (-not (Test-Path $SignatureFile)) {
        throw "Signature file not found: $SignatureFile"
    }
    
    Write-Host "Applying ESRP signature from: $SignatureFile"
    
    # Use the conversion script to apply the signature
    $convertScript = Join-Path $PSScriptRoot "Convert-P7sToMcpbSignature.ps1"
    
    if (-not (Test-Path $convertScript)) {
        throw "Convert-P7sToMcpbSignature.ps1 not found at: $convertScript"
    }
    
    try {
        & $convertScript -P7sFile $SignatureFile -McpbFile $McpbFile -OutputFile $OutputFile
    }
    catch {
        throw "Signature conversion failed for $mcpbFileName : $_"
    }
    
    # Verify the output file was created
    if (-not (Test-Path $OutputFile)) {
        throw "Signature conversion failed - output file not created: $OutputFile"
    }
}

# Verify signature unless skipped
if (-not $SkipVerification) {
    Write-Host "`nVerifying signature..."
    
    & mcpb verify $OutputFile
    $verifyExitCode = $LASTEXITCODE
    
    if ($verifyExitCode -ne 0) {
        throw "Signature verification failed for $mcpbFileName (exit code: $verifyExitCode)"
    }
    
    Write-Host "Signature verified successfully!" -ForegroundColor Green
}
else {
    Write-Host "`nSkipping verification (not recommended for production)." -ForegroundColor Yellow
}

# Show info about the signed package
Write-Host "`nSigned package info:"
& mcpb info $OutputFile

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "Signing completed: $mcpbFileName" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green
