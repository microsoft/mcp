#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates and optionally fixes CommandGroup description line lengths in Setup.cs files

.DESCRIPTION
    This script checks that all CommandGroup description parameters in Setup.cs files have lines <= 72 characters.
    It can identify violations and optionally attempt to fix them.

.PARAMETER MaxLineLength
    Maximum allowed line length for descriptions (default: 72)

.PARAMETER Fix
    Attempt to automatically fix violations by re-wrapping descriptions

.PARAMETER Path
    Specific path to check (default: tools/ and core/ directories)
#>

param(
    [int]$MaxLineLength = 72,
    [switch]$Fix,
    [string]$Path
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

Write-Host "Checking CommandGroup description line lengths in Setup.cs files (max: $MaxLineLength characters)`n"

function Wrap-Description {
    param(
        [string]$Text,
        [int]$MaxWidth,
        [string]$Indent = " "
    )

    $words = $Text -split '\s+' | Where-Object { $_ -ne '' }
    $lines = @()
    $currentLine = ""

    foreach ($word in $words) {
        # If the word is a URL that exceeds max width, put it on its own line
        if ($word -match '^https?://' -and $word.Length -gt $MaxWidth) {
            if ($currentLine -ne "") {
                $lines += $currentLine
            }
            $lines += $word
            $currentLine = ""
            continue
        }

        $testLine = if ($currentLine -eq "") { $word } else { "$currentLine $word" }

        if ($testLine.Length -le $MaxWidth) {
            $currentLine = $testLine
        } else {
            if ($currentLine -ne "") {
                $lines += $currentLine
            }
            $currentLine = $word
        }
    }

    if ($currentLine -ne "") {
        $lines += $currentLine
    }

    return ($lines -join "`n${Indent}")
}

$searchPaths = if ($Path) { @($Path) } else { @("$RepoRoot/tools", "$RepoRoot/core") }
$setupFiles = @()

foreach ($searchPath in $searchPaths) {
    $setupFiles += Get-ChildItem -Path $searchPath -Recurse -Filter "*Setup.cs" -File
}

Write-Host "Found $($setupFiles.Count) setup files`n"

$violations = @()
$fixedFiles = @()

foreach ($file in $setupFiles) {
    $content = Get-Content $file.FullName -Raw
    $fileModified = $false
    
    $rawStringPattern = '(?s)new\s+CommandGroup\([^,]+,\s*"""(.*?)"""'
    
    $regularStringPattern = 'new\s+CommandGroup\([^,]+,\s*@?"((?:[^"\\]|""|\\.)*)"'
    
    $rawMatches = [regex]::Matches($content, $rawStringPattern)
    $regularMatches = [regex]::Matches($content, $regularStringPattern)
    
    $allMatches = @()
    foreach ($match in $rawMatches) { 
        $allMatches += @{ Match = $match; IsRawString = $true } 
    }
    foreach ($match in $regularMatches) {
        # Skip if this match overlaps with a raw string match asraw strings also contain ""
        $skip = $false
        foreach ($rawMatch in $rawMatches) {
            if ($match.Index -ge $rawMatch.Index -and $match.Index -lt ($rawMatch.Index + $rawMatch.Length)) {
                $skip = $true
                break
            }
        }
        if (-not $skip) {
            $allMatches += @{ Match = $match; IsRawString = $false } 
        }
    }
    
    foreach ($entry in $allMatches) {
        $match = $entry.Match
        $isRawString = $entry.IsRawString
        $description = $match.Groups[1].Value
        $lines = $description -split "`r?`n"
        
        $hasViolation = $false
        $violatingLines = @()
        
        # Trim leading whitespace for line length check, skip empty lines and skip urls that cannot be wrapped
        foreach ($line in $lines) {
            $trimmedLine = $line.TrimStart()
            if ($trimmedLine.Length -eq 0) { continue }
            if ($trimmedLine -match '^https?://\S+$') { continue }
            if ($trimmedLine.Length -gt $MaxLineLength) {
                $hasViolation = $true
                $violatingLines += [PSCustomObject]@{
                    Length = $trimmedLine.Length
                    Text = if ($trimmedLine.Length -gt 60) { $trimmedLine.Substring(0, 57) + "..." } else { $trimmedLine }
                }
            }
        }
        
        if ($hasViolation) {
            $relativePath = $file.FullName.Replace($RepoRoot, "").TrimStart("/", "\")
            $violations += [PSCustomObject]@{
                File = $relativePath
                Lines = $violatingLines
                IsRawString = $isRawString
                FullPath = $file.FullName
                MatchValue = $match.Value
                OriginalDescription = $description
            }
            
            if ($Fix) {
                # Detect indentation by finding whitespace before 'new CommandGroup]
                $linesBefore = $content.Substring(0, $match.Index) -split "`n"
                $baseIndent = ($linesBefore[-1] -replace '\S.*', '')
                $contentIndent = $baseIndent + "    "
                
                # Extract the part before the description string
                $matchText = $match.Value
                $prefix = $matchText.Substring(0, $matchText.IndexOf(',') + 1)
                
                $wrappedDescription = Wrap-Description -Text $description -MaxWidth $MaxLineLength -Indent $contentIndent
                
                $replacement = "${prefix}`n${contentIndent}`"`"`"`n${contentIndent}${wrappedDescription}`n${contentIndent}`"`"`""
                
                $content = $content.Replace($matchText, $replacement)
                $fileModified = $true
            }
        }
    }
    
    if ($fileModified) {
        $relativePath = $file.FullName.Replace($RepoRoot, "").TrimStart("/", "\")
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $fixedFiles += $relativePath
    }
}

Write-Host "RESULTS"
if ($violations.Count -eq 0) {
    Write-Host "All CommandGroup descriptions are within $MaxLineLength character line limit!" -ForegroundColor Green
    exit 0
}

Write-Host "Found $($violations.Count) file(s) with violations:`n" -ForegroundColor Yellow

foreach ($violation in $violations) {
    Write-Host "  $($violation.File)" -ForegroundColor Red
    foreach ($line in $violation.Lines) {
        Write-Host "    - $($line.Length) chars: $($line.Text)" -ForegroundColor Gray
    }
    Write-Host ""
}

if ($Fix) {
    Write-Host "`nFixed $($fixedFiles.Count) file(s):" -ForegroundColor Green
    foreach ($fixed in $fixedFiles) {
        Write-Host "  $fixed"
    }
    Write-Host ""
    Write-Host "`nReview the changes to check indentation." -ForegroundColor Yellow
}
else {
    Write-Host "Run with -Fix to attempt automatic wrapping" -ForegroundColor Cyan
}

exit 1
