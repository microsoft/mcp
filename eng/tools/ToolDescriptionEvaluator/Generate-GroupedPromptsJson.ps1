<#
.SYNOPSIS
Generates prompts JSON files for consolidated tools, namespace tools, or both.

.DESCRIPTION
Modes:
    Consolidated - Produces consolidated-prompts.json mapping each consolidated tool name to aggregated prompts.
    Namespace    - Produces namespace-prompts.json mapping each namespace to aggregated prompts from all descendant subcommands. Keys are prefixed with 'azmcp_' for consistency with per-command prompt keys.
    Both         - Produces both outputs in a single run.

Reads:
    - consolidated-tools.json (contains consolidated_azure_tools array; each tool has an available_commands list)
    - namespace-tools.json (contains top-level namespaces with recursive subcommands)
    - tools.json (optional for future enrichment / validation)
    - prompts.json (maps individual command keys to prompt examples; e.g. command "azmcp acr registry list" => key "azmcp_acr_registry_list")

For every entity (consolidated tool name OR namespace name) the script:
    1. Resolves its underlying command strings
    2. Converts each to the prompts.json key format (spaces -> underscores, provenance prefix stripped)
    3. Collects & de-duplicates prompts
    4. Emits ordered JSON.

.PARAMETER Mode
Which generation mode to run (required): Consolidated | Namespace | Both.

.PARAMETER ConsolidatedToolsPath
Path to consolidated-tools.json.

.PARAMETER NamespaceToolsPath
Path to namespace-tools.json.

.PARAMETER PromptsPath
Path to prompts.json containing per-command prompt arrays.

.PARAMETER ToolsPath
Path to tools.json (optional; used for validation only).

.PARAMETER OutputPath
Destination path for generated prompts JSON.

.PARAMETER Force
Overwrite existing output file(s).

.PARAMETER VerboseWarnings
Emit detailed warnings for unmatched commands.

.EXAMPLES
pwsh ./Generate-ConsolidatedPrompts.ps1 -Mode Consolidated                              # Consolidated only
pwsh ./Generate-ConsolidatedPrompts.ps1 -Mode Namespace                                 # Namespace only
pwsh ./Generate-ConsolidatedPrompts.ps1 -Mode Both                                      # Both files, overwrite if exist
pwsh ./Generate-ConsolidatedPrompts.ps1 -Mode Namespace -NamespaceToolsPath ./namespace-tools.json -OutputPath ./namespace-prompts.json

.NOTES
Idempotent. Safe to re-run. Designed to be executed from repo root or script directory.
#>
param(
    [Parameter(Mandatory)][ValidateSet('Consolidated','Namespace','Both')][string]$Mode,
    [string]$ConsolidatedToolsPath = "./consolidated-tools.json",
    [string]$NamespaceToolsPath = "./namespace-tools.json",
    [string]$PromptsPath = "./prompts.json",
    [string]$ToolsPath = "./tools.json",
    [string]$OutputPath,
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

# Shared helper: given a set of command strings, aggregate matching prompts
function Resolve-PromptsForCommands {
    param(
        [Parameter(Mandatory)][string[]]$CommandStrings,
        [Parameter(Mandatory)]$PromptsJson,
        [Parameter(Mandatory)][string[]]$AllPromptKeys,
        [switch]$VerboseWarnings,
        [ref]$Warnings
    )

    $aggregated = New-Object System.Collections.Generic.HashSet[string]
    $localUnmatched = @()
    foreach ($commandString in $CommandStrings) {
        if ([string]::IsNullOrWhiteSpace($commandString)) { continue }
        $promptKey = Convert-CommandToPromptKey -Command $commandString
        if (-not $promptKey) { continue }
        $candidateKeys = @($promptKey)
        if ($promptKey -match '-') { $candidateKeys += ($promptKey -replace '-', '_') }
        if ($promptKey -match '_') { $candidateKeys += ($promptKey -replace '_', '-') }
        $matched = $false
        foreach ($ck in ($candidateKeys | Select-Object -Unique)) {
            if ($AllPromptKeys -contains $ck) {
                $list = $PromptsJson.$ck
                foreach ($p in $list) { if (-not [string]::IsNullOrWhiteSpace($p)) { [void]$aggregated.Add($p) } }
                $matched = $true
            }
        }
        if (-not $matched) { $localUnmatched += "No prompts found for command '$commandString' (tried keys: $([string]::Join(', ',$candidateKeys)))" }
    }
    $sorted = @(@($aggregated) | Sort-Object -Unique)
    if ($sorted.Count -eq 0 -and $localUnmatched.Count -gt 0) {
        # Only record unmatched details if the entity produced zero prompts overall.
        foreach ($w in $localUnmatched) { $Warnings.Value += $w }
    }
    return $sorted
}

<#
    NOTE: Namespace tools JSON is now a FLAT list where each element is either:
      1. A top-level namespace entry:   { name, command: "azmcp <ns>" }
      2. A surfaced leaf command entry: { name, command: "azmcp extension <leaf>" }

    Previously the structure contained recursive subcommands; the old recursive walker is retained
    only for backwards compatibility scenarios (not currently invoked). The new logic derives the
    leaf command set for a namespace by scanning all prompt keys with the prefix 'azmcp_<ns>_'.
    Surfaced leaf commands simply aggregate their own prompts.
#>
function Get-NamespaceCommandStrings {
    param(
        [Parameter(Mandatory)] $Node,
        [Parameter(Mandatory)] [string[]] $AllPromptKeys
    )

    $acc = @()
    if ($null -eq $Node -or -not $Node.command) { return $acc }
    $cmd = ($Node.command -replace '\s+', ' ').Trim()

    # Pattern: azmcp <namespace>
    if ($cmd -match '^(azmcp)\s+([^\s]+)$') {
        $ns = $Matches[2]
        # Collect all prompt keys beginning with azmcp_<ns>_ (leaf commands)
        $prefix = "azmcp_${ns}_"
        $matchingLeafKeys = $AllPromptKeys | Where-Object { $_.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase) }
        foreach ($k in $matchingLeafKeys) {
            # Convert prompt key back to command string: underscores -> spaces
            $acc += ($k -replace '_', ' ')
        }
        return ($acc | Sort-Object -Unique)
    }

    # Surfaced leaf (e.g., azmcp extension azqr) – aggregate only itself
    $acc += $cmd
    return $acc
}

