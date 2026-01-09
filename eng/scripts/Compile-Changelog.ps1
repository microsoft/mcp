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
    Path to the CHANGELOG.md file (required).
    The changelog-entries directory is inferred from this path (same directory as CHANGELOG.md).
    Examples: "servers/Azure.Mcp.Server/CHANGELOG.md", "servers/Fabric.Mcp.Server/CHANGELOG.md"

.PARAMETER Version
    Target version section to compile entries into (e.g., "2.0.0-beta.3", "1.5.2").
    If not specified, uses the "Unreleased" section or creates one.

.PARAMETER DryRun
    Preview what will be compiled without modifying any files.

.PARAMETER DeleteFiles
    Delete YAML files after successful compilation.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -DryRun

    Preview what will be compiled for Azure.Mcp.Server without making changes.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md"

    Compile entries into the Unreleased section for Fabric.Mcp.Server.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -Version "2.0.0-beta.3"

    Compile entries into the 2.0.0-beta.3 version section for Azure.Mcp.Server.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md" -Version "1.0.0"

    Compile entries into the 1.0.0 version section for Fabric.Mcp.Server.

.EXAMPLE
    ./eng/scripts/Compile-Changelog.ps1 -DeleteFiles

    Compile entries and remove YAML files after successful compilation.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ChangelogPath,

    [Parameter(Mandatory = $false)]
    [string]$Version,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$DeleteFiles
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/../common/scripts/Helpers/PSModule-Helpers.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Infer changelog-entries path from CHANGELOG.md path
$changelogDir = Split-Path $ChangelogPath -Parent
$ChangelogEntriesPath = Join-Path $changelogDir "changelog-entries"

# Helper function to convert text to title case (capitalize first letter of each word)
function ConvertTo-TitleCase {
    param([string]$Text)
    
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $Text
    }
    
    # Use TextInfo for proper title casing
    $textInfo = (Get-Culture).TextInfo
    return $textInfo.ToTitleCase($Text.ToLowerInvariant())
}

# Helper function to extract PR number from the git commit that introduced a file
# GitHub squash merges include PR number in format "... (#1234)" in commit message
function Get-PrFromGitCommit {
    param([string]$FilePath)
    
    try {
        # Get the first (oldest) commit that touched this file
        # Using --follow to track renames, and --reverse to get oldest first
        $commitMessage = git log --follow --reverse --format="%s" -- $FilePath 2>$null | Select-Object -First 1
        
        if ($commitMessage -and $commitMessage -match '\(#(\d+)\)\s*$') {
            return [int]$matches[1]
        }
    }
    catch {
        # Git command failed, return 0
    }
    
    return 0
}

# Helper function to normalize indentation in multi-line text
# Converts tabs to spaces and standardizes indent levels to 2 spaces
function Normalize-Indentation {
    param([string]$Line)
    
    # Convert tabs to 2 spaces
    $line = $Line -replace "`t", "  "
    
    # Count leading spaces and normalize to multiples of 2
    if ($line -match '^(\s+)(.*)$') {
        $leadingSpaces = $matches[1].Length
        $content = $matches[2]
        # Round to nearest multiple of 2 (minimum 2 if any indentation exists)
        $normalizedSpaces = [Math]::Max(2, [Math]::Ceiling($leadingSpaces / 2) * 2)
        return (" " * $normalizedSpaces) + $content
    }
    return $line
}

