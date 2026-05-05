#!/bin/env pwsh
#Requires -Version 7

# Validates that a PR includes a changelog entry or has the 'skip-changelog' label.

. "$PSScriptRoot/../common/scripts/common.ps1"

Push-Location $RepoRoot
try {
    $changedFiles = git diff --name-only --diff-filter=A "origin/main...HEAD" 2>&1
    $hasChangelog = $changedFiles | Where-Object { $_ -match 'changelog-entries/.*\.yml$' -or $_ -match 'CHANGELOG\.md'}

    if ($hasChangelog) {
        Write-Host "Found changelog entry: $($hasChangelog -join ', ')"
        exit 0
    }

    # Check for skip-changelog label via gh CLI
    $prNumber = $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
    Write-Host "DEBUG: PR Number = '$prNumber'"
    Write-Host "DEBUG: GH_TOKEN set = $([bool]$env:GH_TOKEN)"
    if (-not $prNumber) {
        Write-Host "Not a PR build - skipping skip-changelog label check."
        exit 0
    }
    Write-Host "DEBUG: Running 'gh pr view $prNumber --json labels --jq .labels[].name'"
    $labels = gh pr view $prNumber --json labels --jq '.labels[].name' 2>&1
    Write-Host "DEBUG: gh exit code = $LASTEXITCODE"
    Write-Host "DEBUG: Labels returned = '$($labels -join ', ')'"
    if ($labels -contains 'skip-changelog') {
        Write-Host "'skip-changelog' label found — skipping."
        exit 0
    }

    Write-Host "No changelog entry and no 'skip-changelog' label. If changelog entry is not required, add the 'skip-changelog' label to the PR to bypass this check. Or add a changelog entry TO the PR."
    exit 1
}
finally {
    Pop-Location
}
