#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Runs tool selection confidence analysis as part of CI pipeline

.DESCRIPTION
    This script runs the tool selection analysis to validate that the Azure MCP Server's
    tool selection algorithm works correctly. It's designed to be CI-friendly and
    will gracefully skip when required credentials are not available.

.PARAMETER SkipIfMissingCredentials
    Skip the test if Azure OpenAI credentials are not configured (default: true in CI)

.PARAMETER ToolsFile
    Use a JSON file containing tool data instead of dynamically loading it from the Azure MCP Server

.PARAMETER PromptsFile
    Use a JSON or markdown file containing prompts to test instead of the default ../../../docs/e2eTestPrompts.md

.PARAMETER OutputMarkdown
    Generate output in markdown format instead of plain text

.PARAMETER ValidateMode
    Run in validation mode to test specific tool descriptions

.PARAMETER ToolDescription
    Tool description to test (used with -ValidateMode)

.PARAMETER TestPrompts
    Array of test prompts (used with -ValidateMode)

.EXAMPLE
    # Standard CI run (graceful skip if no credentials)
    ./Test-ToolSelection.ps1
    
.EXAMPLE
    # Use custom tools file
    ./Test-ToolSelection.ps1 -ToolsFile "my-tools.json"
    
.EXAMPLE
    # Generate markdown output
    ./Test-ToolSelection.ps1 -OutputMarkdown
    
.EXAMPLE
    # Validation mode - test specific tool description
    ./Test-ToolSelection.ps1 -ValidateMode -ToolDescription "Lists storage accounts" -TestPrompts @("show storage accounts", "list my storage")
    
.EXAMPLE
    # Full custom analysis
    ./Test-ToolSelection.ps1 -ToolsFile "custom-tools.json" -PromptsFile "custom-prompts.md" -OutputMarkdown
#>

