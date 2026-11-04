#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Validates that production tool command IDs are unique.

.DESCRIPTION
    Reads CommandMetadata attributes using the C# parser bundled with PowerShell.
    Scans toolset src directories, excluding tests and build outputs, without
    building the servers. Rejects duplicate IDs, invalid ID declarations, and
    scans that find no command metadata.

.PARAMETER ToolsDirectory
    Directory containing the toolsets. Defaults to the repository's tools directory.

.OUTPUTS
    An object containing ToolCount and ViolationCount. Exits with code 0 on success
    or 1 when validation fails.
#>
[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$ToolsDirectory = (Join-Path $PSScriptRoot '..\..\tools')
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName Microsoft.CodeAnalysis.CSharp

$commandFiles = @(Get-ChildItem -LiteralPath $ToolsDirectory -Directory |
    ForEach-Object { Join-Path $_.FullName 'src' } |
    Where-Object { Test-Path -LiteralPath $_ -PathType Container } |
    ForEach-Object { Get-ChildItem -LiteralPath $_ -Recurse -File -Filter '*Command.cs' } |
    Where-Object {
        [System.IO.Path]::GetRelativePath($ToolsDirectory, $_.FullName) -notmatch '[\\/](bin|obj)[\\/]'
    } |
    Sort-Object FullName)

$toolCount = 0
$metadataCount = 0
$dictionary = @{}
$invalidDeclarations = [System.Collections.Generic.List[string]]::new()

foreach ($file in $commandFiles) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
        [System.IO.File]::ReadAllText($file.FullName))

    foreach ($node in $tree.GetRoot().DescendantNodes()) {
        if ($node -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax]) {
            continue
        }

        $attributeName = $node.Name.GetLastToken().ValueText
        if ($attributeName -cne 'CommandMetadata' -and $attributeName -cne 'CommandMetadataAttribute') {
            continue
        }

        $metadataCount++
        $lineNumber = $node.GetLocation().GetLineSpan().StartLinePosition.Line + 1
        $location = "$($file.FullName):$lineNumber"
        $idArguments = @()
        if ($null -ne $node.ArgumentList) {
            $idArguments = @($node.ArgumentList.Arguments | Where-Object {
                $null -ne $_.NameEquals -and $_.NameEquals.Name.Identifier.ValueText -ceq 'Id'
            })
        }

        if ($node.ContainsDiagnostics -or $idArguments.Count -ne 1 -or
            $idArguments[0].Expression -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax] -or
            $idArguments[0].Expression.Token.Value -isnot [string] -or
            [string]::IsNullOrWhiteSpace($idArguments[0].Expression.Token.Value)) {
            $invalidDeclarations.Add("$location - CommandMetadata.Id must be a non-empty string literal.")
            continue
        }

        $toolId = $idArguments[0].Expression.Token.Value
        $toolCount++
        if ($dictionary.ContainsKey($toolId)) {
            $dictionary[$toolId] += ,$location
        } else {
            $dictionary[$toolId] = @($location)
        }
    }
}

if ($metadataCount -eq 0) {
    $invalidDeclarations.Add("No command metadata IDs were found in production command files under '$ToolsDirectory'.")
}

$overallViolations = @()
foreach ($key in ($dictionary.Keys | Sort-Object)) {
    if ($dictionary[$key].Count -gt 1) {
        $overallViolations += $key
    }
}

$violationCount = $overallViolations.Count + $invalidDeclarations.Count

# Final summary
Write-Host "=================================================="
Write-Host "SUMMARY"
Write-Host "=================================================="
Write-Host "Tool IDs checked: $toolCount"
Write-Host "Total violations: $violationCount"
Write-Host ""

if ($violationCount -gt 0) {
    Write-Host "VIOLATIONS FOUND:" -ForegroundColor Red
    Write-Host ""

    foreach ($declaration in $invalidDeclarations) {
        Write-Host $declaration
        Write-Host ""
    }

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
    ToolCount = $toolCount
    ViolationCount = $violationCount
}

$result

if ($violationCount -eq 0) {
    Write-Host "Passed validation!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "Validation failed - see violations above" -ForegroundColor Red
    exit 1
}