#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a new changelog entry YAML file.

.DESCRIPTION
    This script helps create properly formatted changelog entry files.
    Each entry is stored as a separate YAML file to avoid merge conflicts.

.PARAMETER Description
    Description of the change (minimum 10 characters).

.PARAMETER Section
    The changelog section. Valid values: "Features Added", "Breaking Changes", "Bugs Fixed", "Other Changes".

.PARAMETER Subsection
    Optional subsection for grouping related changes (e.g., "Dependency Updates", "Telemetry").

.PARAMETER PR
    Pull request number (integer).

.PARAMETER ChangelogEntriesPath
    Path to the changelog-entries directory. Defaults to servers/Azure.Mcp.Server/changelog-entries.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1

    Runs in interactive mode, prompting for all required fields.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -Description "Added new feature" -Section "Features Added" -PR 1234

    Creates a changelog entry with the specified parameters.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -Description "Updated Azure.Core to 1.2.3" -Section "Other Changes" -Subsection "Dependency Updates" -PR 1234

    Creates a changelog entry with a subsection.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Description,

    [Parameter(Mandatory = $false)]
    [string]$Section,

    [Parameter(Mandatory = $false)]
    [string]$Subsection,

    [Parameter(Mandatory = $false)]
    [int]$PR,

    [Parameter(Mandatory = $false)]
    [string]$ChangelogEntriesPath = "servers/Azure.Mcp.Server/changelog-entries"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Valid sections for validation
$validSections = @("Features Added", "Breaking Changes", "Bugs Fixed", "Other Changes")

# Validate and normalize Section parameter if provided (case-insensitive)
if ($Section) {
    $matchedSection = $validSections | Where-Object { $_ -ieq $Section }
    if ($matchedSection) {
        # Use the properly cased version
        $Section = $matchedSection
    } else {
        Write-Host ""
        Write-Host "ERROR: Invalid section '$Section'" -ForegroundColor Red
        Write-Host ""
        Write-Host "Valid sections are:" -ForegroundColor Yellow
        Write-Host "  - Features Added" -ForegroundColor Yellow
        Write-Host "  - Breaking Changes" -ForegroundColor Yellow
        Write-Host "  - Bugs Fixed" -ForegroundColor Yellow
        Write-Host "  - Other Changes" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Example usage:" -ForegroundColor Cyan
        Write-Host '  .\eng\scripts\New-ChangelogEntry.ps1 -Section "Features Added" -Description "..." -PR 1234' -ForegroundColor Gray
        Write-Host ""
        exit 1
    }
}

# Validate PR parameter if provided
if ($PR -and $PR -lt 1) {
    Write-Host ""
    Write-Host "ERROR: PR number must be a positive integer (got: $PR)" -ForegroundColor Red
    Write-Host ""
    exit 1
}

# Get repository root
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$changelogEntriesDir = Join-Path $repoRoot $ChangelogEntriesPath
$schemaPath = Join-Path $repoRoot "eng/schemas/changelog-entry.schema.json"

# Create changelog-entries directory if it doesn't exist
if (-not (Test-Path $changelogEntriesDir)) {
    Write-Host "Creating changelog-entries directory: $changelogEntriesDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $changelogEntriesDir | Out-Null
}

# Interactive mode if parameters not provided
if (-not $Description) {
    Write-Host "`nChangelog Entry Creator" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    Write-Host ""
    
    $Description = Read-Host "Description (minimum 10 characters)"
    while ($Description.Length -lt 10) {
        Write-Host "Description must be at least 10 characters long." -ForegroundColor Red
        $Description = Read-Host "Description (minimum 10 characters)"
    }
}

