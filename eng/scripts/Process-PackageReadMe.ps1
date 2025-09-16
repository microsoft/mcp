#!/bin/env pwsh
#Requires -Version 7

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
        $processedReadMe += ''
        continue
    }

    $lineToAppend = ''
    $lineInProcess = $line

    while($lineInProcess.Length -gt 0) {
        # remove-section: start marks the start of section removal for the package type
        # e.g. <!-- remove-section: start nuget;vsix -->
        if ($lineInProcess -match "(?i)<!--\s*remove-section:\s*start\s+([^>]+?)\s*-->") {
            $action = $actionStack.Peek()
            $pkgTypeInfo = $matches[1]
            $pkgTypes = $pkgTypeInfo -split ';'
            if ($pkgTypes -contains $PackageType) {
                $action = [ActionType]::Skip
            }
            $actionStack.Push($action)
            $matchIdx = $lineInProcess.IndexOf($matches[0])
            $lineToAppend += $lineInProcess.Substring(0, $matchIdx)
            $lineInProcess = $lineInProcess.Substring($matchIdx + $matches[0].Length)
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
        $lineToAppend += $lineInProcess
        break
    }

    # insert-chunk: start marks chunk insertion for the package type
    # e.g. <!-- insert-chunk: nuget;vsix;npm {{ToolTitle}} -->
    # ToolTitle will be inserted from the InsertPayload hashtable
    $insertChunkPattern = "(?i)<!--\s*insert-chunk:\s+([^{}]+?)\s*\{\{([\s\S]*?)\}\}\s*-->"
    $matches = [regex]::Matches($lineToAppend, $insertChunkPattern)

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

    if ($actionStack.Peek() -eq [ActionType]::Append) {
        $processedReadMe += $lineToAppend
    }
}
Set-Content -Path "$OutputDirectory/README$PackageType.md" -Value $processedReadMe -Force
