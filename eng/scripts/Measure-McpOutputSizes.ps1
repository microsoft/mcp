#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Builds and runs the MCP output size measurement test, then summarizes the results.

.DESCRIPTION
    Runs the full measurement workflow end to end:

    1. Builds the Azure.Mcp.Server project and the standalone McpOutputSizeMeasurer tool.
    2. Runs McpOutputSizeMeasurer, which starts the MCP server over stdio in both
       consolidated and namespace modes and measures the initialize greeting,
       tools/list discovery, and learn-mode responses (including inner commands) for
       every tool.
    3. Summarizes the report to produce console, JSON, and Markdown summaries,
       extract readable description text, and split each top tool's inner commands
       into individual files.

.PARAMETER OutputDirectory
    Directory for the measurement report and its artifacts.
    Defaults to `<repo-root>/.work/mcp-output-size`.

.PARAMETER Configuration
    The build configuration to use. Defaults to `Debug`.

.PARAMETER SkipBuild
    Skip the build step and use the existing binaries. Implied (and not required) when
    -ServerExecutable or -ReleaseTag is supplied, since that server binary isn't built by
    this script.

.PARAMETER Clean
    Remove the output directory before running so stale artifacts are not mixed with
    the new results.

.PARAMETER LearnResponseThresholdUtf8Bytes
    Include every learn response over this UTF-8 byte threshold in the summary.
    Defaults to 45000.

.PARAMETER ServerExecutable
    Path to an already-built or published azmcp server executable to measure, instead of
    building and using the local servers/Azure.Mcp.Server/src project. Use this to measure
    a previously released version of the server (e.g. a binary extracted from a release
    asset or installed via a package manager) so its output sizes can be diffed against
    the current source tree. When supplied, the local server project is not built or
    resolved; only the McpOutputSizeMeasurer tool is still built (unless -SkipBuild is
    also passed). Mutually exclusive with -ReleaseTag.

.PARAMETER ReleaseTag
    A GitHub release tag from the microsoft/mcp repository (e.g.
    `Azure.Mcp.Server-3.0.0-beta.36`) whose azmcp server asset should be downloaded and
    measured, instead of building the local source tree. The matching platform zip
    (`Azure.Mcp.Server-<os>-<arch>.zip`) is downloaded to
    `<OutputDirectory>/release-download/<ReleaseTag>` and extracted before measurement.
    Mutually exclusive with -ServerExecutable.

.PARAMETER GitHubRepository
    The `owner/repo` used to resolve -ReleaseTag. Defaults to `microsoft/mcp`. Only used
    when -ReleaseTag is supplied.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1

    Builds, measures, and summarizes using default paths.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -SkipBuild -Clean

    Reuses the existing build, clears previous results, then measures and summarizes.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -ServerExecutable C:\releases\azmcp-1.2.3\azmcp.exe -OutputDirectory .work/mcp-output-size/released-1.2.3

    Measures a previously released azmcp build (e.g. downloaded/extracted from a GitHub
    release) instead of the local source tree, writing results to a separate directory so
    they can be compared against a current-source run.

.EXAMPLE
    ./eng/scripts/Measure-McpOutputSizes.ps1 -ReleaseTag Azure.Mcp.Server-3.0.0-beta.36 -OutputDirectory .work/mcp-output-size/beta.36

    Downloads the azmcp server asset for the given release tag from GitHub, measures it,
    and writes results to a separate directory so they can be compared against a
    current-source run.
#>

[CmdletBinding()]
param(
    [string]$OutputDirectory,
    [string]$Configuration = 'Debug',
    [switch]$SkipBuild,
    [switch]$Clean,
    [int]$LearnResponseThresholdUtf8Bytes = 45000,
    [string]$ServerExecutable,
    [string]$ReleaseTag,
    [string]$GitHubRepository = 'microsoft/mcp'
)

if ($ServerExecutable -and $ReleaseTag) {
    Write-Error "-ServerExecutable and -ReleaseTag are mutually exclusive."
    exit 1
}

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$repoRoot = $RepoRoot.Path

$serverProject = Join-Path $repoRoot 'servers/Azure.Mcp.Server/src'
$measurerProject = Join-Path $repoRoot 'eng/tools/McpOutputSizeMeasurer/src'

