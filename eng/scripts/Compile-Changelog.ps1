#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compiles changelog entries from YAML files into CHANGELOG.md.

.DESCRIPTION
    This script reads all YAML files from the changelog-entries directory,
    validates them against the schema, groups them by section and subsection,
    and inserts the compiled entries into CHANGELOG.md under the "Unreleased" section.

.PARAMETER ChangelogPath
    Path to the CHANGELOG.md file. Defaults to servers/Azure.Mcp.Server/CHANGELOG.md.

.PARAMETER ChangelogEntriesPath
    Path to the changelog-entries directory. Defaults to servers/Azure.Mcp.Server/changelog-entries.

.PARAMETER DryRun
    Preview what will be compiled without modifying any files.

.PARAMETER DeleteFiles
    Delete YAML files after successful compilation.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -DryRun

    Preview what will be compiled without making changes.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1

    Compile entries into CHANGELOG.md.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -DeleteFiles

    Compile entries and remove YAML files after successful compilation.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ChangelogPath = "servers/Azure.Mcp.Server/CHANGELOG.md",

    [Parameter(Mandatory = $false)]
    [string]$ChangelogEntriesPath = "servers/Azure.Mcp.Server/changelog-entries",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$DeleteFiles
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Get repository root
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$changelogFile = Join-Path $repoRoot $ChangelogPath
$changelogEntriesDir = Join-Path $repoRoot $ChangelogEntriesPath
$schemaPath = Join-Path $repoRoot "eng/schemas/changelog-entry.schema.json"

Write-Host "Changelog Compiler" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
Write-Host ""

# Check if changelog-entries directory exists
if (-not (Test-Path $changelogEntriesDir)) {
    Write-Error "Changelog entries directory not found: $changelogEntriesDir"
    exit 1
}

# Check if CHANGELOG.md exists
if (-not (Test-Path $changelogFile)) {
    Write-Error "CHANGELOG.md not found: $changelogFile"
    exit 1
}

# Get all YAML files
$yamlFiles = Get-ChildItem -Path $changelogEntriesDir -Filter "*.yml" -File | Where-Object { $_.Name -ne "README.yml" }

if ($yamlFiles.Count -eq 0) {
    Write-Host "No changelog entries found in $changelogEntriesDir" -ForegroundColor Yellow
    Write-Host "Nothing to compile." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($yamlFiles.Count) changelog entry file(s)" -ForegroundColor Green
Write-Host ""

# Install PowerShell-Yaml module if not available
$yamlModule = Get-Module -ListAvailable -Name "powershell-yaml"
if (-not $yamlModule) {
    Write-Host "Installing powershell-yaml module..." -ForegroundColor Yellow
    Install-Module -Name powershell-yaml -Force -Scope CurrentUser -AllowClobber
}
Import-Module powershell-yaml -ErrorAction Stop

# Load schema
$schema = Get-Content -Path $schemaPath -Raw | ConvertFrom-Json

# Parse and validate YAML files
$entries = @()
$validSections = @("Features Added", "Breaking Changes", "Bugs Fixed", "Other Changes")

foreach ($file in $yamlFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Gray
    
    # Validate filename format (should be numeric timestamp)
    if ($file.BaseName -notmatch '^\d+$') {
        Write-Warning "  Filename '$($file.Name)' doesn't follow timestamp convention (numeric only)"
    }
    
    try {
        $yamlContent = Get-Content -Path $file.FullName -Raw
        $entry = $yamlContent | ConvertFrom-Yaml
        
        # Validate required fields
        if (-not $entry.section) {
            Write-Error "  Missing required field 'section' in $($file.Name)"
            continue
        }
        
        if ($entry.section -notin $validSections) {
            Write-Error "  Invalid section '$($entry.section)' in $($file.Name). Must be one of: $($validSections -join ', ')"
            continue
        }
        
        if (-not $entry.description) {
            Write-Error "  Missing required field 'description' in $($file.Name)"
            continue
        }
        
        if ($entry.description.Length -lt 10) {
            Write-Error "  Description too short in $($file.Name) (minimum 10 characters)"
            continue
        }
        
        if (-not $entry.pr -or $entry.pr -eq 0) {
            Write-Warning "  Missing or invalid PR number in $($file.Name)"
        }
        elseif ($entry.pr -lt 1) {
            Write-Error "  Invalid PR number in $($file.Name) (must be positive)"
            continue
        }
        
        # Add to entries collection
        $entries += [PSCustomObject]@{
            Section = $entry.section
            Subsection = if ($entry.subsection -and $entry.subsection -ne "null") { $entry.subsection } else { $null }
            Description = $entry.description
            PR = $entry.pr
            Filename = $file.Name
        }
        
        Write-Host "  ✓ Valid" -ForegroundColor Green
    }
    catch {
        Write-Error "  Failed to parse $($file.Name): $_"
        continue
    }
}

if ($entries.Count -eq 0) {
    Write-Error "No valid changelog entries found"
    exit 1
}

Write-Host ""
Write-Host "Successfully validated $($entries.Count) entry/entries" -ForegroundColor Green
Write-Host ""

# Group entries by section and subsection
$sectionOrder = @("Features Added", "Breaking Changes", "Bugs Fixed", "Other Changes")
$groupedEntries = @{}

foreach ($section in $sectionOrder) {
    $sectionEntries = $entries | Where-Object { $_.Section -eq $section }
    if ($sectionEntries) {
        $groupedEntries[$section] = @{}
        
        # Group by subsection
        $withSubsection = $sectionEntries | Where-Object { $_.Subsection }
        $withoutSubsection = $sectionEntries | Where-Object { -not $_.Subsection }
        
        if ($withoutSubsection) {
            $groupedEntries[$section][""] = $withoutSubsection
        }
        
        foreach ($entry in $withSubsection) {
            if (-not $groupedEntries[$section].ContainsKey($entry.Subsection)) {
                $groupedEntries[$section][$entry.Subsection] = @()
            }
            $groupedEntries[$section][$entry.Subsection] += $entry
        }
    }
}

# Generate markdown
$markdown = @()

foreach ($section in $sectionOrder) {
    if (-not $groupedEntries.ContainsKey($section)) {
        continue
    }
    
    $markdown += ""
    $markdown += "### $section"
    $markdown += ""
    
    $sectionData = $groupedEntries[$section]
    
    # Entries without subsection first
    if ($sectionData.ContainsKey("")) {
        foreach ($entry in $sectionData[""]) {
            $prLink = if ($entry.PR -gt 0) { " [[#$($entry.PR)](https://github.com/microsoft/mcp/pull/$($entry.PR))]" } else { "" }
            $markdown += "- $($entry.Description)$prLink"
        }
    }
    
    # Entries with subsections
    $subsections = $sectionData.Keys | Where-Object { $_ -ne "" } | Sort-Object
    foreach ($subsection in $subsections) {
        $markdown += ""
        $markdown += "#### $subsection"
        foreach ($entry in $sectionData[$subsection]) {
            $prLink = if ($entry.PR -gt 0) { " [[#$($entry.PR)](https://github.com/microsoft/mcp/pull/$($entry.PR))]" } else { "" }
            $markdown += "- $($entry.Description)$prLink"
        }
    }
}

# Preview output
Write-Host "Compiled Output:" -ForegroundColor Cyan
Write-Host "================" -ForegroundColor Cyan
Write-Host ""
$markdown | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN - No files were modified" -ForegroundColor Yellow
    exit 0
}

