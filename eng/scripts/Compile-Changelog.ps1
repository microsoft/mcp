#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Compiles changelog entries from YAML files into CHANGELOG.md.

.DESCRIPTION
    This script reads all YAML files from the changelog-entries directory,
    validates them against the schema, groups them by section and subsection,
    and inserts the compiled entries into CHANGELOG.md under the specified version section.
    
    If no version is specified, entries are added to the "Unreleased" section at the top.
    If there is no "Unreleased" section and no version is specified, a new "Unreleased" 
    section is created using the next semantic version number.

.PARAMETER ChangelogPath
    Path to the CHANGELOG.md file. Defaults to servers/Azure.Mcp.Server/CHANGELOG.md.

.PARAMETER ChangelogEntriesPath
    Path to the changelog-entries directory. Defaults to servers/Azure.Mcp.Server/changelog-entries.

.PARAMETER Version
    Target version section to compile entries into (e.g., "2.0.0-beta.3", "1.5.2").
    If not specified, uses the "Unreleased" section or creates one.

.PARAMETER DryRun
    Preview what will be compiled without modifying any files.

.PARAMETER DeleteFiles
    Delete YAML files after successful compilation.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -DryRun

    Preview what will be compiled without making changes.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1

    Compile entries into the Unreleased section of CHANGELOG.md.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -Version "2.0.0-beta.3"

    Compile entries into the 2.0.0-beta.3 version section.

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
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$DeleteFiles
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Helper function to convert text to title case (capitalize first letter of each word)
function ConvertTo-TitleCase {
    param([string]$Text)
    
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $Text
    }
    
    # Use TextInfo for proper title casing
    $textInfo = (Get-Culture).TextInfo
    return $textInfo.ToTitleCase($Text.ToLower())
}

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
        
        # Validate and normalize section (case-insensitive)
        $matchedSection = $validSections | Where-Object { $_ -ieq $entry.section }
        if ($matchedSection) {
            # Use the properly cased version
            $normalizedSection = $matchedSection
        } else {
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
        
        # Add to entries collection with normalized section name
        $entries += [PSCustomObject]@{
            Section = $normalizedSection
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
            # Normalize subsection name to title case for grouping
            $normalizedSubsection = ConvertTo-TitleCase $entry.Subsection
            if (-not $groupedEntries[$section].ContainsKey($normalizedSubsection)) {
                $groupedEntries[$section][$normalizedSubsection] = @()
            }
            $groupedEntries[$section][$normalizedSubsection] += $entry
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
        $markdown += ""  # Empty line before subsection entries
        foreach ($entry in $sectionData[$subsection]) {
            $prLink = if ($entry.PR -gt 0) { " [[#$($entry.PR)](https://github.com/microsoft/mcp/pull/$($entry.PR))]" } else { "" }
            $markdown += "- $($entry.Description)$prLink"
        }
    }
}

# Read existing CHANGELOG.md to determine target version
$changelogContent = Get-Content -Path $changelogFile -Raw

# Determine target version section
$targetVersionHeader = $null
$isUnreleased = $false

if ($Version) {
    # User specified a version - find it in the changelog
    Write-Host "Looking for version section: $Version" -ForegroundColor Cyan
    
    # Look for exact version match (with or without date)
    $escapedVersion = [regex]::Escape($Version)
    $versionPattern = "(?m)^##\s+$escapedVersion(\s+\(.*?\))?\s*$"
    $versionMatch = [regex]::Match($changelogContent, $versionPattern)
    
    if (-not $versionMatch.Success) {
        Write-Error "Version '$Version' not found in CHANGELOG.md. Please ensure this version section exists before compiling entries."
        exit 1
    }
    
    $targetVersionHeader = $versionMatch.Value
    $isUnreleased = $targetVersionHeader -match '\(Unreleased\)'
    Write-Host "✓ Found version section: $targetVersionHeader" -ForegroundColor Green
    Write-Host ""
} else {
    # No version specified - look for Unreleased section at the top
    Write-Host "No version specified - looking for Unreleased section..." -ForegroundColor Cyan
    
    # Find the first ## header after any initial # headers
    $firstSectionPattern = '(?m)^##\s+(.+?)(\s+\(.*?\))?\s*$'
    $firstSectionMatch = [regex]::Match($changelogContent, $firstSectionPattern)
    
    if ($firstSectionMatch.Success) {
        $firstSectionFull = $firstSectionMatch.Value
        
        # Check if it's Unreleased
        if ($firstSectionFull -match '\(Unreleased\)') {
            $targetVersionHeader = $firstSectionFull
            $isUnreleased = $true
            Write-Host "✓ Found Unreleased section: $targetVersionHeader" -ForegroundColor Green
            Write-Host ""
        } else {
            # First section is not Unreleased - create new Unreleased section
            Write-Host "No Unreleased section found at the top of CHANGELOG.md" -ForegroundColor Yellow
            Write-Host "Creating new Unreleased section with next version number..." -ForegroundColor Yellow
            
            # Parse current version to determine next version
            $currentVersion = $firstSectionMatch.Groups[1].Value.Trim()
            Write-Host "Current version: $currentVersion" -ForegroundColor Gray
            
            # Parse semantic version (handles formats like "2.0.0-beta.3" or "1.5.2")
            if ($currentVersion -match '^(\d+)\.(\d+)\.(\d+)(?:-(.+?)\.(\d+))?') {
                $major = [int]$matches[1]
                $minor = [int]$matches[2]
                $patch = [int]$matches[3]
                $prerelease = $matches[4]
                $prereleaseNum = if ($matches[5]) { [int]$matches[5] } else { 0 }
                
                # Increment based on prerelease status
                if ($prerelease) {
                    # Increment prerelease number (e.g., beta.3 -> beta.4)
                    $nextVersion = "$major.$minor.$patch-$prerelease.$($prereleaseNum + 1)"
                } else {
                    # Increment patch version (e.g., 1.5.2 -> 1.5.3)
                    $nextVersion = "$major.$minor.$($patch + 1)"
                }
                
                $targetVersionHeader = "## $nextVersion (Unreleased)"
                $isUnreleased = $true
                Write-Host "Next version: $nextVersion" -ForegroundColor Green
                Write-Host ""
            } else {
                Write-Error "Unable to parse version number '$currentVersion' to determine next version. Please specify -Version parameter or ensure CHANGELOG.md has a valid version format."
                exit 1
            }
        }
    } else {
        Write-Error "No version sections found in CHANGELOG.md. Please add a version section manually or specify -Version parameter."
        exit 1
    }
}

