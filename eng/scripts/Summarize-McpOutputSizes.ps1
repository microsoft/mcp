#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Summarizes an MCP output size measurement report.

.DESCRIPTION
    Reads the JSON report produced by the McpOutputSizeTests measurement test
    (see Measure-McpOutputSizes.ps1) and prints a console summary comparing the
    consolidated and namespace server modes: tool counts, initialize greeting size,
    tools/list discovery size, and learn-mode response sizes.

    Also extracts readable text and inner-command detail for the largest learn
    responses (top 10 and any response over -LearnResponseThresholdUtf8Bytes),
    and can optionally write JSON and Markdown summary files.

.PARAMETER InputPath
    Path to the mcp-output-size.json report produced by the measurement test.

.PARAMETER OutputPath
    Optional path to write a JSON summary of the report.

.PARAMETER MarkdownPath
    Optional path to write a Markdown summary of the report.

.PARAMETER LearnResponseThresholdUtf8Bytes
    Include every learn response over this UTF-8 byte threshold in the summary,
    in addition to the top 10 largest. Defaults to 45000.

.EXAMPLE
    ./eng/scripts/Summarize-McpOutputSizes.ps1 -InputPath TestResults/mcp-output-size.json

    Prints a console summary of the measurement report.

.EXAMPLE
    ./eng/scripts/Summarize-McpOutputSizes.ps1 -InputPath TestResults/mcp-output-size.json `
        -OutputPath TestResults/mcp-output-size-summary.json `
        -MarkdownPath TestResults/mcp-output-size-summary.md

    Prints the console summary and also writes JSON and Markdown summary files.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $InputPath,

    [string] $OutputPath,

    [string] $MarkdownPath,

    [int] $LearnResponseThresholdUtf8Bytes = 45000
)

$ErrorActionPreference = 'Stop'

if (!(Test-Path -LiteralPath $InputPath -PathType Leaf)) {
    throw "Measurement report not found: $InputPath"
}

$report = Get-Content -LiteralPath $InputPath -Raw | ConvertFrom-Json
$modeResults = @{}
foreach ($mode in $report.modes) {
    $modeResults[$mode.mode] = $mode
}

foreach ($modeName in @('consolidated', 'namespace')) {
    if (!$modeResults.ContainsKey($modeName)) {
        throw "The report does not contain the '$modeName' mode."
    }
}

function Get-PercentDifference([double] $value, [double] $baseline) {
    if ($baseline -eq 0) {
        return $null
    }

    return [math]::Round((($value - $baseline) / $baseline) * 100, 2)
}

function Get-ModeSummary($mode) {
    $learnCount = @($mode.learnResponses).Count
    $discoveryCount = @($mode.discoveryResponses).Count
    $averageLearnBytes = if ($learnCount -eq 0) { 0 } else {
        [math]::Round($mode.learnTotalUtf8Bytes / $learnCount, 2)
    }

    return [ordered]@{
        mode = $mode.mode
        toolCount = $mode.toolCount
        greetingUtf8Bytes = $mode.initialGreetingResponse.utf8Bytes
        discoveryMessageCount = $discoveryCount
        discoveryUtf8Bytes = $mode.discoveryTotalUtf8Bytes
        learnMessageCount = $learnCount
        learnUtf8Bytes = $mode.learnTotalUtf8Bytes
        averageLearnUtf8Bytes = $averageLearnBytes
        totalUtf8Bytes = $mode.totalUtf8Bytes
    }
}

$consolidated = Get-ModeSummary $modeResults['consolidated']
$namespace = Get-ModeSummary $modeResults['namespace']

$comparisonMetrics = @(
    'toolCount',
    'greetingUtf8Bytes',
    'discoveryMessageCount',
    'discoveryUtf8Bytes',
    'learnMessageCount',
    'learnUtf8Bytes',
    'averageLearnUtf8Bytes',
    'totalUtf8Bytes'
)

$comparison = [ordered]@{}
foreach ($metric in $comparisonMetrics) {
    $consolidatedValue = [double]$consolidated[$metric]
    $namespaceValue = [double]$namespace[$metric]
    $comparison[$metric] = [ordered]@{
        consolidated = $consolidatedValue
        namespace = $namespaceValue
        difference = $consolidatedValue - $namespaceValue
        percentDifferenceFromNamespace = Get-PercentDifference $consolidatedValue $namespaceValue
    }
}