param(
    [switch]$SkipIfMissingCredentials,
    [string]$ToolsFile,
    [string]$PromptsFile,
    [switch]$OutputMarkdown,
    [switch]$ValidateMode,
    [string]$ToolDescription,
    [string[]]$TestPrompts
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0
. "$PSScriptRoot/../common/scripts/common.ps1"

$toolSelectionPath = "$RepoRoot/eng/tools/ToolDescriptionEvaluator"
$defaultMarkdownPrompts = "$RepoRoot/docs/e2eTestPrompts.md"

if (-not (Test-Path $toolSelectionPath)) {
    Write-Host "⏭️  Tool Description Evaluator utility not found at $toolSelectionPath - skipping"

    exit 0
}

Push-Location $toolSelectionPath

try {
    # Check if we have the required sources for dynamic loading
    $hasSourceCode = Test-Path "$RepoRoot/servers/Azure.Mcp.Server/src"
    $hasMarkdownPrompts = Test-Path $defaultMarkdownPrompts
    
    # Check if we have fallback test data files
    $hasToolsData = Test-Path "$toolSelectionPath/tools.json"
    $hasPromptsData = Test-Path "$toolSelectionPath/prompts.json"
    $hasApiKey = -not [string]::IsNullOrEmpty($env:TEXT_EMBEDDING_API_KEY)
    $hasEndpoint = -not [string]::IsNullOrEmpty($env:AOAI_ENDPOINT)
    
    # In validation mode, check for required parameters
    if ($ValidateMode) {
        $noPrompts = (-not $TestPrompts) -or ($TestPrompts.Count -eq 0)

        if ([string]::IsNullOrEmpty($ToolDescription) -or $noPrompts) {
            Write-Host "❌ Validation mode requires a -ToolDescription and at least one of -TestPrompts"
            Write-Host "Example: -ValidateMode -ToolDescription 'Lists storage accounts' -TestPrompts @('show storage accounts', 'list my storage')"

            exit 1
        }
        
        # Validation mode requires credentials
        if (-not ($hasApiKey -and $hasEndpoint)) {
            if ($SkipIfMissingCredentials) {
                Write-Host "⏭️  Skipping validation mode in CI - Azure OpenAI credentials not available"

                exit 0
            }

            Write-Host "❌ Validation mode requires AOAI_ENDPOINT and TEXT_EMBEDDING_API_KEY environment variables"

            exit 1
        }
    }
    else {
        # In CI mode, skip gracefully if no sources or credentials are available
        if (-not ($hasSourceCode -or $hasToolsData) -and -not ($hasMarkdownPrompts -or $hasPromptsData) -and -not ($hasApiKey -and $hasEndpoint)) {
            if ($SkipIfMissingCredentials -or $env:BUILD_BUILDID -or $env:GITHUB_ACTIONS) {
                Write-Host "⏭️  Skipping tool selection analysis in CI - required data sources or credentials not available"

                exit 0
            }
            # In non-CI mode, let Program.cs handle the error messaging with detailed help
        }
    }
    
    # Resolve custom file paths relative to repo root if they're not absolute
    $resolvedToolsFile = $null
    $resolvedPromptsFile = $null
    if (-not [string]::IsNullOrEmpty($ToolsFile)) {
        $candidate = if ([System.IO.Path]::IsPathRooted($ToolsFile)) { $ToolsFile } else { Join-Path $RepoRoot $ToolsFile }
        $resolved = Resolve-Path -LiteralPath $candidate -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path -First 1

        if (-not $resolved) {
            Write-Host "❌ Custom tools file not found: $resolvedToolsFile"

            exit 1
        }

        $resolvedToolsFile = $resolved
    }
    
    if (-not [string]::IsNullOrEmpty($PromptsFile)) {
        $candidate = if ([System.IO.Path]::IsPathRooted($PromptsFile)) { $PromptsFile } else { Join-Path $RepoRoot $PromptsFile }
        $resolved = Resolve-Path -LiteralPath $candidate -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path -First 1

        if (-not $resolved) {
            Write-Host "❌ Custom prompts file not found: $resolvedPromptsFile"

            exit 1
        }

        $resolvedPromptsFile = $resolved
    }
    
    # Show configuration info
    Write-Host "🔍 Running tool selection analysis..."

    if ($ValidateMode) {
        Write-Host "📝 Mode: Validation"
        Write-Host "🔧 Tool Description: $ToolDescription"
        Write-Host "💬 Test Prompts: $($TestPrompts -join ', ')"
    } else {
        Write-Host "📝 Mode: Full Analysis"

        if ($resolvedToolsFile) {
            Write-Host "🔧 Tools File: $resolvedToolsFile"
        } else {
            Write-Host "🔧 Tools Source: Dynamic loading"
        }

        if ($resolvedPromptsFile) {
            Write-Host "💬 Prompts File: $resolvedPromptsFile"
        } else {
            Write-Host "💬 Prompts Source: $defaultMarkdownPrompts"
        }

        Write-Host "📄 Output Format: $(if ($OutputMarkdown) { 'Markdown' } else { 'Plain Text' })"
    }
    
    # Build and run the tool
    dotnet build --verbosity quiet

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to build tool selection analyzer"

        exit $LASTEXITCODE
    }
    
    # Prepare run arguments
    $runArgs = @("--no-build", "--")
    
    # Add CI flag for graceful degradation (except in validation mode where we want explicit feedback)
    if (-not $ValidateMode) {
        $runArgs += "--ci"
    }
    
    # Add validation mode arguments
    if ($ValidateMode) {
        $runArgs += "--validate"
        $runArgs += "--tool-description"
        $runArgs += $ToolDescription
        
        foreach ($prompt in $TestPrompts) {
            $runArgs += "--prompt"
            $runArgs += $prompt
        }
    }
    else {
        # Add file override arguments for full analysis mode
        if (-not [string]::IsNullOrEmpty($resolvedToolsFile)) {
            $runArgs += "--tools-file"
            $runArgs += $resolvedToolsFile
        }
        
        if (-not [string]::IsNullOrEmpty($resolvedPromptsFile)) {
            $runArgs += "--prompts-file"
            $runArgs += $resolvedPromptsFile
        }
    }
    
    dotnet run @runArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tool selection analysis failed"

        exit 1
    }
    
    # Check if results were generated (only for full analysis mode)
    if (-not $ValidateMode) {
        $expectedResultFile = if ($OutputMarkdown) { "results.md" } else { "results.txt" }

        if (-not (Test-Path $expectedResultFile)) {
            Write-Host "⚠️  Expected result file '$expectedResultFile' was not generated"
        }
    }
} finally {
    Pop-Location
}
