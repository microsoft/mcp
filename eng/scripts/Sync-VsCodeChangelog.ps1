#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Syncs the Unreleased section from the main CHANGELOG to the VS Code extension CHANGELOG.

.DESCRIPTION
    This script extracts the Unreleased section from the main server CHANGELOG
    and creates a corresponding entry in the VS Code extension CHANGELOG with renamed sections:
    - "Features Added" → "Added"
    - "Breaking Changes" + "Other Changes" → "Changed"
    - "Bugs Fixed" → "Fixed"

.PARAMETER ServerName
    Name of the server to sync changelog for (e.g., "Azure.Mcp.Server", "Fabric.Mcp.Server").
    Defaults to "Azure.Mcp.Server".

.PARAMETER MainChangelogPath
    Path to the main CHANGELOG.md file. If not specified, uses servers/{ServerName}/CHANGELOG.md.

.PARAMETER VsCodeChangelogPath
    Path to the VS Code extension CHANGELOG.md file. If not specified, uses servers/{ServerName}/vscode/CHANGELOG.md.

.PARAMETER Version
    The version number to use for the new VS Code changelog entry. If not specified, extracts from the Unreleased section header.

.PARAMETER DryRun
    Preview the changes without modifying the VS Code CHANGELOG.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -DryRun

    Preview the sync for Azure.Mcp.Server without making changes.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ServerName "Fabric.Mcp.Server"

    Sync the Unreleased section for Fabric.Mcp.Server.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -Version "2.0.3"

    Sync the Unreleased section and create version 2.0.3 entry in Azure.Mcp.Server VS Code CHANGELOG.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ServerName "Fabric.Mcp.Server" -Version "1.0.0"

    Sync and create version 1.0.0 entry for Fabric.Mcp.Server.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ServerName = "Azure.Mcp.Server",

    [Parameter(Mandatory = $false)]
    [string]$MainChangelogPath,

    [Parameter(Mandatory = $false)]
    [string]$VsCodeChangelogPath,

    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Set default paths based on ServerName
if (-not $MainChangelogPath) {
    $MainChangelogPath = "servers/$ServerName/CHANGELOG.md"
}
if (-not $VsCodeChangelogPath) {
    $VsCodeChangelogPath = "servers/$ServerName/vscode/CHANGELOG.md"
}

# Get repository root
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$mainChangelogFile = Join-Path $repoRoot $MainChangelogPath
$vscodeChangelogFile = Join-Path $repoRoot $VsCodeChangelogPath

# Validate files exist
if (-not (Test-Path $mainChangelogFile)) {
    Write-Error "Main CHANGELOG not found: $mainChangelogFile"
    exit 1
}

if (-not (Test-Path $vscodeChangelogFile)) {
    Write-Error "VS Code CHANGELOG not found: $vscodeChangelogFile"
    exit 1
}

Write-Host "`nVS Code Changelog Sync" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
Write-Host ""

# Read the main CHANGELOG
$mainContent = Get-Content -Path $mainChangelogFile -Raw

# Extract the Unreleased section
$unreleasedMatch = $mainContent -match '(?ms)^## ([\d\.]+-[\w\.]+) \(Unreleased\)\s*\n(.*?)(?=\n## |\z)'
if (-not $unreleasedMatch) {
    Write-Error "No Unreleased section found in main CHANGELOG"
    exit 1
}

$unreleasedVersion = $Matches[1]
$unreleasedContent = $Matches[2]

# Use provided version or extract from Unreleased header
if (-not $Version) {
    $Version = $unreleasedVersion
}

Write-Host "Source: $mainChangelogFile" -ForegroundColor Gray
Write-Host "Target: $vscodeChangelogFile" -ForegroundColor Gray
Write-Host "Version: $Version" -ForegroundColor Gray
Write-Host ""

# Parse sections from unreleased content
$sections = @{
    'Features Added' = @()
    'Breaking Changes' = @()
    'Bugs Fixed' = @()
    'Other Changes' = @()
}

$currentSection = $null
$currentEntries = @()

foreach ($line in $unreleasedContent -split "`n") {
    # Check for section headers
    if ($line -match '^### (.+)$') {
        # Save previous section
        if ($currentSection -and $currentEntries.Count -gt 0) {
            $sections[$currentSection] = $currentEntries
        }
        
        $currentSection = $Matches[1].Trim()
        $currentEntries = @()
        continue
    }
    
    # Skip lines before any section
    if (-not $currentSection) {
        continue
    }
    
    # Collect all lines for current section (including empty lines for spacing)
    # but trim trailing empty lines later
    $currentEntries += $line
}

