#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Performs tool name character validations for MCP tools.

.DESCRIPTION
    This script validates that tool names defined in *Command.cs and *Setup.cs files follow 
    the naming conventions. Each tool name group must:
    - Contain only alphanumeric characters or dashes
    - Not start or end with a dash

    The script scans all tool area directories under the specified ToolsDirectory, extracts
    tool names from the Name property definitions, and validates them against the naming rules.

.PARAMETER ToolsDirectory
    The root directory containing tool area subdirectories. Each subdirectory is scanned for
    *Command.cs and *Setup.cs files containing tool name definitions.

.OUTPUTS
    PSCustomObject
    Returns an object with the following properties:

    HasViolations [bool]
        True if any tool name violations were found, False otherwise.

    Violations [ToolNameViolation[]]
        An array of ToolNameViolation objects, each containing:
        - ToolArea [string]: The name of the tool area directory where the violation was found
        - FileName [string]: Full path to the file containing the violation
        - ToolName [string]: The invalid tool name

    ErrorMessage [string]
        Description of the naming rule that was violated.

.EXAMPLE
    PS> $result = .\Test-ToolNameCharacters.ps1 -ToolsDirectory "D:\git\mcp\tools"
    PS> $result.HasViolations
    False

    Validates all tools and returns no violations when all names are valid.

.EXAMPLE
    PS> $result = .\Test-ToolNameCharacters.ps1 -ToolsDirectory "D:\git\mcp\tools"
    PS> $result.HasViolations
    True
    PS> $result.Violations | Format-Table ToolArea, ToolName, FileName

    ToolArea                     ToolName          FileName
    --------                     --------          --------
    Azure.Mcp.Tools.Storage      -invalid-start    D:\git\mcp\tools\...\BlobCommand.cs
    Azure.Mcp.Tools.KeyVault     ends-with-dash-   D:\git\mcp\tools\...\SecretCommand.cs

    Shows how to list all violations when invalid tool names are found.

.EXAMPLE
    PS> $result = .\Test-ToolNameCharacters.ps1 -ToolsDirectory "D:\git\mcp\tools"
    PS> $result.Violations | Format-List *

    ToolArea : Azure.Mcp.Tools.Storage
    FileName : D:\git\mcp\tools\Azure.Mcp.Tools.Storage\src\Commands\BlobCommand.cs
    ToolName : storage_blob_get

    Detailed view of a violation. The tool name "storage_blob_get" is invalid because it 
    contains underscores instead of dashes.

.EXAMPLE
    PS> $result = .\Test-ToolNameCharacters.ps1 -ToolsDirectory "D:\git\mcp\tools" -Debug
    
    Runs validation with debug output showing each file processed and a summary of violations.
#>
param(
    [Parameter(Mandatory)]
    [string]$ToolsDirectory
)

class ToolArea {
    [string]$ToolArea
    [ToolNameViolation[]]$Violations

    ToolArea([string]$ToolArea) {
        $this.ToolArea = $ToolArea
        $this.Violations = @()
    }

    [void]AddViolation([ToolNameViolation]$violation) {
        $this.Violations += $violation
    }

    [bool]HasViolations() {
        return $this.Violations.Count -gt 0
    }
}

class ToolNameViolation {
    [string]$ToolArea
    [string]$ToolName
    [string]$FileName
    
    ToolNameViolation([string]$ToolArea, [string]$FileName, [string]$ToolName) {
        $this.ToolArea = $ToolArea
        $this.FileName = $FileName
        $this.ToolName = $ToolName
    }
}

function Test-ToolAreaTools {
    param(
        [Parameter(Mandatory)]
        [string]$ToolAreaDirectory
    )

    $name = $(Split-Path $ToolAreaDirectory -Leaf)
    $toolViolationsForArea = @()

    $areaTools = Get-ChildItem -Path $ToolAreaDirectory -Recurse -Include *Command.cs, *Setup.cs

    if ($areaTools.Count -eq 0) {
        Write-Warning "No files with *Command.cs or *Setup.cs found in area: $ToolAreaDirectory"
    }

    foreach ($file in $areaTools) {
        Write-Debug "Processing: $($file.FullName)"
        $content = Get-Content $file.FullName

        if ($file.Name -like '*Command.cs') {
            $matchingPattern = 'public override string Name => "(.*)";'
        } elseif ($file.Name -like '*Setup.cs') {
            $matchingPattern = 'public string Name => "(.*)";'
        } else {
            throw "Unexpected file name: $($file.Name)"
        }
        
        foreach ($line in $content) {
            if ($line -match $matchingPattern) {
                $toolName = $matches[1]

                # Validate tool name format:
                # - Each group contains alphanumeric chars or dashes
                # - Each group cannot start or end with a dash
                # Pattern breakdown:
                #   ^                           - Start of string
                #   [a-zA-Z0-9]                 - First char must be alphanumeric
                #   ([a-zA-Z0-9-]*[a-zA-Z0-9])? - Optional: middle chars (alphanumeric or dash) ending with alphanumeric
                #   $                           - End of string
                $isValid = $($ToolName -match '^[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?$')

                if (-not $isValid) {
                    $toolViolationsForArea += [ToolNameViolation]::new($name, $file.FullName, $toolName)
                }
            }
        }
    }

    return $toolViolationsForArea
}

$ErrorActionPreference = 'Stop'

$toolAreaDirectories = Get-ChildItem -Path $ToolsDirectory -Directory
$overallViolations = @()

Write-Debug "Starting tool name character validation for $ToolsDirectory"

foreach ($toolAreaDir in $toolAreaDirectories) {
    Write-Debug "Processing tool area: $($toolAreaDir.FullName)"
    $toolAreaResult = Test-ToolAreaTools -ToolAreaDirectory $toolAreaDir.FullName

    if ($toolAreaResult.Count -gt 0) {
        $overallViolations += $toolAreaResult
    }
}

Write-Debug "=================================================="
Write-Debug "SUMMARY"
Write-Debug "Tools Directory: $ToolsDirectory"
Write-Debug "Total violations found: $($overallViolations.Count)"

foreach ($violation in $overallViolations) {
    Write-Debug "Area: $($violation.ToolArea)"
    Write-Debug "  - ToolName: $($violation.ToolName)"
    Write-Debug "  - File: $($violation.FileName)"
}

Write-Debug "=================================================="

$result = [PSCustomObject]@{
    HasViolations = $overallViolations.Count -gt 0
    Violations     = $overallViolations
    ErrorMessage  = "Names can only contain alphanumeric characters or dashes (cannot start or end with a dash)"
}

$result

if ($overallViolations.Count -gt 0) {
    exit 1
} else {
    exit 0
}