# Read existing CHANGELOG.md
$changelogContent = Get-Content -Path $changelogFile -Raw

# Find the Unreleased section
$unreleasedPattern = '(?s)(##\s+.*?Unreleased.*?\r?\n)(.*?)(?=##\s+\d+\.\d+\.\d+|$)'
$match = [regex]::Match($changelogContent, $unreleasedPattern)

if (-not $match.Success) {
    Write-Error "Could not find 'Unreleased' section in CHANGELOG.md"
    exit 1
}

$unreleasedHeader = $match.Groups[1].Value
$existingContent = $match.Groups[2].Value

# Check if there's already content under unreleased
if ($existingContent.Trim()) {
    Write-Warning "Existing content found under 'Unreleased' section"
    Write-Host "This script will append new entries. You may need to manually review and merge." -ForegroundColor Yellow
    Write-Host ""
}

# Insert compiled entries
$newContent = $unreleasedHeader + ($markdown -join "`n") + "`n" + $existingContent

# Replace in changelog
$updatedChangelog = $changelogContent -replace [regex]::Escape($unreleasedHeader + $existingContent), $newContent

# Write updated CHANGELOG.md
$updatedChangelog | Set-Content -Path $changelogFile -Encoding UTF8 -NoNewline

Write-Host "✓ Updated CHANGELOG.md" -ForegroundColor Green
Write-Host "  Location: $changelogFile" -ForegroundColor Gray
Write-Host ""

# Delete YAML files if requested
if ($DeleteFiles) {
    Write-Host "Deleting changelog entry files..." -ForegroundColor Yellow
    foreach ($file in $yamlFiles) {
        Remove-Item -Path $file.FullName -Force
        Write-Host "  Deleted: $($file.Name)" -ForegroundColor Gray
    }
    Write-Host "✓ Deleted $($yamlFiles.Count) file(s)" -ForegroundColor Green
    Write-Host ""
}

Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Entries compiled: $($entries.Count)" -ForegroundColor Gray
Write-Host "  Sections updated: $($groupedEntries.Keys.Count)" -ForegroundColor Gray
if ($DeleteFiles) {
    Write-Host "  Files deleted: $($yamlFiles.Count)" -ForegroundColor Gray
}
Write-Host ""
Write-Host "Done!" -ForegroundColor Green
