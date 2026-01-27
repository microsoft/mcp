#!/bin/env pwsh
#Requires -Version 7

# Defines shared constants used by both Analyze-AOT-Compact.ps1 and Render-AOT-Analysis-Result.ps1

param(
    [string]$ServerName = "Azure.Mcp.Server"
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$script:AOTConfig = @{
    # Base paths
    RootPath = $RepoRoot
    ServerName = $ServerName
    ProjectFile = "$RepoRoot/servers/$ServerName/src/$ServerName.csproj"

    # AOT report directories and files
    ReportDirectory = "$RepoRoot/.work/aotCompactReport"
    RawReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.txt"
    JsonReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.json"
    HtmlReportPath = "$RepoRoot/.work/aotCompactReport/aot-compact-report.html"
}

function Get-AOTConfig {
    param(
        [string]$ServerName = "Azure.Mcp.Server"
    )
    
    if ($ServerName -ne $script:AOTConfig.ServerName) {
        # Update config if server name changed
        $script:AOTConfig.ServerName = $ServerName
        $script:AOTConfig.ProjectFile = "$($script:AOTConfig.RootPath)/servers/$ServerName/src/$ServerName.csproj"
    }
    
    return $script:AOTConfig
}
