#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Compares Vally evaluation metrics for a single tool namespace between two-step
    and three-step Azure MCP Server tool discovery.

.DESCRIPTION
    Runs `Invoke-VallyEvalTests.ps1` twice for the given namespace -- once with
    `-ToolDiscoveryMode TwoStep` and once with `-ToolDiscoveryMode ThreeStep` --
    restricted to the transient eval.yaml file(s) generated under
    `<repo-root>/.work/vally/evals/<Namespace>` (the "official" eval suite produced
    by VallyEvaluator). Checked-in eval.yaml files under `tools/*/tests` are
    excluded.

    For each stimulus present in both runs' `results.jsonl` output, this script
    extracts total tokens, turn count, tool-call count, wall-clock time, and AI
    credits consumed, then prints a table contrasting the two-step and three-step
    values (absolute and percentage difference).

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
    Base directory used to hold the two-step and three-step Vally output folders
    for this comparison. Defaults to `<repo-root>/.work/vally/compare-results/<Namespace>`.

.PARAMETER SkipAgentsInstructions
    Passed through to `Invoke-VallyEvalTests.ps1` for both runs. See that script's
    help for details.

.PARAMETER IsDebug
    Passed through to `Invoke-VallyEvalTests.ps1` for both runs to add `--verbose`
    output.

.EXAMPLE
    ./eng/scripts/Compare-VallyToolDiscoveryMetrics.ps1 -Namespace appconfig

    Runs the appconfig eval suite in both two-step and three-step discovery modes
    and prints a comparison table of tokens, turns, tool calls, wall time, and AI
    credits.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Namespace,
    [int]$NumberOfRuns = 1,
    [string]$OutputPath,
    [switch]$SkipAgentsInstructions,
    [switch]$IsDebug
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$OutputPath) {
    $OutputPath = Join-Path $RepoRoot ".work/vally/compare-results/$Namespace"
}

if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null

# Restrict discovery to the transient eval.yaml generated for this namespace under
# .work/vally/evals -- explicitly excludes any checked-in tools/*/tests/eval.yaml.
$evalPathFilter = "*evals*$Namespace*eval.yaml"