# Helper function to format a changelog entry with description and PR link
function Format-ChangelogEntry {
    param(
        [string]$Description,
        [int]$PR
    )
    
    # Trim leading and trailing whitespace from the entire description
    $Description = $Description.Trim()
    
    # Normalize tabs to spaces throughout the description
    $Description = $Description -replace "`t", "  "
    
    # Check if description contains multiple lines
    if ($Description.Contains("`n")) {
        # Remove trailing newlines from YAML block scalars
        $cleanedDescription = $Description.TrimEnd("`n", "`r")
        $lines = $cleanedDescription -split "`n"
        $formattedLines = @()
        
        # Check if this is a list (first line followed by bullet items)
        $isList = $lines.Length -gt 1 -and $lines[1].TrimStart() -match '^-\s+'
        $prLink = if ($PR -gt 0) { " [[#$PR](https://github.com/microsoft/mcp/pull/$PR)]" } else { "" }
        
        for ($i = 0; $i -lt $lines.Length; $i++) {
            $line = $lines[$i].TrimEnd()  # Trim trailing spaces from each line
            
            if ($i -eq 0) {
                # First line: main bullet point - also trim leading spaces
                $line = $line.TrimStart()
                # Ensure it ends with colon if followed by a list, otherwise period
                if ($isList) {
                    # Next line is a bullet, so first line should end with colon
                    if (-not $line.EndsWith(":")) {
                        $line += ":"
                    }
                    # For lists, add PR link to first line
                    $formattedLines += "- $line$prLink"
                }
                else {
                    # Regular multi-line text, ensure valid sentence ending
                    if ($line -notmatch '[.!?\)\]]$') {
                        $line += "."
                    }
                    $formattedLines += "- $line"
                }
            }
            elseif ($i -eq $lines.Length - 1) {
                # Last line: normalize indentation and add PR link for non-list entries
                $normalizedLine = Normalize-Indentation $line.TrimEnd()
                
                if ($isList) {
                    # For lists, PR link was added to first line, just add the last bullet
                    $formattedLines += "  $normalizedLine"
                }
                else {
                    # For regular multi-line text, add PR link to last line
                    # Ensure valid sentence ending for non-bullet text
                    $content = $normalizedLine.TrimStart()
                    $needsPeriod = -not ($content -match '^-\s+') -and ($content -notmatch '[.!?\)\]]$')
                    if ($needsPeriod) {
                        $normalizedLine += "."
                    }
                    $formattedLines += "  $normalizedLine$prLink"
                }
            }
            else {
                # Middle lines: normalize indentation and add 2 spaces for nesting
                $normalizedLine = Normalize-Indentation $line.TrimEnd()
                $formattedLines += "  $normalizedLine"
            }
        }
        
        return $formattedLines -join "`n"
    }
    else {
        # Single-line description: original logic
        $formattedDescription = $Description
        # Ensure valid sentence ending (avoid double punctuation)
        if ($formattedDescription -notmatch '[.!?\)\]]$') {
            $formattedDescription += "."
        }
        
        # Add PR link if available
        $prLink = if ($PR -gt 0) { " [[#$PR](https://github.com/microsoft/mcp/pull/$PR)]" } else { "" }
        return "- $formattedDescription$prLink"
    }
}

# Helper function to build subsection mapping (merging with case-insensitive matching)
function Build-SubsectionMapping {
    param(
        [hashtable]$ExistingSections,
        [hashtable]$GroupedEntries,
        [string]$Section
    )
    
    $subsectionMapping = @{}
    $hasExisting = $ExistingSections.ContainsKey($Section)
    $hasNew = $GroupedEntries.ContainsKey($Section)
    
    if ($hasExisting) {
        foreach ($key in $ExistingSections[$Section].Keys) {
            if ($key -ne "") {
                $titleCased = ConvertTo-TitleCase $key
                if (-not $subsectionMapping.ContainsKey($titleCased)) {
                    $subsectionMapping[$titleCased] = @{
                        Existing = $key
                        New      = $null
                    }
                }
            }
        }
    }
    
    if ($hasNew) {
        foreach ($key in $GroupedEntries[$Section].Keys) {
            if ($key -ne "") {
                $titleCased = ConvertTo-TitleCase $key
                if ($subsectionMapping.ContainsKey($titleCased)) {
                    $subsectionMapping[$titleCased].New = $key
                }
                else {
                    $subsectionMapping[$titleCased] = @{
                        Existing = $null
                        New      = $key
                    }
                }
            }
        }
    }
    
    return $subsectionMapping
}

# Set up paths using $RepoRoot from common.ps1
$changelogFile = Join-Path $RepoRoot $ChangelogPath
$changelogEntriesDir = Join-Path $RepoRoot $ChangelogEntriesPath

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

# Get all YAML files, excluding the example template file
# Note: Using -Filter twice and combining results since -Include requires -Recurse
$yamlFiles = @(Get-ChildItem -Path $changelogEntriesDir -Filter "*.yaml" -File)
$yamlFiles += @(Get-ChildItem -Path $changelogEntriesDir -Filter "*.yml" -File)
$yamlFiles = @($yamlFiles | Where-Object { $_.BaseName -ne 'username-example-brief-description' })