function Save-LearnResponseText($entries) {
    # Extract result.content[].text (the human-readable "learn" description) from each
    # saved learn-response JSON file and write it out as a plain-text sibling file so it
    # can be read directly without unescaping JSON.
    foreach ($entry in $entries) {
        if (!$entry.learnResponseFile -or !(Test-Path -LiteralPath $entry.learnResponseFile -PathType Leaf)) {
            continue
        }

        $learnJson = Get-Content -LiteralPath $entry.learnResponseFile -Raw | ConvertFrom-Json
        $textParts = @(
            $learnJson.result.content |
                Where-Object { $_.type -eq 'text' -and $null -ne $_.text } |
                ForEach-Object { $_.text }
        )

        if ($textParts.Count -eq 0) {
            continue
        }

        $textPath = [IO.Path]::ChangeExtension($entry.learnResponseFile, '.txt')
        Set-Content -LiteralPath $textPath -Value ($textParts -join "`r`n`r`n") -Encoding utf8NoBOM
        $entry | Add-Member -NotePropertyName learnResponseTextFile -NotePropertyValue $textPath -Force
    }
}

function Save-InnerCommands($entries) {
    # The learn text is a short preamble followed by a JSON array describing the tool's
    # inner commands. Split that array out into one file per inner command, in a
    # per-tool subdirectory alongside the tool's own .json/.txt files.
    foreach ($entry in $entries) {
        $entry | Add-Member -NotePropertyName innerCommandCount -NotePropertyValue 0 -Force
        $entry | Add-Member -NotePropertyName innerCommandDirectory -NotePropertyValue $null -Force

        if (!$entry.learnResponseTextFile -or !(Test-Path -LiteralPath $entry.learnResponseTextFile -PathType Leaf)) {
            continue
        }

        $textLines = (Get-Content -LiteralPath $entry.learnResponseTextFile -Raw) -split "`r?`n"
        $jsonLine = $textLines | Where-Object { $_.TrimStart().StartsWith('[') } | Select-Object -First 1
        if (!$jsonLine) {
            continue
        }

        try {
            $commands = @($jsonLine | ConvertFrom-Json)
        } catch {
            Write-Warning "Could not parse inner commands for '$($entry.tool)': $_"
            continue
        }

        if ($commands.Count -eq 0) {
            continue
        }

        $toolDirectory = [IO.Path]::Combine(
            [IO.Path]::GetDirectoryName($entry.learnResponseTextFile),
            [IO.Path]::GetFileNameWithoutExtension($entry.learnResponseTextFile) + '-commands')
        New-Item -ItemType Directory -Path $toolDirectory -Force | Out-Null

        foreach ($command in $commands) {
            $commandName = if ($command.command) { $command.command } else { 'unnamed' }
            foreach ($invalid in [IO.Path]::GetInvalidFileNameChars()) {
                $commandName = $commandName.Replace($invalid, '-')
            }

            $commandPath = [IO.Path]::Combine($toolDirectory, "$commandName.json")
            Set-Content -LiteralPath $commandPath -Value ($command | ConvertTo-Json -Depth 30) -Encoding utf8NoBOM
        }

        $entry.innerCommandCount = $commands.Count
        $entry.innerCommandDirectory = $toolDirectory
    }
}

# Extract text/inner-command artifacts for every tool's learn response (the test dumps a
# .json for every tool). Ranking/filtering into "top" and "large" subsets happens below,
# after every tool's artifacts already exist on disk.
$allEntriesByMode = [ordered]@{
    consolidated = @($modeResults['consolidated'].learnResponses)
    namespace = @($modeResults['namespace'].learnResponses)
}
foreach ($modeName in @('consolidated', 'namespace')) {
    foreach ($entry in $allEntriesByMode[$modeName]) {
        $entry | Add-Member -NotePropertyName learnResponseTextFile -NotePropertyValue $null -Force
    }
    Save-LearnResponseText $allEntriesByMode[$modeName]
    Save-InnerCommands $allEntriesByMode[$modeName]
}

function Get-TopLearnResponses($entries) {
    # Select the top 10 by UTF-8 byte size; filtering/ranking lives in this script, not
    # the test, which now dumps every tool's learn response unconditionally.
    return @($entries) |
        Sort-Object -Property utf8Bytes -Descending |
        Select-Object -First 10 |
        ForEach-Object {
            [ordered]@{
                tool = $_.tool
                utf8Bytes = $_.utf8Bytes
                characterCount = $_.characterCount
                learnResponseFile = $_.learnResponseFile
                learnResponseTextFile = $_.learnResponseTextFile
                innerCommandCount = $_.innerCommandCount
                innerCommandDirectory = $_.innerCommandDirectory
            }
        }
}

