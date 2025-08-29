#!/bin/env pwsh
#Requires -Version 7

# Defines shared constants used by both Analyze-AOT-Compact.ps1 and Render-AOT-Analysis-Result.ps1

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$script:AOTConfig = @{
    # Base paths
    RootPath = $RepoRoot
    ProjectFile = "$RepoRoot/servers/Azure.Mcp.Server/src/Azure.Mcp.Server.csproj"

    # AOT report directories and files
    ReportDirectory = "$RepoRoot/.work/aotCompactReport"
    RawReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.txt"
    JsonReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.json"
    HtmlReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.html"
}

function Get-AOTConfig {
    return $script:AOTConfig
}
