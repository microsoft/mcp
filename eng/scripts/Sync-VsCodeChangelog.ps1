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

.PARAMETER ChangelogPath
    Path to the main CHANGELOG.md file (required).
    The VS Code CHANGELOG.md path is inferred from this path (vscode subdirectory).
    Examples: "servers/Azure.Mcp.Server/CHANGELOG.md", "servers/Fabric.Mcp.Server/CHANGELOG.md"

.PARAMETER Version
    The version number to use for the new VS Code changelog entry. If not specified, extracts from the Unreleased section header.

.PARAMETER DryRun
    Preview the changes without modifying the VS Code CHANGELOG.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -DryRun

    Preview the sync for Azure.Mcp.Server without making changes.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md"

    Sync the Unreleased section for Fabric.Mcp.Server.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -Version "2.0.3"

    Sync the Unreleased section and create version 2.0.3 entry in Azure.Mcp.Server VS Code CHANGELOG.

.EXAMPLE
    ./eng/scripts/Sync-VsCodeChangelog.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md" -Version "1.0.0"

    Sync and create version 1.0.0 entry for Fabric.Mcp.Server.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ChangelogPath,

    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# Infer VS Code changelog path from main CHANGELOG.md path
$changelogDir = Split-Path $ChangelogPath -Parent
$VsCodeChangelogPath = Join-Path $changelogDir "vscode/CHANGELOG.md"
$MainChangelogPath = $ChangelogPath
$mainChangelogFile = Join-Path $RepoRoot $MainChangelogPath
$vscodeChangelogFile = Join-Path $RepoRoot $VsCodeChangelogPath

# Validate files exist
if (-not (Test-Path $mainChangelogFile)) {
    LogError "Main CHANGELOG not found: $mainChangelogFile"
    exit 1
}

if (-not (Test-Path $vscodeChangelogFile)) {
    LogError "VS Code CHANGELOG not found: $vscodeChangelogFile"
    exit 1
}

LogInfo ""
LogInfo "VS Code Changelog Sync"
LogInfo "======================"
LogInfo ""

# Read the main CHANGELOG
$mainContent = Get-Content -Path $mainChangelogFile -Raw

# Extract the Unreleased section (supports both "2.0.0" and "2.0.0-beta.3" formats)
$unreleasedMatch = $mainContent -match '(?ms)^## ([\d\.]+(?:-[\w\.]+)?) \(Unreleased\)\s*\n(.*?)(?=\n## |\z)'
if (-not $unreleasedMatch) {
    LogError "No Unreleased section found in main CHANGELOG"
    exit 1
}

$unreleasedVersion = $Matches[1]
$unreleasedContent = $Matches[2]

# Use provided version or extract from Unreleased header
if (-not $Version) {
    $Version = $unreleasedVersion
}

LogInfo "Source: $mainChangelogFile"
LogInfo "Target: $vscodeChangelogFile"
LogInfo "Version: $Version"
LogInfo ""

# Parse sections from unreleased content
# Initialize sections dynamically from $RecommendedSectionHeaders (from ChangeLog-Operations.ps1 via common.ps1)
$sections = @{}
foreach ($header in $RecommendedSectionHeaders) {
    $sections[$header] = @()
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
        # Only process known sections (use $RecommendedSectionHeaders from ChangeLog-Operations.ps1)
        if ($currentSection -notin $RecommendedSectionHeaders) {
            LogWarning "Unknown section '$currentSection' found in main CHANGELOG - skipping"
            $currentSection = $null
        }
        $currentEntries = @()
        continue
    }
    
    # Skip lines before any section or in unknown sections
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

# Trim trailing empty lines (with safety check for empty array)
while ($vscodeEntry.Count -gt 0 -and $vscodeEntry[-1] -eq "") {
    $vscodeEntry = $vscodeEntry[0..($vscodeEntry.Count - 2)]
}

$vscodeEntryText = $vscodeEntry -join "`n"

if ($DryRun) {
    LogInfo "Preview of new VS Code CHANGELOG entry:"
    LogInfo "========================================"
    LogInfo ""
    LogInfo $vscodeEntryText
    LogInfo ""
    LogWarning "DRY RUN - No files were modified"
    exit 0
}

# Read current VS Code changelog
$vscodeContent = Get-Content -Path $vscodeChangelogFile -Raw

# Find insertion point (after "# Release History" header)
$headerMatch = $vscodeContent -match '(?ms)^(# Release History\s*\n)'
if (-not $headerMatch) {
    LogError "Could not find '# Release History' header in VS Code CHANGELOG"
    exit 1
}

$headerEnd = $Matches[0].Length
$beforeHeader = $vscodeContent.Substring(0, $headerEnd)
$afterHeader = $vscodeContent.Substring($headerEnd)

# Insert new entry
$newVscodeContent = $beforeHeader + "`n" + $vscodeEntryText + "`n`n" + $afterHeader.TrimStart("`n", "`r")

# Write updated VS Code changelog
$newVscodeContent | Set-Content -Path $vscodeChangelogFile -NoNewline -Encoding UTF8

LogSuccess "✓ Synced Unreleased section to VS Code CHANGELOG"
LogInfo "  Version: $Version"
LogInfo "  Location: $vscodeChangelogFile"
LogInfo ""
LogInfo "Summary:"

$addedCount = @($sections['Features Added'] | Where-Object { $_.Trim() -ne '' }).Count
$breakingCount = @($sections['Breaking Changes'] | Where-Object { $_.Trim() -ne '' }).Count
$otherCount = @($sections['Other Changes'] | Where-Object { $_.Trim() -ne '' }).Count
$fixedCount = @($sections['Bugs Fixed'] | Where-Object { $_.Trim() -ne '' }).Count

if ($addedCount -gt 0) {
    LogInfo "  - Added: $addedCount entries"
}
if ($breakingCount -gt 0 -or $otherCount -gt 0) {
    $totalChanged = $breakingCount + $otherCount
    LogInfo "  - Changed: $totalChanged entries ($breakingCount breaking, $otherCount other)"
}
if ($fixedCount -gt 0) {
    LogInfo "  - Fixed: $fixedCount entries"
}
LogInfo ""
LogInfo "Next steps:"
LogInfo "1. Review the changes in the VS Code CHANGELOG"
LogInfo "2. Commit the updated VS Code CHANGELOG with your release"
LogInfo ""
