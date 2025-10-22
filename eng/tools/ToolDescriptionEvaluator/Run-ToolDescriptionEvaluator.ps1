#!/bin/env pwsh
#Requires -Version 7


<#
.SYNOPSIS
    Builds and runs the Tool Description Evaluator application with default settings

.DESCRIPTION
    This script performs the following steps:
    - Dynamically load tools from an Azure MCP Server Debug or Release build, preferring the former
    - Load prompts from the file located at servers/Azure.Mcp.Server/docs/e2eTestPrompts.md
    - Build the Tool Description Evaluator application in Debug configuration and run it

.PARAMETER BuildAzureMcp
    Optionally build the root project in Debug mode to ensure tools can be loaded dynamically

.PARAMETER Area
    Filter prompts by tool name prefix. Service names are auto-prefixed with "azmcp_" (e.g., "keyvault" becomes "azmcp_keyvault")

.EXAMPLE
    ./Run-ToolDescriptionEvaluator.ps1
    Builds and runs the Tool Description Evaluator application with default settings

.EXAMPLE
    ./Run-ToolDescriptionEvaluator.ps1 -BuildAzureMcp
    Builds the Azure MCP Server project in Debug mode, then builds and runs the Tool Description Evaluator application
    with default settings

.EXAMPLE
    ./Run-ToolDescriptionEvaluator.ps1 -Area "storage"
    Runs the Tool Description Evaluator filtering prompts to only tools with the azmcp_storage prefix

.EXAMPLE
    ./Run-ToolDescriptionEvaluator.ps1 -Area "keyvault"
    Runs the Tool Description Evaluator filtering prompts to only tools with the azmcp_keyvault prefix

.EXAMPLE
    ./Run-ToolDescriptionEvaluator.ps1 -Area "functionapp" -BuildAzureMcp
    Builds the Azure MCP Server project, then runs the Tool Description Evaluator filtering prompts to only Function App tools
#>

[CmdletBinding()]
param(
    [switch]$BuildAzureMcp,
    [string]$Area
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

try {
    # Get absolute paths
    $repoRoot = Resolve-Path "$PSScriptRoot/../../../" | Select-Object -ExpandProperty Path
    $toolDir = Resolve-Path "$PSScriptRoot" | Select-Object -ExpandProperty Path

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

    # Locate azmcp CLI artifact (platform & build-type agnostic)
    $platformIsWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
    $candidateNames = if ($platformIsWindows) { @('azmcp.exe','azmcp','azmcp.dll') } else { @('azmcp','azmcp.dll') }
    $searchRoots = @(
        "$repoRoot/servers/Azure.Mcp.Server/src/bin/Debug",
        "$repoRoot/servers/Azure.Mcp.Server/src/bin/Release"
    ) | Where-Object { Test-Path $_ }

    $cliArtifact = $null

    foreach ($root in $searchRoots) {
        foreach ($name in $candidateNames) {
            $found = Get-ChildItem -Path $root -Filter $name -Recurse -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer } | Select-Object -First 1

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
        # Broad fallback to help user diagnose
        foreach ($root in $searchRoots) {
            $any = Get-ChildItem -Path $root -Filter 'azmcp*' -Recurse -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer }
            
            if ($any) {
                Write-Host "[WARNING] In $root found: $($any | Select-Object -ExpandProperty Name -Join ', ')" -ForegroundColor Yellow
            }
        }

        Write-Host "[ERROR] No azmcp CLI artifact found. Try rerunning with -BuildAzureMcp or ensure a Debug/Release build completed." -ForegroundColor Red
        
        exit 1
    }

    Write-Host "Discovered CLI artifact: $($cliArtifact.FullName)" -ForegroundColor Green
    Write-Host "Building and running tool selection confidence score calculation app..." -ForegroundColor Green
    Write-Host "Building application..." -ForegroundColor Yellow

    & dotnet build "$toolDir/ToolDescriptionEvaluator.csproj"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build application"
    }

    Write-Host "Build completed successfully!" -ForegroundColor Green
    
    # Build the command arguments
    $runArgs = @()
    
    if ($Area) {
        $runArgs += "--area"
        $runArgs += $Area
        Write-Host "Running with area filter: $Area" -ForegroundColor Cyan
    }
    
    $runCommand = "dotnet run"
    if ($runArgs.Count -gt 0) {
        $runCommand += " -- " + ($runArgs -join " ")
    }
    
    Write-Host "Running with: $runCommand" -ForegroundColor Cyan
    Push-Location $toolDir

    if ($runArgs.Count -gt 0) {
        & dotnet run -- @runArgs
    } else {
        & dotnet run
    }

    Pop-Location
}
catch {
    Write-Error "Build failed: $_"

    exit 1
}