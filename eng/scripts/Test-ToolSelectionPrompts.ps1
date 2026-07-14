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
#>
[CmdletBinding()]
param (
    # Common Parameters
    [string]$OutputPath,
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

function Get-PlatformName {
    [string]$fullPlatform = ""

    if ($IsWindows) {
        $fullPlatform = "windows"
    } elseif ($IsLinux) {
        $fullPlatform = "linux"
    } elseif ($IsMacOS) {
        $fullPlatform = "macos"
    } else {
        throw "Unsupported platform"
    }

    $currentArch = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier.Split('-')[1]
    $fullPlatform += "-$currentArch"

    return $fullPlatform
}

# Start of script
if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

# Use the build infrastructure - New-BuildInfo.ps1 and Build-Code.ps1
$buildInfoPath = "$RepoRoot/.work/build_info.json"
$buildOutputPath = "$RepoRoot/.work/build"


if ($ServerName) {
    Write-Host "Validating tool name length for $ServerName"
} else {
    Write-Host "Validating tool name length for all servers"
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

# Build the servers
$platformName = Get-PlatformName
& "$RepoRoot/eng/scripts/Build-Code.ps1" -BuildInfoPath $buildInfoPath -PlatformName $platformName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build servers."
    exit 1
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
        continue
    }
    else {
        Write-Host "Using prompts file: $serverPromptsFile"
    }

    # Get the executable name and find the built platform
    $executableName = $serverInfo.cliName + $(if ($IsWindows) { ".exe" } else { "" })

    # Find the first platform that was actually built
    $builtPlatform = $serverInfo.platforms | Where-Object { 
        Test-Path "$buildOutputPath/$($_.artifactPath)" 
    } | Select-Object -First 1

    if (-not $builtPlatform) {
        Write-Warning "No built platform found for $currentServerName - skipping tool prompt validation"
        continue
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
        continue
    }

    if ([string]::IsNullOrWhiteSpace($toolsJson)) {
        Write-Warning "No output received from '$currentServerName tools list --name-only' - skipping"
        continue
    }

    $toolsResult = $toolsJson | ConvertFrom-Json
    $tools = $toolsResult.results

    if ($null -eq $tools) {
        Write-Warning "Server [$currentServerName] 'tools list' command did not return any tools - skipping"
        continue
    } elseif ($null -eq $tools.names) {
        Write-Warning "Server [$currentServerName] No 'names' property found in response - skipping. Response: `n$toolsJson`n"
        continue
    } elseif ($tools.names.Count -eq 0) {
        Write-Warning "Server [$currentServerName] No tool names found - skipping"
        continue
    }

    Write-Host "Loaded $($tools.names.Count) tools"

    
    $allPrompts = Read-PromptFile -PromptsFile $serverPromptsFile
    $violations = [System.Collections.Generic.List[Prompt]]::new()

    foreach ($prompt in $allPrompts) {
        if ($tools.names -notcontains $prompt.ToolName) {
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

if ($violationsCount -eq 0) {
    Write-Host "All tested servers passed validation!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "Validation failed - see violations above" -ForegroundColor Red
    exit 1
}