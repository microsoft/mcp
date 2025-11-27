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
    Optional subsection for grouping related changes. Valid values: "Dependency Updates".

.PARAMETER PR
    Pull request number (integer).

.PARAMETER Filename
    Optional custom filename for the changelog entry (without path).
    If not provided, a timestamp-based filename will be generated.
    Example: "vcolin7-fix-serialization.yaml"

.PARAMETER ChangelogPath
    Path to the CHANGELOG.md file
    The changelog-entries directory is inferred from this path (same directory as CHANGELOG.md).
    Examples: "servers/Azure.Mcp.Server/CHANGELOG.md", "servers/Fabric.Mcp.Server/CHANGELOG.md"
    If not provided, you will be prompted to select a server interactively.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1

    Runs in fully interactive mode, prompting for server selection and all required fields.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md"

    Runs in interactive mode for Azure.Mcp.Server, prompting for all required fields.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md"

    Runs in interactive mode for Fabric.Mcp.Server.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -Description "Added new feature" -Section "Features Added" -PR 1234

    Creates a changelog entry for Azure.Mcp.Server with the specified parameters.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Fabric.Mcp.Server/CHANGELOG.md" -Description "Updated Azure.Core to 1.2.3" -Section "Other Changes" -Subsection "Dependency Updates" -PR 1234

    Creates a changelog entry for Fabric.Mcp.Server with a subsection.

.EXAMPLE
    ./eng/scripts/New-ChangelogEntry.ps1 -ChangelogPath "servers/Azure.Mcp.Server/CHANGELOG.md" -Description "Fixed serialization bug" -Section "Bugs Fixed" -PR 1234 -Filename "vcolin7-fix-serialization"

    Creates a changelog entry with a custom filename (vcolin7-fix-serialization.yaml).

.EXAMPLE
    $description = @"
Added new AI Foundry tools:
- foundry_agents_create: Create a new AI Foundry agent
- foundry_threads_create: Create a new AI Foundry Agent Thread
- foundry_threads_list: List all AI Foundry Agent Threads
"@
    ./eng/scripts/New-ChangelogEntry.ps1 -Description $description -Section "Features Added" -PR 945

    Creates a changelog entry with a multi-line description containing a list.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ChangelogPath,

    [Parameter(Mandatory = $false)]
    [string]$Description,

    [Parameter(Mandatory = $false)]
    [string]$Section,

    [Parameter(Mandatory = $false)]
    [string]$Subsection,

    [Parameter(Mandatory = $false)]
    [int]$PR,

    [Parameter(Mandatory = $false)]
    [string]$Filename
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Determine if we're in interactive mode (any required parameter is missing)
$isInteractive = (-not $ChangelogPath) -or (-not $Description) -or (-not $Section) -or (-not $PSBoundParameters.ContainsKey('PR'))

# Show header once if in interactive mode
if ($isInteractive) {
    Write-Host "`nChangelog Entry Creator" -ForegroundColor Cyan
    Write-Host "======================" -ForegroundColor Cyan
    Write-Host ""
}

# Helper function to convert text to title case (capitalize first letter of each word)
function ConvertTo-TitleCase {
    param([string]$Text)
    
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $Text
    }
    
    # Use invariant culture TextInfo for proper title casing
    $textInfo = [System.Globalization.CultureInfo]::InvariantCulture.TextInfo
    return $textInfo.ToTitleCase($Text.ToLowerInvariant())
}

# Valid sections for validation
$validSections = @("Features Added", "Breaking Changes", "Bugs Fixed", "Other Changes")
$validSubsections = @("Dependency Updates")

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

