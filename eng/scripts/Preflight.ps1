#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Runs all CI preflight checks locally so a developer can verify their changes
    will pass the PR validation pipeline before pushing.

.DESCRIPTION
    Executes the same checks that run in the PR validation pipeline:
    1. Code formatting (dotnet format)
    2. Spelling (cspell)
    3. Build (dotnet build with warnings-as-errors)
    4. Tool metadata validation (descriptions, name lengths, duplicate IDs)
    5. Documentation validation (README annotations)
    6. Unit tests

    Use -Quick to skip tests and spelling for a fast feedback loop.
    Use -IncludeAot to add AOT compatibility analysis (slow, linux-x64 only).

.EXAMPLE
    ./eng/scripts/Preflight.ps1
    # Runs all standard checks

.EXAMPLE
    ./eng/scripts/Preflight.ps1 -Quick
    # Runs only format, build, and tool validation (fastest)

.EXAMPLE
    ./eng/scripts/Preflight.ps1 -IncludeAot
    # Runs all checks including AOT analysis
#>

[CmdletBinding()]
param(
    [switch] $Quick,
    [switch] $IncludeAot,
    [string[]] $Paths
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$solutionFile = "$RepoRoot/Microsoft.Mcp.slnx"

$stepsPassed = 0
$stepsFailed = 0
$stepsSkipped = 0
$failedSteps = @()
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

function Write-StepHeader([string]$name) {
    Write-Host ""
    Write-Host "━━━ $name " -ForegroundColor Cyan -NoNewline
    Write-Host ("━" * [Math]::Max(0, 60 - $name.Length - 5)) -ForegroundColor Cyan
}

function Write-StepResult([string]$name, [bool]$passed, [string]$detail) {
    if ($passed) {
        Write-Host "✅ $name" -ForegroundColor Green
        $script:stepsPassed++
    } else {
        Write-Host "❌ $name" -ForegroundColor Red
        if ($detail) { Write-Host "   $detail" -ForegroundColor Yellow }
        $script:stepsFailed++
        $script:failedSteps += $name
    }
}

function Write-StepSkipped([string]$name, [string]$reason) {
    Write-Host "⏭️  $name (skipped: $reason)" -ForegroundColor DarkGray
    $script:stepsSkipped++
}

Push-Location $RepoRoot
try {
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                   MCP Preflight Checks                      ║" -ForegroundColor Cyan
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    if ($Quick) {
        Write-Host "  Mode: Quick (format + build + tool validation)" -ForegroundColor Yellow
    } elseif ($IncludeAot) {
        Write-Host "  Mode: Full + AOT" -ForegroundColor Yellow
    } else {
        Write-Host "  Mode: Standard" -ForegroundColor Yellow
    }

    # ── 1. Code Formatting ──────────────────────────────────────────
    Write-StepHeader "Code Formatting"

    # Excluding diagnostics IL2026 and IL3050 due to known issues with source generator
    dotnet format $solutionFile --verify-no-changes --exclude-diagnostics IL2026 IL3050 2>&1 | Out-Host

    if ($LASTEXITCODE -eq 0) {
        Write-StepResult "dotnet format" $true
    } else {
        Write-StepResult "dotnet format" $false "Run 'dotnet format `"$solutionFile`"' to fix"
    }

    # ── 2. Spelling ─────────────────────────────────────────────────
    Write-StepHeader "Spelling"

    if ($Quick) {
        Write-StepSkipped "cspell" "quick mode"
    } else {
        & "$RepoRoot/eng/common/spelling/Invoke-Cspell.ps1" *>&1
        | Tee-Object -Variable cspellOutput
        | Where-Object { $_ -like '*Unknown word*' }

        if ($LASTEXITCODE -eq 0) {
            Write-StepResult "cspell" $true
        } else {
            Write-StepResult "cspell" $false "Fix unknown words listed above"
        }
    }

    # ── 3. Build ────────────────────────────────────────────────────
    Write-StepHeader "Build"

    dotnet build $solutionFile 2>&1 | Out-Host

    if ($LASTEXITCODE -eq 0) {
        Write-StepResult "dotnet build" $true
    } else {
        Write-StepResult "dotnet build" $false "Fix compilation errors above"
    }

    # ── 4. Tool Metadata Validation ─────────────────────────────────
    Write-StepHeader "Tool Metadata Validation"

    & "$PSScriptRoot/Test-ToolSelection.ps1" 2>&1 | Out-Host
    if ($LASTEXITCODE -eq 0) {
        Write-StepResult "Tool description evaluation" $true
    } else {
        Write-StepResult "Tool description evaluation" $false
    }

    $toolNameResult = & "$PSScriptRoot/Test-ToolNameLength.ps1"
    if ($LASTEXITCODE -eq 0) {
        Write-StepResult "Tool name length" $true
    } else {
        Write-StepResult "Tool name length" $false "$($toolNameResult.ViolationCount) tool(s) exceed max length"
    }

    $toolIdResult = & "$PSScriptRoot/Test-ToolId.ps1"
    if ($LASTEXITCODE -eq 0) {
        Write-StepResult "Tool ID uniqueness" $true
    } else {
        Write-StepResult "Tool ID uniqueness" $false "$($toolIdResult.ViolationCount) duplicate ID(s)"
    }

    # ── 5. Documentation Validation ────────────────────────────────
    Write-StepHeader "Documentation Validation"

    if ($Quick) {
        Write-StepSkipped "README validation" "quick mode"
    } else {
        $readmeResult = & "$PSScriptRoot/Process-PackageReadMe.ps1" -Command "validate-all" 2>&1

        # Script returns $true if failures exist, $false if all passed
        if ($readmeResult -eq $false -or $readmeResult -eq $null) {
            Write-StepResult "README validation" $true
        } else {
            Write-StepResult "README validation" $false "Fix README annotation or emoji issues above"
        }
    }

    # ── 6. Unit Tests ───────────────────────────────────────────────
    Write-StepHeader "Unit Tests"

    if ($Quick) {
        Write-StepSkipped "Unit tests" "quick mode"
    } else {
        $testArgs = @()
        if ($Paths) { $testArgs += @('-Paths') + $Paths }

        try {
            & "$PSScriptRoot/Test-Code.ps1" @testArgs 2>&1 | Out-Host
        } catch {
            # Test-Code.ps1 uses Write-Error which is terminating under ErrorActionPreference=Stop
        }

        if ($LASTEXITCODE -eq 0) {
            Write-StepResult "Unit tests" $true
        } else {
            Write-StepResult "Unit tests" $false "Fix failing tests above"
        }
    }

    # ── 7. AOT Analysis (optional) ──────────────────────────────────
    Write-StepHeader "AOT Analysis"

    if (!$IncludeAot) {
        Write-StepSkipped "AOT analysis" ($Quick ? "quick mode" : "use -IncludeAot to enable")
    } else {
        & "$PSScriptRoot/Analyze-AOT-Compact.ps1" -Runtime 'linux-x64' -OutputFormat Console 2>&1 | Out-Host

        if ($LASTEXITCODE -eq 0) {
            Write-StepResult "AOT analysis" $true
        } else {
            Write-StepResult "AOT analysis" $false "Fix AOT/trimming warnings above"
        }
    }

    # ── Summary ─────────────────────────────────────────────────────
    $stopwatch.Stop()
    $elapsed = $stopwatch.Elapsed.ToString("mm\:ss")

    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor ($stepsFailed -gt 0 ? "Red" : "Green")
    Write-Host "║                        Results                              ║" -ForegroundColor ($stepsFailed -gt 0 ? "Red" : "Green")
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor ($stepsFailed -gt 0 ? "Red" : "Green")
    Write-Host "  Passed:  $stepsPassed" -ForegroundColor Green
    if ($stepsFailed -gt 0) {
        Write-Host "  Failed:  $stepsFailed" -ForegroundColor Red
        foreach ($step in $failedSteps) {
            Write-Host "           - $step" -ForegroundColor Red
        }
    }
    if ($stepsSkipped -gt 0) {
        Write-Host "  Skipped: $stepsSkipped" -ForegroundColor DarkGray
    }
    Write-Host "  Time:    $elapsed" -ForegroundColor Cyan
    Write-Host ""

    if ($stepsFailed -gt 0) {
        Write-Host "Preflight FAILED. Fix the issues above before pushing." -ForegroundColor Red
        exit 1
    } else {
        Write-Host "Preflight PASSED. Ready to push!" -ForegroundColor Green
        exit 0
    }
}
finally {
    Pop-Location
}
