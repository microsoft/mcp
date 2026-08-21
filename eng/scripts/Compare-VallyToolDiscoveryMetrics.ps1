#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Compares Vally evaluation metrics for a single tool namespace across default,
    two-step, and three-step Azure MCP Server tool discovery.

.DESCRIPTION
    Runs `Invoke-VallyEvalTests.ps1` for the given namespace in Default, TwoStep,
    and ThreeStep modes --
    restricted to the transient eval.yaml file(s) generated under
    `<repo-root>/.work/vally/evals/<Namespace>` (the "official" eval suite produced
    by VallyEvaluator). Checked-in eval.yaml files under `tools/*/tests` are
    excluded.

    For each stimulus present in the runs' `results.jsonl` output, this script
    extracts total tokens, turn count, tool-call count, wall-clock time, and AI
    credits consumed, then prints a table contrasting all three modes and their
    deltas from Default.

    These are end-to-end Vally/Copilot metrics. They include model behavior such as
    extra turns, context replay, retries, and command selection. They are distinct
    from the raw MCP payload measurements produced by Measure-McpOutputSizes.ps1,
    which does not invoke Copilot.

    The generated JSON also includes per-turn context/input tokens, cache-read
    and cache-write tokens, output tokens, AI Credits, and MCP response byte
    counts. These values come from the OTel chat spans when available.

.PARAMETER Namespace
    The tool namespace to evaluate, matching the folder name under
    `.work/vally/evals` (e.g. `appconfig`, `storage`). Case-insensitive.

.PARAMETER NumberOfRuns
    The number of times to run each eval spec in each mode. Defaults to 1.

.PARAMETER OutputPath
    Base directory used to hold the default, two-step, and three-step Vally output folders
    for this comparison. Defaults to `<repo-root>/.work/vally/compare-results/<Namespace>`.

.PARAMETER SkipAgentsInstructions
    Passed through to `Invoke-VallyEvalTests.ps1` for both runs. See that script's
    help for details.

.PARAMETER IsDebug
    Passed through to `Invoke-VallyEvalTests.ps1` for both runs to add `--verbose`
    output.

.PARAMETER SkipEval
    Skip re-running `Invoke-VallyEvalTests.ps1` for all three modes and instead
    reprocess the existing results already present under `-OutputPath` (e.g.
    `.work/vally/compare-results/<Namespace>`). Use this to iterate on output
    formatting (CSV/JSON/summary/turn reports) without re-invoking Vally. Fails
    if `-OutputPath` does not already contain results from a prior run.

.PARAMETER Warmup
    Passed through to `Invoke-VallyEvalTests.ps1` for every mode. Runs a
    throwaway warmup pass (discarded afterward) before each mode's measured
    run, priming the model provider's prompt cache for that mode's system
    prompt/tool schema prefix. Without this, AI-credit comparisons across
    modes can be skewed by cache-state differences left over from whichever
    mode happened to run first, rather than reflecting genuine discovery-mode
    behavior differences. Has no effect when combined with -SkipEval, since no
    Vally run occurs in that case.

.EXAMPLE
    ./eng/scripts/Compare-VallyToolDiscoveryMetrics.ps1 -Namespace appconfig

    Runs the appconfig eval suite in all three discovery modes and prints a
    comparison table of tokens, turns, tool calls, wall time, and AI credits.

.EXAMPLE
    ./eng/scripts/Compare-VallyToolDiscoveryMetrics.ps1 -Namespace appconfig -SkipEval

    Reprocesses the existing appconfig comparison results (from a prior run) to
    regenerate the CSV/JSON/summary/turn reports without re-running Vally. Useful
    when iterating on report formatting.

.EXAMPLE
    ./eng/scripts/Compare-VallyToolDiscoveryMetrics.ps1 -Namespace appconfig -Warmup

    Runs the appconfig eval suite in all three discovery modes, warming up the
    model provider's prompt cache before each mode's measured run so AI-credit
    comparisons aren't skewed by cache-state noise between modes.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Namespace,
    [int]$NumberOfRuns = 1,
    [string]$OutputPath,
    [switch]$SkipAgentsInstructions,
    [switch]$IsDebug,
    [switch]$SkipEval,
    [switch]$Warmup
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$OutputPath) {
    $OutputPath = Join-Path $RepoRoot ".work/vally/compare-results/$Namespace"
}

