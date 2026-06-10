#!/usr/bin/env pwsh
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
        Write-Error "Failed to fetch PR labels from GitHub API for PR #$prNumber. Unable to validate whether the 'skip-changelog' label is present. Ensure the GitHub authentication step ran and GH_TOKEN is available, then retry. Underlying error: $_"
        exit 1
    }
}

Push-Location $RepoRoot
try {
    $diffRange = "origin/main...HEAD"
    $changedFiles = git diff --name-only --diff-filter=AM $diffRange 2>&1
    if ($LASTEXITCODE -ne 0) {
        $gitError = ($changedFiles | Out-String).Trim()
        Write-Error "Failed to determine changed files with 'git diff --name-only --diff-filter=AM $diffRange'. $gitError"
        exit 1
    }
    $hasChangelog = $changedFiles | Where-Object { $_ -match 'changelog-entries/.*\.ya?ml$' -or $_ -match 'CHANGELOG\.md'}

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

    Write-Error "Add a changelog entry to the PR or apply the 'skip-changelog' label."
    exit 1
}
finally {
    Pop-Location
}
