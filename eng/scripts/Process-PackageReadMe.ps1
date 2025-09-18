#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string] $InputReadMePath,
    [Parameter(Mandatory=$true)]
    [string] $OutputDirectory,
    [Parameter(Mandatory=$true)]
    [ValidateSet('nuget','npm','vsix')]
    [string] $PackageType
)

$readMeText = Get-Content $InputReadMePath | Select-Object -Skip 26 # Skip initial comment lines
$processedReadMe = @()
$appendLine = $true

$PackageMap = @{
    Nuget = @{
        ToolTitle = ' .NET Tool'
    }
    Npm = @{
        ToolTitle = ' Node.js Tool'
    }
    Vsix = @{
        ToolTitle = ' Extension for Visual Studio Code'
    }
}

foreach ($line in $readMeText) {
    if ([string]::IsNullOrWhiteSpace($line) -and $appendLine) {
        $processedReadMe += ''
        continue
    }

    if ($line -match "<!--\s*REMOVESECTIONSTART-([^>]+?)\s*-->") {
        $pkgTypeInfo = $matches[1]
        $pkgTypes = $pkgTypeInfo -split ';'
        foreach ($pt in $pkgTypes) {
            if ($pt -eq $PackageType) {
                $appendLine = $false
                break
            }
        }
        continue
    }

    if ($line -match "<!--\s*REMOVESECTIONEND\s*-->") {
        $appendLine = $true
        continue
    }

    if ($appendLine -eq $false) {
        continue
    }

    if ($line -match "<!--\s*INSERTCHUNK-([^{}]+?)\s*\{\{([\s\S]*?)\}\}\s*-->") {
        $pkgTypeInfo = $matches[1]
        $content = $matches[2]
        $pkgTypes = $pkgTypeInfo -split ';'
        foreach ($pt in $pkgTypes) {
            if ($pt -eq $PackageType) {
                $contentToDisplay = if ($PackageMap[$PackageType].ContainsKey($content)) { $PackageMap[$PackageType][$content] } else { $content }
                $line = $line -replace [regex]::Escape($matches[0]), $contentToDisplay
                break
            }
        }
        $line = $line -replace [regex]::Escape($matches[0]), ''
    }

    $tempLine = ''
    $processRemoveChuck = $false
    if ($line -match "<!--\s*REMOVECHUNKSTART-([^>]+?)\s*-->") {
        $pkgTypeInfo = $matches[1]
        $pkgTypes = $pkgTypeInfo -split ';'
        foreach ($pt in $pkgTypes) {
            if ($pt -eq $PackageType) {
                $processRemoveChuck = $true
                $idx = $line.IndexOf($matches[0])
                $tempLine = $line.Substring(0, $idx)
                break
            }
        }
        $line = $line -replace [regex]::Escape($matches[0]), ''
    }

    if ($line -match "<!--\s*REMOVECHUNKEND\s*-->") {
        if ($processRemoveChuck) {
            $idx = $line.IndexOf($matches[0])
            $tempLine += $line.Substring($idx + $matches[0].Length)
            $line = $tempLine
        } else {
            $line = $line -replace [regex]::Escape($matches[0]), ''
        }
    }

    if ($line.Length -gt 0 -and $appendLine) {
        $processedReadMe += $line
    }
}

Set-Content -Path "$OutputDirectory/README$PackageType.md" -Value $processedReadMe -Force