if ($SkipEval) {
    if (!(Test-Path $OutputPath)) {
        Write-Error "-SkipEval was specified but $OutputPath does not exist. Run without -SkipEval first to generate results."
        exit 1
    }
} else {
    if (Test-Path $OutputPath) {
        Remove-Item -Recurse -Force $OutputPath
    }
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Restrict discovery to the transient eval.yaml generated for this namespace under
# .work/vally/evals -- explicitly excludes any checked-in tools/*/tests/eval.yaml.
$evalPathFilter = "*evals*$Namespace*eval.yaml"

function Invoke-DiscoveryMode {
    param(
        [string]$Mode,
        [string]$ResultsRoot,
        [bool]$SkipBuild
    )

    Write-Host "`n=== Running $Mode discovery for namespace '$Namespace' ===" -ForegroundColor Cyan

    $scriptArgs = @{
        ToolDiscoveryMode = $Mode
        EvalPathFilter    = $evalPathFilter
        OutputPath        = $ResultsRoot
        NumberOfRuns      = $NumberOfRuns
    }
    if ($SkipBuild) {
        $scriptArgs["SkipBuild"] = $true
    }
    if ($SkipAgentsInstructions) {
        $scriptArgs["SkipAgentsInstructions"] = $true
    }
    if ($IsDebug) {
        $scriptArgs["IsDebug"] = $true
    }
    if ($Warmup) {
        $scriptArgs["Warmup"] = $true
    }

    & "$PSScriptRoot/Invoke-VallyEvalTests.ps1" @scriptArgs
}

function Get-EventMetrics {
    param([string]$ResultsRoot)

    $eventMetrics = [ordered]@{}
    $toolCallToEval = @{}
    foreach ($eventsFile in (Get-ChildItem -Path $ResultsRoot -Filter "events.jsonl" -Recurse |
            Sort-Object LastWriteTime -Descending)) {
        $evalName = Split-Path (Split-Path (Split-Path (Split-Path $eventsFile.FullName -Parent) -Parent) -Parent) -Leaf
        if ($eventMetrics.Contains($evalName)) {
            continue
        }
        $evaluationIndex = $null
        if ($evalName -match 'evaluation-(\d+)$') {
            $evaluationIndex = [int]$Matches[1]
        }
        $turns = [ordered]@{}
        $session = $null

        foreach ($line in (Get-Content $eventsFile.FullName)) {
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }

            $event = $line | ConvertFrom-Json
            if ($event.type -eq "assistant.message") {
                foreach ($toolRequest in @($event.data.toolRequests)) {
                    if ($toolRequest.toolCallId) {
                        $toolCallToEval[$toolRequest.toolCallId] = $evalName
                    }
                }
            }
            if ($event.type -eq "assistant.message") {
                $turnId = [string]$event.data.turnId
                if (!$turns.Contains($turnId)) {
                    $turns[$turnId] = [ordered]@{
                        turn = $event.data.turnId
                        toolName = $null
                        outputTokens = 0
                        inputTokens = $null
                        cacheReadTokens = $null
                        cacheWriteTokens = $null
                        aiCredits = $null
                        mcpResponseUtf8Bytes = 0
                    }
                }

                $toolNames = @(
                    $event.data.toolRequests |
                    Where-Object { $_ -and $_.name } |
                    ForEach-Object { [string]$_.name }
                )
                if ($toolNames.Count -gt 0) {
                    $turns[$turnId].toolName = ($toolNames | Select-Object -Unique) -join '; '
                }

                $turns[$turnId].outputTokens += [int]($event.data.outputTokens ?? 0)
                if ($null -ne $event.data.inputTokens) {
                    $turns[$turnId].inputTokens = [int]$event.data.inputTokens
                }
                if ($null -ne $event.data.cost.amount) {
                    $turns[$turnId].aiCredits = [double]$event.data.cost.amount / 1000000000.0
                }
            } elseif ($event.type -eq "tool.execution_complete") {
                $turnId = [string]$event.data.turnId
                if ($turns.Contains($turnId)) {
                    $responseBytes = $event.data.toolTelemetry.metrics.mcp_result_content_bytes
                    if ($null -eq $responseBytes) {
                        $content = $event.data.result.content
                        $responseText = if ($content -is [string]) {
                            $content
                        } else {
                            @(
                                $content |
                                    Where-Object { $null -ne $_.text } |
                                    ForEach-Object { $_.text }
                            ) -join "`n"
                        }
                        $responseBytes = [Text.Encoding]::UTF8.GetByteCount($responseText)
                    }

                    $turns[$turnId].mcpResponseUtf8Bytes += [int]$responseBytes
                }
            } elseif ($event.type -eq "session.shutdown") {
                $session = [ordered]@{
                    aiCredits = if ($null -ne $event.data.totalNanoAiu) {
                        [double]$event.data.totalNanoAiu / 1000000000.0
                    } else { $null }
                    tokenDetails = $event.data.tokenDetails
                    modelMetrics = $event.data.modelMetrics
                }
            }
        }

        $eventMetrics[$evalName] = [ordered]@{
            eventsFile = $eventsFile.FullName
            evaluationIndex = $evaluationIndex
            turns = @($turns.Values)
            session = $session
        }
    }

    foreach ($otelFile in (Get-ChildItem -Path $ResultsRoot -Filter "otel-spans.jsonl" -Recurse)) {
        $traceToEval = @{}
        $chatSpans = @()
        foreach ($line in (Get-Content $otelFile.FullName)) {
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }

            $span = $line | ConvertFrom-Json
            if ($span.name -like "execute_tool *" -and $span.attributes.'gen_ai.tool.call.id') {
                $evalName = $toolCallToEval[$span.attributes.'gen_ai.tool.call.id']
                if ($evalName) {
                    $traceToEval[$span.traceId] = $evalName
                }
            } elseif ($span.name -like "chat *") {
                $chatSpans += $span
            }
        }

        foreach ($span in $chatSpans) {
            $evalName = $traceToEval[$span.traceId]
            if (!$evalName -or !$eventMetrics.Contains($evalName)) {
                continue
            }

            $attributes = $span.attributes
            $turnId = [string]$attributes.'github.copilot.turn_id'
            $turn = @($eventMetrics[$evalName].turns | Where-Object { [string]$_.turn -eq $turnId }) | Select-Object -First 1
            if (!$turn) {
                $turn = [ordered]@{
                    turn = $attributes.'github.copilot.turn_id'
                    toolName = $null
                    outputTokens = 0
                    inputTokens = $null
                    cacheReadTokens = $null
                    cacheWriteTokens = $null
                    aiCredits = $null
                    mcpResponseUtf8Bytes = 0
                }
                $eventMetrics[$evalName].turns += $turn
            }

            $turn.inputTokens = $attributes.'gen_ai.usage.input_tokens'
            $turn.outputTokens = $attributes.'gen_ai.usage.output_tokens'
            $turn.cacheReadTokens = $attributes.'gen_ai.usage.cache_read.input_tokens'
            $turn.cacheWriteTokens = $attributes.'gen_ai.usage.cache_creation.input_tokens'
            if ($null -ne $attributes.'github.copilot.nano_aiu') {
                $turn.aiCredits = [double]$attributes.'github.copilot.nano_aiu' / 1000000000.0
            }
        }
    }

    return $eventMetrics
}