if ($yamlFiles.Count -eq 0) {
    Write-Host "No changelog entries found in $changelogEntriesDir" -ForegroundColor Yellow
    Write-Host "Nothing to compile." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($yamlFiles.Count) changelog entry file(s)" -ForegroundColor Green
Write-Host ""

Install-ModuleIfNotInstalled "powershell-yaml" "0.4.7" | Import-Module

# Load JSON schema and extract valid values
$schemaPath = Join-Path $RepoRoot "eng/schemas/changelog-entry.schema.json"
if (-not (Test-Path $schemaPath)) {
    Write-Error "Schema file not found: $schemaPath"
    exit 1
}

$schema = Get-Content -Path $schemaPath -Raw | ConvertFrom-Json
$validSections = $schema.properties.changes.items.properties.section.enum
$validSubsections = $schema.properties.changes.items.properties.subsection.enum
$minDescriptionLength = $schema.properties.changes.items.properties.description.minLength

Write-Host "Loaded validation rules from schema:" -ForegroundColor Gray
Write-Host "  Valid sections: $($validSections -join ', ')" -ForegroundColor Gray
Write-Host "  Valid subsections: $($validSubsections -join ', ')" -ForegroundColor Gray
Write-Host "  Min description length: $minDescriptionLength" -ForegroundColor Gray
Write-Host ""

# Parse and validate YAML files
$entries = @()

foreach ($file in $yamlFiles) {
    Write-Host "Processing: $($file.Name)" -ForegroundColor Gray
    
    try {
        $yamlContent = Get-Content -Path $file.FullName -Raw
        $entry = $yamlContent | ConvertFrom-Yaml
        
        # Handle PR field - it's optional, will be auto-detected from git if not provided
        if (-not $entry.ContainsKey('pr') -or -not $entry['pr']) {
            # Try to extract PR from git commit that introduced this file
            $detectedPR = Get-PrFromGitCommit -FilePath $file.FullName
            if ($detectedPR -gt 0) {
                $entry['pr'] = $detectedPR
                Write-Host "  Auto-detected PR #$detectedPR from git history" -ForegroundColor Green
            }
            else {
                $entry['pr'] = 0
                Write-Warning "  PR number not provided in $($file.Name) and could not be auto-detected from git"
            }
        }
        elseif ($entry['pr'] -lt 1) {
            Write-Error "  Invalid PR number in $($file.Name) (must be a positive integer)"
            continue
        }
        
        if (-not $entry.ContainsKey('changes') -or -not $entry['changes'] -or $entry['changes'].Count -eq 0) {
            Write-Error "  Missing or empty 'changes' array in $($file.Name)"
            continue
        }
        
        # Process each change in the array
        $changeCount = 0
        foreach ($change in $entry.changes) {
            # Validate required fields for each change
            if (-not $change['section']) {
                Write-Error "  Missing required field 'section' in change #$($changeCount + 1) of $($file.Name)"
                continue
            }
            
            # Validate and normalize section (case-insensitive)
            $matchedSection = $validSections | Where-Object { $_ -ieq $change['section'] }
            if ($matchedSection) {
                $normalizedSection = $matchedSection
            }
            else {
                Write-Error "  Invalid section '$($change['section'])' in change #$($changeCount + 1) of $($file.Name). Must be one of: $($validSections -join ', ')"
                continue
            }
            
            if (-not $change['description']) {
                Write-Error "  Missing required field 'description' in change #$($changeCount + 1) of $($file.Name)"
                continue
            }
            
            if ($change['description'].Length -lt $minDescriptionLength) {
                Write-Error "  Description too short in change #$($changeCount + 1) of $($file.Name) (minimum $minDescriptionLength characters)"
                continue
            }
            
            # Validate subsection if provided
            $subsection = $null
            if ($change.ContainsKey('subsection') -and $change['subsection'] -and $change['subsection'] -ne "null") {
                $providedSubsection = $change['subsection']
                $matchedSubsection = $validSubsections | Where-Object { $_ -ieq $providedSubsection }
                if (-not $matchedSubsection) {
                    Write-Error "  Invalid subsection '$providedSubsection' in change #$($changeCount + 1) of $($file.Name). Valid subsections: $($validSubsections -join ', ')"
                    continue
                }
                $subsection = $matchedSubsection
            }
            
            # Add to entries collection with normalized section name
            $entries += [PSCustomObject]@{
                Section     = $normalizedSection
                Subsection  = $subsection
                Description = $change['description']
                PR          = $entry['pr']
                Filename    = $file.Name
            }
            $changeCount++
        }
        
        Write-Host "  ✓ Valid ($changeCount change(s))" -ForegroundColor Green
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
# Use $RecommendedSectionHeaders from ChangeLog-Operations.ps1 (imported via common.ps1)
$groupedEntries = @{}

foreach ($section in $RecommendedSectionHeaders) {
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

# Read existing CHANGELOG.md to determine target version
$changelogContent = Get-Content -Path $changelogFile -Raw

# Determine target version section
$targetVersionHeader = $null

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
    Write-Host "✓ Found version section: $targetVersionHeader" -ForegroundColor Green
    Write-Host ""
}
else {
    # No version specified - look for Unreleased section at the top
    Write-Host "No version specified - looking for Unreleased section..." -ForegroundColor Cyan
    
    # Find the first ## header after any initial # headers
    # This regex handles both formats: "## 2.0.0 (Unreleased)" and "## [0.0.1] - 2025-09-16"
    $firstSectionPattern = '(?m)^##\s+(?:\[(.+?)\]|(\S+))(?:\s+-\s+\S+|\s+\([^)]+\))'
    $firstSectionMatch = [regex]::Match($changelogContent, $firstSectionPattern)
    
    if ($firstSectionMatch.Success) {
        $firstSectionFull = $firstSectionMatch.Value
        
        # Check if it's Unreleased
        if ($firstSectionFull -match '\(Unreleased\)') {
            $targetVersionHeader = $firstSectionFull
            Write-Host "✓ Found Unreleased section: $targetVersionHeader" -ForegroundColor Green
            Write-Host ""
        }
        else {
            # First section is not Unreleased - create new Unreleased section
            Write-Host "No Unreleased section found at the top of CHANGELOG.md" -ForegroundColor Yellow
            Write-Host "Creating new Unreleased section with next version number..." -ForegroundColor Yellow
            
            # Parse current version to determine next version
            # Handle both bracketed and non-bracketed version formats
            $currentVersion = if ($firstSectionMatch.Groups[1].Success) { 
                $firstSectionMatch.Groups[1].Value.Trim() 
            }
            else { 
                $firstSectionMatch.Groups[2].Value.Trim() 
            }
            Write-Host "Current version: $currentVersion" -ForegroundColor Gray
            
            # Parse semantic version using AzureEngSemanticVersion from SemVer.ps1 (imported via common.ps1)
            $semVer = [AzureEngSemanticVersion]::ParseVersionString($currentVersion)
            if ($semVer) {
                # Increment to next prerelease version
                $semVer.IncrementAndSetToPrerelease()
                $nextVersion = $semVer.ToString()
                
                $targetVersionHeader = "## $nextVersion (Unreleased)"
                Write-Host "Next version: $nextVersion" -ForegroundColor Green
                Write-Host ""
            }
            else {
                Write-Error "Unable to parse version number '$currentVersion' to determine next version. Please specify -Version parameter or ensure CHANGELOG.md has a valid version format."
                exit 1
            }
        }
    }
    else {
        Write-Error "No version sections found in CHANGELOG.md. Please add a version section manually or specify -Version parameter."
        exit 1
    }
}

Write-Host "Target section: $targetVersionHeader" -ForegroundColor Cyan
Write-Host ""

# Generate markdown content from grouped entries (needed for new sections)
$newEntriesMarkdown = @()
$isFirstNewSection = $true
foreach ($section in $RecommendedSectionHeaders) {
    if ($groupedEntries.ContainsKey($section)) {
        # Add empty line before section (but not before the very first section)
        if (-not $isFirstNewSection) {
            $newEntriesMarkdown += ""
        }
        $isFirstNewSection = $false
        $newEntriesMarkdown += "### $section"
        $newEntriesMarkdown += ""
        
        $subsections = $groupedEntries[$section]
        # Process entries without subsection first
        if ($subsections.ContainsKey("")) {
            foreach ($entry in $subsections[""]) {
                $newEntriesMarkdown += Format-ChangelogEntry -Description $entry.Description -PR $entry.PR
            }
        }
        
        # Then process subsections
        foreach ($subsectionName in ($subsections.Keys | Where-Object { $_ -ne "" } | Sort-Object)) {
            $newEntriesMarkdown += ""
            $newEntriesMarkdown += "#### $subsectionName"
            $newEntriesMarkdown += ""
            foreach ($entry in $subsections[$subsectionName]) {
                $newEntriesMarkdown += Format-ChangelogEntry -Description $entry.Description -PR $entry.PR
            }
        }
    }
}

# Find the target section in the changelog
$versionSectionPattern = '(?s)(' + [regex]::Escape($targetVersionHeader) + '\r?\n)(.*?)(?=##\s+\d+\.\d+\.\d+|##\s+[^\s]+\s+\(|$)'
$match = [regex]::Match($changelogContent, $versionSectionPattern)

# Build the merged content for preview
$mergedContent = @()
$mergedContent += $targetVersionHeader
$mergedContent += ""  # Empty line after version header

if (-not $match.Success) {
    # New section - just use the new entries
    $mergedContent += $newEntriesMarkdown
}
else {
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
                }
                elseif (-not $existingSections[$currentSection].ContainsKey($currentSubsection)) {
                    $existingSections[$currentSection][$currentSubsection] = @()
                }
            }
        }
        elseif ($line -match '^-\s+' -or ($currentSection -and $line.Trim() -ne "" -and $line -match '^\s+')) {
            # Entry line (starts with -) or continuation line (indented, non-empty)
            # Skip if it's empty or if we're not in a section yet
            if ($currentSection) {
                if ($currentSubsection) {
                    $existingSections[$currentSection][$currentSubsection] += $line
                }
                else {
                    $existingSections[$currentSection][""] += $line
                }
            }
        }
    }
    
    # Build merged content by combining existing and new entries
    $isFirstSection = $true
    foreach ($section in $RecommendedSectionHeaders) {
        # Check if we have entries (existing or new) for this section
        $hasExisting = $existingSections.ContainsKey($section)
        $hasNew = $groupedEntries.ContainsKey($section)
        
        if (-not $hasExisting -and -not $hasNew) {
            continue
        }
        
        # Collect entries for this section
        $sectionContent = @()
        
        # Merge entries without subsection
        $existingMainEntries = @()
        if ($hasExisting -and $existingSections[$section].ContainsKey("")) {
            $existingMainEntries = @($existingSections[$section][""] | Where-Object { $_.Trim() -ne '' })
        }
        $newMainEntries = @()
        
        if ($hasNew -and $groupedEntries[$section].ContainsKey("")) {
            foreach ($entry in $groupedEntries[$section][""]) {
                $newMainEntries += Format-ChangelogEntry -Description $entry.Description -PR $entry.PR
            }
        }
        
        # Add main entries to section content
        $totalMainEntries = $existingMainEntries.Count + $newMainEntries.Count
        if ($totalMainEntries -gt 0) {
            $sectionContent += ""  # Empty line before entries
            foreach ($line in $existingMainEntries) {
                $sectionContent += $line
            }
            foreach ($line in $newMainEntries) {
                $sectionContent += $line
            }
        }
        
        # Merge subsections - normalize subsection names (case-insensitive, title case)
        $subsectionMapping = Build-SubsectionMapping -ExistingSections $existingSections -GroupedEntries $groupedEntries -Section $section
        $allSubsections = $subsectionMapping.Keys | Sort-Object
        
        foreach ($subsectionTitleCased in $allSubsections) {
            $mapping = $subsectionMapping[$subsectionTitleCased]
            $subsectionEntries = @()
            
            # Existing subsection entries (filter out empty lines)
            if ($mapping.Existing -and $hasExisting -and $existingSections[$section].ContainsKey($mapping.Existing)) {
                $subsectionEntries += @($existingSections[$section][$mapping.Existing] | Where-Object { $_.Trim() -ne '' })
            }
            
            # New subsection entries
            if ($mapping.New -and $hasNew -and $groupedEntries[$section].ContainsKey($mapping.New)) {
                foreach ($entry in $groupedEntries[$section][$mapping.New]) {
                    $subsectionEntries += Format-ChangelogEntry -Description $entry.Description -PR $entry.PR
                }
            }
            
            # Only add subsection if it has content
            if ($subsectionEntries.Count -gt 0) {
                $sectionContent += ""
                $sectionContent += "#### $subsectionTitleCased"
                $sectionContent += ""  # Empty line before entries
                $sectionContent += $subsectionEntries
            }
        }
        
        # Only add section to merged content if it has any content
        if ($sectionContent.Count -gt 0) {
            # Add empty line before section (but not before the very first section)
            if (-not $isFirstSection) {
                $mergedContent += ""
            }
            $isFirstSection = $false
            
            $mergedContent += "### $section"
            $mergedContent += $sectionContent
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
    }
    else {
        # If no ## section found, append to the end of the file
        $updatedChangelog = $changelogContent + "`n$newVersionSection"
    }
}
else {
    # Replace existing section with merged content
    $versionHeader = $match.Groups[1].Value
    $existingContent = $match.Groups[2].Value
    
    # Build the new content from merged array
    $newContent = ($mergedContent -join "`n")
    
    # Ensure there's an empty line at the end of the version section (two newlines total)
    if (-not $newContent.EndsWith("`n`n")) {
        if ($newContent.EndsWith("`n")) {
            $newContent += "`n"
        }
        else {
            $newContent += "`n`n"
        }
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
