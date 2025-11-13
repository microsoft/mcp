#!/bin/env pwsh
#Requires -Version 7

. "$PSScriptRoot/../common/scripts/common.ps1"

Push-Location $RepoRoot
try {
    Write-Host "Running dotnet format to check for formatting issues..."
    $solutionFile = Get-ChildItem -Path . -Filter *.sln | Select-Object -First 1

    # Excluding diagnostics IL2026 and IL3050 due to known issues with source generator
    # Can be removed when https://github.com/dotnet/sdk/issues/45054 is resolved
    dotnet format $solutionFile --verify-no-changes --exclude-diagnostics IL2026 IL3050

    # Run dotnet format
    if ($LASTEXITCODE) {
        Write-Host "❌ dotnet format detected formatting issues."
        Write-Host "Please run 'dotnet format `"$solutionFile`"' to fix the issues and then try committing again."
        $hasErrors = $true
    } else {
        Write-Host "✅ dotnet format did not detect any formatting issues."
    }

    # Run cspell spell check
    if (!$env:TF_BUILD) {
        Write-Host "Running cspell spell check..."
        & "$RepoRoot/eng/common/spelling/Invoke-Cspell.ps1" *>&1
        | Tee-Object -Variable cspellOutput
        | Where-Object { $_ -like '*Unknown word*' }

        if ($LASTEXITCODE) {
            Write-Host "❌ Spell check detected issues. Please fix the above errors before committing."
            $hasErrors = $true
        } else {
            Write-Host "✅ Spell check did not detect any issues."
        }
    }

    # Run tool description evaluation
    & "$PSScriptRoot/Test-ToolSelection.ps1"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tool description evaluation failed"
    } else {
        Write-Host "✅ Tool description evaluation did not detect any issues."
    }

    # Run tool name length validation
    $toolNameResult = & "$PSScriptRoot/Test-ToolNameLength.ps1"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tool name length validation failed"
        Write-Host "Maximum Length Allowed: $($toolNameResult.MaxAllowed). $($toolNameResult.ViolationCount) tool(s) exceeding this limit. Review the above output for details."
        $hasErrors = $true
    } else {
        Write-Host "✅ Tool name length validation passed."
        Write-Host "All tools are within the $($toolNameResult.MaxAllowed) character limit."
    }

    # Run tool id validation
    $toolIdResult = & "$PSScriptRoot/Test-ToolId.ps1"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tool id validation failed"
        Write-Host "$($toolIdResult.ViolationCount) duplicated tool id(s) found. Review the above output for details."
        $hasErrors = $true
    } else {
        Write-Host "✅ Tool id validation passed."
    }

    if($hasErrors) {
        exit 1
    }
}
finally {
    Pop-Location
}