function Get-StimulusMetrics {
    param(
        [string]$ResultsRoot
    )

    $resultsFile = Get-ChildItem -Path $ResultsRoot -Filter "results.jsonl" -Recurse |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (!$resultsFile) {
        throw "No results.jsonl found under $ResultsRoot."
    }

    $metrics = [ordered]@{}
    $eventMetrics = Get-EventMetrics $ResultsRoot
    Get-Content $resultsFile.FullName | ForEach-Object {
        if ([string]::IsNullOrWhiteSpace($_)) {
            return
        }
        $record = $_ | ConvertFrom-Json
        if ($record.type -ne "trial-result") {
            return
        }

        $costAmount = $record.trajectory.metrics.tokenUsage.cost.amount
        $aiCredits = if ($null -ne $costAmount) { $costAmount / 1000000000.0 } else { $null }

        $key = "$($record.evalName)::$($record.stimulus)"
        $evaluationIndex = $null
        if ($record.stimulus -match '(\d+)\s*$') {
            $evaluationIndex = [int]$Matches[1]
        }
        $eventDetail = @($eventMetrics.Values |
            Where-Object { $_.evaluationIndex -eq $evaluationIndex } |
            Select-Object -First 1)
        if ($eventDetail.Count -eq 0) {
            $eventDetail = $null
        } else {
            $eventDetail = $eventDetail[0]
        }
        $metrics[$key] = [PSCustomObject]@{
            Stimulus      = $record.stimulus
            EvalName      = $record.evalName
            Status        = $record.status
            TotalTokens   = $record.trajectory.metrics.tokenUsage.totalTokens
            TurnCount     = $record.trajectory.metrics.turnCount
            ToolCallCount = $record.trajectory.metrics.toolCallCount
            WallTimeMs    = $record.trajectory.metrics.wallTimeMs
            AICredits     = $aiCredits
            Turns         = if ($eventDetail) { $eventDetail.turns } else { @() }
            SessionCost   = if ($eventDetail) { $eventDetail.session } else { $null }
            EventsFile    = if ($eventDetail) { $eventDetail.eventsFile } else { $null }
        }
    }

    return $metrics
}

