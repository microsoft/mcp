#!/bin/env pwsh
#Requires -Version 7

# Validates that a PR includes a changelog entry or has the 'skip-changelog' label.

. "$PSScriptRoot/../common/scripts/common.ps1"

# Fetch PR labels once at the start via GitHub REST API
$prNumber = $env:SYSTEM_PULLREQUEST_PULLREQUESTNUMBER
$prLabels = @()
if ($prNumber) {
    $repo = $env:BUILD_REPOSITORY_NAME
    if (-not $repo) {
        $repo = "microsoft/mcp"
    }
    $apiUrl = "https://api.github.com/repos/$repo/pulls/$prNumber"
    $headers = @{ "Accept" = "application/vnd.github+json"; "User-Agent" = "mcp-changelog-check" }
    if ($env:GH_TOKEN) {
        $headers["Authorization"] = "Bearer $($env:GH_TOKEN)"
    }
    try {
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $headers -Method Get -ErrorAction Stop
        $prLabels = @($response.labels | ForEach-Object { $_.name })
        Write-Host "PR #$prNumber labels: $($prLabels -join ', ')"
    }
    catch {
        Write-Warning "Failed to fetch PR labels from GitHub API: $_"
    }
}

Push-Location $RepoRoot
try {
    $changedFiles = git diff --name-only --diff-filter=A "origin/main...HEAD" 2>&1
    $hasChangelog = $changedFiles | Where-Object { $_ -match 'changelog-entries/.*\.yml$' -or $_ -match 'CHANGELOG\.md'}

    if ($hasChangelog) {
        Write-Host "Found changelog entry: $($hasChangelog -join ', ')"
        exit 0
    }

    if (-not $prNumber) {
        Write-Host "Not a PR build - skipping skip-changelog label check."
        exit 0
    }

    if ($prLabels -contains 'skip-changelog') {
        Write-Host "'skip-changelog' label found — skipping."
        exit 0
    }

    Write-Host "No changelog entry and no 'skip-changelog' label. If changelog entry is not required, add the 'skip-changelog' label to the PR to bypass this check. Or add a changelog entry TO the PR."
    exit 1
}
finally {
    Pop-Location
}