# Normalize subsection to title case if provided
if ($Subsection) {
    $Subsection = ConvertTo-TitleCase -Text $Subsection
    
    # Validate subsection against allowed values
    $matchedSubsection = $validSubsections | Where-Object { $_ -ieq $Subsection }
    if (-not $matchedSubsection) {
        Write-Host ""
        Write-Host "ERROR: Invalid subsection '$Subsection'" -ForegroundColor Red
        Write-Host ""
        Write-Host "Valid subsections are:" -ForegroundColor Yellow
        foreach ($validSub in $validSubsections) {
            Write-Host "  - $validSub" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "If you need a new subsection, please add it to the schema first." -ForegroundColor Cyan
        Write-Host ""
        exit 1
    }
    # Use the properly cased version
    $Subsection = $matchedSubsection
}

# Interactive prompt for ChangelogPath if not provided
if (-not $ChangelogPath) {
    Write-Host "Available servers:" -ForegroundColor Yellow
    Write-Host "  1. Azure.Mcp.Server   (servers/Azure.Mcp.Server/CHANGELOG.md)"
    Write-Host "  2. Fabric.Mcp.Server  (servers/Fabric.Mcp.Server/CHANGELOG.md)"
    Write-Host "  3. Custom path"
    Write-Host ""
    
    $serverChoice = Read-Host "Select server (1-3)"
    $ChangelogPath = switch ($serverChoice) {
        "1" { "servers/Azure.Mcp.Server/CHANGELOG.md" }
        "2" { "servers/Fabric.Mcp.Server/CHANGELOG.md" }
        "3" { 
            $customPath = Read-Host "Enter custom CHANGELOG.md path"
            $customPath.Trim()
        }
        default {
            Write-Host ""
            Write-Host "ERROR: Invalid choice '$serverChoice'. Please select 1-3." -ForegroundColor Red
            Write-Host ""
            exit 1
        }
    }
    
    Write-Host ""
    Write-Host "Using: $ChangelogPath" -ForegroundColor Green
    Write-Host ""
}

# Infer changelog-entries path from CHANGELOG.md path
$changelogDir = Split-Path $ChangelogPath -Parent
$ChangelogEntriesPath = Join-Path $changelogDir "changelog-entries"

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
    Write-Host "Note: For multi-line descriptions (e.g., with lists), use the -Description parameter with a here-string." -ForegroundColor Gray
    Write-Host ""
    
    $Description = Read-Host "Description (minimum 10 characters)"
    # Trim whitespace from user input
    $Description = $Description.Trim()
    
    while ($Description.Length -lt 10) {
        Write-Host "Description must be at least 10 characters long." -ForegroundColor Red
        $Description = Read-Host "Description (minimum 10 characters)"
        $Description = $Description.Trim()
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

# Allow subsection for any section in interactive mode
if (-not $PSBoundParameters.ContainsKey('Subsection') -and $isInteractive) {
    $subsectionInput = Read-Host "`nSubsection (optional, press Enter to skip)"
    if ($subsectionInput) {
        # Trim and title case the subsection
        $Subsection = ConvertTo-TitleCase -Text $subsectionInput.Trim()
        
        # Validate subsection against allowed values
        $matchedSubsection = $validSubsections | Where-Object { $_ -ieq $Subsection }
        if (-not $matchedSubsection) {
            Write-Host ""
            Write-Host "ERROR: Invalid subsection '$Subsection'" -ForegroundColor Red
            Write-Host ""
            Write-Host "Valid subsections are:" -ForegroundColor Yellow
            foreach ($validSub in $validSubsections) {
                Write-Host "  - $validSub" -ForegroundColor Yellow
            }
            Write-Host ""
            Write-Host "If you need a new subsection, please add it to the schema first." -ForegroundColor Cyan
            Write-Host ""
            exit 1
        }
        # Use the properly cased version
        $Subsection = $matchedSubsection
    }
}

if (-not $PR -and $isInteractive) {
    $prInput = Read-Host "`nPR number (press Enter to skip if not known yet)"
    if ($prInput) {
        $PR = [int]$prInput
    }
}

# Trim whitespace from description if provided via parameter
$Description = $Description.Trim()

# Generate filename (use custom if provided, otherwise timestamp-based)
if ($Filename) {
    # Ensure filename has proper extension
    if (-not ($Filename.EndsWith('.yml') -or $Filename.EndsWith('.yaml'))) {
        $Filename = "$Filename.yaml"
    }
    $filename = $Filename
} else {
    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
    $filename = "$timestamp.yaml"
}
$filepath = Join-Path $changelogEntriesDir $filename

# Create YAML content in new format: pr at top level, changes as an array
if ($PR) {
    $yamlContent = "pr: $PR`n"
} else {
    $yamlContent = "pr: 0  # TODO: Update with actual PR number`n"
}

$yamlContent += "changes:`n"

# Create the change entry
$yamlContent += "  - section: `"$Section`"`n"

# Use block scalar (|) for multi-line descriptions, quoted string for single-line
if ($Description.Contains("`n")) {
    # Multi-line description - use block scalar with proper indentation
    $descriptionLines = $Description -split "`n"
    $yamlContent += "    description: |`n"
    foreach ($line in $descriptionLines) {
        $yamlContent += "      $line`n"
    }
} else {
    # Single-line description - use quoted string
    # Escape backslashes and double quotes in the description
    $escapedDescription = $Description -replace '\\', '\\\\' -replace '"', '\"'
    $yamlContent += "    description: `"$escapedDescription`"`n"
}

if ($Subsection) {
    $yamlContent += "    subsection: `"$Subsection`"`n"
}

# Remove trailing newline
$yamlContent = $yamlContent.TrimEnd("`n")

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
            
            # Validate new format
            if (-not $yamlData.pr -and $yamlData.pr -ne 0) {
                Write-Host ""
                Write-Host "ERROR: PR field is required" -ForegroundColor Red
                Write-Host ""
                exit 1
            }
            
            if (-not $yamlData.changes -or $yamlData.changes.Count -eq 0) {
                Write-Host ""
                Write-Host "ERROR: At least one change is required" -ForegroundColor Red
                Write-Host ""
                exit 1
            }
            
            foreach ($change in $yamlData.changes) {
                if ($change.description.Length -lt 10) {
                    Write-Host ""
                    Write-Host "ERROR: Description must be at least 10 characters" -ForegroundColor Red
                    Write-Host ""
                    exit 1
                }
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
