#!/bin/env pwsh
#Requires -Version 7
<#
## Instructions

This script is used to generate a package-specific README.md from a project README in the repository,
using special comment annotations to mark sections of the markdown that should be conditionally excluded or replaced.

## Usage

To generate a README tailored for a specific package type, run:

eng\scripts\Process-PackageReadMe.ps1 -InputReadMePath "<path to readme>" -OutputDirectory "<output directory>" -PackageType "<nuget|npm|vsix>"

## Supported Annotations

### 1. Section Removal

**Purpose:** Remove one or more lines, or parts of a line of markdown for specified package types.

**Syntax:**
<!-- remove-section: start packageType1;packageType2 -->

e.g.
<!-- remove-section: start nuget;vsix;npm -->
......
various markdown lines
.....
<!-- remove-section: end -->

### 1. Insert Section

insert-section is used to insert a chunk of text into a line for a specified package type. 
e.g.
<!-- insert-section: nuget;vsix;npm {{Text to be inserted}} -->
#>

param(
    [Parameter(Mandatory=$true)]
    [string] $InputReadMePath,
    [Parameter(Mandatory=$true)]
    [string] $OutputDirectory,
    [Parameter(Mandatory=$true)]
    [ValidateSet('nuget','npm','vsix')]
    [string] $PackageType,
    [hashtable] $InsertPayload = @{}
)

$readMeText = Get-Content $InputReadMePath         
$processedReadMe = @()
enum ActionType {
    Append = 0
    Skip = 1
}
$actionStack = [System.Collections.Generic.Stack[ActionType]]::new()
$actionStack.Push([ActionType]::Append)

function AppendLine ([string]$Line) {
    if([string]::IsNullOrWhiteSpace($Line) -and $processedReadMe.Count -gt 0 -and [string]::IsNullOrWhiteSpace($processedReadMe[-1])) {
        return
    }
    $script:processedReadMe += $Line
} 


# Remove leading comment block if present
if ($readMeText[0] -eq "<!--") {
    for ($i = 0; $i -lt $readMeText.Count; $i++) {
        if ($readMeText[$i] -eq "-->") {
            $readMeText = $readMeText | Select-Object -Skip ($i + 1)
            break
        }
    }
}

foreach($line in $readMeText) {
    if ([string]::IsNullOrWhiteSpace($line) -and $actionStack.Peek() -eq [ActionType]::Append) {
        AppendLine -Line ""
        continue
    }

    $lineToAppend = ''
    $lineInProcess = $line

    while($lineInProcess.Length -gt 0) {
        # remove-section: start marks the start of section removal for the package type
        # e.g. <!-- remove-section: start nuget;vsix -->
        if ($lineInProcess -match "(?i)<!--\s*remove-section:\s*start\s+([^>]+?)\s*-->") {
            $action = $actionStack.Peek()
            $matchIdx = $lineInProcess.IndexOf($matches[0])
            if ($action -eq [ActionType]::Append) {
                $lineToAppend += $lineInProcess.Substring(0, $matchIdx)
            }
            $lineInProcess = $lineInProcess.Substring($matchIdx + $matches[0].Length)
            $pkgTypeInfo = $matches[1]
            $pkgTypes = $pkgTypeInfo -split ';'
            if ($pkgTypes -contains $PackageType) {
                $action = [ActionType]::Skip
            }
            $actionStack.Push($action)
            continue
        }

        # remove-section: end marks the end of section removal for the package type
        # e.g. <!-- remove-section: end -->
        if ($lineInProcess -match "(?i)<!--\s*remove-section:\s*end\s*-->") {
            $matchIdx = $lineInProcess.IndexOf($matches[0])
            if ($actionStack.Peek() -eq [ActionType]::Append) {
                $lineToAppend += $lineInProcess.Substring(0, $matchIdx)
            }
            $lineInProcess = $lineInProcess.Substring($matchIdx + $matches[0].Length)
            $actionStack.Pop() | Out-Null
            continue
        }
        if ($actionStack.Peek() -eq [ActionType]::Append) {
            $lineToAppend += $lineInProcess
        }
        break
    }

    ## insert-chunk: start marks chunk insertion for the package type
    ## e.g. <!-- insert-section: nuget;vsix;npm {{ToolTitle}} -->
    ## ToolTitle will be inserted from the InsertPayload hashtable
    $insertSectionPattern = "(?i)<!--\s*insert-section:\s+([^{}]+?)\s*\{\{([\s\S]*?)\}\}\s*-->"
    $matches = [regex]::Matches($lineToAppend, $insertSectionPattern)

    foreach ($match in $matches) {
        $pkgTypeInfo = $match.Groups[1].Value
        $content = $match.Groups[2].Value
        $pkgTypes = $pkgTypeInfo -split ';'
        if ($pkgTypes -contains $PackageType) {
            $contentToDisplay = if ($InsertPayload.ContainsKey($content)) { $InsertPayload[$content] } else { $content }
            $lineToAppend = $lineToAppend -replace [regex]::Escape($match.Value), $contentToDisplay
        } else {
            $lineToAppend = $lineToAppend -replace [regex]::Escape($match.Value), ''
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($lineToAppend)) {
        AppendLine -Line $lineToAppend
    }
}
Set-Content -Path "$OutputDirectory/README.md" -Value $processedReadMe -Force