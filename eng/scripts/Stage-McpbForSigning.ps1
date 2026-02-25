#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Stages MCPB files for ESRP detached signing.

.DESCRIPTION
    This script prepares MCPB files for ESRP's Pkcs7DetachedSign operation.
    ESRP replaces the input file content with the signature, so we:
    1. Copy each .mcpb file to the staging directory
    2. Create a .signature.p7s copy for ESRP to process
    
    After ESRP signing, the .signature.p7s files will contain the detached signatures.

.PARAMETER ArtifactsPath
    Path to the directory containing unsigned MCPB files.

.PARAMETER OutputPath
    Path to the staging directory for ESRP signing.

.EXAMPLE
    ./Stage-McpbForSigning.ps1 -ArtifactsPath "./mcpb" -OutputPath "./to_sign"
#>
param(
    [Parameter(Mandatory = $true)]
    [string] $ArtifactsPath,

    [Parameter(Mandatory = $true)]
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

# Maximum signature block size in bytes. The ZIP EOCD comment length is set to this
# value before signing so that the final signed file remains a valid ZIP archive.
# The signature block (MCPB_SIG_V1 + length + signature + padding + MCPB_SIG_END)
# is padded with zeros to exactly this size.
# Current ESRP signatures are ~4KB; 16384 provides ample headroom.
$MAX_SIG_BLOCK_SIZE = 16384

<#
.SYNOPSIS
    Updates the ZIP End of Central Directory (EOCD) comment length field.

.DESCRIPTION
    Sets the EOCD comment length to a fixed value so that after the MCPB signature
    block is appended (and padded to this exact size), the file remains a valid ZIP.
    This is necessary because some ZIP parsers (e.g., Claude Desktop) strictly
    validate that file_size == EOCD_offset + 22 + comment_length.
#>
function Set-ZipEocdCommentLength {
    param(
        [Parameter(Mandatory)]
        [string] $FilePath,

        [Parameter(Mandatory)]
        [uint16] $CommentLength
    )

    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    $len = $bytes.Length

    # EOCD signature: 0x06054b50 (PK\x05\x06)
    $eocdSig = @([byte]0x50, [byte]0x4B, [byte]0x05, [byte]0x06)
    $eocdOffset = -1

    # Search backwards from end of file (EOCD is at most 65557 bytes from end)
    $searchStart = [Math]::Max(0, $len - 65557)
    for ($i = $len - 22; $i -ge $searchStart; $i--) {
        if ($bytes[$i] -eq $eocdSig[0] -and $bytes[$i+1] -eq $eocdSig[1] -and
            $bytes[$i+2] -eq $eocdSig[2] -and $bytes[$i+3] -eq $eocdSig[3]) {
            $eocdOffset = $i
            break
        }
    }

    if ($eocdOffset -lt 0) {
        throw "ZIP EOCD signature not found in $FilePath"
    }

    # EOCD comment length is at offset 20-21 (2 bytes, little-endian)
    $commentLenBytes = [BitConverter]::GetBytes($CommentLength)
    $bytes[$eocdOffset + 20] = $commentLenBytes[0]
    $bytes[$eocdOffset + 21] = $commentLenBytes[1]

    [System.IO.File]::WriteAllBytes($FilePath, $bytes)
}

if (!(Test-Path $ArtifactsPath)) {
    LogError "MCPB directory not found: $ArtifactsPath"
    exit 1
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

LogInfo "Staging MCPB files for signing..."

# Find all .mcpb files recursively (they're organized by server name)
$mcpbFiles = Get-ChildItem -Path $ArtifactsPath -Filter "*.mcpb" -Recurse

if ($mcpbFiles.Count -eq 0) {
    LogError "No .mcpb files found in $ArtifactsPath"
    exit 1
}

foreach ($mcpb in $mcpbFiles) {
    # Preserve directory structure
    $relativePath = $mcpb.Directory.FullName.Substring((Resolve-Path $ArtifactsPath).Path.Length).TrimStart('\', '/')
    $targetDir = Join-Path $OutputPath $relativePath
    
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    
    # Copy original .mcpb and update EOCD comment length.
    # The comment length is set to MAX_SIG_BLOCK_SIZE so that after the signature
    # block is appended (padded to this size), the file is a valid ZIP. ESRP signs
    # this modified content, so mcpb verify still works because the "original content"
    # extracted during verification matches what was signed.
    $mcpbDest = Join-Path $targetDir $mcpb.Name
    Copy-Item $mcpb.FullName $mcpbDest -Force
    Set-ZipEocdCommentLength -FilePath $mcpbDest -CommentLength $MAX_SIG_BLOCK_SIZE
    LogInfo "  Updated EOCD comment length to $MAX_SIG_BLOCK_SIZE for $($mcpb.Name)"
    
    # Create .signature.p7s copy for ESRP to sign (same modified content)
    $sigName = $mcpb.BaseName + ".signature.p7s"
    $sigDest = Join-Path $targetDir $sigName
    Copy-Item $mcpbDest $sigDest -Force
    
    LogInfo "  Staged: $($mcpb.Name) -> $sigName"
}

LogInfo "`nFiles staged for signing:"
Get-ChildItem -Path $OutputPath -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring((Resolve-Path $OutputPath).Path.Length).TrimStart('\', '/')
    LogInfo "  $rel ($($_.Length) bytes)"
}

LogInfo "`nStaged $($mcpbFiles.Count) MCPB file(s) for signing"
