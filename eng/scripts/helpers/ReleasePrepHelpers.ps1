function Get-USFederalHoliday {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [datetime]$Date
    )

    $fixedHolidays = @(
        @{ Name = "New Year's Day"; Month = 1; Day = 1 }
        @{ Name = 'Independence Day'; Month = 7; Day = 4 }
        @{ Name = 'Christmas Eve'; Month = 12; Day = 24 }
        @{ Name = 'Christmas Day'; Month = 12; Day = 25 }
    )

    # Next year's New Year's Day can be observed on December 31 of this year.
    foreach ($year in @($Date.Year, ($Date.Year + 1))) {
        foreach ($holiday in $fixedHolidays) {
            $actualDate = [datetime]::new($year, $holiday.Month, $holiday.Day)
            if ($Date.Date -eq $actualDate) {
                return $holiday.Name
            }
            $observedDate = switch ($actualDate.DayOfWeek) {
                Saturday { $actualDate.AddDays(-1) }
                Sunday { $actualDate.AddDays(1) }
                default { $actualDate }
            }
            if ($Date.Date -eq $observedDate) {
                return "$($holiday.Name) (observed)"
            }
        }
    }

    $floatingHolidays = @(
        @{ Name = 'Martin Luther King Jr. Day'; Month = 1; Weekday = 1; Week = 3 }
        @{ Name = "Washington's Birthday"; Month = 2; Weekday = 1; Week = 3 }
        @{ Name = 'Memorial Day'; Month = 5; Weekday = 1; Week = -1 }
        @{ Name = 'Labor Day'; Month = 9; Weekday = 1; Week = 1 }
        @{ Name = 'Thanksgiving Day'; Month = 11; Weekday = 4; Week = 4 }
        @{ Name = 'Day after Thanksgiving'; Month = 11; Weekday = 4; Week = 4; DayOffset = 1 }
    )
    foreach ($holiday in $floatingHolidays) {
        $firstDay = [datetime]::new($Date.Year, $holiday.Month, 1)
        if ($holiday.Week -eq -1) {
            $lastDay = $firstDay.AddMonths(1).AddDays(-1)
            $holidayDate = $lastDay.AddDays(-(([int]$lastDay.DayOfWeek - $holiday.Weekday + 7) % 7))
        }
        else {
            $holidayDate = $firstDay.AddDays(
                (($holiday.Weekday - [int]$firstDay.DayOfWeek + 7) % 7) + 7 * ($holiday.Week - 1))
        }
        if ($holiday.ContainsKey('DayOffset')) {
            $holidayDate = $holidayDate.AddDays($holiday.DayOffset)
        }
        if ($Date.Date -eq $holidayDate) {
            return $holiday.Name
        }
    }
}

function Get-ReleasePrepSchedule {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$ScheduleName,

        [DateTimeOffset]$UtcNow = [DateTimeOffset]::UtcNow
    )

    $scheduledUtcHour = switch ($ScheduleName) {
        'Release preparation (PDT)' { 19 }
        'Release preparation (PST)' { 20 }
        default { throw "Unrecognized release-preparation schedule: $ScheduleName" }
    }
    $pacificZone = [TimeZoneInfo]::FindSystemTimeZoneById('Pacific Standard Time')
    $pacificTime = [TimeZoneInfo]::ConvertTime($UtcNow, $pacificZone)
    $noonUtc = [TimeZoneInfo]::ConvertTimeToUtc($pacificTime.Date.AddHours(12), $pacificZone)
    $holiday = Get-USFederalHoliday -Date $pacificTime.Date
    $shouldRun = $false

    if ($pacificTime.DayOfWeek -notin @([DayOfWeek]::Tuesday, [DayOfWeek]::Thursday)) {
        $reason = 'Skipping: the Pacific date is not a Tuesday or Thursday.'
    }
    elseif ($noonUtc.Hour -ne $scheduledUtcHour) {
        $reason = 'Skipping: this UTC schedule is inactive for the current Pacific daylight-saving offset.'
    }
    elseif ($holiday) {
        $reason = "Skipping US federal holiday: $holiday."
    }
    else {
        $shouldRun = $true
        $reason = 'Preparing a release for the scheduled 12 PM Pacific run.'
    }

    [PSCustomObject]@{
        ShouldRun = $shouldRun
        ReleaseDate = $pacificTime.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
        Reason = $reason
    }
}

