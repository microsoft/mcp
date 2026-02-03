#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Converts a PKCS#7 detached signature (.p7s) to MCPB embedded signature format.

.DESCRIPTION
    This script takes a detached PKCS#7 signature file (.p7s) and an unsigned MCPB file,
    then creates a signed MCPB file by appending the signature in MCPB's embedded format.

    The MCPB signature format (per https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md):
    
    [Original MCPB ZIP content]
    MCPB_SIG_V1
    [4-byte little-endian length prefix]
    [DER-encoded PKCS#7 signature]
    MCPB_SIG_END

.PARAMETER P7sFile
    Path to the detached PKCS#7 signature file (.p7s).

.PARAMETER McpbFile
    Path to the unsigned MCPB file.

.PARAMETER OutputFile
    Path for the signed MCPB output file. If not specified, defaults to the input
    MCPB file path (in-place signing).

.EXAMPLE
    ./Convert-P7sToMcpbSignature.ps1 -P7sFile ./signature.p7s -McpbFile ./server.mcpb

.EXAMPLE
    ./Convert-P7sToMcpbSignature.ps1 -P7sFile ./signature.p7s -McpbFile ./unsigned.mcpb -OutputFile ./signed.mcpb
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $P7sFile,

    [Parameter(Mandatory)]
    [string] $McpbFile,

    [string] $OutputFile
)

$ErrorActionPreference = "Stop"

# Default output to input file (in-place)
if (-not $OutputFile) {
    $OutputFile = $McpbFile
}

# Validate inputs
if (-not (Test-Path $P7sFile)) {
    throw "Signature file not found: $P7sFile"
}

if (-not (Test-Path $McpbFile)) {
    throw "MCPB file not found: $McpbFile"
}

# MCPB signature markers (ASCII)
$SIG_V1_MARKER = "MCPB_SIG_V1"
$SIG_END_MARKER = "MCPB_SIG_END"

Write-Host "Converting PKCS#7 signature to MCPB format..."
Write-Host "  Signature: $P7sFile"
Write-Host "  MCPB: $McpbFile"
Write-Host "  Output: $OutputFile"

# Read signature bytes
$signatureBytes = [System.IO.File]::ReadAllBytes($P7sFile)
Write-Host "  Signature size: $($signatureBytes.Length) bytes"

# Create length prefix (4-byte little-endian)
$lengthBytes = [BitConverter]::GetBytes([uint32]$signatureBytes.Length)

# Create markers as byte arrays
$sigV1MarkerBytes = [System.Text.Encoding]::ASCII.GetBytes($SIG_V1_MARKER)
$sigEndMarkerBytes = [System.Text.Encoding]::ASCII.GetBytes($SIG_END_MARKER)

# Read original MCPB content
$mcpbContent = [System.IO.File]::ReadAllBytes($McpbFile)
Write-Host "  MCPB size: $($mcpbContent.Length) bytes"

# Check if the MCPB is already signed (contains MCPB_SIG_V1 marker)
$mcpbString = [System.Text.Encoding]::ASCII.GetString($mcpbContent)
if ($mcpbString.Contains($SIG_V1_MARKER)) {
    throw "MCPB file appears to already be signed. Use 'mcpb unsign' to remove existing signature first."
}

# Combine: MCPB + MCPB_SIG_V1 + length + signature + MCPB_SIG_END
$signedContent = New-Object byte[] ($mcpbContent.Length + $sigV1MarkerBytes.Length + $lengthBytes.Length + $signatureBytes.Length + $sigEndMarkerBytes.Length)

$offset = 0

# Copy MCPB content
[Array]::Copy($mcpbContent, 0, $signedContent, $offset, $mcpbContent.Length)
$offset += $mcpbContent.Length

# Copy MCPB_SIG_V1 marker
[Array]::Copy($sigV1MarkerBytes, 0, $signedContent, $offset, $sigV1MarkerBytes.Length)
$offset += $sigV1MarkerBytes.Length

# Copy length prefix
[Array]::Copy($lengthBytes, 0, $signedContent, $offset, $lengthBytes.Length)
$offset += $lengthBytes.Length

# Copy signature
[Array]::Copy($signatureBytes, 0, $signedContent, $offset, $signatureBytes.Length)
$offset += $signatureBytes.Length

# Copy MCPB_SIG_END marker
[Array]::Copy($sigEndMarkerBytes, 0, $signedContent, $offset, $sigEndMarkerBytes.Length)

# Write signed file
[System.IO.File]::WriteAllBytes($OutputFile, $signedContent)

$finalSize = (Get-Item $OutputFile).Length
Write-Host "  Signed MCPB size: $finalSize bytes"
Write-Host "  Signature block: $($finalSize - $mcpbContent.Length) bytes"

Write-Host "`nSignature applied successfully!" -ForegroundColor Green
