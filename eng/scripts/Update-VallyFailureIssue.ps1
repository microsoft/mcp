#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Creates or updates an issue for Vally evaluation failures.

.DESCRIPTION
    Finds the immediately preceding completed pull request run for the workflow.
    If that run failed and has an open Vally failure issue, adds the current
    failure to that issue. Otherwise, creates an issue for the current pull
    request.

.PARAMETER Repository
    GitHub repository in owner/name format.

.PARAMETER Workflow
    Workflow file name or workflow ID.

.PARAMETER RunId
    ID of the current workflow run.

.PARAMETER RunUrl
    URL of the current workflow run.

.PARAMETER PullRequestNumber
    Number of the pull request that triggered the current run.

.PARAMETER PullRequestUrl
    URL of the pull request that triggered the current run.

.PARAMETER PullRequestAuthor
    GitHub login of the pull request author.
#>

param(
    [Parameter(Mandatory)]
    [string]$Repository,

    [Parameter(Mandatory)]
    [string]$Workflow,

    [Parameter(Mandatory)]
    [long]$RunId,

    [Parameter(Mandatory)]
    [string]$RunUrl,

    [Parameter(Mandatory)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory)]
    [string]$PullRequestUrl,

    [Parameter(Mandatory)]
    [string]$PullRequestAuthor
)

$ErrorActionPreference = 'Stop'

$issueTitlePrefix = 'New vally failures in main'
$runsJson = gh api "repos/$Repository/actions/workflows/$Workflow/runs" `
    -f event=pull_request -f status=completed -f per_page=20

if ($LASTEXITCODE -ne 0) {
    throw 'Failed to retrieve previous workflow runs.'
}

$runs = ($runsJson | ConvertFrom-Json).workflow_runs
$previousMergedRun = $null

foreach ($run in $runs | Where-Object { $_.id -ne $RunId }) {
    $runPullRequest = $run.pull_requests | Select-Object -First 1
    if (!$runPullRequest) {
        continue
    }

    $pullRequestJson = gh api --method GET "repos/$Repository/pulls/$($runPullRequest.number)"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to retrieve pull request #$($runPullRequest.number)."
    }

    $pullRequest = $pullRequestJson | ConvertFrom-Json
    if ($pullRequest.merged_at) {
        $previousMergedRun = $run
        break
    }
}

$currentFailureBody = @"
Vally evaluation failures were detected for PR [#$PullRequestNumber]($PullRequestUrl).

PR author: @$PullRequestAuthor

Workflow run: [View run]($RunUrl)
"@

if ($previousMergedRun -and $previousMergedRun.conclusion -eq 'failure') {
    $issuesJson = gh issue list --repo $Repository --state open --search '"vally failures" in:title' `
        --json number,title,body,comments --limit 100

    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to retrieve existing Vally failure issues.'
    }

    $previousIssue = $issuesJson |
        ConvertFrom-Json |
        Where-Object {
            $_.title.StartsWith($issueTitlePrefix, [StringComparison]::Ordinal) -and
            (
                $_.body.Contains($previousMergedRun.html_url, [StringComparison]::Ordinal) -or
                ($_.comments | Where-Object {
                    $_.body.Contains($previousMergedRun.html_url, [StringComparison]::Ordinal)
                })
            )
        } |
        Select-Object -First 1

    if ($previousIssue) {
        gh issue comment $previousIssue.number --repo $Repository --body $currentFailureBody

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to comment on issue #$($previousIssue.number)."
        }

        Write-Host "Added the current Vally failure to issue #$($previousIssue.number)."
        exit 0
    }
}

gh issue create --repo $Repository --title "$issueTitlePrefix$PullRequestNumber" --body $currentFailureBody

if ($LASTEXITCODE -ne 0) {
    throw 'Failed to create a Vally failure issue.'
}