function Invoke-DiscoveryMode {
    param(
        [string]$Mode,
        [string]$ResultsRoot
    )

    Write-Host "`n=== Running $Mode discovery for namespace '$Namespace' ===" -ForegroundColor Cyan

    $scriptArgs = @{
        ToolDiscoveryMode = $Mode
        EvalPathFilter    = $evalPathFilter
        OutputPath        = $ResultsRoot
        NumberOfRuns      = $NumberOfRuns
    }
    if ($SkipAgentsInstructions) {
        $scriptArgs["SkipAgentsInstructions"] = $true
    }
    if ($IsDebug) {
        $scriptArgs["IsDebug"] = $true
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

$twoStepResultsRoot = Join-Path $OutputPath "twostep"
$threeStepResultsRoot = Join-Path $OutputPath "threestep"

Invoke-DiscoveryMode -Mode "TwoStep" -ResultsRoot $twoStepResultsRoot
Invoke-DiscoveryMode -Mode "ThreeStep" -ResultsRoot $threeStepResultsRoot

$twoStepMetrics = Get-StimulusMetrics -ResultsRoot $twoStepResultsRoot
$threeStepMetrics = Get-StimulusMetrics -ResultsRoot $threeStepResultsRoot

$allKeys = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
$twoStepMetrics.Keys | ForEach-Object { [void]$allKeys.Add($_) }
$threeStepMetrics.Keys | ForEach-Object { [void]$allKeys.Add($_) }

function Get-Delta {
    param([Nullable[double]]$TwoStep, [Nullable[double]]$ThreeStep)

    if ($null -eq $TwoStep -or $null -eq $ThreeStep) {
        return $null
    }
    return $ThreeStep - $TwoStep
}

function Get-PercentDelta {
    param([Nullable[double]]$TwoStep, [Nullable[double]]$ThreeStep)

    if ($null -eq $TwoStep -or $null -eq $ThreeStep -or $TwoStep -eq 0) {
        return $null
    }
    return [math]::Round((($ThreeStep - $TwoStep) / $TwoStep) * 100, 1)
}

$comparison = @()
foreach ($key in ($allKeys | Sort-Object)) {
    $two = $twoStepMetrics[$key]
    $three = $threeStepMetrics[$key]

    $stimulus = if ($two) { $two.Stimulus } else { $three.Stimulus }
    $evalName = if ($two) { $two.EvalName } else { $three.EvalName }

    $comparison += [PSCustomObject]@{
        EvalName            = $evalName
        Stimulus            = $stimulus
        TwoStepStatus       = if ($two) { $two.Status } else { "missing" }
        ThreeStepStatus     = if ($three) { $three.Status } else { "missing" }
        TwoStepTokens       = if ($two) { $two.TotalTokens } else { $null }
        ThreeStepTokens     = if ($three) { $three.TotalTokens } else { $null }
        TokensDelta         = Get-Delta -TwoStep $two.TotalTokens -ThreeStep $three.TotalTokens
        TokensDeltaPct      = Get-PercentDelta -TwoStep $two.TotalTokens -ThreeStep $three.TotalTokens
        TwoStepTurns        = if ($two) { $two.TurnCount } else { $null }
        ThreeStepTurns      = if ($three) { $three.TurnCount } else { $null }
        TurnsDelta          = Get-Delta -TwoStep $two.TurnCount -ThreeStep $three.TurnCount
        TwoStepToolCalls    = if ($two) { $two.ToolCallCount } else { $null }
        ThreeStepToolCalls  = if ($three) { $three.ToolCallCount } else { $null }
        ToolCallsDelta      = Get-Delta -TwoStep $two.ToolCallCount -ThreeStep $three.ToolCallCount
        TwoStepWallMs       = if ($two) { $two.WallTimeMs } else { $null }
        ThreeStepWallMs     = if ($three) { $three.WallTimeMs } else { $null }
        WallMsDelta         = Get-Delta -TwoStep $two.WallTimeMs -ThreeStep $three.WallTimeMs
        WallMsDeltaPct      = Get-PercentDelta -TwoStep $two.WallTimeMs -ThreeStep $three.WallTimeMs
        TwoStepAICredits    = if ($two) { $two.AICredits } else { $null }
        ThreeStepAICredits  = if ($three) { $three.AICredits } else { $null }
        AICreditsDelta      = Get-Delta -TwoStep $two.AICredits -ThreeStep $three.AICredits
        AICreditsDeltaPct   = Get-PercentDelta -TwoStep $two.AICredits -ThreeStep $three.AICredits
    }
}

Write-Host "`n=== Comparison: $Namespace (TwoStep vs ThreeStep) ===" -ForegroundColor Green
if (!$IsLinux -and !$IsMacOS) {
    try {
        $Host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(300, $Host.UI.RawUI.BufferSize.Height)
    } catch {
        # Ignore -- not all hosts (e.g. non-interactive) support resizing the buffer.
    }
}
$comparison |
    Format-Table -Wrap -Property `
        Stimulus,
    @{ Label = "2S Tok"; Expression = { $_.TwoStepTokens } },
    @{ Label = "3S Tok"; Expression = { $_.ThreeStepTokens } },
    @{ Label = "TokDelta"; Expression = { $_.TokensDelta } },
    @{ Label = "TokDelta%"; Expression = { $_.TokensDeltaPct } },
    @{ Label = "2S Turns"; Expression = { $_.TwoStepTurns } },
    @{ Label = "3S Turns"; Expression = { $_.ThreeStepTurns } },
    @{ Label = "2S Tools"; Expression = { $_.TwoStepToolCalls } },
    @{ Label = "3S Tools"; Expression = { $_.ThreeStepToolCalls } },
    @{ Label = "2S Wall(ms)"; Expression = { $_.TwoStepWallMs } },
    @{ Label = "3S Wall(ms)"; Expression = { $_.ThreeStepWallMs } },
    @{ Label = "WallDelta(ms)"; Expression = { $_.WallMsDelta } },
    @{ Label = "WallDelta%"; Expression = { $_.WallMsDeltaPct } },
    @{ Label = "2S AICr"; Expression = { $_.TwoStepAICredits } },
    @{ Label = "3S AICr"; Expression = { $_.ThreeStepAICredits } },
    @{ Label = "AICrDelta"; Expression = { $_.AICreditsDelta } },
    @{ Label = "AICrDelta%"; Expression = { $_.AICreditsDeltaPct } }

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
Write-Host "The table above contains end-to-end Vally/Copilot usage metrics."
Write-Host "It includes model turns, context replay, retries, and command selection."
Write-Host "Raw MCP response sizes require Measure-McpOutputSizes.ps1 and are not measured here."

$averages = [PSCustomObject]@{
    AvgTwoStepTokens     = [math]::Round(($comparison.TwoStepTokens | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    AvgThreeStepTokens   = [math]::Round(($comparison.ThreeStepTokens | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    AvgTwoStepWallMs     = [math]::Round(($comparison.TwoStepWallMs | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    AvgThreeStepWallMs   = [math]::Round(($comparison.ThreeStepWallMs | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 0)
    AvgTwoStepAICredits  = [math]::Round(($comparison.TwoStepAICredits | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 4)
    AvgThreeStepAICredits = [math]::Round(($comparison.ThreeStepAICredits | Where-Object { $null -ne $_ } | Measure-Object -Average).Average, 4)
}
Write-Host "`n=== Averages ===" -ForegroundColor Green
$averages | Format-Table -AutoSize

$csvPath = Join-Path $OutputPath "comparison.csv"
$comparison | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host "`nFull comparison data written to $csvPath"

$jsonPath = Join-Path $OutputPath "comparison.json"
[ordered]@{
    namespace = $Namespace
    generatedAtUtc = [DateTimeOffset]::UtcNow
    measurement = $measurementInfo
    twoStepResultsRoot = $twoStepResultsRoot
    threeStepResultsRoot = $threeStepResultsRoot
    averages = $averages
    comparison = $comparison
    perTurn = [ordered]@{
        twoStep = @($twoStepMetrics.Values | ForEach-Object {
            [ordered]@{
                evalName = $_.EvalName
                stimulus = $_.Stimulus
                eventsFile = $_.EventsFile
                turns = $_.Turns
                session = $_.SessionCost
            }
        })
        threeStep = @($threeStepMetrics.Values | ForEach-Object {
            [ordered]@{
                evalName = $_.EvalName
                stimulus = $_.Stimulus
                eventsFile = $_.EventsFile
                turns = $_.Turns
                session = $_.SessionCost
            }
        })
    }
} | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding utf8
Write-Host "Comparison metadata and results written to $jsonPath"

$turnCsvPath = Join-Path $OutputPath "turns.csv"
$turnRows = @()
foreach ($mode in @(
    [ordered]@{ name = 'TwoStep'; metrics = $twoStepMetrics },
    [ordered]@{ name = 'ThreeStep'; metrics = $threeStepMetrics }
)) {
    foreach ($metric in $mode.metrics.Values) {
        foreach ($turn in $metric.Turns) {
            $toolName = if ($turn.toolName) { [string]$turn.toolName } else { $null }
            $turnLabel = if ($toolName) { "{0}: {1}" -f [string]$turn.turn, $toolName } else { [string]$turn.turn }
            $turnRows += [PSCustomObject]@{
                Mode = $mode.name
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
    $two = $twoStepMetrics[$key]
    $three = $threeStepMetrics[$key]

    $stimulus = if ($two) { $two.Stimulus } else { $three.Stimulus }
    $turnNumbers = [System.Collections.Generic.SortedSet[int]]::new()
    foreach ($turn in @($two.Turns) + @($three.Turns)) {
        if ($null -ne $turn -and $null -ne $turn.turn) {
            [void]$turnNumbers.Add([int]$turn.turn)
        }
    }

    $stimulusRows = @()
    foreach ($turnNumber in $turnNumbers) {
        $twoTurn = Get-TurnByNumber -Turns $two.Turns -TurnNumber $turnNumber
        $threeTurn = Get-TurnByNumber -Turns $three.Turns -TurnNumber $turnNumber

        $toolNames = @()
        foreach ($turnObj in @($twoTurn, $threeTurn)) {
            if ($null -ne $turnObj -and $null -ne $turnObj.toolName -and $turnObj.toolName -ne '') {
                $toolNames += [string]$turnObj.toolName
            }
        }
        $toolName = if ($toolNames.Count -gt 0) { ($toolNames | Select-Object -Unique) -join '; ' } else { '' }
        $displayTurn = Get-TurnDisplayLabel -TurnNumber $turnNumber -ToolName $toolName

        $stimulusRows += [PSCustomObject]@{
            Stimulus = $stimulus
            Turn = $displayTurn
            TurnNumber = $turnNumber
            ToolName = $toolName
            TwoStepInputTokens = $twoTurn.inputTokens
            ThreeStepInputTokens = $threeTurn.inputTokens
            InputTokensDelta = Get-Delta -TwoStep $twoTurn.inputTokens -ThreeStep $threeTurn.inputTokens
            TwoStepOutputTokens = $twoTurn.outputTokens
            ThreeStepOutputTokens = $threeTurn.outputTokens
            OutputTokensDelta = Get-Delta -TwoStep $twoTurn.outputTokens -ThreeStep $threeTurn.outputTokens
            TwoStepMcpBytes = $twoTurn.mcpResponseUtf8Bytes
            ThreeStepMcpBytes = $threeTurn.mcpResponseUtf8Bytes
            McpBytesDelta = Get-Delta -TwoStep $twoTurn.mcpResponseUtf8Bytes -ThreeStep $threeTurn.mcpResponseUtf8Bytes
            TwoStepAICredits = $twoTurn.aiCredits
            ThreeStepAICredits = $threeTurn.aiCredits
            AICreditsDelta = Get-Delta -TwoStep $twoTurn.aiCredits -ThreeStep $threeTurn.aiCredits
        }
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
$markdownLines.Add('- **Modes:** two-step vs. three-step tool discovery')
$markdownLines.Add('- **Delta:** three-step minus two-step')
$markdownLines.Add('')

foreach ($report in $reportRows) {
    $rows = $report.Rows

    $totalRow = [PSCustomObject]@{
        Turn = 'Total'
        TwoStepInputTokens = Get-Total -Rows $rows -Property 'TwoStepInputTokens'
        ThreeStepInputTokens = Get-Total -Rows $rows -Property 'ThreeStepInputTokens'
        TwoStepOutputTokens = Get-Total -Rows $rows -Property 'TwoStepOutputTokens'
        ThreeStepOutputTokens = Get-Total -Rows $rows -Property 'ThreeStepOutputTokens'
        TwoStepMcpBytes = Get-Total -Rows $rows -Property 'TwoStepMcpBytes'
        ThreeStepMcpBytes = Get-Total -Rows $rows -Property 'ThreeStepMcpBytes'
        TwoStepAICredits = Get-Total -Rows $rows -Property 'TwoStepAICredits'
        ThreeStepAICredits = Get-Total -Rows $rows -Property 'ThreeStepAICredits'
    }

    $textLines.Add("Stimulus: $($report.Stimulus)")
    $textLines.Add(('{0,-6} {1,12} {2,12} {3,12} {4,10} {5,10} {6,10} {7,12} {8,12} {9,12}' -f `
        'Turn', '2S Input', '3S Input', 'InputDelta', '2S Out', '3S Out', 'OutDelta', '2S MCP B', '3S MCP B', 'MCPDelta'))

    $markdownLines.Add("## $($report.Stimulus)")
    $markdownLines.Add('')
    $markdownLines.Add('| Turn | 2S Input Tokens | 3S Input Tokens | Input Delta | 2S Output | 3S Output | Output Delta | 2S MCP Bytes | 3S MCP Bytes | MCP Bytes Delta | 2S AI Credits | 3S AI Credits | AI Credits Delta |')
    $markdownLines.Add('| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |')

    foreach ($row in $rows) {
        $textLines.Add(('{0,-6} {1,12} {2,12} {3,12} {4,10} {5,10} {6,10} {7,12} {8,12} {9,12}' -f `
            (Format-Value $row.Turn),
            (Format-Value $row.TwoStepInputTokens),
            (Format-Value $row.ThreeStepInputTokens),
            (Format-Value $row.InputTokensDelta),
            (Format-Value $row.TwoStepOutputTokens),
            (Format-Value $row.ThreeStepOutputTokens),
            (Format-Value $row.OutputTokensDelta),
            (Format-Value $row.TwoStepMcpBytes),
            (Format-Value $row.ThreeStepMcpBytes),
            (Format-Value $row.McpBytesDelta)))

        $markdownLines.Add(
            "| $(Format-Value $row.Turn) | $(Format-Value $row.TwoStepInputTokens) | $(Format-Value $row.ThreeStepInputTokens) | " +
            "$(Format-Value $row.InputTokensDelta) | $(Format-Value $row.TwoStepOutputTokens) | $(Format-Value $row.ThreeStepOutputTokens) | " +
            "$(Format-Value $row.OutputTokensDelta) | $(Format-Value $row.TwoStepMcpBytes) | $(Format-Value $row.ThreeStepMcpBytes) | " +
            "$(Format-Value $row.McpBytesDelta) | $(Format-Value $row.TwoStepAICredits) | $(Format-Value $row.ThreeStepAICredits) | " +
            "$(Format-Value $row.AICreditsDelta) |")
    }

    $totalInputDelta = Get-Delta -TwoStep $totalRow.TwoStepInputTokens -ThreeStep $totalRow.ThreeStepInputTokens
    $totalOutputDelta = Get-Delta -TwoStep $totalRow.TwoStepOutputTokens -ThreeStep $totalRow.ThreeStepOutputTokens
    $totalMcpDelta = Get-Delta -TwoStep $totalRow.TwoStepMcpBytes -ThreeStep $totalRow.ThreeStepMcpBytes
    $totalCreditsDelta = Get-Delta -TwoStep $totalRow.TwoStepAICredits -ThreeStep $totalRow.ThreeStepAICredits

    $textLines.Add(('{0,-6} {1,12} {2,12} {3,12} {4,10} {5,10} {6,10} {7,12} {8,12} {9,12}' -f `
        'Total',
        (Format-Value $totalRow.TwoStepInputTokens),
        (Format-Value $totalRow.ThreeStepInputTokens),
        (Format-Value $totalInputDelta),
        (Format-Value $totalRow.TwoStepOutputTokens),
        (Format-Value $totalRow.ThreeStepOutputTokens),
        (Format-Value $totalOutputDelta),
        (Format-Value $totalRow.TwoStepMcpBytes),
        (Format-Value $totalRow.ThreeStepMcpBytes),
        (Format-Value $totalMcpDelta)))
    $textLines.Add('')

    $markdownLines.Add(
        "| **Total** | $(Format-Value $totalRow.TwoStepInputTokens) | $(Format-Value $totalRow.ThreeStepInputTokens) | " +
        "$(Format-Value $totalInputDelta) | $(Format-Value $totalRow.TwoStepOutputTokens) | $(Format-Value $totalRow.ThreeStepOutputTokens) | " +
        "$(Format-Value $totalOutputDelta) | $(Format-Value $totalRow.TwoStepMcpBytes) | $(Format-Value $totalRow.ThreeStepMcpBytes) | " +
        "$(Format-Value $totalMcpDelta) | $(Format-Value $totalRow.TwoStepAICredits) | $(Format-Value $totalRow.ThreeStepAICredits) | " +
        "$(Format-Value $totalCreditsDelta) |")
    $markdownLines.Add('')
}

$textReportPath = Join-Path $OutputPath "$Namespace-turn-comparison.txt"
$markdownReportPath = Join-Path $OutputPath "$Namespace-turn-comparison.md"
Set-Content -LiteralPath $textReportPath -Value $textLines -Encoding utf8
Set-Content -LiteralPath $markdownReportPath -Value $markdownLines -Encoding utf8
Write-Host "Per-turn comparison report written to $textReportPath"
Write-Host "Per-turn comparison report written to $markdownReportPath"