Write-Host "Target section: $targetVersionHeader" -ForegroundColor Cyan
Write-Host ""

# Find the target section in the changelog
$versionSectionPattern = '(?s)(' + [regex]::Escape($targetVersionHeader) + '\r?\n)(.*?)(?=##\s+\d+\.\d+\.\d+|##\s+[^\s]+\s+\(|$)'
$match = [regex]::Match($changelogContent, $versionSectionPattern)

# Build the merged content for preview
$mergedContent = @()
$mergedContent += $targetVersionHeader

if (-not $match.Success) {
    # New section - just use the new entries
    $mergedContent += $markdown
} else {
    # Existing section - merge with existing content
    $versionHeader = $match.Groups[1].Value
    $existingContent = $match.Groups[2].Value
    
    # Parse existing content by sections
    $existingSections = @{}
    $currentSection = $null
    $currentSubsection = $null
    $lines = $existingContent -split "`r?`n"
    
    foreach ($line in $lines) {
        if ($line -match '^###\s+(.+?)\s*$') {
            # Main section header - normalize to match valid sections (case-insensitive)
            $rawSection = $matches[1]
            $matchedSection = $validSections | Where-Object { $_ -ieq $rawSection }
            $currentSection = if ($matchedSection) { $matchedSection } else { $rawSection }
            $currentSubsection = $null
            if (-not $existingSections.ContainsKey($currentSection)) {
                $existingSections[$currentSection] = @{
                    "" = @()  # Entries without subsection
                }
            }
        }
        elseif ($line -match '^####\s+(.+?)\s*$') {
            # Subsection header - title case it
            $rawSubsection = $matches[1]
            $currentSubsection = ConvertTo-TitleCase $rawSubsection
            if ($currentSection) {
                # Check if this subsection already exists (case-insensitive)
                $existingKey = $existingSections[$currentSection].Keys | Where-Object { $_ -ieq $currentSubsection -and $_ -ne "" }
                if ($existingKey) {
                    $currentSubsection = $existingKey
                } elseif (-not $existingSections[$currentSection].ContainsKey($currentSubsection)) {
                    $existingSections[$currentSection][$currentSubsection] = @()
                }
            }
        }
        elseif ($line -match '^-\s+(.+)$') {
            # Entry line
            if ($currentSection) {
                if ($currentSubsection) {
                    $existingSections[$currentSection][$currentSubsection] += $line
                } else {
                    $existingSections[$currentSection][""] += $line
                }
            }
        }
    }
    
    # Build merged content by combining existing and new entries
    $isFirstSection = $true
    foreach ($section in $sectionOrder) {
        # Check if we have entries (existing or new) for this section
        $hasExisting = $existingSections.ContainsKey($section)
        $hasNew = $groupedEntries.ContainsKey($section)
        
        if (-not $hasExisting -and -not $hasNew) {
            continue
        }
        
        # Add empty line before section (but not before the very first section)
        if (-not $isFirstSection) {
            $mergedContent += ""
        }
        $isFirstSection = $false
        
        $mergedContent += "### $section"
        
        # Merge entries without subsection
        $existingMainEntries = @()
        if ($hasExisting -and $existingSections[$section].ContainsKey("")) {
            $existingMainEntries = @($existingSections[$section][""])
        }
        $newMainEntries = @()
        
        if ($hasNew -and $groupedEntries[$section].ContainsKey("")) {
            foreach ($entry in $groupedEntries[$section][""]) {
                $description = $entry.Description
                # Ensure description ends with a period
                if (-not $description.EndsWith(".")) {
                    $description = $description + "."
                }
                $prLink = if ($entry.PR -gt 0) { " [[#$($entry.PR)](https://github.com/microsoft/mcp/pull/$($entry.PR))]" } else { "" }
                $newMainEntries += "- $description$prLink"
            }
        }
        
        # Append new entries after existing ones (with empty line before entries)
        $totalEntries = $existingMainEntries.Count + $newMainEntries.Count
        if ($totalEntries -gt 0) {
            $mergedContent += ""  # Empty line before entries
            foreach ($line in $existingMainEntries) {
                if ($line) {
                    $mergedContent += $line
                }
            }
            foreach ($line in $newMainEntries) {
                $mergedContent += $line
            }
        }
        
        # Merge subsections - normalize subsection names (case-insensitive, title case)
        $allSubsections = @()
        $subsectionMapping = @{}  # Maps normalized (title-cased) name to actual name
        
        if ($hasExisting) {
            foreach ($key in $existingSections[$section].Keys) {
                if ($key -ne "") {
                    $titleCased = ConvertTo-TitleCase $key
                    if (-not $subsectionMapping.ContainsKey($titleCased)) {
                        $subsectionMapping[$titleCased] = @{
                            Existing = $key
                            New = $null
                        }
                    }
                }
            }
        }
        
        if ($hasNew) {
            foreach ($key in $groupedEntries[$section].Keys) {
                if ($key -ne "") {
                    $titleCased = ConvertTo-TitleCase $key
                    if ($subsectionMapping.ContainsKey($titleCased)) {
                        $subsectionMapping[$titleCased].New = $key
                    } else {
                        $subsectionMapping[$titleCased] = @{
                            Existing = $null
                            New = $key
                        }
                    }
                }
            }
        }
        
        $allSubsections = $subsectionMapping.Keys | Sort-Object
        
        foreach ($subsectionTitleCased in $allSubsections) {
            $mapping = $subsectionMapping[$subsectionTitleCased]
            $mergedContent += ""
            $mergedContent += "#### $subsectionTitleCased"
            $mergedContent += ""  # Empty line before entries
            
            # Existing subsection entries
            if ($mapping.Existing -and $hasExisting -and $existingSections[$section].ContainsKey($mapping.Existing)) {
                foreach ($line in $existingSections[$section][$mapping.Existing]) {
                    $mergedContent += $line
                }
            }
            
            # New subsection entries
            if ($mapping.New -and $hasNew -and $groupedEntries[$section].ContainsKey($mapping.New)) {
                foreach ($entry in $groupedEntries[$section][$mapping.New]) {
                    $description = $entry.Description
                    # Ensure description ends with a period
                    if (-not $description.EndsWith(".")) {
                        $description = $description + "."
                    }
                    $prLink = if ($entry.PR -gt 0) { " [[#$($entry.PR)](https://github.com/microsoft/mcp/pull/$($entry.PR))]" } else { "" }
                    $mergedContent += "- $description$prLink"
                }
            }
        }
    }
}