function Get-LargeLearnResponses($entries) {
    return @($entries) |
        Where-Object { $_.utf8Bytes -gt $LearnResponseThresholdUtf8Bytes } |
        Sort-Object -Property utf8Bytes -Descending |
        ForEach-Object {
            [ordered]@{
                tool = $_.tool
                utf8Bytes = $_.utf8Bytes
                characterCount = $_.characterCount
                learnResponseFile = $_.learnResponseFile
                learnResponseTextFile = $_.learnResponseTextFile
                innerCommandCount = $_.innerCommandCount
                innerCommandDirectory = $_.innerCommandDirectory
            }
        }
}

$topLearnByMode = [ordered]@{
    consolidated = Get-TopLearnResponses $allEntriesByMode['consolidated']
    namespace = Get-TopLearnResponses $allEntriesByMode['namespace']
}

$largeLearnByMode = [ordered]@{
    consolidated = Get-LargeLearnResponses $allEntriesByMode['consolidated']
    namespace = Get-LargeLearnResponses $allEntriesByMode['namespace']
}

$summary = [ordered]@{
    sourceReport = (Resolve-Path -LiteralPath $InputPath).Path
    generatedAtUtc = [DateTimeOffset]::UtcNow
    transport = $report.transport
    learnResponseThresholdUtf8Bytes = $LearnResponseThresholdUtf8Bytes
    modes = @($consolidated, $namespace)
    comparison = $comparison
    topLearnResponses = $topLearnByMode
    largeLearnResponses = $largeLearnByMode
}

Write-Host "MCP output size summary ($($report.transport))"
Write-Host ""
$consoleRows = @($consolidated, $namespace) | ForEach-Object {
    [pscustomobject]@{
        mode = $_['mode']
        toolCount = $_['toolCount']
        greetingUtf8Bytes = $_['greetingUtf8Bytes']
        discoveryUtf8Bytes = $_['discoveryUtf8Bytes']
        learnUtf8Bytes = $_['learnUtf8Bytes']
        totalUtf8Bytes = $_['totalUtf8Bytes']
    }
}
$consoleRows |
    Format-Table mode, toolCount, greetingUtf8Bytes, discoveryUtf8Bytes, learnUtf8Bytes, totalUtf8Bytes |
    Out-Host

Write-Host "Consolidated relative to namespace:"
foreach ($metric in $comparisonMetrics) {
    $result = $comparison[$metric]
    $percent = if ($null -eq $result.percentDifferenceFromNamespace) {
        'n/a'
    } else {
        "$($result.percentDifferenceFromNamespace)%"
    }
    Write-Host ("  {0}: difference {1}, {2}" -f $metric, $result.difference, $percent)
}

foreach ($modeName in @('consolidated', 'namespace')) {
    Write-Host ""
    Write-Host "Top 10 largest learn responses ($modeName):"
    $topLearnByMode[$modeName] | ForEach-Object {
        [pscustomobject]@{
            tool = $_.tool
            utf8Bytes = $_.utf8Bytes
            innerCommands = $_.innerCommandCount
        }
    } | Format-Table tool, utf8Bytes, innerCommands | Out-Host

    Write-Host "Learn responses over $LearnResponseThresholdUtf8Bytes UTF-8 bytes ($modeName):"
    $largeLearnByMode[$modeName] | ForEach-Object {
        [pscustomobject]@{
            tool = $_.tool
            utf8Bytes = $_.utf8Bytes
        }
    } | Format-Table tool, utf8Bytes | Out-Host
}

$summaryJson = $summary | ConvertTo-Json -Depth 10
if (![string]::IsNullOrWhiteSpace($OutputPath)) {
    $resolvedOutputPath = [IO.Path]::GetFullPath($OutputPath)
    $outputDirectory = [IO.Path]::GetDirectoryName($resolvedOutputPath)
    if (![string]::IsNullOrWhiteSpace($outputDirectory)) {
        New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
    }
    Set-Content -LiteralPath $resolvedOutputPath -Value $summaryJson -Encoding utf8
    Write-Host ""
    Write-Host "Summary JSON saved to $resolvedOutputPath"
} else {
    Write-Output $summaryJson
}

