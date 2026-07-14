[CmdletBinding()]
param (
    # Common Parameters
    [string]$OutputPath,
    [Mandatory()]
    [string]$ServerName,
    [string]$PromptsFile
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/helpers/BuildHelpers.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

class Prompt {
    [string] $ToolArea
    [string] $ToolName
    [string] $Prompt
}

function Read-PromptFile {
    <#
    .SYNOPSIS
        Parses an end-to-end test prompts Markdown file into structured Prompt objects.

        Expected Markdown structure:

            ## <ToolArea>

            | Tool Name | Test Prompt |
            |:----------|:----------|
            | <tool_name> | <prompt text> |

    .PARAMETER PromptsFile
        Path to the Markdown file to parse. Must exist and follow the expected format.

    .OUTPUTS
        System.Collections.Generic.List[Prompt]
        A list of Prompt objects, one per table data row in the file, in document order.
    #>
    param(
        [Parameter(Mandatory)]
        [string] $PromptsFile
    )

    $prompts = [System.Collections.Generic.List[Prompt]]::new()
    $currentArea = $null

    foreach ($line in [System.IO.File]::ReadLines($PromptsFile)) {
        # Match ## section headings as ToolArea
        if ($line -match '^##\s+(.+)$') {
            $currentArea = $Matches[1].Trim()
            continue
        }

        # Skip lines that are not inside a section, are table headers, or are separator rows
        if (-not $currentArea) { continue }
        if ($line -notmatch '^\|') { continue }
        if ($line -match '^\|\s*Tool Name\s*\|') { continue }
        if ($line -match '^\|[-:\s|]+$') { continue }

        # Parse table data rows: | ToolName | Prompt |
        if ($line -match '^\|\s*(.+?)\s*\|\s*(.+?)\s*\|') {
            $entry = [Prompt]::new()
            $entry.ToolArea = $currentArea
            $entry.ToolName = $Matches[1].Trim()
            $entry.Prompt   = $Matches[2].Trim()
            $prompts.Add($entry)
        }
    }

    return $prompts
}

# Start of script
if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

# Use the build infrastructure - New-BuildInfo.ps1 and Build-Code.ps1
$buildInfoPath = "$RepoRoot/.work/build_info.json"
$buildOutputPath = "$RepoRoot/.work/build"

if (!$PromptsFile) {
    $PromptsFile = Join-Path $RepoRoot "servers" $ServerName "docs" "e2eTestPrompts.md"
}

if (!(Test-Path $PromptsFile)) {
    Write-Information "Prompts file not found: $PromptsFile - skipping prompt validation"
    return
}

# Clean up previous build artifacts
Remove-Item -Path $buildOutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

# Create build metadata
& "$RepoRoot/eng/scripts/New-BuildInfo.ps1" `
    -ServerName $ServerName `
    -PublishTarget none `
    -BuildId 12345

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create build info"
    exit 1
}

# Build the server
& "$RepoRoot/eng/scripts/Build-Code.ps1" -ServerName $ServerName -OutputPath $buildOutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build $ServerName"
    exit 1
}

# Read build_info.json to get server information
$buildInfo = Get-Content $buildInfoPath -Raw | ConvertFrom-Json -AsHashtable

# Get servers to test
$serverInfo = $buildInfo.servers | Where-Object { $_.name -eq $ServerName } | Select-Object -First 1
if (-not $serverInfo) {
    Write-Error "No servers found in build_info.json"
    exit 1
}

$currentServerName = $serverInfo.name
Write-Host "=================================================="
Write-Host "Testing: $currentServerName"
Write-Host "=================================================="

# Get the executable name and find the built platform
$executableName = $serverInfo.cliName + $(if ($IsWindows) { ".exe" } else { "" })

# Find the first platform that was actually built
$builtPlatform = $serverInfo.platforms | Where-Object { 
    Test-Path "$buildOutputPath/$($_.artifactPath)" 
} | Select-Object -First 1

if (-not $builtPlatform) {
    Write-Warning "No built platform found for $currentServerName - aborting tool prompt validation"
    exit 1
}

$executablePath = "$buildOutputPath/$($builtPlatform.artifactPath)/$executableName"

if (-not (Test-Path $executablePath)) {
    Write-Error "Executable not found at $executablePath for $currentServerName"
    exit 1
}

# Try to get tools - some servers may not support 'tools list'
Write-Host "Loading tools from $currentServerName"

# Example response from 'tools list --name-only' command:
# {
#   "status": 200,
#   "message": "Success",
#   "results": {
#     "names": [ 
#        "acr_registry_list",
#         "acr_registry_repository_list",
#     ]
#   }
# }
$toolsJson = & $executablePath tools list --name-only 2>&1 | Out-String

if ($LASTEXITCODE -ne 0) {
    Write-Warning "$currentServerName 'tools list' command failed with exit code $LASTEXITCODE (may have no tools) - skipping"
    return
}

if ([string]::IsNullOrWhiteSpace($toolsJson)) {
    Write-Warning "No output received from '$currentServerName tools list --name-only' - skipping"
    $skippedServers++
    return
}

$toolsResult = $toolsJson | ConvertFrom-Json
$tools = $toolsResult.results

if ($null -eq $tools) {
    Write-Warning "Server [$currentServerName] 'tools list' command did not return any tools - skipping"
    return
} elseif ($null -eq $tools.names) {
    Write-Warning "Server [$currentServerName] No 'names' property found in response - skipping. Response: `n$toolsJson`n"
    return
} elseif ($tools.names.Count -eq 0) {
    Write-Warning "Server [$currentServerName] No tool names found - skipping"
    return
}

Write-Host "Loaded $($tools.names.Count) tools"

if (Test-Path $PromptsFile) {
    Write-Host "Using prompts file: $PromptsFile"
} else {
    Write-Warning "Prompts file not found: $PromptsFile - skipping prompt validation"
    return
}

$allPrompts = Read-PromptFile -PromptsFile $PromptsFile
$violations = [System.Collections.Generic.List[Prompt]]::new()

foreach ($prompt in $allPrompts) {
    if ($tools.names -notcontains $prompt.ToolName) {
        $violations.Add($prompt)
    }
}

if ($violations.Count -eq 0) {
    Write-Host "All E2E prompts are valid!" -ForegroundColor Green
}
else {
    Write-Host "Found $($violations.Count) violation(s).  The following prompts have tool names that do not exist:" -ForegroundColor Red
    $violations | ForEach-Object {
        Write-Host "[$($_.ToolArea)]`t$($_.ToolName):`t$($_.Prompt)" -ForegroundColor Red
    }
}

Write-Host ""