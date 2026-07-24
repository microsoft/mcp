#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates that all C# class files have a namespace declaration.

.DESCRIPTION
    Scans all .cs files under core/, servers/, and tools/ directories (excluding
    obj/, bin/, and auto-generated files) and ensures each file contains a
    namespace declaration. Files without a namespace cause compilation issues
    and pollute the global namespace.

.PARAMETER Path
    Root directory to scan. Defaults to the repository root.

.OUTPUTS
    Returns an object with ViolationCount property. Sets exit code 1 if violations found.
#>

param(
    [string]$Path
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"

if (-not $Path) {
    $Path = $RepoRoot
}

$rootPath = (Resolve-Path $Path).Path.TrimEnd('\', '/')

$searchDirs = @(
    Join-Path $rootPath "core"
    Join-Path $rootPath "servers"
    Join-Path $rootPath "tools"
)

$violations = @()

foreach ($dir in $searchDirs) {
    if (-not (Test-Path $dir)) {
        continue
    }

    $csFiles = Get-ChildItem -Path $dir -Filter "*.cs" -Recurse | Where-Object {
        $_.FullName -notmatch '[/\\](obj|bin)[/\\]' -and
        $_.FullName -notmatch '\.g\.cs$' -and
        $_.FullName -notmatch '\.generated\.cs$' -and
        $_.FullName -notmatch 'GlobalUsings\.cs$' -and
        $_.FullName -notmatch 'AssemblyInfo\.cs$' -and
        $_.FullName -notmatch 'AssemblyAttributes\.cs$' -and
        $_.Name -ne 'Program.cs' -and
        $_.Name -ne 'Usings.cs' -and
        $_.FullName -notmatch '[/\\]templates[/\\]'
    }

    foreach ($file in $csFiles) {
        $content = Get-Content $file.FullName -Raw

        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        if ($content -notmatch '(?m)^\s*namespace\s+[A-Za-z_][A-Za-z0-9_.]*\s*;') {
            $relativePath = [System.IO.Path]::GetRelativePath($rootPath, $file.FullName)
            $violations += $relativePath
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "❌ Found $($violations.Count) C# file(s) missing a namespace declaration:"
    Write-Host ""
    foreach ($v in $violations) {
        Write-Host "  - $v"
    }
    Write-Host ""
    Write-Host "All C# class files must declare a namespace. Use file-scoped namespaces:"
    Write-Host "  namespace Azure.Mcp.Tools.MyToolset.Commands;"
    Write-Host ""

    $result = [PSCustomObject]@{ ViolationCount = $violations.Count }
    Write-Output $result
    exit 1
} else {
    Write-Output ([PSCustomObject]@{ ViolationCount = 0 })
}