# Save last section
if ($currentSection -and $currentEntries.Count -gt 0) {
    # Trim trailing empty lines from entries
    while ($currentEntries.Count -gt 0 -and $currentEntries[-1].Trim() -eq '') {
        $currentEntries = $currentEntries[0..($currentEntries.Count - 2)]
    }
    $sections[$currentSection] = $currentEntries
}

# Build VS Code changelog entry
$vscodeEntry = @()
$vscodeEntry += "## $Version ($(Get-Date -Format 'yyyy-MM-dd')) (pre-release)"
$vscodeEntry += ""

# Helper function to add section if it has content
function Add-Section {
    param(
        [string]$SectionName,
        [array]$Entries
    )
    
    if (-not $Entries -or $Entries.Count -eq 0) {
        return
    }
    
    # Filter out empty entries
    $nonEmptyEntries = @($Entries | Where-Object { $_.Trim() -ne '' })
    if ($nonEmptyEntries.Count -eq 0) {
        return
    }
    
    $script:vscodeEntry += "### $SectionName"
    $script:vscodeEntry += ""
    $script:vscodeEntry += $nonEmptyEntries
    $script:vscodeEntry += ""
}

# Added section (from Features Added)
Add-Section -SectionName "Added" -Entries $sections['Features Added']

# Changed section (from Breaking Changes + Other Changes)
$changedEntries = @()
$breakingChanges = @($sections['Breaking Changes'] | Where-Object { $_.Trim() -ne '' })
if ($breakingChanges.Count -gt 0) {
    $changedEntries += $breakingChanges | ForEach-Object {
        if ($_ -match '^-\s+(.+)$') {
            # Add "**Breaking:**" prefix to breaking changes
            "- **Breaking:** $($Matches[1])"
        } else {
            $_
        }
    }
}

$otherChanges = @($sections['Other Changes'] | Where-Object { $_.Trim() -ne '' })
if ($otherChanges.Count -gt 0) {
    $changedEntries += $otherChanges
}

Add-Section -SectionName "Changed" -Entries $changedEntries

# Fixed section (from Bugs Fixed)
Add-Section -SectionName "Fixed" -Entries $sections['Bugs Fixed']

# Trim trailing empty line
while ($vscodeEntry[-1] -eq "") {
    $vscodeEntry = $vscodeEntry[0..($vscodeEntry.Count - 2)]
}

$vscodeEntryText = $vscodeEntry -join "`n"

if ($DryRun) {
    Write-Host "Preview of new VS Code CHANGELOG entry:" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host $vscodeEntryText
    Write-Host ""
    Write-Host "DRY RUN - No files were modified" -ForegroundColor Yellow
    exit 0
}

# Read current VS Code changelog
$vscodeContent = Get-Content -Path $vscodeChangelogFile -Raw

# Find insertion point (after "# Release History" header)
$headerMatch = $vscodeContent -match '(?ms)^(# Release History\s*\n)'
if (-not $headerMatch) {
    Write-Error "Could not find '# Release History' header in VS Code CHANGELOG"
    exit 1
}

$headerEnd = $Matches[0].Length
$beforeHeader = $vscodeContent.Substring(0, $headerEnd)
$afterHeader = $vscodeContent.Substring($headerEnd)

# Insert new entry
$newVscodeContent = $beforeHeader + "`n" + $vscodeEntryText + "`n`n" + $afterHeader.TrimStart("`n", "`r")

# Write updated VS Code changelog
$newVscodeContent | Set-Content -Path $vscodeChangelogFile -NoNewline -Encoding UTF8

Write-Host "✓ Synced Unreleased section to VS Code CHANGELOG" -ForegroundColor Green
Write-Host "  Version: $Version" -ForegroundColor Gray
Write-Host "  Location: $vscodeChangelogFile" -ForegroundColor Gray
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan

$addedCount = @($sections['Features Added'] | Where-Object { $_.Trim() -ne '' }).Count
$breakingCount = @($sections['Breaking Changes'] | Where-Object { $_.Trim() -ne '' }).Count
$otherCount = @($sections['Other Changes'] | Where-Object { $_.Trim() -ne '' }).Count
$fixedCount = @($sections['Bugs Fixed'] | Where-Object { $_.Trim() -ne '' }).Count

if ($addedCount -gt 0) {
    Write-Host "  - Added: $addedCount entries" -ForegroundColor Gray
}
if ($breakingCount -gt 0 -or $otherCount -gt 0) {
    $totalChanged = $breakingCount + $otherCount
    Write-Host "  - Changed: $totalChanged entries ($breakingCount breaking, $otherCount other)" -ForegroundColor Gray
}
if ($fixedCount -gt 0) {
    Write-Host "  - Fixed: $fixedCount entries" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Review the changes in the VS Code CHANGELOG" -ForegroundColor Gray
Write-Host "2. Commit the updated VS Code CHANGELOG with your release" -ForegroundColor Gray
Write-Host ""
