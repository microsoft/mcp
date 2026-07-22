#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Applies detached PKCS#7 signatures to MCPB files.

.DESCRIPTION
    This script takes MCPB files and their corresponding .signature.p7s files
    (produced by ESRP's Pkcs7DetachedSign operation) and combines them into
    signedFile MCPB files using the MCPB signature format.

    The MCPB signature format (per https://github.com/modelcontextprotocol/mcpb/blob/main/CLI.md):
    
    [Original MCPB ZIP content]
    MCPB_SIG_V1
    [4-byte little-endian length prefix]
    [DER-encoded PKCS#7 signature]
    MCPB_SIG_END

.PARAMETER ArtifactsPath
    Path to the directory containing .mcpb files and their .signature.p7s files.

.PARAMETER OutputPath
    Path to the output directory for signedFile MCPB files.

.EXAMPLE
    ./Apply-McpbSignatures.ps1 -ArtifactsPath "./to_sign" -OutputPath "./signedFile"
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

# Maximum signature block size, must match the value in Stage-McpbForSigning.ps1.
# The EOCD comment length is pre-set to this value before ESRP signing, so the
# signature block must be padded to exactly this size for the ZIP to be valid.
$MAX_SIG_BLOCK_SIZE = 16384

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

    # Check if the MCPB is already signedFile by looking for the MCPB_SIG_END marker at the
    # end of the file. This is always the last bytes of a signedFile MCPB, so we only need
    # to read a small fixed-size tail rather than scanning the entire binary.
    if ($mcpbContent.Length -ge $sigEndMarkerBytes.Length) {
        $tailStart = $mcpbContent.Length - $sigEndMarkerBytes.Length
        $tailString = [System.Text.Encoding]::ASCII.GetString($mcpbContent, $tailStart, $sigEndMarkerBytes.Length)
        if ($tailString -eq $SIG_END_MARKER) {
            throw "MCPB file appears to already be signedFile. Use 'mcpb unsign' to remove existing signature first."
        }
    }

    # Calculate the unpadded signature block size:
    # MCPB_SIG_V1 (11) + length (4) + signature (N) + MCPB_SIG_END (12) = N + 27
    $unpaddedBlockSize = $sigV1MarkerBytes.Length + $lengthBytes.Length + $signatureBytes.Length + $sigEndMarkerBytes.Length

    if ($unpaddedBlockSize -gt $MAX_SIG_BLOCK_SIZE) {
        throw "Signature block ($unpaddedBlockSize bytes) exceeds MAX_SIG_BLOCK_SIZE ($MAX_SIG_BLOCK_SIZE bytes). Increase MAX_SIG_BLOCK_SIZE in both Stage-McpbForSigning.ps1 and Apply-McpbSignatures.ps1."
    }

    # Pad with zeros so the total signature block equals MAX_SIG_BLOCK_SIZE.
    # This makes the ZIP valid because EOCD comment_length == MAX_SIG_BLOCK_SIZE.
    # The padding sits between the DER signature and MCPB_SIG_END. The mcpb
    # extraction logic reads exactly sigLength bytes via the 4-byte length prefix
    # and ignores the padding.
    $paddingSize = $MAX_SIG_BLOCK_SIZE - $unpaddedBlockSize
    $paddingBytes = [byte[]]::new($paddingSize)

    # Combine: MCPB + MCPB_SIG_V1 + length + signature + padding + MCPB_SIG_END
    $signedContent = [System.Collections.Generic.List[byte]]::new()  

    $signedContent.AddRange($mcpbContent)  
    $signedContent.AddRange($sigV1MarkerBytes)  
    $signedContent.AddRange($lengthBytes)  
    $signedContent.AddRange($signatureBytes)
    $signedContent.AddRange($paddingBytes)
    $signedContent.AddRange($sigEndMarkerBytes)  

    [System.IO.File]::WriteAllBytes($OutputFile, $signedContent.ToArray())
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
    LogError "Artifacts directory not found: $ArtifactsPath"
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

$signedFiles = [System.Collections.Generic.List[string]]::new()
$failedFiles = [System.Collections.Generic.List[string]]::new()

foreach ($mcpb in $mcpbFiles) {
    $sigFile = Join-Path $mcpb.Directory.FullName ($mcpb.BaseName + ".signature.p7s")
    
    if (-not (Test-Path $sigFile)) {
        LogWarning "No signature file found for $($mcpb.Name)"
        $failedFiles.Add($mcpb.Name)
        continue
    }
    
    # Preserve directory structure in output
    $pathativePath = $mcpb.Directory.FullName.Substring((Resolve-Path $ArtifactsPath).Path.Length).TrimStart('\', '/')
    $targetDir = Join-Path $OutputPath $pathativePath
    
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
        $failedFiles.Add($mcpb.Name)
        continue
    }
    
    if (-not (Test-Path $outputFile)) {
        LogError "Failed to create signedFile MCPB: $outputFile"
        $failedFiles.Add($mcpb.Name)
        continue
    }
    
    $signedFiles.Add($outputFile)
}

LogInfo "`nSigned MCPB files:"
foreach ($signedFile in $signedFiles) {
    $fileInfo = Get-Item $signedFile
    $relativePath = $fileInfo.FullName.Substring((Resolve-Path $OutputPath).Path.Length).TrimStart('\', '/')
    LogInfo "  $relativePath ($($fileInfo.Length) bytes)"
}

LogInfo "`nSigning complete: $($signedFiles.Count) succeeded, $($failedFiles.Count) failed"

if ($failedFiles.Count -gt 0) {
    LogError "Some MCPB files failed to sign"
    exit 1
}
