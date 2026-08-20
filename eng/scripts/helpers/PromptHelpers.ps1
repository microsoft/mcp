<#
.SYNOPSIS
Provides helpers for reading MCP end-to-end test prompts and identifying the current platform.

.DESCRIPTION
Dot-source this file from another PowerShell script to use the Prompt model,
Read-PromptFile, and Get-McpServerToolNames.

.EXAMPLE
. "$PSScriptRoot/helpers/PromptHelpers.ps1"

$prompts = Read-PromptFile -PromptsFile "./servers/Azure.Mcp.Server/docs/e2eTestPrompts.md"
$platformName = Get-PlatformName
#>

# Represents one prompt parsed from a tool-area table in an end-to-end prompts file.
class Prompt {
    # Markdown section heading that contains the prompt.
    [string] $ToolArea

    # Tool command from the first table column.
    [string] $ToolName

    # User prompt text from the second table column.
    [string] $Prompt

    # Interaction type from the third table column.
    [string] $Interaction
}

<#
.SYNOPSIS
Parses an end-to-end test prompts Markdown file into structured Prompt objects.

.DESCRIPTION
Reads the file in document order. A level-two Markdown heading (`##`) starts a
tool area, and each subsequent table data row produces one Prompt object. Table
headers, separator rows, and content outside a tool-area section are ignored.

Expected Markdown structure:

    ## <ToolArea>

    | Tool Name | Test Prompt | Interaction |
    |:----------|:------------|:------------|
    | <tool_name> | <prompt text> | <interaction>

.PARAMETER PromptsFile
Path to the Markdown file to parse. Must exist and follow the expected format.

.OUTPUTS
System.Collections.Generic.List[Prompt]
A list of Prompt objects, one per table data row in the file, in document order.

.EXAMPLE
$prompts = Read-PromptFile -PromptsFile "./servers/Azure.Mcp.Server/docs/e2eTestPrompts.md"
$prompts | Where-Object ToolArea -eq "Storage"

Reads the Azure MCP Server prompt file and selects prompts from its Storage
section.

.NOTES
The parser reads the tool name, prompt text, and interaction value from the
first three table columns. It does not validate tool names.
#>
function Read-PromptFile {
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

        # Parse table data rows: | ToolName | Prompt | Interaction |
        # Interaction is optional to preserve compatibility with older two-column tables.
        if ($line -match '^\|\s*(.+?)\s*\|\s*(.+?)\s*\|\s*(.*?)\s*(?:\||$)') {
            $entry = [Prompt]::new()
            $entry.ToolArea = $currentArea
            $entry.ToolName = $Matches[1].Trim()
            $entry.Prompt   = $Matches[2].Trim()
            $entry.Interaction = $Matches[3].Trim()
            $prompts.Add($entry)
        }
    }

    return $prompts
}

<#
.SYNOPSIS
Gets MCP tool names from a built server described by build_info metadata.

.DESCRIPTION
Resolves the current platform's built server executable from the provided
`serverInfo` object, executes `tools list --name-only`, and returns the tool
names from the response.

When a server cannot be queried for tools (for example, no built platform,
empty output, unsupported command, or missing `results.names`), this function
emits a warning and returns `$null` so callers can skip that server.

Throws when the expected executable path for a discovered built platform does
not exist.

.PARAMETER ServerInfo
Server metadata object from `build_info.json` (for example an entry from
`$buildInfo.servers`) with `name`, `cliName`, and `platforms` properties.

.PARAMETER OutputPath
Root build output directory containing server artifacts (for example
`$RepoRoot/.work/build`).

.OUTPUTS
System.String[]
Array of MCP tool names from `results.names`.

System.Object
`$null` when the server should be skipped.

.EXAMPLE
$toolNames = Get-McpServerToolNames -ServerInfo $serverInfo -OutputPath "$RepoRoot/.work/build"
if ($null -eq $toolNames) { return }

Loads tool names for one server from local build artifacts and skips gracefully
when tool listing is unavailable.
#>
function Get-McpServerToolNames {
    param(
        [Parameter(Mandatory)]
        [hashtable] $ServerInfo,

        [Parameter(Mandatory)]
        [string] $OutputPath
    )

    $currentServerName = $ServerInfo.name

    # Get the executable name and find the built platform
    $executableName = $ServerInfo.cliName + $(if ($IsWindows) { ".exe" } else { "" })

    # Find the first platform that was actually built
    $builtPlatform = $ServerInfo.platforms | Where-Object {
        Test-Path "$OutputPath/$($_.artifactPath)"
    } | Select-Object -First 1

    if (-not $builtPlatform) {
        Write-Warning "No built platform found for $currentServerName - skipping tool prompt validation"
        return $null
    }

    $executablePath = "$OutputPath/$($builtPlatform.artifactPath)/$executableName"

    if (-not (Test-Path $executablePath)) {
        throw "Executable not found at $executablePath for $currentServerName"
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
    #        "acr_registry_repository_list",
    #     ]
    #   }
    # }
    $toolsJson = & $executablePath tools list --name-only 2>&1 | Out-String

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "$currentServerName 'tools list' command failed with exit code $LASTEXITCODE (may have no tools) - skipping"
        return $null
    }

    if ([string]::IsNullOrWhiteSpace($toolsJson)) {
        Write-Warning "No output received from '$currentServerName tools list --name-only' - skipping"
        return $null
    }

    $toolsResult = $toolsJson | ConvertFrom-Json
    $tools = $toolsResult.results

    if ($null -eq $tools) {
        Write-Warning "Server [$currentServerName] 'tools list' command did not return any tools - skipping"
        return $null
    }

    if ($null -eq $tools.names) {
        Write-Warning "Server [$currentServerName] No 'names' property found in response - skipping. Response: `n$toolsJson`n"
        return $null
    }

    if ($tools.names.Count -eq 0) {
        Write-Warning "Server [$currentServerName] No tool names found - skipping"
        return $null
    }

    return @($tools.names)
}