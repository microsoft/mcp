#!/bin/env pwsh
#Requires -Version 7

. "$PSScriptRoot/../common/scripts/common.ps1"

Push-Location $RepoRoot
try {
    Write-Host "Running dotnet format to check for formatting issues..."
    $solutionFile = Get-ChildItem -Path . -Filter *.sln | Select-Object -First 1
    dotnet format $solutionFile --verify-no-changes

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

    if($hasErrors) {
        exit 1
    }
}
finally {
    Pop-Location
}
