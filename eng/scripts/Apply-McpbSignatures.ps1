#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Applies detached PKCS#7 signatures to MCPB files.

.DESCRIPTION
    This script takes MCPB files and their corresponding .signature.p7s files
    (produced by ESRP's Pkcs7DetachedSign operation) and combines them into
    signed MCPB files using the MCPB signature format.

    The MCPB signature format (per https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md):
    
    [Original MCPB ZIP content]
    MCPB_SIG_V1
    [4-byte little-endian length prefix]
    [DER-encoded PKCS#7 signature]
    MCPB_SIG_END

.PARAMETER ArtifactsPath
    Path to the directory containing .mcpb files and their .signature.p7s files.

.PARAMETER OutputPath
    Path to the output directory for signed MCPB files.

.EXAMPLE
    ./Apply-McpbSignatures.ps1 -ArtifactsPath "./to_sign" -OutputPath "./signed"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $ArtifactsPath,

    [Parameter(Mandatory = $false)]
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

# MCPB signature markers (ASCII)
$SIG_V1_MARKER = "MCPB_SIG_V1"
$SIG_END_MARKER = "MCPB_SIG_END"

<#
.SYNOPSIS
    Converts a PKCS#7 detached signature to MCPB embedded signature format.
#>
function Convert-P7sToMcpbSignature {
    param(
        [Parameter(Mandatory)]
        [string] $P7sFile,

        [Parameter(Mandatory)]
        [string] $McpbFile,

        [Parameter(Mandatory)]
        [string] $OutputFile
    )

    # Validate inputs
    if (-not (Test-Path $P7sFile)) {
        throw "Signature file not found: $P7sFile"
    }

    if (-not (Test-Path $McpbFile)) {
        throw "MCPB file not found: $McpbFile"
    }

    # Read signature bytes
    $signatureBytes = [System.IO.File]::ReadAllBytes($P7sFile)

    # Create length prefix (4-byte little-endian)
    $lengthBytes = [BitConverter]::GetBytes([uint32]$signatureBytes.Length)

    # Create markers as byte arrays
    $sigV1MarkerBytes = [System.Text.Encoding]::ASCII.GetBytes($SIG_V1_MARKER)
    $sigEndMarkerBytes = [System.Text.Encoding]::ASCII.GetBytes($SIG_END_MARKER)

    # Read original MCPB content
    $mcpbContent = [System.IO.File]::ReadAllBytes($McpbFile)

    # Check if the MCPB is already signed by looking for the MCPB_SIG_END marker at the
    # end of the file. This is always the last bytes of a signed MCPB, so we only need
    # to read a small fixed-size tail rather than scanning the entire binary.
    if ($mcpbContent.Length -ge $sigEndMarkerBytes.Length) {
        $tailStart = $mcpbContent.Length - $sigEndMarkerBytes.Length
        $tailString = [System.Text.Encoding]::ASCII.GetString($mcpbContent, $tailStart, $sigEndMarkerBytes.Length)
        if ($tailString -eq $SIG_END_MARKER) {
            throw "MCPB file appears to already be signed. Use 'mcpb unsign' to remove existing signature first."
        }
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
}

# Validate required parameters when running as script
if (-not $ArtifactsPath) {
    LogError "ArtifactsPath is required"
    exit 1
}
if (-not $OutputPath) {
    LogError "OutputPath is required"
    exit 1
}

# Main script logic
if (!(Test-Path $ArtifactsPath)) {
    LogError "Staging directory not found: $ArtifactsPath"
    exit 1
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

LogInfo "Applying signatures to MCPB files..."

# Find all .mcpb files
$mcpbFiles = Get-ChildItem -Path $ArtifactsPath -Filter "*.mcpb" -Recurse

if ($mcpbFiles.Count -eq 0) {
    LogError "No .mcpb files found in $ArtifactsPath"
    exit 1
}

$signedCount = 0
$failedCount = 0

foreach ($mcpb in $mcpbFiles) {
    $sigFile = Join-Path $mcpb.Directory.FullName ($mcpb.BaseName + ".signature.p7s")
    
    if (-not (Test-Path $sigFile)) {
        LogWarning "No signature file found for $($mcpb.Name)"
        $failedCount++
        continue
    }
    
    # Preserve directory structure in output
    $relativePath = $mcpb.Directory.FullName.Substring((Resolve-Path $ArtifactsPath).Path.Length).TrimStart('\', '/')
    $targetDir = Join-Path $OutputPath $relativePath
    
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    $outputFile = Join-Path $targetDir $mcpb.Name
    
    LogInfo "  Signing: $($mcpb.Name)"
    
    try {
        Convert-P7sToMcpbSignature -P7sFile $sigFile -McpbFile $mcpb.FullName -OutputFile $outputFile
    }
    catch {
        LogError "Failed to sign $($mcpb.Name): $_"
        $failedCount++
        continue
    }
    
    if (-not (Test-Path $outputFile)) {
        LogError "Failed to create signed MCPB: $outputFile"
        $failedCount++
        continue
    }
    
    $signedCount++
}

LogInfo "`nSigned MCPB files:"
Get-ChildItem -Path $OutputPath -Filter "*.mcpb" -Recurse | ForEach-Object {
    $rel = $_.FullName.Substring((Resolve-Path $OutputPath).Path.Length).TrimStart('\', '/')
    LogInfo "  $rel ($($_.Length) bytes)"
}

LogInfo "`nSigning complete: $signedCount succeeded, $failedCount failed"

if ($failedCount -gt 0) {
    LogError "Some MCPB files failed to sign"
    exit 1
}
