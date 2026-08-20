<#
.SYNOPSIS
Validates that tool names used in prompt documentation exist in built MCP servers.

.DESCRIPTION
Builds one or more servers, reads end-to-end prompt markdown, and compares documented
tool names against the server's runtime tool list from `tools list --name-only`.

For each server, this script:
- Locates the prompts file (default: servers/<ServerName>/docs/e2eTestPrompts.md)
- Parses markdown prompt rows into structured entries
- Executes the built server binary to retrieve available tool names
- Reports prompt entries that reference missing tools

The script exits with code 0 when all checked prompts are valid, or 1 when any
violations are found.

.PARAMETER OutputPath
Optional build output directory. Defaults to $RepoRoot/.work/build.

.PARAMETER ServerName
Optional server name to validate. If omitted, all servers from build metadata are validated.

.PARAMETER PromptsFile
Optional path to a markdown prompts file. If omitted, the server default prompts path is used.

.PARAMETER SkipServerBuild
Skips building the server and uses existing build artifacts from OutputPath.

.OUTPUTS
None. Writes validation results to standard output and exits with status code 0 or 1.

.EXAMPLE
./eng/scripts/Test-ToolSelectionPrompts.ps1
Builds and validates all servers using each server's default prompts file.

.EXAMPLE
./eng/scripts/Test-ToolSelectionPrompts.ps1 -ServerName Azure.Mcp.Server
Builds and validates only Azure.Mcp.Server with its default prompts file.

.EXAMPLE
./eng/scripts/Test-ToolSelectionPrompts.ps1 -ServerName Azure.Mcp.Server -PromptsFile ./servers/Azure.Mcp.Server/docs/e2eTestPrompts.md
Validates Azure.Mcp.Server using the provided prompts file.

.EXAMPLE
./eng/scripts/Test-ToolSelectionPrompts.ps1 -ServerName Azure.Mcp.Server -SkipServerBuild
Validates Azure.Mcp.Server using existing build artifacts without rebuilding the server.
#>
[CmdletBinding()]
param (
    # Common Parameters
    [string]$OutputPath,
    [string]$ServerName,
    [string]$PromptsFile,
    [switch]$SkipServerBuild
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/helpers/BuildHelpers.ps1"
. "$PSScriptRoot/helpers/PromptHelpers.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# Start of script
if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

# Use the build infrastructure - New-BuildInfo.ps1 and Build-Code.ps1
$buildInfoPath = "$RepoRoot/.work/build_info.json"

if ($ServerName) {
    Write-Host "Validating tool selection prompts for $ServerName"
} else {
    Write-Host "Validating tool selection prompts for all servers"
}

if ($SkipServerBuild) {
    Write-Host "Skipping server build. Reusing existing build info and existing build artifacts."
} else {
    # Clean up previous build artifacts
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

    # Create build metadata
    & "$RepoRoot/eng/scripts/New-BuildInfo.ps1" `
        -ServerName $ServerName `
        -PublishTarget none `
        -BuildId 12345

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create build info"
        exit 1
    }

    # Build the servers
    $platformName = Get-PlatformName
    & "$RepoRoot/eng/scripts/Build-Code.ps1" -BuildInfoPath $buildInfoPath -PlatformName $platformName -OutputPath $OutputPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build servers."
        exit 1
    }
}

# Read build_info.json to get server information
$buildInfo = Get-Content $buildInfoPath -Raw | ConvertFrom-Json -AsHashtable

# Get servers to test
$serversToTest = $buildInfo.servers
if (-not $serversToTest -or $serversToTest.Count -eq 0) {
    Write-Error "No servers found in build_info.json"
    exit 1
}

Write-Host "Testing $($serversToTest.Count) server(s)"
Write-Host ""

[int]$violationsCount = 0
[int]$testedServers = 0
[int]$skippedServers = 0

foreach ($serverInfo in $serversToTest) {
    $currentServerName = $serverInfo.name
    Write-Host "=================================================="
    Write-Host "Testing: $currentServerName"
    Write-Host "=================================================="

    $serverPromptsFile = $PromptsFile
    if (!$serverPromptsFile) {
        $serverPromptsFile = Join-Path $RepoRoot "servers" $currentServerName "docs" "e2eTestPrompts.md"
    }

    if (!(Test-Path $serverPromptsFile)) {
        Write-Host "Prompts file not found: $serverPromptsFile - skipping prompt validation"
        $skippedServers++
        Write-Host ""
        continue
    }
    else {
        Write-Host "Using prompts file: $serverPromptsFile"
    }

    $toolNames = Get-McpServerToolNames -ServerInfo $serverInfo -OutputPath $OutputPath

    if ($null -eq $toolNames -or $toolNames.Count -eq 0) {
        $skippedServers++
        Write-Host ""
        continue
    }

    Write-Host "Loaded $($toolNames.Count) tools"
    $testedServers++

    $allPrompts = Read-PromptFile -PromptsFile $serverPromptsFile
    $violations = [System.Collections.Generic.List[Prompt]]::new()

    foreach ($prompt in $allPrompts) {
        if ($toolNames -notcontains $prompt.ToolName) {
            $violations.Add($prompt)
        }
    }

    $violationsCount += $violations.Count

    if ($violations.Count -eq 0) {
        Write-Host "All prompts are valid for $currentServerName" -ForegroundColor Green
    }
    else {
        Write-Host "Found $($violations.Count) violation(s).  The following prompts have tool names that do not exist:" -ForegroundColor Red
        $violations | ForEach-Object {
            Write-Host "[$($_.ToolArea)]`t$($_.ToolName):`t$($_.Prompt)" -ForegroundColor Red
        }
    }
}

# Final summary
Write-Host "=================================================="
Write-Host "SUMMARY"
Write-Host "=================================================="
Write-Host "Servers tested: $testedServers"
Write-Host "Servers skipped: $skippedServers"
Write-Host "Total violations: $violationsCount"
Write-Host ""

if ($testedServers -gt 0 -and $violationsCount -eq 0) {
    Write-Host "All tested servers passed validation!" -ForegroundColor Green
    exit 0
}
elseif ($testedServers -eq 0) {
    Write-Error "No servers were successfully tested. All $($skippedServers) server(s) were skipped."
    exit 1
}
else {
    Write-Host "Validation failed - see violations above" -ForegroundColor Red
    exit 1
}