if ([string]::IsNullOrWhiteSpace($MarkdownPath)) {
    $MarkdownPath = [IO.Path]::ChangeExtension(
        [IO.Path]::GetFullPath($InputPath),
        '.md')
}

$markdownLines = [System.Collections.Generic.List[string]]::new()
$markdownLines.Add('# MCP Output Size Summary')
$markdownLines.Add('')
$markdownLines.Add('- **Source report:** `' + $summary.sourceReport + '`')
$markdownLines.Add("- **Transport:** $($summary.transport)")
$markdownLines.Add("- **Generated:** $($summary.generatedAtUtc)")
$markdownLines.Add("- **Large learn response threshold:** $($summary.learnResponseThresholdUtf8Bytes) UTF-8 bytes")
$markdownLines.Add('')
$markdownLines.Add('## Mode Summary')
$markdownLines.Add('')
$markdownLines.Add('| Mode | Tools | Greeting (bytes) | Discovery (bytes) | Learn (bytes) | Total (bytes) |')
$markdownLines.Add('| --- | ---: | ---: | ---: | ---: | ---: |')
foreach ($mode in $summary.modes) {
    $markdownLines.Add(
        "| $($mode.mode) | $($mode.toolCount) | $($mode.greetingUtf8Bytes) | " +
        "$($mode.discoveryUtf8Bytes) | $($mode.learnUtf8Bytes) | $($mode.totalUtf8Bytes) |")
}

$markdownLines.Add('')
$markdownLines.Add('## Consolidated vs. Namespace')
$markdownLines.Add('')
$markdownLines.Add('| Metric | Consolidated | Namespace | Difference | Difference vs. namespace |')
$markdownLines.Add('| --- | ---: | ---: | ---: | ---: |')
foreach ($metric in $comparisonMetrics) {
    $result = $comparison[$metric]
    $percent = if ($null -eq $result.percentDifferenceFromNamespace) {
        'n/a'
    } else {
        "$($result.percentDifferenceFromNamespace)%"
    }
    $markdownLines.Add(
        "| $metric | $($result.consolidated) | $($result.namespace) | " +
        "$($result.difference) | $percent |")
}

$resolvedMarkdownPath = [IO.Path]::GetFullPath($MarkdownPath)
$markdownDirectory = [IO.Path]::GetDirectoryName($resolvedMarkdownPath)
if (![string]::IsNullOrWhiteSpace($markdownDirectory)) {
    New-Item -ItemType Directory -Path $markdownDirectory -Force | Out-Null
}

foreach ($modeName in @('consolidated', 'namespace')) {
    $markdownLines.Add('')
    $markdownLines.Add("## Top 10 Largest Learn Responses ($modeName)")
    $markdownLines.Add('')
    $markdownLines.Add('| Rank | Tool | Bytes | Inner Commands | Saved Response File | Description Text File |')
    $markdownLines.Add('| ---: | --- | ---: | ---: | --- | --- |')
    $rank = 1
    foreach ($entry in $topLearnByMode[$modeName]) {
        $fileLink = if ($entry.learnResponseFile) { "``$($entry.learnResponseFile)``" } else { 'n/a' }
        $textLink = if ($entry.learnResponseTextFile) { "``$($entry.learnResponseTextFile)``" } else { 'n/a' }
        $markdownLines.Add("| $rank | $($entry.tool) | $($entry.utf8Bytes) | $($entry.innerCommandCount) | $fileLink | $textLink |")
        $rank++
    }

    $markdownLines.Add('')
    $markdownLines.Add("## Learn Responses Over $LearnResponseThresholdUtf8Bytes UTF-8 Bytes ($modeName)")
    $markdownLines.Add('')
    $markdownLines.Add('| Tool | Bytes | Character Count | Saved Response File | Description Text File |')
    $markdownLines.Add('| --- | ---: | ---: | --- | --- |')
    foreach ($entry in $largeLearnByMode[$modeName]) {
        $fileLink = if ($entry.learnResponseFile) { "``$($entry.learnResponseFile)``" } else { 'n/a' }
        $textLink = if ($entry.learnResponseTextFile) { "``$($entry.learnResponseTextFile)``" } else { 'n/a' }
        $markdownLines.Add("| $($entry.tool) | $($entry.utf8Bytes) | $($entry.characterCount) | $fileLink | $textLink |")
    }
}

Set-Content -LiteralPath $resolvedMarkdownPath -Value $markdownLines -Encoding utf8
Write-Host "Markdown summary saved to $resolvedMarkdownPath"