$modes = @(
    [ordered]@{ Name = 'Default'; Directory = 'default' },
    [ordered]@{ Name = 'TwoStep'; Directory = 'twostep' },
    [ordered]@{ Name = 'ThreeStep'; Directory = 'threestep' }
)
$modeMetrics = [ordered]@{}
$modeRoots = [ordered]@{}

for ($modeIndex = 0; $modeIndex -lt $modes.Count; $modeIndex++) {
    $mode = $modes[$modeIndex]
    $modeRoot = Join-Path $OutputPath $mode.Directory
    $modeRoots[$mode.Name] = $modeRoot
    if (!$SkipEval) {
        Invoke-DiscoveryMode -Mode $mode.Name -ResultsRoot $modeRoot -SkipBuild ($modeIndex -gt 0)
    }
    $modeMetrics[$mode.Name] = Get-StimulusMetrics -ResultsRoot $modeRoot
}

$allKeys = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($metrics in $modeMetrics.Values) {
    $metrics.Keys | ForEach-Object { [void]$allKeys.Add($_) }
}

function Get-Delta {
    param([Nullable[double]]$Baseline, [Nullable[double]]$Value)

    if ($null -eq $Baseline -or $null -eq $Value) {
        return $null
    }
    return $Value - $Baseline
}

function Get-PercentDelta {
    param([Nullable[double]]$Baseline, [Nullable[double]]$Value)

    if ($null -eq $Baseline -or $null -eq $Value -or $Baseline -eq 0) {
        return $null
    }
    return [math]::Round((($Value - $Baseline) / $Baseline) * 100, 1)
}

$comparison = @()
foreach ($key in ($allKeys | Sort-Object)) {
    $row = [ordered]@{
        EvalName = $null
        Stimulus = $null
    }
    foreach ($mode in $modes) {
        $metric = $modeMetrics[$mode.Name][$key]
        if ($metric) {
            if ($null -eq $row.EvalName) { $row.EvalName = $metric.EvalName }
            if ($null -eq $row.Stimulus) { $row.Stimulus = $metric.Stimulus }
        }
        $prefix = $mode.Name
        $row["${prefix}Status"] = if ($metric) { $metric.Status } else { "missing" }
        $row["${prefix}Tokens"] = if ($metric) { $metric.TotalTokens } else { $null }
        $row["${prefix}Turns"] = if ($metric) { $metric.TurnCount } else { $null }
        $row["${prefix}ToolCalls"] = if ($metric) { $metric.ToolCallCount } else { $null }
        $row["${prefix}WallMs"] = if ($metric) { $metric.WallTimeMs } else { $null }
        $row["${prefix}AICredits"] = if ($metric) { $metric.AICredits } else { $null }
    }
    $baseline = $modeMetrics['Default'][$key]
    foreach ($mode in $modes | Where-Object { $_.Name -ne 'Default' }) {
        $metric = $modeMetrics[$mode.Name][$key]
        $prefix = $mode.Name
        $row["${prefix}TokensDelta"] = Get-Delta -Baseline $baseline.TotalTokens -Value $metric.TotalTokens
        $row["${prefix}TokensDeltaPct"] = Get-PercentDelta -Baseline $baseline.TotalTokens -Value $metric.TotalTokens
        $row["${prefix}TurnsDelta"] = Get-Delta -Baseline $baseline.TurnCount -Value $metric.TurnCount
        $row["${prefix}ToolCallsDelta"] = Get-Delta -Baseline $baseline.ToolCallCount -Value $metric.ToolCallCount
        $row["${prefix}WallMsDelta"] = Get-Delta -Baseline $baseline.WallTimeMs -Value $metric.WallTimeMs
        $row["${prefix}WallMsDeltaPct"] = Get-PercentDelta -Baseline $baseline.WallTimeMs -Value $metric.WallTimeMs
        $row["${prefix}AICreditsDelta"] = Get-Delta -Baseline $baseline.AICredits -Value $metric.AICredits
        $row["${prefix}AICreditsDeltaPct"] = Get-PercentDelta -Baseline $baseline.AICredits -Value $metric.AICredits
    }
    $comparison += [PSCustomObject]$row
}