function Format-ReleasePrepPullRequestBody {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Template,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Version,

        [Parameter(Mandatory)]
        [datetime]$ReleaseDate,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]]$ReleaseOwners,

        [Parameter(Mandatory)]
        [uri]$BuildUrl
    )

    if (-not $BuildUrl.IsAbsoluteUri -or $BuildUrl.Scheme -ne 'https') {
        throw 'The pipeline build URL must be an absolute HTTPS URL.'
    }
    $mentions = foreach ($owner in $ReleaseOwners) {
        if ($owner -notmatch '^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,37}[a-zA-Z0-9])?$') {
            throw "Invalid GitHub alias '$owner'. Configure usernames without the @ prefix."
        }
        "@$owner"
    }

    $date = $ReleaseDate.ToString('yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
    $replacements = @{
        '`[Provide a clear, concise description of the changes]`' =
            "Prepare Azure MCP Server ``$Version`` for release on $date by compiling the main and VS Code changelogs. The checked-in server version is unchanged."
        '`[Add additional context, screenshots, or information that helps reviewers]`' =
            "$($mentions -join ' '): please review the release notes, approve and merge this PR after required checks pass, and then start the ``mcp - Azure.Mcp.Server`` release pipeline with ``PublishTarget=public`` using the merged commit. This preparation job does not merge the PR or start a release.`n`nPreparation pipeline: [view run]($BuildUrl)."
        '`[Link to the GitHub issue this PR addresses]`' =
            'Not applicable: scheduled release preparation.'
    }
    $body = $Template
    foreach ($placeholder in $replacements.Keys) {
        if (-not $body.Contains($placeholder)) {
            throw "The PR template is missing the expected placeholder: $placeholder"
        }
        $body = $body.Replace($placeholder, $replacements[$placeholder])
    }

    $compileCommand = ".\eng\scripts\Compile-Changelog.ps1 -ChangelogPath 'servers\Azure.Mcp.Server\CHANGELOG.md'"
    $body.TrimEnd() + @"


## Validation
- ``$compileCommand``: passed, including changelog entry validation and VS Code changelog synchronization.
- Builds and tests: not run (release-note-only changes). Required PR checks must pass before merge.

## Invoking Livetests

Copilot submitted PRs are not trustworthy by default. Users with ``write`` access to the repo need to validate the contents of this PR before leaving a comment with the text ``/azp run mcp - pullrequest - live``. This will trigger the necessary livetest workflows to complete required validation.
"@
}