# Preview output
Write-Host "Compiled Output (as it will appear in CHANGELOG.md):" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
$mergedContent | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN - No files were modified" -ForegroundColor Yellow
    exit 0
}

# Now apply the changes to the file
if (-not $match.Success) {
    Write-Host "Target section '$targetVersionHeader' not found in CHANGELOG.md" -ForegroundColor Yellow
    Write-Host "Creating new section..." -ForegroundColor Yellow
    
    # Create a new section using the merged content
    $newVersionSection = ($mergedContent -join "`n") + "`n`n"
    
    # Find the first ## section and insert before it
    $firstSectionPattern = '(?m)^##\s+'
    $firstSectionMatch = [regex]::Match($changelogContent, $firstSectionPattern)
    
    if ($firstSectionMatch.Success) {
        # Insert the new section right before the first ## section
        $insertPosition = $firstSectionMatch.Index
        $updatedChangelog = $changelogContent.Insert($insertPosition, "$newVersionSection")
    } else {
        # If no ## section found, append to the end of the file
        $updatedChangelog = $changelogContent + "`n$newVersionSection"
    }
} else {
    # Replace existing section with merged content
    $versionHeader = $match.Groups[1].Value
    $existingContent = $match.Groups[2].Value
    
    # Build the new content from merged array
    $newContent = ($mergedContent -join "`n")
    
    # Ensure there's an empty line at the end of the version section
    if (-not $newContent.EndsWith("`n`n")) {
        $newContent += "`n"
    }
    
    # Replace in changelog
    $updatedChangelog = $changelogContent -replace [regex]::Escape($versionHeader + $existingContent), $newContent
}

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