Write-Host "`n=== Comparison: $Namespace (Default, TwoStep, and ThreeStep) ===" -ForegroundColor Green
if (!$IsLinux -and !$IsMacOS) {
    try {
        $Host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(300, $Host.UI.RawUI.BufferSize.Height)
    } catch {
        # Ignore -- not all hosts (e.g. non-interactive) support resizing the buffer.
    }
}
$tableProperties = @([string]'Stimulus')
foreach ($mode in $modes) {
    $tableProperties += @{ Label = "$($mode.Name) Tok"; Expression = [scriptblock]::Create("`$_.$($mode.Name)Tokens") }
    $tableProperties += @{ Label = "$($mode.Name) Turns"; Expression = [scriptblock]::Create("`$_.$($mode.Name)Turns") }
    $tableProperties += @{ Label = "$($mode.Name) Tools"; Expression = [scriptblock]::Create("`$_.$($mode.Name)ToolCalls") }
}
foreach ($mode in $modes | Where-Object { $_.Name -ne 'Default' }) {
    $tableProperties += @{ Label = "$($mode.Name) Tok Δ"; Expression = [scriptblock]::Create("`$_.$($mode.Name)TokensDelta") }
    $tableProperties += @{ Label = "$($mode.Name) Tok Δ%"; Expression = [scriptblock]::Create("`$_.$($mode.Name)TokensDeltaPct") }
}
$comparison | Format-Table -Wrap -Property $tableProperties

$measurementInfo = [ordered]@{
    scope = 'End-to-end Vally/Copilot evaluation'
    includes = @(
        'Model input and output tokens',
        'Model turns and tool calls',
        'Wall-clock evaluation time',
        'AI Credits'
    )
    excludes = @(
        'A direct measurement of raw MCP response sizes'
    )
    perTurn = @(
        'Input/context tokens',
        'Cache-read and cache-write tokens',
        'Assistant output tokens',
        'MCP response UTF-8 bytes',
        'AI Credits'
    )
    rawMcpPayloadMeasurement = @{
        script = 'eng/scripts/Measure-McpOutputSizes.ps1'
        invokesCopilot = $false
        description = 'Sends scripted MCP requests directly to the server and measures UTF-8 response sizes.'
    }
}
Write-Host "`n=== Measurement scope ===" -ForegroundColor Green
Write-Host "The table above contains end-to-end Vally/Copilot usage metrics for all three modes."
Write-Host "It includes model turns, context replay, retries, and command selection."
Write-Host "Raw MCP response sizes require Measure-McpOutputSizes.ps1 and are not measured here."

function Format-Value {
    param($Value)

    if ($null -eq $Value) {
        return 'n/a'
    }
    if ($Value -is [double]) {
        return [string][math]::Round($Value, 5)
    }
    return [string]$Value
}

