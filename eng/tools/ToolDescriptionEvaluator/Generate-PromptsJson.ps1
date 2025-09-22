<#!
.SYNOPSIS
Generates a prompts JSON from a markdown file.

.DESCRIPTION
Parses a prompts markdown file like docs/e2etestPrompts.md to produce a JSON file used by ToolDescriptionEvaluator and
related tooling.

.OUTPUTS
Writes a prompts.json to the current working directory by default (where the script is invoked) unless otherwise specified.

.PARAMETER PromptsSource
Path to the source prompts markdown file. This is a required parameter.

.PARAMETER OutputPath
Destination JSON file path. If not provided, the output file will be placed in the current working directory
with the same base file name as the source markdown file (e.g. prompts.md -> prompts.json).

.EXAMPLE
./Generate-PromptsJson.ps1 -PromptsSource ./prompts.md
Generates a prompts.json file based on the specified markdown in the current directory.

.EXAMPLE
./Generate-PromptsJson.ps1 -PromptsSource ./prompts.md -OutputPath ./eng/tools/ToolDescriptionEvaluator/myprompts.json
Generates a JSON file with the specified name and path. If no filename is provided, the same name as the source markdown
is used with a .json extension.

.NOTES
Escapes only prompt text by normalizing curly quotes to straight ASCII quotes.
#>

[CmdletBinding()] 
param(
    [Parameter(Mandatory=$true, HelpMessage='Path to markdown file containing prompts table.')]
    [string]$PromptsSource,
    [Parameter(Mandatory=$false, HelpMessage='Destination JSON file path (file or directory). Defaults to the current directory.')]
    [string]$OutputPath
)

function Write-Info($Message) { Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-ErrorLine($Message) { Write-Host "[ERROR] $Message" -ForegroundColor Red }

# Normalize curly quotes & unescape escaped angle brackets (mirrors Program.cs intent + prior logic)
function Convert-Special-Characters([string]$Text) {
    if (-not $Text) { return $Text }
    return $Text.Replace("\u2018", "'").Replace("\u2019", "'").Replace("\u201C", '"').Replace("\u201D", '"').Replace("\<", "<")
}

# Determine final output path given optional user input that may be a file or directory
function Resolve-OutputPath([string]$SourcePath, [string]$UserPath) {
    $baseName = [IO.Path]::GetFileNameWithoutExtension($SourcePath)

    if ([string]::IsNullOrWhiteSpace($UserPath)) {
        return Join-Path (Get-Location) ("$baseName.json")
    }

    $resolved = Resolve-Path -Path $UserPath -ErrorAction SilentlyContinue
    if ($resolved) { $UserPath = $resolved.Path }

    $isDir = $false

    if (Test-Path $UserPath -PathType Container) { $isDir = $true }
    elseif ($UserPath.EndsWith([IO.Path]::DirectorySeparatorChar) -or $UserPath.EndsWith([IO.Path]::AltDirectorySeparatorChar)) { $isDir = $true }
    else {
        $ext = [IO.Path]::GetExtension($UserPath)
        if ([string]::IsNullOrEmpty($ext) -and -not (Test-Path $UserPath -PathType Leaf)) { $isDir = $true }
    }

    if ($isDir) {
        if (-not (Test-Path $UserPath)) { New-Item -ItemType Directory -Force -Path $UserPath | Out-Null }
        return Join-Path $UserPath ("$baseName.json")
    }

    return $UserPath
}

function Get-ToolPrompts([string]$Path) {
    if (-not (Test-Path $Path)) { throw "Prompts markdown not found: $Path" }
    $result = @{}
    Get-Content -Raw -Path $Path | Select-String -Pattern '.*' -AllMatches | ForEach-Object { $_.Matches } | ForEach-Object {
        $line = $_.Value.Trim()
        if (-not $line.StartsWith('|')) { return }
        if ($line -match '^\|\s*Tool Name' -or $line -match '^\|:?-') { return }
        if ($line -match '^##' -or $line -match '^#') { return }
        $parts = $line.Split('|') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }
        if ($parts.Count -lt 2) { return }
        $tool = $parts[0]
        $prompt = $parts[1]
        if (-not $tool.StartsWith('azmcp_')) { return }
        if (-not $prompt) { return }
        if (-not $result.ContainsKey($tool)) { $result[$tool] = @() }
        $result[$tool] += (Convert-Special-Characters $prompt)
    }
    return $result
}

function Save-Json([hashtable]$Data, [string]$Path) {
    $ordered = [System.Collections.Specialized.OrderedDictionary]::new()
    foreach ($key in ($Data.Keys | Sort-Object)) { $ordered[$key] = $Data[$key] }
    $json = $ordered | ConvertTo-Json -Depth 5
    Set-Content -Path $Path -Value $json -Encoding UTF8
    Write-Info "Wrote $Path"
}

# --- Main flow ---

if (-not (Test-Path $PromptsSource)) { Write-ErrorLine "Source markdown not found: $PromptsSource"; exit 1 }

$OutputPath = Resolve-OutputPath -SourcePath $PromptsSource -UserPath $OutputPath
$parent = Split-Path -Parent $OutputPath
if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }

Write-Info "Parsing prompts from: $PromptsSource"
$toolPrompts = Get-ToolPrompts -Path $PromptsSource
if (-not $toolPrompts -or $toolPrompts.Keys.Count -eq 0) { Write-Warn 'No prompts parsed.'; exit 2 }

Save-Json -Data $toolPrompts -Path $OutputPath
Write-Host "Generated: $OutputPath" -ForegroundColor Green
