#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Updates the tools.json file by calling the "azmcp tools list" command

.DESCRIPTION
    Generates a fresh tools.json by executing the "azmcp tools list" command from an Azure
    MCP Server Debug or Release build, preferring the former.

.PARAMETER Force
    Overwrite the existing tools.json file without prompting

.PARAMETER BuildAzureMcp
    Build the root project in Debug mode to ensure tools can be loaded dynamically

.EXAMPLE
    ./Update-ToolsJson.ps1
    Updates the tools.json file, prompting before overwriting if it exists

.EXAMPLE
    ./Update-ToolsJson.ps1 -Force
    Updates the tools.json file, overwriting without prompting
    
.EXAMPLE
    ./Update-ToolsJson.ps1 -BuildAzureMcp
    Updates the tools.json file after building the Azure MCP Server project in Debug mode
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$BuildAzureMcp
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# Resolve important paths
$repoRoot = Resolve-Path "$PSScriptRoot/../../../" | Select-Object -ExpandProperty Path
$toolDir  = Resolve-Path "$PSScriptRoot" | Select-Object -ExpandProperty Path
$jsonFile = "$toolDir/tools.json"

# Build the whole Azure MCP Server project if needed
if ($BuildAzureMcp)
{
    Write-Host "Building root project to enable dynamic tool loading..." -ForegroundColor Yellow

    & dotnet build "$repoRoot/AzureMcp.sln"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build root project"
    }

    Write-Host "Root project build completed successfully!" -ForegroundColor Green
}

# Locate azmcp CLI artifact (platform & build-type agnostic) like Run-ToolDescriptionEvaluator.ps1
$candidateNames = if ($IsWindows) { @('azmcp.exe','azmcp','azmcp.dll') } else { @('azmcp','azmcp.dll') }
$searchRoots = @(
    "$repoRoot/servers/Azure.Mcp.Server/src/bin/Debug",
    "$repoRoot/servers/Azure.Mcp.Server/src/bin/Release"
) | Where-Object { Test-Path $_ }

$cliArtifact = $null

foreach ($root in $searchRoots) {
    foreach ($name in $candidateNames) {
        $found = Get-ChildItem -Path $root -Filter $name -Recurse -ErrorAction SilentlyContinue |
                 Where-Object { -not $_.PSIsContainer } |
                 Select-Object -First 1

        if ($found) {
            $cliArtifact = $found
            
            break
        }   
    }

    if ($cliArtifact) {
        break
    }
}

if (-not $cliArtifact) {
    Write-Error "Could not locate 'azmcp' CLI under: $($searchRoots -join ', ')"
    Write-Host "Try building the solution first:" -ForegroundColor Yellow
    Write-Host "  dotnet build `"$repoRoot/AzureMcp.sln`"" -ForegroundColor Yellow

    exit 1
}

# Confirm overwrite unless -Force
if ((Test-Path $jsonFile) -and -not $Force) {
    $response = Read-Host "tools.json already exists. Overwrite? (y/N)"

    if ($response -notmatch '^[Yy]') {
        Write-Host "Operation cancelled." -ForegroundColor Yellow

        exit 0
    }
}

Write-Host "Generating tools.json..." -ForegroundColor Green

try {
    # Execute azmcp tools list and capture output
    if ($cliArtifact.Extension -ieq '.dll') {
        $output = & dotnet $cliArtifact.FullName tools list 2>&1
    }
    else {
        $output = & $cliArtifact.FullName tools list 2>&1
    }

    # Extract pure JSON in case the CLI prints extra logs
    if ($null -eq $output) {
        throw "No output received from azmcp."
    }

    $outputText = $output | Out-String
    $start = $outputText.IndexOf('{')
    $end   = $outputText.LastIndexOf('}')
    $jsonText = if ($start -ge 0 -and $end -ge $start) { $outputText.Substring($start, $end - $start + 1) } else { $outputText }

    $jsonText | Out-File -FilePath $jsonFile -Encoding utf8

    # Verify the file was created and has content
    if ((Test-Path $jsonFile) -and ((Get-Item $jsonFile).Length -gt 0)) {
        $fileSize = [math]::Round((Get-Item $jsonFile).Length / 1KB, 2)

        Write-Host "Successfully generated tools.json ($fileSize KB)" -ForegroundColor Green

        # Try to parse the JSON to verify it's valid
        try {
            $json = Get-Content $jsonFile -Raw | ConvertFrom-Json
            $toolCount = if ($null -ne $json.results) { $json.results.Count } elseif ($null -ne $json.tools) { $json.tools.Count } else { $null }

            if ($null -ne $toolCount) {
                Write-Host "Contains $toolCount tools" -ForegroundColor Cyan
            }
        }
        catch {
            Write-Warning "Generated JSON file may not be valid: $_"
        }
    }
    else {
        Write-Error "Failed to generate tools.json or file is empty"
    }
}
catch {
    Write-Error "Failed to execute azmcp: $_"

    exit 1
}
