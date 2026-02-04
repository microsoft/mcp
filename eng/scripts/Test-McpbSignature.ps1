#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Verifies the signature of one or more MCPB files.

.DESCRIPTION
    This script wraps the 'mcpb verify' command to verify signatures on MCPB files.
    It supports verifying single files, multiple files, or all .mcpb files in a directory.

.PARAMETER McpbPath
    Path to an MCPB file or directory containing MCPB files.

.PARAMETER Recurse
    When McpbPath is a directory, search recursively for .mcpb files.

.EXAMPLE
    # Verify a single file
    ./Test-McpbSignature.ps1 -McpbPath ./server.mcpb

.EXAMPLE
    # Verify all MCPB files in a directory
    ./Test-McpbSignature.ps1 -McpbPath ./packages_mcpb/

.EXAMPLE
    # Verify all MCPB files recursively
    ./Test-McpbSignature.ps1 -McpbPath ./artifacts/ -Recurse
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $McpbPath,

    [switch] $Recurse
)

$ErrorActionPreference = "Stop"

# Collect files to verify
$mcpbFiles = @()

if (Test-Path $McpbPath -PathType Leaf) {
    # Single file
    $mcpbFiles += Get-Item $McpbPath
}
elseif (Test-Path $McpbPath -PathType Container) {
    # Directory - find all .mcpb files
    if ($Recurse) {
        $mcpbFiles = Get-ChildItem -Path $McpbPath -Filter "*.mcpb" -Recurse
    }
    else {
        $mcpbFiles = Get-ChildItem -Path $McpbPath -Filter "*.mcpb"
    }
}
else {
    throw "Path not found: $McpbPath"
}

if ($mcpbFiles.Count -eq 0) {
    Write-Host "No .mcpb files found at: $McpbPath" -ForegroundColor Yellow
    exit 0
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verifying $($mcpbFiles.Count) MCPB file(s)" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passed = 0
$failed = 0
$unsigned = 0
$results = @()

foreach ($file in $mcpbFiles) {
    Write-Host "Verifying: $($file.Name)..." -NoNewline
    
    $output = & mcpb verify $file.FullName 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        # Check if it's actually signed or just unsigned (mcpb verify returns 0 for unsigned too)
        $infoOutput = & mcpb info $file.FullName 2>&1
        if ($infoOutput -match "WARNING: Not signed") {
            Write-Host " UNSIGNED" -ForegroundColor Yellow
            $unsigned++
            $results += [PSCustomObject]@{
                File = $file.Name
                Status = "Unsigned"
                Details = "No signature present"
            }
        }
        else {
            Write-Host " PASSED" -ForegroundColor Green
            $passed++
            $results += [PSCustomObject]@{
                File = $file.Name
                Status = "Passed"
                Details = "Signature valid"
            }
        }
    }
    else {
        Write-Host " FAILED" -ForegroundColor Red
        $failed++
        $results += [PSCustomObject]@{
            File = $file.Name
            Status = "Failed"
            Details = $output -join "; "
        }
    }
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verification Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total:    $($mcpbFiles.Count)"
Write-Host "Passed:   $passed" -ForegroundColor Green
Write-Host "Unsigned: $unsigned" -ForegroundColor Yellow
Write-Host "Failed:   $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "White" })

if ($failed -gt 0) {
    Write-Host "`nFailed files:" -ForegroundColor Red
    $results | Where-Object { $_.Status -eq "Failed" } | ForEach-Object {
        Write-Host "  - $($_.File): $($_.Details)" -ForegroundColor Red
    }
    exit 1
}

exit 0
