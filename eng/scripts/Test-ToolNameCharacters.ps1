#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Performs tool name character validations.  Each tool name must only contain alphanumeric characters, hyphens, and underscores.

.DESCRIPTION
    This script validates that tool names are valid across all tools.
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
    [string]$FileName
    [string]$ToolName
    
    ToolNameViolation([string]$FileName, [string]$ToolName) {
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
    $toolAreaResult = [ToolArea]::new($name)

    $areaTools = Get-ChildItem -Path $ToolAreaDirectory -Recurse -Include *Command.cs, *Setup.cs

    if ($areaTools.Count -eq 0) {
        Write-Warning "No files with *Command.cs or *Setup.cs found in area: $ToolAreaDirectory"
    }

    foreach ($file in $areaTools) {
        Write-Debug "Processing file: $($file.FullName)"
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
                    $toolAreaResult.AddViolation([ToolNameViolation]::new($file.FullName, $toolName))
                }
            }
        }
    }

    return $toolAreaResult
}

$ErrorActionPreference = 'Stop'

$toolAreaDirectories = Get-ChildItem -Path $ToolsDirectory -Directory
$overallViolations = @()

Write-Debug "Starting tool name character validation for $ToolsDirectory"

foreach ($toolAreaDir in $toolAreaDirectories) {
    Write-Debug "Processing tool area: $($toolAreaDir.FullName)"
    $toolAreaResult = Test-ToolAreaTools -ToolAreaDirectory $toolAreaDir.FullName

    if ($toolAreaResult.HasViolations()) {
        $overallViolations += $toolAreaResult
    }
}

Write-Debug "=================================================="
Write-Debug "SUMMARY"
Write-Debug "Tools Directory: $ToolsDirectory"
Write-Debug "Total violations found: $($overallViolations.Count)"

foreach ($area in $overallViolations) {
    Write-Debug "Area: $($area.ToolArea)"
    foreach ($violation in $area.Violations) {
        Write-Debug "  - ToolName: $($violation.ToolName)"
        Write-Debug "  - File: $($violation.FileName)"
    }
    Write-Debug
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