function Invoke-ReleasePrepPullRequest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string[]]$ReleaseOwners,

        [Parameter(Mandatory)]
        [datetime]$ReleaseDate,

        [Parameter(Mandatory)]
        [uri]$BuildUrl,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$GitHubToken,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$PushToken
    )

    Set-StrictMode -Version Latest
    $ErrorActionPreference = 'Stop'
    Push-Location $RepositoryRoot
    try {
        $uncommittedChanges = @(git status --porcelain)
        if ($LASTEXITCODE -ne 0) {
            throw "Unable to check the working tree (git exit code $LASTEXITCODE)."
        }
        if ($uncommittedChanges.Count -gt 0) {
            throw 'Release preparation requires a clean working tree.'
        }

        $serverPath = Join-Path $RepositoryRoot 'servers\Azure.Mcp.Server'
        [xml]$project = Get-Content (Join-Path $serverPath 'src\Azure.Mcp.Server.csproj') -Raw
        $versionNode = $project.SelectSingleNode('/Project/PropertyGroup/Version')
        if ($null -eq $versionNode -or $null -eq [AzureEngSemanticVersion]::ParseVersionString($versionNode.InnerText)) {
            throw 'The Azure MCP Server project must contain a valid release version.'
        }
        $version = $versionNode.InnerText
        $changelogPath = Join-Path $serverPath 'CHANGELOG.md'
        $changelog = Get-Content $changelogPath -Raw
        $heading = [regex]::Match($changelog, '(?m)^## (?<version>\S+) \((?<status>[^)\r\n]+)\)[ \t]*\r?$')
        if (-not $heading.Success -or $heading.Groups['version'].Value -ne $version) {
            throw "The latest changelog heading must match project version '$version'. Merge the post-release version increment before preparing another release."
        }
        if ($heading.Groups['status'].Value -ne 'Unreleased') {
            $releasedDate = [datetime]::MinValue
            if (-not [datetime]::TryParseExact(
                $heading.Groups['status'].Value, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture,
                [Globalization.DateTimeStyles]::None, [ref]$releasedDate)) {
                throw 'The latest changelog heading must contain Unreleased or a valid release date.'
            }
            LogInfo "Version $version is already finalized. Waiting for the post-release version increment."
            return
        }

        $entriesPath = Join-Path $serverPath 'changelog-entries'
        $entries = @(Get-ChildItem $entriesPath -File | Where-Object {
            $_.Extension -in @('.yaml', '.yml') -and $_.BaseName -ne 'username-example-brief-description'
        })
        if ($entries.Count -eq 0) {
            LogInfo 'No changelog entries are available. Skipping release preparation.'
            return
        }

        $body = Format-ReleasePrepPullRequestBody `
            -Template (Get-Content (Join-Path $RepositoryRoot '.github\PULL_REQUEST_TEMPLATE.md') -Raw) `
            -Version $version -ReleaseDate $ReleaseDate -ReleaseOwners $ReleaseOwners -BuildUrl $BuildUrl
        $branch = "prepare-release-Azure.Mcp.Server-$version"
        $existingPullRequests = @(Get-GitHubPullRequests -RepoOwner microsoft -RepoName mcp `
            -Head "azure-sdk:$branch" -Base main -State open -AuthToken $GitHubToken)
        if ($existingPullRequests.Count -gt 0) {
            LogInfo "Release-preparation PR already exists: $($existingPullRequests[0].html_url). Leaving it unchanged."
            return
        }

        $global:LASTEXITCODE = 0
        & (Join-Path $RepositoryRoot 'eng\scripts\Compile-Changelog.ps1') `
            -ChangelogPath 'servers\Azure.Mcp.Server\CHANGELOG.md'
        if ($LASTEXITCODE -ne 0) {
            throw "Changelog compilation failed with exit code $LASTEXITCODE."
        }

        git diff --quiet
        if ($LASTEXITCODE -eq 0) {
            LogInfo 'Changelog compilation produced no changes. Skipping PR creation.'
            return
        }
        if ($LASTEXITCODE -ne 1) {
            throw "Unable to check changelog changes (git exit code $LASTEXITCODE)."
        }

        $title = "Prepare Azure MCP Server $version release"
        $global:LASTEXITCODE = 0
        & (Join-Path $RepositoryRoot 'eng\common\scripts\git-branch-push.ps1') `
            -PRBranchName $branch -CommitMsg $title `
            -GitUrl "https://x-access-token:$PushToken@github.com/azure-sdk/mcp.git"
        if ($LASTEXITCODE -ne 0) {
            throw "Pushing the release-preparation branch failed with exit code $LASTEXITCODE."
        }

        & (Join-Path $RepositoryRoot 'eng\common\scripts\Submit-PullRequest.ps1') `
            -RepoOwner microsoft -RepoName mcp -BaseBranch main -PROwner azure-sdk -PRBranch $branch `
            -AuthToken $GitHubToken -PRTitle $title -PRBody $body -MaintainerCanModify $false
        if ($LASTEXITCODE -ne 0) {
            throw "Submitting the release-preparation PR failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}