function Convert-PromptKeyToCommand {
    param([Parameter(Mandatory)][string]$Key)
    if ([string]::IsNullOrWhiteSpace($Key)) { return $null }
    return ($Key -replace '_', ' ') -replace '\s+', ' '
}

function Invoke-ConsolidatedGeneration {
    param(
        [string]$ConsolidatedToolsPath,
        [string]$PromptsPath,
        [string]$ToolsPath,
        [string]$OutputPath,
        $PromptsJson,
        [string[]]$AllPromptKeys,
        [switch]$Force,
        [switch]$VerboseWarnings
    )

    if (-not (Test-Path $ConsolidatedToolsPath)) { throw "Consolidated tools file not found: $ConsolidatedToolsPath" }
    $consolidatedJson = Get-Content -Raw -Path $ConsolidatedToolsPath | ConvertFrom-Json
    if (-not $consolidatedJson.consolidated_azure_tools) { throw "Input consolidated tools JSON missing 'consolidated_azure_tools' array" }

    $warnings = @()
    $outputMap = [ordered]@{}
    foreach ($tool in $consolidatedJson.consolidated_azure_tools) {
        if (-not $tool.name) { continue }
        $toolName = $tool.name
        $available = @()
        if ($tool.available_commands) { $available = $tool.available_commands }

        $commandStrings = @()
        foreach ($cmdEntry in $available) {
            $commandString = $null
            if ($cmdEntry -is [string]) { $commandString = $cmdEntry }
            elseif ($cmdEntry -and ($cmdEntry.PSObject.Properties.Name -contains 'command')) { $commandString = $cmdEntry.command }
            elseif ($cmdEntry -and ($cmdEntry.PSObject.Properties.Name -contains 'name')) { $commandString = $cmdEntry.name }
            if ($commandString) { $commandStrings += $commandString }
        }
        $resolved = Resolve-PromptsForCommands -CommandStrings $commandStrings -PromptsJson $PromptsJson -AllPromptKeys $AllPromptKeys -Warnings ([ref]$warnings) -VerboseWarnings:$VerboseWarnings
        $outputMap[$toolName] = @($resolved)
    }

    $jsonOutput = $outputMap | ConvertTo-Json -Depth 100
    if ((Test-Path $OutputPath) -and -not $Force) { throw "Output file already exists: $OutputPath (use -Force to overwrite)" }
    Write-Log "Writing consolidated prompts to $OutputPath" 'INFO'
    $jsonOutput | Out-File -FilePath $OutputPath -Encoding UTF8 -NoNewline
    Write-Log "Consolidated prompts written. Count: $($outputMap.Keys.Count)" 'INFO'
    return ,$warnings
}