$averages = [ordered]@{}
foreach ($mode in $modes) {
    $prefix = $mode.Name
    $averages["Avg${prefix}Tokens"] = [math]::Round(($comparison | ForEach-Object { $_."${prefix}Tokens" } | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    $averages["Avg${prefix}Turns"] = [math]::Round(($comparison | ForEach-Object { $_."${prefix}Turns" } | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 1)
    $averages["Avg${prefix}WallMs"] = [math]::Round(($comparison | ForEach-Object { $_."${prefix}WallMs" } | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    $averages["Avg${prefix}AICredits"] = [math]::Round(($comparison | ForEach-Object { $_."${prefix}AICredits" } | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 4)
}
$aggregate = [ordered]@{}
foreach ($mode in $modes) {
    $prefix = $mode.Name
    foreach ($metric in @('Tokens', 'Turns', 'ToolCalls', 'WallMs', 'AICredits')) {
        $values = @($comparison | ForEach-Object { $_."${prefix}$metric" } | Where-Object { $null -ne $_ })
        $aggregate["${prefix}$metric"] = if ($values.Count -gt 0) {
            ($values | Measure-Object -Sum).Sum
        } else {
            $null
        }
    }
}
$averageRows = @()
foreach ($metric in @(
    [ordered]@{ Key = 'Tokens'; Label = 'Avg Tokens' },
    [ordered]@{ Key = 'Turns'; Label = 'Avg Turns' },
    [ordered]@{ Key = 'WallMs'; Label = 'Avg Wall (ms)' },
    [ordered]@{ Key = 'AICredits'; Label = 'Avg AI Credits' }
)) {
    $row = [ordered]@{ Metric = $metric.Label }
    foreach ($mode in $modes) {
        $row[$mode.Name] = $averages["Avg$($mode.Name)$($metric.Key)"]
    }
    $averageRows += [PSCustomObject]$row
}

$aggregateRows = @()
foreach ($metric in @(
    [ordered]@{ Key = 'Tokens'; Label = 'Tokens' },
    [ordered]@{ Key = 'Turns'; Label = 'Turns' },
    [ordered]@{ Key = 'ToolCalls'; Label = 'Tool Calls' },
    [ordered]@{ Key = 'WallMs'; Label = 'Wall (ms)' },
    [ordered]@{ Key = 'AICredits'; Label = 'AI Credits' }
)) {
    $row = [ordered]@{ Metric = $metric.Label }
    foreach ($mode in $modes) {
        $row[$mode.Name] = $aggregate["$($mode.Name)$($metric.Key)"]
    }
    $aggregateRows += [PSCustomObject]$row
}

Write-Host "`n=== Averages (per evaluation) ===" -ForegroundColor Green
$averageRows | Format-Table -AutoSize
Write-Host "`n=== Aggregate costs across evaluations ===" -ForegroundColor Green
$aggregateRows | Format-Table -AutoSize

$summaryLines = [System.Collections.Generic.List[string]]::new()
$summaryLines.Add("Tool discovery aggregate comparison: $Namespace")
$summaryLines.Add("Generated: $([DateTimeOffset]::UtcNow)")
$summaryLines.Add("Each row aggregates all discovered evaluations; Default is the baseline.")
$summaryLines.Add('')
$summaryLines.Add('Metric           Default     TwoStep    ThreeStep')
$summaryLines.Add('---------------  ----------  ----------  ----------')
foreach ($row in $aggregateRows) {
    $summaryLines.Add(('{0,-15}  {1,10}  {2,10}  {3,10}' -f `
        $row.Metric, (Format-Value $row.Default), (Format-Value $row.TwoStep), (Format-Value $row.ThreeStep)))
}
$summaryTextPath = Join-Path $OutputPath "$Namespace-summary.txt"
Set-Content -LiteralPath $summaryTextPath -Value $summaryLines -Encoding utf8

$summaryMarkdown = [System.Collections.Generic.List[string]]::new()
$summaryMarkdown.Add("# Tool Discovery Aggregate Comparison: $Namespace")
$summaryMarkdown.Add('')
$summaryMarkdown.Add('Default is the baseline; this table aggregates all discovered evaluations.')
$summaryMarkdown.Add('')
$summaryMarkdown.Add('| Metric | Default | TwoStep | ThreeStep |')
$summaryMarkdown.Add('| --- | ---: | ---: | ---: |')
foreach ($row in $aggregateRows) {
    $summaryMarkdown.Add("| $($row.Metric) | $($row.Default) | $($row.TwoStep) | $($row.ThreeStep) |")
}
$summaryMarkdownPath = Join-Path $OutputPath "$Namespace-summary.md"
Set-Content -LiteralPath $summaryMarkdownPath -Value $summaryMarkdown -Encoding utf8
Write-Host "Aggregate summary written to $summaryTextPath"
Write-Host "Aggregate summary written to $summaryMarkdownPath"

$csvPath = Join-Path $OutputPath "comparison.csv"
$comparison | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "`nFull comparison data written to $csvPath"

$jsonPath = Join-Path $OutputPath "comparison.json"
$perTurn = [ordered]@{}
foreach ($mode in $modes) {
    $perTurn[$mode.Name] = @($modeMetrics[$mode.Name].Values | ForEach-Object {
        [ordered]@{
            evalName = $_.EvalName
            stimulus = $_.Stimulus
            eventsFile = $_.EventsFile
            turns = $_.Turns
            session = $_.SessionCost
        }
    })
}
[ordered]@{
    namespace = $Namespace
    generatedAtUtc = [DateTimeOffset]::UtcNow
    measurement = $measurementInfo
    resultsRoots = $modeRoots
    averages = $averages
    aggregate = $aggregate
    comparison = $comparison
    perTurn = $perTurn
} | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding utf8
Write-Host "Comparison metadata and results written to $jsonPath"

$turnCsvPath = Join-Path $OutputPath "turns.csv"
$turnRows = @()
foreach ($mode in $modes) {
    foreach ($metric in $modeMetrics[$mode.Name].Values) {
        foreach ($turn in $metric.Turns) {
            $toolName = if ($turn.toolName) { [string]$turn.toolName } else { $null }
            $turnLabel = if ($toolName) { "{0}: {1}" -f [string]$turn.turn, $toolName } else { [string]$turn.turn }
            $turnRows += [PSCustomObject]@{
                Mode = $mode.Name
                EvalName = $metric.EvalName
                Stimulus = $metric.Stimulus
                Turn = $turnLabel
                TurnNumber = [int]$turn.turn
                ToolName = $toolName
                OutputTokens = $turn.outputTokens
                InputTokens = $turn.inputTokens
                CacheReadTokens = $turn.cacheReadTokens
                CacheWriteTokens = $turn.cacheWriteTokens
                AICredits = $turn.aiCredits
                McpResponseUtf8Bytes = $turn.mcpResponseUtf8Bytes
                SessionAICredits = $metric.SessionCost.aiCredits
            }
        }
    }
}
$turnRows | Export-Csv -Path $turnCsvPath -NoTypeInformation
Write-Host "Per-turn data written to $turnCsvPath"

function Get-TurnByNumber {
    param($Turns, $TurnNumber)

    return @($Turns | Where-Object { [string]$_.turn -eq [string]$TurnNumber }) | Select-Object -First 1
}

function Get-TurnDisplayLabel {
    param($TurnNumber, $ToolName)

    $turn = [string]$TurnNumber
    if ($null -ne $ToolName -and $ToolName -ne '') {
        return "${turn}: ${ToolName}"
    }
    return $turn
}

$reportRows = @()
foreach ($key in ($allKeys | Sort-Object)) {
    $metricValues = @($modes | ForEach-Object { $modeMetrics[$_.Name][$key] } | Where-Object { $null -ne $_ })
    $stimulus = $metricValues[0].Stimulus
    $turnNumbers = [System.Collections.Generic.SortedSet[int]]::new()
    foreach ($mode in $modes) {
        foreach ($turn in $modeMetrics[$mode.Name][$key].Turns) {
            if ($null -ne $turn -and $null -ne $turn.turn) {
                [void]$turnNumbers.Add([int]$turn.turn)
            }
        }
    }

    $stimulusRows = @()
    foreach ($turnNumber in $turnNumbers) {
        $toolNames = @()
        foreach ($mode in $modes) {
            $turnObj = Get-TurnByNumber -Turns $modeMetrics[$mode.Name][$key].Turns -TurnNumber $turnNumber
            if ($null -ne $turnObj -and $turnObj.toolName) {
                $toolNames += [string]$turnObj.toolName
            }
        }
        $toolName = if ($toolNames.Count -gt 0) { ($toolNames | Select-Object -Unique) -join '; ' } else { '' }
        $displayTurn = Get-TurnDisplayLabel -TurnNumber $turnNumber -ToolName $toolName

        $turnRow = [ordered]@{ Stimulus = $stimulus; Turn = $displayTurn; TurnNumber = $turnNumber; ToolName = $toolName }
        foreach ($mode in $modes) {
            $turn = Get-TurnByNumber -Turns $modeMetrics[$mode.Name][$key].Turns -TurnNumber $turnNumber
            $prefix = $mode.Name
            $turnRow["${prefix}InputTokens"] = $turn.inputTokens
            $turnRow["${prefix}OutputTokens"] = $turn.outputTokens
            $turnRow["${prefix}McpBytes"] = $turn.mcpResponseUtf8Bytes
            $turnRow["${prefix}AICredits"] = $turn.aiCredits
        }
        foreach ($mode in $modes | Where-Object { $_.Name -ne 'Default' }) {
            $prefix = $mode.Name
            $baselineTurn = Get-TurnByNumber -Turns $modeMetrics.Default[$key].Turns -TurnNumber $turnNumber
            $modeTurn = Get-TurnByNumber -Turns $modeMetrics[$mode.Name][$key].Turns -TurnNumber $turnNumber
            $turnRow["${prefix}InputDelta"] = Get-Delta -Baseline $baselineTurn.inputTokens -Value $modeTurn.inputTokens
            $turnRow["${prefix}OutputDelta"] = Get-Delta -Baseline $baselineTurn.outputTokens -Value $modeTurn.outputTokens
            $turnRow["${prefix}McpBytesDelta"] = Get-Delta -Baseline $baselineTurn.mcpResponseUtf8Bytes -Value $modeTurn.mcpResponseUtf8Bytes
            $turnRow["${prefix}AICreditsDelta"] = Get-Delta -Baseline $baselineTurn.aiCredits -Value $modeTurn.aiCredits
        }
        $stimulusRows += [PSCustomObject]$turnRow
    }

    $reportRows += [PSCustomObject]@{
        Stimulus = $stimulus
        Rows = $stimulusRows
    }
}

function Get-Total {
    param($Rows, [string]$Property)

    $values = @($Rows | ForEach-Object { $_.$Property } | Where-Object { $null -ne $_ })
    if ($values.Count -eq 0) {
        return $null
    }
    return ($values | Measure-Object -Sum).Sum
}

$textLines = [System.Collections.Generic.List[string]]::new()
$markdownLines = [System.Collections.Generic.List[string]]::new()

$textLines.Add("Tool discovery cost comparison: $Namespace")
$textLines.Add("Generated: $([DateTimeOffset]::UtcNow)")
$textLines.Add('')

$markdownLines.Add("# Tool Discovery Cost Comparison: $Namespace")
$markdownLines.Add('')
$markdownLines.Add("- **Generated:** $([DateTimeOffset]::UtcNow)")
$markdownLines.Add('- **Modes:** Default, TwoStep, and ThreeStep tool discovery')
$markdownLines.Add('- **Delta:** TwoStep and ThreeStep minus Default')
$markdownLines.Add('')

foreach ($report in $reportRows) {
    $rows = $report.Rows

    $totalRow = [ordered]@{ Turn = 'Total' }
    foreach ($mode in $modes) {
        foreach ($metric in @('InputTokens', 'OutputTokens', 'McpBytes', 'AICredits')) {
            $totalRow["$($mode.Name)$metric"] = Get-Total -Rows $rows -Property "$($mode.Name)$metric"
        }
    }
    foreach ($mode in $modes | Where-Object { $_.Name -ne 'Default' }) {
        foreach ($metric in @('InputTokens', 'OutputTokens', 'McpBytes', 'AICredits')) {
            $totalRow["$($mode.Name)$metric`Delta"] = Get-Delta `
                -Baseline $totalRow["Default$metric"] -Value $totalRow["$($mode.Name)$metric"]
        }
    }

    $textLines.Add("Stimulus: $($report.Stimulus)")
    $textLines.Add("Modes: Default baseline; deltas are mode minus Default")

    $markdownLines.Add("## $($report.Stimulus)")
    $markdownLines.Add('')
    $markdownLines.Add('| Turn | Default input | TwoStep input | TwoStep Δ | ThreeStep input | ThreeStep Δ | Default output | TwoStep output | TwoStep Δ | ThreeStep output | ThreeStep Δ |')
    $markdownLines.Add('| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |')

    foreach ($row in $rows) {
        $textLines.Add(("{0}: Default input={1}, TwoStep input={2} (Δ {3}), ThreeStep input={4} (Δ {5}); Default output={6}, TwoStep output={7} (Δ {8}), ThreeStep output={9} (Δ {10})" -f `
            $row.Turn, $row.DefaultInputTokens, $row.TwoStepInputTokens, $row.TwoStepInputDelta,
            $row.ThreeStepInputTokens, $row.ThreeStepInputDelta, $row.DefaultOutputTokens,
            $row.TwoStepOutputTokens, $row.TwoStepOutputDelta, $row.ThreeStepOutputTokens, $row.ThreeStepOutputDelta))

        $markdownLines.Add("| $($row.Turn) | $($row.DefaultInputTokens) | $($row.TwoStepInputTokens) | $($row.TwoStepInputDelta) | $($row.ThreeStepInputTokens) | $($row.ThreeStepInputDelta) | $($row.DefaultOutputTokens) | $($row.TwoStepOutputTokens) | $($row.TwoStepOutputDelta) | $($row.ThreeStepOutputTokens) | $($row.ThreeStepOutputDelta) |")
    }

    $textLines.Add(("Total: Default input={0}, TwoStep input={1} (Δ {2}), ThreeStep input={3} (Δ {4}); Default output={5}, TwoStep output={6} (Δ {7}), ThreeStep output={8} (Δ {9})" -f `
        $totalRow.DefaultInputTokens, $totalRow.TwoStepInputTokens, $totalRow.TwoStepInputTokensDelta,
        $totalRow.ThreeStepInputTokens, $totalRow.ThreeStepInputTokensDelta, $totalRow.DefaultOutputTokens,
        $totalRow.TwoStepOutputTokens, $totalRow.TwoStepOutputTokensDelta, $totalRow.ThreeStepOutputTokens, $totalRow.ThreeStepOutputTokensDelta))
    $textLines.Add('')

    $markdownLines.Add("| **Total** | $($totalRow.DefaultInputTokens) | $($totalRow.TwoStepInputTokens) | $($totalRow.TwoStepInputTokensDelta) | $($totalRow.ThreeStepInputTokens) | $($totalRow.ThreeStepInputTokensDelta) | $($totalRow.DefaultOutputTokens) | $($totalRow.TwoStepOutputTokens) | $($totalRow.TwoStepOutputTokensDelta) | $($totalRow.ThreeStepOutputTokens) | $($totalRow.ThreeStepOutputTokensDelta) |")
    $markdownLines.Add('')
}

$textReportPath = Join-Path $OutputPath "$Namespace-turn-comparison.txt"
$markdownReportPath = Join-Path $OutputPath "$Namespace-turn-comparison.md"
Set-Content -LiteralPath $textReportPath -Value $textLines -Encoding utf8
Set-Content -LiteralPath $markdownReportPath -Value $markdownLines -Encoding utf8
Write-Host "Per-turn comparison report written to $textReportPath"
Write-Host "Per-turn comparison report written to $markdownReportPath"
