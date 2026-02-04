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
    
    # Copy original .mcpb
    $mcpbDest = Join-Path $targetDir $mcpb.Name
    Copy-Item $mcpb.FullName $mcpbDest -Force
    
    # Create .signature.p7s copy for ESRP to sign
    $sigName = $mcpb.BaseName + ".signature.p7s"
    $sigDest = Join-Path $targetDir $sigName
    Copy-Item $mcpb.FullName $sigDest -Force
    
    LogInfo "  Staged: $($mcpb.Name) -> $sigName"
}

LogInfo "`nFiles staged for signing:"
Get-ChildItem -Path $OutputPath -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring((Resolve-Path $OutputPath).Path.Length).TrimStart('\', '/')
    LogInfo "  $rel ($($_.Length) bytes)"
}

LogInfo "`nStaged $($mcpbFiles.Count) MCPB file(s) for signing"