function Invoke-NamespaceGeneration {
    param(
        [string]$NamespaceToolsPath,
        [string]$OutputPath,
        $PromptsJson,
        [string[]]$AllPromptKeys,
        [switch]$Force,
        [switch]$VerboseWarnings
    )

    if (-not (Test-Path $NamespaceToolsPath)) { throw "Namespace tools file not found: $NamespaceToolsPath" }
    $namespaceJson = Get-Content -Raw -Path $NamespaceToolsPath | ConvertFrom-Json
    if (-not $namespaceJson.results) { throw "Input namespace tools JSON missing 'results' array" }

    $warnings = @()
    $outputMap = [ordered]@{}
    foreach ($ns in $namespaceJson.results) {
    if (-not $ns.name) { continue }

        $commandStrings = Get-NamespaceCommandStrings -Node $ns -AllPromptKeys $AllPromptKeys

        # Determine aggregation key: namespace entries get 'azmcp_<ns>', surfaced leaf commands keep their converted key
        $isNamespace = $ns.command -match '^(azmcp)\s+([^\s]+)$'
        if ($isNamespace) {
            $nsName = $Matches[2]
            switch ($nsName) { 'azqr' { $nsName = 'extension_azqr'; break } }
            if (-not $nsName.StartsWith('azmcp_')) { $nsName = 'azmcp_' + $nsName }
            $resolved = Resolve-PromptsForCommands -CommandStrings $commandStrings -PromptsJson $PromptsJson -AllPromptKeys $AllPromptKeys -Warnings ([ref]$warnings) -VerboseWarnings:$VerboseWarnings
            $outputMap[$nsName] = @($resolved)
        }
        else {
            # Surfaced leaf command: just map its own prompts under its prompt key
            $key = Convert-CommandToPromptKey -Command $ns.command
            $resolved = Resolve-PromptsForCommands -CommandStrings @($ns.command) -PromptsJson $PromptsJson -AllPromptKeys $AllPromptKeys -Warnings ([ref]$warnings) -VerboseWarnings:$VerboseWarnings
            $outputMap[$key] = @($resolved)
        }
    }

    $jsonOutput = $outputMap | ConvertTo-Json -Depth 100
    if ((Test-Path $OutputPath) -and -not $Force) { throw "Output file already exists: $OutputPath (use -Force to overwrite)" }
    Write-Log "Writing namespace prompts to $OutputPath" 'INFO'
    $jsonOutput | Out-File -FilePath $OutputPath -Encoding UTF8 -NoNewline
    Write-Log "Namespace prompts written. Count: $($outputMap.Keys.Count)" 'INFO'
    return ,$warnings
}

if (-not (Test-Path $PromptsPath)) { throw "Prompts file not found: $PromptsPath" }
if (-not (Test-Path $ToolsPath))   { Write-Log "Tools file not found ($ToolsPath) - continuing without validation" 'WARN' }

Write-Log "Loading prompts JSON" 'INFO'
$promptsJson = Get-Content -Raw -Path $PromptsPath | ConvertFrom-Json
$allPromptKeys = @($promptsJson.PSObject.Properties.Name)

# (Optional) tools.json load removed because it is not currently used; reintroduce if validation logic is added.

$allWarnings = @()

switch ($Mode) {
    'Consolidated' {
        if (-not $OutputPath) { $OutputPath = "./consolidated-prompts.json" }
        $w = Invoke-ConsolidatedGeneration -ConsolidatedToolsPath $ConsolidatedToolsPath -PromptsPath $PromptsPath -ToolsPath $ToolsPath -OutputPath $OutputPath -PromptsJson $promptsJson -AllPromptKeys $allPromptKeys -Force:$Force -VerboseWarnings:$VerboseWarnings
        $allWarnings += $w
    }
    'Namespace' {
        if (-not $OutputPath) { $OutputPath = "./namespace-prompts.json" }
        $w = Invoke-NamespaceGeneration -NamespaceToolsPath $NamespaceToolsPath -OutputPath $OutputPath -PromptsJson $promptsJson -AllPromptKeys $allPromptKeys -Force:$Force -VerboseWarnings:$VerboseWarnings
        $allWarnings += $w
    }
    'Both' {
        if (-not $OutputPath) { $OutputPath = "./prompts.json" }

        $baseDir = Split-Path -Path $OutputPath -Parent
        $leaf    = Split-Path -Path $OutputPath -Leaf

        if (-not $baseDir) { $baseDir = '.' }

        $consolidatedPath = Join-Path $baseDir ('consolidated-' + $leaf)
        $namespacePath    = Join-Path $baseDir ('namespace-'    + $leaf)

        $allWarnings += Invoke-ConsolidatedGeneration -ConsolidatedToolsPath $ConsolidatedToolsPath `
            -PromptsPath $PromptsPath -ToolsPath $ToolsPath -OutputPath $consolidatedPath `
            -PromptsJson $promptsJson -AllPromptKeys $allPromptKeys -Force:$Force -VerboseWarnings:$VerboseWarnings

        $allWarnings += Invoke-NamespaceGeneration -NamespaceToolsPath $NamespaceToolsPath `
            -OutputPath $namespacePath -PromptsJson $promptsJson -AllPromptKeys $allPromptKeys `
            -Force:$Force -VerboseWarnings:$VerboseWarnings
    }
}

if ($allWarnings.Count -gt 0) {
    Write-Log "Encountered $($allWarnings.Count) unmatched command(s) across selected mode(s)." 'WARN'
    if ($VerboseWarnings) { foreach ($w in $allWarnings) { Write-Log $w 'WARN' } }
}

Write-Log "Done. Mode(s) completed: $Mode" 'INFO'
if ($allWarnings.Count -gt 0) { Write-Log "See warnings above for missing mappings." 'WARN' }
