#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates that all tool ids are unique.

.DESCRIPTION
    This script validates that tool id is unique across all tools.
#>

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$ToolsDirectory = Join-Path $RepoRoot "tools"
$ServersDirectory = Join-Path $RepoRoot "servers"

$CommandFiles = Get-ChildItem -Path $ToolsDirectory -Recurse -Filter *Command.cs

$dictionary = @{}

foreach ($file in $CommandFiles) {
    $content = Get-Content $file.FullName

    foreach ($line in $content) {
        if ($line -match 'public override string Id => "(.*)";') {
            $toolId = $matches[1]

            if ($dictionary.ContainsKey($toolId)) {
                $dictionary[$toolId] += ,$file.FullName
            } else {
                $dictionary[$toolId] = @($file.FullName)
            }
        }
    }
}

$hasViolations = $false;
$overallViolations = @();

foreach ($key in $dictionary.Keys) {
    if ($dictionary[$key].Count -gt 1) {
        $hasViolations = $true
        $overallViolations += $key
    }
}

# Final summary
Write-Host "=================================================="
Write-Host "SUMMARY"
Write-Host "=================================================="
Write-Host "Total violations: $($overallViolations.Count)"
Write-Host ""

if ($overallViolations.Count -gt 0) {
    Write-Host "VIOLATIONS FOUND:" -ForegroundColor Red
    Write-Host ""

    foreach ($violation in $overallViolations) {
        Write-Host "Tool ID: $violation"

        foreach ($file in $dictionary[$violation]) {
            Write-Host "  - $file"
        }
        Write-Host ""
    }

}

# Prepare return object
$result = [PSCustomObject]@{
    ViolationCount = $overallViolations.Count
}

$result

if (!$hasViolations) {
    Write-Host "Passed validation!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "Validation failed - see violations above" -ForegroundColor Red
    exit 1
}