if (-not $Section) {
    Write-Host "`nSelect a section:" -ForegroundColor Cyan
    Write-Host "1. Features Added"
    Write-Host "2. Breaking Changes"
    Write-Host "3. Bugs Fixed"
    Write-Host "4. Other Changes"
    
    $choice = Read-Host "`nEnter choice (1-4)"
    $Section = switch ($choice) {
        "1" { "Features Added" }
        "2" { "Breaking Changes" }
        "3" { "Bugs Fixed" }
        "4" { "Other Changes" }
        default {
            Write-Host ""
            Write-Host "ERROR: Invalid choice '$choice'. Please select 1-4." -ForegroundColor Red
            Write-Host ""
            exit 1
        }
    }
}

if (-not $Subsection -and $Section -eq "Other Changes") {
    $subsectionInput = Read-Host "`nSubsection (optional, press Enter to skip)"
    if ($subsectionInput) {
        # Title case the subsection
        $textInfo = (Get-Culture).TextInfo
        $Subsection = $textInfo.ToTitleCase($subsectionInput.ToLower())
    }
}

if (-not $PR) {
    $prInput = Read-Host "`nPR number (press Enter to skip if not known yet)"
    if ($prInput) {
        $PR = [int]$prInput
    }
}

# Ensure description ends with a period
if (-not $Description.EndsWith(".")) {
    $Description = $Description + "."
}

# Generate filename with timestamp in milliseconds
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
$filename = "$timestamp.yml"
$filepath = Join-Path $changelogEntriesDir $filename

# Create YAML content
$yamlContent = @"
section: "$Section"
description: "$Description"
"@

if ($Subsection) {
    $yamlContent += "`nsubsection: `"$Subsection`""
} else {
    $yamlContent += "`nsubsection: null"
}

if ($PR) {
    $yamlContent += "`npr: $PR"
} else {
    $yamlContent += "`npr: 0  # TODO: Update with actual PR number"
}

# Write YAML file
$yamlContent | Set-Content -Path $filepath -Encoding UTF8 -NoNewline

Write-Host "`n✓ Created changelog entry: $filename" -ForegroundColor Green
Write-Host "  Location: $filepath" -ForegroundColor Gray
Write-Host "  Section: $Section" -ForegroundColor Gray
if ($Subsection) {
    Write-Host "  Subsection: $Subsection" -ForegroundColor Gray
}
Write-Host "  Description: $Description" -ForegroundColor Gray
if ($PR) {
    Write-Host "  PR: #$PR" -ForegroundColor Gray
} else {
    Write-Host "  PR: Not set (remember to update before merging)" -ForegroundColor Yellow
}

# Validate against schema if available
if (Test-Path $schemaPath) {
    Write-Host "`nValidating against schema..." -ForegroundColor Cyan
    
    # Try to use PowerShell-Yaml module if available
    $yamlModule = Get-Module -ListAvailable -Name "powershell-yaml"
    if ($yamlModule) {
        Import-Module powershell-yaml -ErrorAction SilentlyContinue
        
        try {
            $yamlData = Get-Content -Path $filepath -Raw | ConvertFrom-Yaml
            
            # Basic validation (section already validated at top of script)
            if ($yamlData.description.Length -lt 10) {
                Write-Host ""
                Write-Host "ERROR: Description must be at least 10 characters" -ForegroundColor Red
                Write-Host ""
                exit 1
            }
            
            if ($PR -and $yamlData.pr -lt 1) {
                Write-Host ""
                Write-Host "ERROR: PR number must be positive" -ForegroundColor Red
                Write-Host ""
                exit 1
            }
            
            Write-Host "✓ Validation passed" -ForegroundColor Green
        }
        catch {
            Write-Warning "Could not validate YAML: $_"
        }
    }
    else {
        Write-Host "  Note: Install 'powershell-yaml' module for automatic validation" -ForegroundColor Gray
        Write-Host "  Run: Install-Module -Name powershell-yaml" -ForegroundColor Gray
    }
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Commit this file with your changes" -ForegroundColor Gray
if (-not $PR) {
    Write-Host "2. Update the 'pr' field in the YAML file once you have a PR number" -ForegroundColor Gray
}
Write-Host ""
