#!/bin/env pwsh
#Requires -Version 7
<#
## Instructions

This script is used to generate a package-specific README.md from a project README in the repository,
using special comment annotations to mark sections of the markdown that should be conditionally excluded or replaced.

## Usage

To generate a README tailored for a specific package type, run:

. eng\scripts\Process-PackageReadMe.ps1
Extract-PackageSpecificReadMe -InputReadMePath "<path to readme>" -OutputDirectory "<output directory>" -PackageType "<nuget|npm|vsix>"

To validate the annotations in a README file, run:
Validate-PackageReadMe -InputReadMePath "<path to readme>"

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

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

class TagInfo {
    [string] $Tag
    [string] $Position
    [string] $Comment

    TagInfo([string]$tag, [string]$position, [string]$comment) {
        $this.Tag = $tag
        $this.Position = $position
        $this.Comment = $comment
    }
}

class HeadingInfo {
    [int] $Level
    [string] $Title
    [string] $AnchorLink

    HeadingInfo([int]$level, [string]$title, [string]$anchorLink) {
        $this.Level = $level
        $this.Title = $title
        $this.AnchorLink = $anchorLink
    }
}   

function AppendLine ([string]$Line, [ref]$processedReadMe) {
    if([string]::IsNullOrWhiteSpace($Line) -and $processedReadMe.Count -gt 0 -and [string]::IsNullOrWhiteSpace($processedReadMe[-1])) {
        return
    }
    $processedReadMe.Value += $Line
}

function RemoveLeadingComments ([ref]$readMeText) {
    if ($readMeText.Value[0] -eq "<!--") {
        for ($i = 0; $i -lt $readMeText.Value.Count; $i++) {
            if ($readMeText.Value[$i] -eq "-->") {
                $readMeText.Value = $readMeText.Value | Select-Object -Skip ($i + 1)
                break
            }
        }
    }
}

function Extract-PackageSpecificReadMe {
    param (
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

    # Remove leading comment block if present
    RemoveLeadingComments -readMeText ([ref]$readMeText)

    foreach($line in $readMeText) {
        if ([string]::IsNullOrWhiteSpace($line) -and $actionStack.Peek() -eq [ActionType]::Append) {
            AppendLine -Line "" -processedReadMe ([ref]$processedReadMe)
            continue
        }

        $lineToAppend = ''
        $lineInProcess = $line

        while($lineInProcess.Length -gt 0) {
            # remove-section: start marks the start of section removal for the package type
            # e.g. <!-- remove-section: start nuget;vsix remove_azure_logo -->
            if ($lineInProcess -match "(?i)<!--\s*remove-section:\s*start\s+([^\s>]+(?:;[^\s>]+)*)\s*(\w+)?\s*-->") {
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
            # e.g. <!-- remove-section: end remove_azure_logo -->
            if ($lineInProcess -match "(?i)<!--\s*remove-section:\s*end\s*(\w+)?\s*-->") {
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
            AppendLine -Line $lineToAppend -processedReadMe ([ref]$processedReadMe)
        }
    }
    Set-Content -Path "$OutputDirectory/README.md" -Value $processedReadMe -Force
}

function Validate-PackageReadme {
    param (
        [string] $InputReadMePath
    )

    $tagValidationStack = [System.Collections.Generic.Stack[TagInfo]]::new()
    $headingValidationList = [System.Collections.Generic.Dictionary[string, HeadingInfo]]::new()
    $allowedTags = @('remove-section:', 'insert-section:')
    $allowedPositions = @('start', 'end', '')

    $readMeText = Get-Content $InputReadMePath
    
    # Remove leading comment block if present
    RemoveLeadingComments -readMeText ([ref]$readMeText)

    foreach($line in $readMeText) {
        # Save Table of content headings for validation
        if ($line -match "^([ \t]*)- [ ]*\[(.+?)\]\(#([^)]+)\)") {
            $level = $matches[1].Length / 4 + 1
            $title = $matches[2]
            $anchorLink = $matches[3]
            $headingValidationList.Add($title, [HeadingInfo]::new($level, $title, $anchorLink))
        }

        if ($line -match "^(#+)\s+(.*)$") {
            $level = $matches[1].Length
            $title = $matches[2].Trim()
            $anchorLink = $title.ToLower() -replace '[^a-z0-9 ]', '' -replace ' ', '-'
            if ($headingValidationList.ContainsKey($title)) {
                $headingInfo = $headingValidationList[$title]
                if ($headingInfo.Level -ne $level) {
                    Write-Host "Heading level mismatch for '$title'. TOC level: $($headingInfo.Level), Actual level: $level" -ForegroundColor Red
                }
                if ($headingInfo.AnchorLink -ne $anchorLink) {
                    Write-Host "Anchor link mismatch for '$title'. TOC anchor: '$($headingInfo.AnchorLink)', Actual anchor: '$anchorLink'" -ForegroundColor Red
                }
                $headingValidationList.Remove($title) | Out-Null
            }
        }

        $matches = [regex]::Matches($line, "<!--(.*?)-->")

        foreach ($match in $matches) {
            $parts = $match.Value -split "\s+"
            $tag = $parts[1]
            if (-not ($allowedTags -contains $tag)) {
                throw "Invalid tag '$tag'. Allowed values are: $($allowedTags -join ', ')"
            }
            $position = ''
            if ($tag -eq 'remove-section:') { 
                $position = $parts[2]
                if (-not ($allowedPositions -contains $position)) {
                    throw "Invalid position '$position' for tag '$tag'. Allowed values are: $($allowedPositions -join ', ')"
                }
                $comment = ''
                if ($position -eq 'start') {
                    $comment = $parts[4]
                    $tagValidationStack.Push([TagInfo]::new($tag, $position, $comment))
                }
                else {
                    $comment = $parts[3]
                    if ($tagValidationStack.Count -eq 0 -or $tagValidationStack.Peek().Position -ne 'start' -or $tagValidationStack.Peek().Comment -ne $comment) {
                        throw "Mismatched remove-section end tag or missing corresponding start tag for comment '$comment'."
                    }
                    else {
                        $tagValidationStack.Pop() | Out-Null
                    }
                }
            }
        }
    }

    if ($tagValidationStack.Count -gt 0) {
        foreach ($unclosedTag in $tagValidationStack) {
            Write-Host "Unclosed tag found: '$($unclosedTag.Tag)' at position '$($unclosedTag.Position)' with comment '$($unclosedTag.Comment)'" -ForegroundColor Red
        }
        throw "There are unclosed tags in the README. Please check the log for details."
    }

    if ($headingValidationList.Count -gt 0) {
        foreach ($missingHeading in $headingValidationList.Values) {
            Write-Host "Heading '$($missingHeading.Title)' listed in TOC but not found in document." -ForegroundColor Red
        }
        throw "There are missing headings in the README. Please check the log for details."
    }
}

function Validate-All-PackageReadmes {
    $readmeFiles = Get-ChildItem -Path "$RepoRoot/servers" -Recurse -Filter "README.md"
    $hasFailures = $false
    foreach ($file in $readmeFiles) {
        Write-Host "Validating README: $($file.FullName)" -ForegroundColor Yellow
        try {
            Validate-PackageReadme -InputReadMePath $file.FullName
            Write-Host "Validation passed for: $($file.FullName)" -ForegroundColor Green
        } catch {
            Write-Host "Validation failed for: $($file.FullName). Error: $_" -ForegroundColor Red
            $hasFailures = $true
        }
    }
    return $hasFailures
}