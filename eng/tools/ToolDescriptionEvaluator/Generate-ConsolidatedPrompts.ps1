<#
.SYNOPSIS
Generates a consolidated prompts JSON file that maps each consolidated Azure tool name to an aggregated list of natural language prompts.

.DESCRIPTION
Reads:
  - consolidated-tools.json (contains consolidated_azure_tools array; each tool has an available_commands list)
  - tools.json (optional for future enrichment; currently only used to validate command presence)
  - prompts.json (maps individual command keys to prompt examples; e.g. command "azmcp acr registry list" => key "azmcp_acr_registry_list")

For every consolidated tool, the script resolves its available command strings, converts each to the prompts.json key format, pulls the associated prompts, merges them (deduped, sorted), and outputs a JSON file:
{
  "<consolidated_tool_name>": [ "prompt1", "prompt2", ... ],
  ...
}

.PARAMETER ConsolidatedToolsPath
Path to consolidated-tools.json.

.PARAMETER PromptsPath
Path to prompts.json containing per-command prompt arrays.

.PARAMETER ToolsPath
Path to tools.json (optional; used for validation only).

.PARAMETER OutputPath
Destination path for generated consolidated-prompts.json.

.PARAMETER Force
Overwrite output file if it exists.

.PARAMETER VerboseWarnings
Emit detailed warnings for unmatched commands.

.EXAMPLE
pwsh ./Generate-ConsolidatedPrompts.ps1 -OutputPath consolidated-prompts.json -Verbose

.NOTES
Idempotent. Safe to re-run. Designed to be executed from repo root or script directory.
#>
param(
    [string]$ConsolidatedToolsPath = "./consolidated-tools.json",
    [string]$PromptsPath = "./prompts.json",
    [string]$ToolsPath = "./tools.json",
    [string]$OutputPath = "./consolidated-prompts.json",
    [switch]$Force,
    [switch]$VerboseWarnings
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Write-Log {
    param([string]$Message, [string]$Level = 'INFO')
    $ts = (Get-Date).ToString('u')
    Write-Host "[$ts][$Level] $Message"
}

function Convert-CommandToPromptKey {
    param([string]$Command)
    if ([string]::IsNullOrWhiteSpace($Command)) { return $null }
    $normalized = ($Command -replace '\s+', ' ').Trim()
    # Some consolidated tool command entries appear to already be identifier-like strings with prefixes such as
    # 'mcp_azure-mcp-ser_' followed by what would normally be the prompt key. We strip that transport / provenance prefix.
    $normalized = $normalized -replace '^mcp_azure-mcp-ser_', ''
    # Replace spaces with underscores
    $key = ($normalized -replace ' ', '_')
    # Normalize any leftover dashes that should be underscores for prompt keys (e.g., usersession-list vs usersession_list)
    # Keep dashes inside azure command segments that are legitimate (firewall-rule) by also trying a secondary variant.
    return $key
}

if (-not (Test-Path $ConsolidatedToolsPath)) { throw "Consolidated tools file not found: $ConsolidatedToolsPath" }
if (-not (Test-Path $PromptsPath))          { throw "Prompts file not found: $PromptsPath" }
if (-not (Test-Path $ToolsPath))            { Write-Log "Tools file not found ($ToolsPath) - continuing without validation" 'WARN' }

Write-Log "Loading JSON inputs" 'INFO'
# Note: -Depth parameter is not available in Windows PowerShell 5.1 ConvertFrom-Json; omitted for compatibility.
$consolidatedJson = Get-Content -Raw -Path $ConsolidatedToolsPath | ConvertFrom-Json
$promptsJson       = Get-Content -Raw -Path $PromptsPath | ConvertFrom-Json
$toolsJson         = if (Test-Path $ToolsPath) { Get-Content -Raw -Path $ToolsPath | ConvertFrom-Json } else { $null }

if (-not $consolidatedJson.consolidated_azure_tools) { throw "Input consolidated tools JSON missing 'consolidated_azure_tools' array" }

$allPromptKeys = @($promptsJson.PSObject.Properties.Name)
$toolCommandSet = New-Object System.Collections.Generic.HashSet[string]
if ($toolsJson -and $toolsJson.results) {
    foreach ($t in $toolsJson.results) {
        if ($t.command) { [void]$toolCommandSet.Add(($t.command -replace '\s+', ' ').Trim()) }
    }
}

$outputMap = [ordered]@{}
$warnings = @()

foreach ($tool in $consolidatedJson.consolidated_azure_tools) {
    if (-not $tool.name) { continue }
    $toolName = $tool.name
    $promptsAggregated = New-Object System.Collections.Generic.HashSet[string]

    $available = @()
    if ($tool.available_commands) { $available = $tool.available_commands }

    foreach ($cmdEntry in $available) {
        $commandString = $null
        if ($cmdEntry -is [string]) { $commandString = $cmdEntry }
        elseif ($cmdEntry -and ($cmdEntry.PSObject.Properties.Name -contains 'command')) { $commandString = $cmdEntry.command }
        elseif ($cmdEntry -and ($cmdEntry.PSObject.Properties.Name -contains 'name')) { $commandString = $cmdEntry.name }
        if (-not $commandString) { continue }

        $promptKey = Convert-CommandToPromptKey -Command $commandString
        if (-not $promptKey) { continue }

        $candidateKeys = @($promptKey)
        # Additional heuristics: attempt dash->underscore and underscore->dash variations
        if ($promptKey -match '-') { $candidateKeys += ($promptKey -replace '-', '_') }
        if ($promptKey -match '_') { $candidateKeys += ($promptKey -replace '_', '-') }
        $matched = $false
        foreach ($ck in ($candidateKeys | Select-Object -Unique)) {
            if ($allPromptKeys -contains $ck) {
                $promptList = $promptsJson.$ck
                foreach ($p in $promptList) { if (-not [string]::IsNullOrWhiteSpace($p)) { [void]$promptsAggregated.Add($p) } }
                $matched = $true
            }
        }
        if (-not $matched) {
            $warnings += "No prompts found for command '$commandString' (tried keys: $([string]::Join(', ',$candidateKeys))) for consolidated tool '$toolName'"
        }
    }

    # Convert to sorted array (HashSet.ToArray() not available in some Windows PowerShell versions)
    $sorted = @($promptsAggregated) | Sort-Object -Unique
    $outputMap[$toolName] = @($sorted)  # ensure plain array (empty => [])
}

if ($warnings.Count -gt 0) {
    Write-Log "Encountered $($warnings.Count) unmatched command(s)." 'WARN'
    if ($VerboseWarnings) {
        foreach ($w in $warnings) { Write-Log $w 'WARN' }
    }
}

$jsonOutput = $outputMap | ConvertTo-Json -Depth 100

if ((Test-Path $OutputPath) -and -not $Force) {
    throw "Output file already exists: $OutputPath (use -Force to overwrite)"
}

Write-Log "Writing consolidated prompts to $OutputPath" 'INFO'
$jsonOutput | Out-File -FilePath $OutputPath -Encoding UTF8 -NoNewline

Write-Log "Done. Consolidated tools processed: $($outputMap.Keys.Count)." 'INFO'
if ($warnings.Count -gt 0) { Write-Log "See warnings above for missing mappings." 'WARN' }