function Invoke-McpOutputSizeSummary {
    param(
        [Parameter(Mandatory)]
        [string] $InputPath,

        [string] $OutputPath,

        [string] $MarkdownPath,

        [int] $LearnResponseThresholdUtf8Bytes = 45000
    )

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
        foreach ($entry in $entries) {
            $entry | Add-Member -NotePropertyName innerCommandCount -NotePropertyValue 0 -Force
            $entry | Add-Member -NotePropertyName innerCommandDirectory -NotePropertyValue $null -Force

            if (!$entry.learnResponseTextFile -or !(Test-Path -LiteralPath $entry.learnResponseTextFile -PathType Leaf)) {
                continue
            }

            $text = Get-Content -LiteralPath $entry.learnResponseTextFile -Raw
            $start = $text.IndexOf('[')
            if ($start -lt 0) {
                continue
            }

            try {
                $commands = @($text.Substring($start) | ConvertFrom-Json)
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
}

if (!$OutputDirectory) {
    $OutputDirectory = Join-Path $repoRoot '.work/mcp-output-size'
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)

if ($Clean -and (Test-Path -LiteralPath $OutputDirectory)) {
    Write-Host "Removing existing output directory $OutputDirectory"
    Remove-Item -LiteralPath $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$reportPath = Join-Path $OutputDirectory 'mcp-output-size.json'

if ($ServerExecutable) {
    $requestedServerExecutable = [IO.Path]::GetFullPath($ServerExecutable)
    if (!(Test-Path -LiteralPath $requestedServerExecutable -PathType Leaf)) {
        Write-Error "Server executable not found at $requestedServerExecutable."
        exit 1
    }
}

$resolvedServerExecutable = $null

if ($ReleaseTag) {
    # Map the current platform to the asset name pattern used by Pack-Zip.ps1 /
    # New-BuildInfo.ps1, e.g. Azure.Mcp.Server-win-x64.zip, Azure.Mcp.Server-linux-arm64.zip,
    # Azure.Mcp.Server-osx-arm64.zip.
    if ($IsWindows) {
        $releaseOs = 'win'
    } elseif ($IsMacOS) {
        $releaseOs = 'osx'
    } elseif ($IsLinux) {
        $releaseOs = 'linux'
    } else {
        Write-Error "Unable to determine current OS for release asset selection."
        exit 1
    }

    $arch = [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture
    $releaseArch = switch ($arch) {
        'X64' { 'x64' }
        'Arm64' { 'arm64' }
        default {
            Write-Error "Unsupported process architecture '$arch' for release asset selection."
            exit 1
        }
    }

    $assetName = "Azure.Mcp.Server-$releaseOs-$releaseArch.zip"
    $downloadUrl = "https://github.com/$GitHubRepository/releases/download/$ReleaseTag/$assetName"

    $releaseDownloadDir = Join-Path $OutputDirectory "release-download/$ReleaseTag"
    $releaseExtractDir = Join-Path $releaseDownloadDir 'extracted'
    $releaseZipPath = Join-Path $releaseDownloadDir $assetName

    New-Item -ItemType Directory -Path $releaseDownloadDir -Force | Out-Null

    Write-Host "Downloading $assetName from release $ReleaseTag ($GitHubRepository)..."
    Write-Host "  $downloadUrl"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $releaseZipPath

    Write-Host "Extracting $releaseZipPath..."
    if (Test-Path -LiteralPath $releaseExtractDir) {
        Remove-Item -LiteralPath $releaseExtractDir -Recurse -Force
    }
    Expand-Archive -Path $releaseZipPath -DestinationPath $releaseExtractDir -Force

    $resolvedServerExecutable = Join-Path $releaseExtractDir "azmcp$(if ($IsWindows) { '.exe' } else { '' })"
    if (!(Test-Path -LiteralPath $resolvedServerExecutable -PathType Leaf)) {
        Write-Error "azmcp executable not found in downloaded release asset at $resolvedServerExecutable."
        exit 1
    }
    if (!$IsWindows) {
        chmod +x $resolvedServerExecutable
    }
}

if (-not $resolvedServerExecutable -and $requestedServerExecutable) {
    $resolvedServerExecutable = $requestedServerExecutable
}

$usingExternalServerBinary = [bool]$resolvedServerExecutable

if ($SkipBuild) {
    Write-Host "Skipping build."
} else {
    if ($usingExternalServerBinary) {
        Write-Host "Using pre-built server executable $resolvedServerExecutable; skipping local server build."
    } else {
        Write-Host "Building $serverProject ($Configuration)..."
        dotnet build $serverProject --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE."
            exit $LASTEXITCODE
        }
    }

    Write-Host "Building $measurerProject ($Configuration)..."
    dotnet build $measurerProject --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

if ($usingExternalServerBinary) {
    $serverExecutablePath = $resolvedServerExecutable
} else {
    $serverExecutablePath = Join-Path $serverProject "bin/$Configuration/net10.0/azmcp$(if ($IsWindows) { '.exe' } else { '' })"
    if (!(Test-Path -LiteralPath $serverExecutablePath -PathType Leaf)) {
        Write-Error "Server executable not found at $serverExecutablePath. Run without -SkipBuild to build it first."
        exit 1
    }
}

$measurerExecutable = Join-Path $measurerProject "bin/$Configuration/net10.0/McpOutputSizeMeasurer$(if ($IsWindows) { '.exe' } else { '' })"
if (!(Test-Path -LiteralPath $measurerExecutable -PathType Leaf)) {
    Write-Error "Measurer executable not found at $measurerExecutable. Run without -SkipBuild to build it first."
    exit 1
}

Write-Host "Running MCP output size measurement..."
& $measurerExecutable --executable $serverExecutablePath --report $reportPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Measurement failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}

if (!(Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    Write-Error "Measurement report was not produced at $reportPath."
    exit 1
}

Write-Host "Summarizing results..."
Invoke-McpOutputSizeSummary `
    -InputPath $reportPath `
    -OutputPath (Join-Path $OutputDirectory 'mcp-output-size-summary.json') `
    -MarkdownPath (Join-Path $OutputDirectory 'mcp-output-size-summary.md') `
    -LearnResponseThresholdUtf8Bytes $LearnResponseThresholdUtf8Bytes

Write-Host ""
Write-Host "Done. Results are in $OutputDirectory"
