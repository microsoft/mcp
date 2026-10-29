# cSpell:ignore Juneteenth
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidGlobalVars', '', Justification = 'Fixture scripts share mock call state across script scopes, and AfterAll removes that state.')]
param()

BeforeAll {
    . "$PSScriptRoot\..\..\common\scripts\common.ps1"
    . "$PSScriptRoot\ReleasePrepHelpers.ps1"
    Import-Module powershell-yaml -MinimumVersion 0.4.7 -ErrorAction Stop

    $script:PrTemplate = Get-Content (Join-Path $RepoRoot '.github\PULL_REQUEST_TEMPLATE.md') -Raw
    $script:Pipeline = Get-Content (Join-Path $RepoRoot 'eng\pipelines\release-prep.yml') -Raw | ConvertFrom-Yaml
}

Describe 'Microsoft US holiday calendar' {
    It 'recognizes <Name> on <Date>' -ForEach @(
        @{ Date = '2026-01-01'; Name = "New Year's Day" }
        @{ Date = '2026-01-19'; Name = 'Martin Luther King Jr. Day' }
        @{ Date = '2026-02-16'; Name = "Washington's Birthday" }
        @{ Date = '2026-05-25'; Name = 'Memorial Day' }
        @{ Date = '2026-07-04'; Name = 'Independence Day' }
        @{ Date = '2026-09-07'; Name = 'Labor Day' }
        @{ Date = '2026-11-26'; Name = 'Thanksgiving Day' }
        @{ Date = '2026-11-27'; Name = 'Day after Thanksgiving' }
        @{ Date = '2026-12-24'; Name = 'Christmas Eve' }
        @{ Date = '2026-12-25'; Name = 'Christmas Day' }
    ) {
        Get-USFederalHoliday -Date ([datetime]$Date) | Should -Be $Name
    }

    It 'recognizes exactly 10 distinct Microsoft holidays' {
        $holidayNames = for ($date = [datetime]::new(2026, 1, 1); $date -lt [datetime]::new(2027, 1, 1); $date = $date.AddDays(1)) {
            $holiday = Get-USFederalHoliday -Date $date
            if ($holiday) {
                $holiday -replace ' \(observed\)$', ''
            }
        }
        @($holidayNames | Sort-Object -Unique).Count | Should -Be 10
    }

    It 'recognizes the observed date of <Name> on <Date>' -ForEach @(
        @{ Date = '2026-07-03'; Name = 'Independence Day (observed)' }
        @{ Date = '2027-07-05'; Name = 'Independence Day (observed)' }
        @{ Date = '2023-01-02'; Name = "New Year's Day (observed)" }
        @{ Date = '2021-12-31'; Name = "New Year's Day (observed)" }
        @{ Date = '2022-12-23'; Name = 'Christmas Eve (observed)' }
        @{ Date = '2022-12-26'; Name = 'Christmas Day (observed)' }
    ) {
        Get-USFederalHoliday -Date ([datetime]$Date) | Should -Be $Name
    }

    It 'recognizes Memorial Day when the last Monday is the fifth Monday' {
        Get-USFederalHoliday -Date '2027-05-31' | Should -Be 'Memorial Day'
    }

    It "recognizes New Year's Day on its actual weekend date" {
        Get-USFederalHoliday -Date '2022-01-01' | Should -Be "New Year's Day"
    }

    It 'recognizes the Friday immediately after Thanksgiving on <Date>' -ForEach @(
        @{ Date = '2018-11-23' }
        @{ Date = '2024-11-29' }
        @{ Date = '2030-11-29' }
    ) {
        Get-USFederalHoliday -Date ([datetime]$Date) | Should -Be 'Day after Thanksgiving'
    }

    It 'does not exclude <Name> on <Date>' -ForEach @(
        @{ Date = '2026-06-19'; Name = 'Juneteenth' }
        @{ Date = '2026-10-12'; Name = 'Columbus Day' }
        @{ Date = '2026-11-11'; Name = 'Veterans Day' }
        @{ Date = '2026-04-03'; Name = 'Good Friday' }
        @{ Date = '2026-11-03'; Name = 'an ordinary Tuesday' }
        @{ Date = '2027-05-24'; Name = 'the Monday before Memorial Day' }
        @{ Date = '2024-11-22'; Name = 'the Friday before Thanksgiving' }
        @{ Date = '2030-11-22'; Name = 'the Friday before Thanksgiving' }
        @{ Date = '2018-11-30'; Name = 'the Friday a week after Thanksgiving' }
    ) {
        Get-USFederalHoliday -Date ([datetime]$Date) | Should -BeNullOrEmpty
    }
}

Describe 'Pacific release-preparation schedules' {
    It 'selects exactly one noon-Pacific trigger on every eligible day from 2026 through 2030' {
        $scheduledHolidayDates = @(
            '2026-01-01', '2026-11-26', '2026-12-24',
            '2027-11-25',
            '2028-07-04', '2028-11-23',
            '2029-11-22', '2029-12-25',
            '2030-01-01', '2030-07-04', '2030-11-28', '2030-12-24'
        )
        $zone = [TimeZoneInfo]::FindSystemTimeZoneById('Pacific Standard Time')
        for ($date = [datetime]::new(2026, 1, 1); $date -lt [datetime]::new(2031, 1, 1); $date = $date.AddDays(1)) {
            if ($date.DayOfWeek -notin @([DayOfWeek]::Tuesday, [DayOfWeek]::Thursday)) {
                continue
            }
            $eligibleSchedules = @(
                foreach ($schedule in $script:Pipeline.schedules) {
                    $hour = [int]($schedule.cron -split ' ')[1]
                    $utcTime = [DateTimeOffset]::new($date.AddHours($hour), [timespan]::Zero)
                    $decision = Get-ReleasePrepSchedule -ScheduleName $schedule.displayName -UtcNow $utcTime
                    if ($decision.ShouldRun) {
                        [TimeZoneInfo]::ConvertTime($utcTime, $zone).Hour | Should -Be 12 -Because "$date must run at noon Pacific"
                        $decision.ReleaseDate | Should -Be $date.ToString('yyyy-MM-dd')
                        $schedule
                    }
                }
            )
            $expectedCount = if ($date.ToString('yyyy-MM-dd') -in $scheduledHolidayDates) { 0 } else { 1 }
            $eligibleSchedules.Count | Should -Be $expectedCount -Because "the calendar must allow exactly $expectedCount trigger(s) on $date"
        }
    }

    It 'does not run on a Wednesday' {
        $decision = Get-ReleasePrepSchedule -ScheduleName 'Release preparation (PDT)' -UtcNow '2026-10-28T19:00:00Z'
        $decision.ShouldRun | Should -BeFalse
        $decision.Reason | Should -Match 'not a Tuesday or Thursday'
    }

    It 'skips Thanksgiving using the Pacific date rather than the UTC date' {
        $decision = Get-ReleasePrepSchedule -ScheduleName 'Release preparation (PST)' -UtcNow '2026-11-27T01:00:00Z'
        $decision.ShouldRun | Should -BeFalse
        $decision.ReleaseDate | Should -Be '2026-11-26'
        $decision.Reason | Should -Match 'Thanksgiving Day'
    }

    It 'uses the selected schedule rather than the delayed agent start hour' {
        $decision = Get-ReleasePrepSchedule -ScheduleName 'Release preparation (PDT)' -UtcNow '2026-10-29T20:45:00Z'
        $decision.ShouldRun | Should -BeTrue
    }

    It 'rejects an unrecognized schedule instead of choosing a default offset' {
        { Get-ReleasePrepSchedule -ScheduleName 'Unknown schedule' } | Should -Throw '*Unrecognized release-preparation schedule*'
    }
}

Describe 'Release-preparation PR description' {
    BeforeEach {
        $script:BodyParameters = @{
            Template = $script:PrTemplate
            Version = '4.0.0-beta.5'
            ReleaseDate = [datetime]'2026-11-03'
            ReleaseOwners = @('owner-one', 'owner-two')
            BuildUrl = [uri]'https://dev.azure.com/azure-sdk/internal/_build/results?buildId=123'
        }
    }

    It 'mentions every configured owner and describes the manual release follow-up' {
        $body = Format-ReleasePrepPullRequestBody @script:BodyParameters
        $body | Should -Match '@owner-one @owner-two'
        $body | Should -Match 'approve and merge this PR after required checks pass'
        $body | Should -Match 'PublishTarget=public'
        $body | Should -Match 'using the merged commit'
        $body | Should -Match 'does not merge the PR or start a release'
        $body | Should -Match '4\.0\.0-beta\.5'
        $body | Should -Match '2026-11-03'
        $body | Should -Match 'https://dev.azure.com/azure-sdk/internal/_build/results\?buildId=123'
    }

    It 'renders every alias configured in the pipeline defaults' {
        $script:BodyParameters.ReleaseOwners = @($script:Pipeline.parameters[0].default)
        $body = Format-ReleasePrepPullRequestBody @script:BodyParameters
        foreach ($owner in $script:BodyParameters.ReleaseOwners) {
            $body.Contains("@$owner") | Should -BeTrue
        }
    }

    It 'preserves all template headings and checklist items in their original order' {
        $body = Format-ReleasePrepPullRequestBody @script:BodyParameters
        $originalHeadings = @([regex]::Matches($script:PrTemplate, '(?m)^## .+\r?$') | ForEach-Object { $_.Value.TrimEnd() })
        $bodyHeadings = @([regex]::Matches($body, '(?m)^## .+\r?$') | ForEach-Object { $_.Value.TrimEnd() })
        ($bodyHeadings[0..($originalHeadings.Count - 1)] -join "`n") | Should -Be ($originalHeadings -join "`n")

        $originalChecklist = [regex]::Matches($script:PrTemplate, '(?m)^[ \t]*- \[[ x]\].+\r?$').Value -join "`n"
        $bodyChecklist = [regex]::Matches($body, '(?m)^[ \t]*- \[[ x]\].+\r?$').Value -join "`n"
        $bodyChecklist | Should -Be $originalChecklist
        $body | Should -Not -Match '\[Provide a clear|\[Add additional|\[Link to the GitHub issue'
        $bodyHeadings[-1] | Should -Be '## Invoking Livetests'
    }

    It 'records the exact compilation command and truthful validation results' {
        $body = Format-ReleasePrepPullRequestBody @script:BodyParameters
        $command = ".\eng\scripts\Compile-Changelog.ps1 -ChangelogPath 'servers\Azure.Mcp.Server\CHANGELOG.md'"
        $body.Contains("``$command``: passed") | Should -BeTrue
        $body | Should -Match 'Builds and tests: not run'
    }

    It 'rejects invalid aliases before any release changes' -ForEach @(
        @{ Alias = '@owner-one' }
        @{ Alias = 'owner one' }
        @{ Alias = 'owner/another' }
        @{ Alias = "owner`nInjected text" }
    ) {
        $script:BodyParameters.ReleaseOwners = @($Alias)
        { Format-ReleasePrepPullRequestBody @script:BodyParameters } | Should -Throw '*Invalid GitHub alias*'
    }

    It 'requires at least one follow-up owner' {
        $script:BodyParameters.ReleaseOwners = @()
        { Format-ReleasePrepPullRequestBody @script:BodyParameters } | Should -Throw
    }

    It 'fails explicitly if the repository template changes unexpectedly' {
        $script:BodyParameters.Template = '## Different template'
        { Format-ReleasePrepPullRequestBody @script:BodyParameters } | Should -Throw '*missing the expected placeholder*'
    }

    It 'rejects a non-HTTPS pipeline link' {
        $script:BodyParameters.BuildUrl = [uri]'http://example.test/build'
        { Format-ReleasePrepPullRequestBody @script:BodyParameters } | Should -Throw '*absolute HTTPS URL*'
    }
}

Describe 'Release-preparation changelog compilation' {
    It 'finalizes both changelogs and removes entries using the real compiler without changing the version' {
        $fixtureRelativePath = Join-Path '.work' "release-prep-compile-$([guid]::NewGuid().ToString('N'))"
        $fixtureRoot = Join-Path $RepoRoot $fixtureRelativePath
        $relativeServerPath = Join-Path $fixtureRelativePath 'Azure.Mcp.Server'
        $serverPath = Join-Path $RepoRoot $relativeServerPath
        try {
            New-Item -ItemType Directory -Force -Path @(
                (Join-Path $serverPath 'src'),
                (Join-Path $serverPath 'changelog-entries'),
                (Join-Path $serverPath 'vscode')
            ) | Out-Null
            Set-Content (Join-Path $serverPath 'src\Azure.Mcp.Server.csproj') '<Project><PropertyGroup><Version>4.0.0-beta.5</Version></PropertyGroup></Project>'
            Set-Content (Join-Path $serverPath 'CHANGELOG.md') "# CHANGELOG`n`n## 4.0.0-beta.5 (Unreleased)`n"
            Set-Content (Join-Path $serverPath 'vscode\CHANGELOG.md') "# Release History`n`n## 4.0.4 (2026-10-29) (pre-release)`n`nPrevious release.`n"
            Set-Content (Join-Path $serverPath 'vscode\package.json') '{"publisher":"ms-azuretools","name":"vscode-azure-mcp-server"}'
            @'
pr: 1
changes:
  - section: Features Added
    description: Added a release-preparation integration test.
'@ | Set-Content (Join-Path $serverPath 'changelog-entries\test-entry.yaml')

            $global:LASTEXITCODE = 0
            & (Join-Path $RepoRoot 'eng\scripts\Compile-Changelog.ps1') `
                -ChangelogPath (Join-Path $relativeServerPath 'CHANGELOG.md')

            $LASTEXITCODE | Should -Be 0
            $mainChangelog = Get-Content (Join-Path $serverPath 'CHANGELOG.md') -Raw
            $mainChangelog | Should -Match '## 4\.0\.0-beta\.5 \(\d{4}-\d{2}-\d{2}\)'
            $mainChangelog | Should -Not -Match 'Unreleased'
            $extensionChangelog = Get-Content (Join-Path $serverPath 'vscode\CHANGELOG.md') -Raw
            $extensionChangelog | Should -Match '## 4\.0\.5 \(\d{4}-\d{2}-\d{2}\) \(pre-release\)'
            $extensionChangelog | Should -Match 'Added a release-preparation integration test'
            Test-Path (Join-Path $serverPath 'changelog-entries\test-entry.yaml') | Should -BeFalse
            Get-Content (Join-Path $serverPath 'src\Azure.Mcp.Server.csproj') -Raw |
                Should -Match '<Version>4\.0\.0-beta\.5</Version>'
        }
        finally {
            if (Test-Path -LiteralPath $fixtureRoot) {
                Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
            }
        }
    }
}

Describe 'Release-preparation orchestration' {
    BeforeEach {
        $script:FixtureRoot = Join-Path $TestDrive 'repo'
        $serverPath = Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server'
        New-Item -ItemType Directory -Force -Path @(
            (Join-Path $serverPath 'src'),
            (Join-Path $serverPath 'changelog-entries'),
            (Join-Path $script:FixtureRoot '.github'),
            (Join-Path $script:FixtureRoot 'eng\scripts'),
            (Join-Path $script:FixtureRoot 'eng\common\scripts')
        ) | Out-Null
        Set-Content (Join-Path $serverPath 'src\Azure.Mcp.Server.csproj') '<Project><PropertyGroup><Version>4.0.0-beta.5</Version></PropertyGroup></Project>'
        Set-Content (Join-Path $serverPath 'CHANGELOG.md') '## 4.0.0-beta.5 (Unreleased)'
        Set-Content (Join-Path $serverPath 'changelog-entries\test-entry.yaml') 'changes: []'
        Set-Content (Join-Path $script:FixtureRoot '.github\PULL_REQUEST_TEMPLATE.md') $script:PrTemplate

        $global:ReleasePrepCalls = [Collections.Generic.List[object]]::new()
        $global:ReleasePrepCompileExitCode = 0
        $global:ReleasePrepPushExitCode = 0
        $global:ReleasePrepSubmitExitCode = 0
        @'
param($ChangelogPath, $Version)
if (-not (Test-Path (Join-Path "$PSScriptRoot\..\.." $ChangelogPath))) {
    throw 'The compiler requires a repository-relative changelog path.'
}
$global:ReleasePrepCalls.Add(@{ Kind = 'Compile'; ChangelogPath = $ChangelogPath; Version = $Version })
$global:LASTEXITCODE = $global:ReleasePrepCompileExitCode
'@ | Set-Content (Join-Path $script:FixtureRoot 'eng\scripts\Compile-Changelog.ps1')
        @'
param($PRBranchName, $CommitMsg, $GitUrl)
$global:ReleasePrepCalls.Add(@{ Kind = 'Push'; Branch = $PRBranchName; Message = $CommitMsg; Url = $GitUrl })
$global:LASTEXITCODE = $global:ReleasePrepPushExitCode
'@ | Set-Content (Join-Path $script:FixtureRoot 'eng\common\scripts\git-branch-push.ps1')
        @'
param($RepoOwner, $RepoName, $BaseBranch, $PROwner, $PRBranch, $AuthToken, $PRTitle, $PRBody, $MaintainerCanModify)
$global:ReleasePrepCalls.Add(@{
    Kind = 'Submit'; RepoOwner = $RepoOwner; RepoName = $RepoName; BaseBranch = $BaseBranch
    PROwner = $PROwner; Branch = $PRBranch; Title = $PRTitle; Body = $PRBody
})
$global:LASTEXITCODE = $global:ReleasePrepSubmitExitCode
'@ | Set-Content (Join-Path $script:FixtureRoot 'eng\common\scripts\Submit-PullRequest.ps1')

        $script:StatusOutput = @()
        $script:StatusExitCode = 0
        $script:DiffExitCode = 1
        Mock git {
            switch ($args[0]) {
                'status' {
                    $global:LASTEXITCODE = $script:StatusExitCode
                    $script:StatusOutput
                }
                'diff' { $global:LASTEXITCODE = $script:DiffExitCode }
                default { throw "Unexpected git command: $args" }
            }
        }
        Mock Get-GitHubPullRequests { @() }
        $script:PrepParameters = @{
            RepositoryRoot = $script:FixtureRoot
            ReleaseOwners = @('owner-one', 'owner-two')
            ReleaseDate = [datetime]'2026-11-03'
            BuildUrl = [uri]'https://dev.azure.com/azure-sdk/internal/_build/results?buildId=123'
            GitHubToken = 'test-github-token'
            PushToken = 'test-push-token'
        }
    }

    It 'compiles the checked-in version and creates a version-specific PR with the configured mentions' {
        Invoke-ReleasePrepPullRequest @script:PrepParameters

        ($global:ReleasePrepCalls.Kind -join ',') | Should -Be 'Compile,Push,Submit'
        $global:ReleasePrepCalls[0].Version | Should -BeNullOrEmpty
        $global:ReleasePrepCalls[0].ChangelogPath | Should -Be 'servers\Azure.Mcp.Server\CHANGELOG.md'
        $global:ReleasePrepCalls[1].Branch | Should -Be 'prepare-release-Azure.Mcp.Server-4.0.0-beta.5'
        $global:ReleasePrepCalls[2].RepoOwner | Should -Be 'microsoft'
        $global:ReleasePrepCalls[2].RepoName | Should -Be 'mcp'
        $global:ReleasePrepCalls[2].PROwner | Should -Be 'azure-sdk'
        $global:ReleasePrepCalls[2].BaseBranch | Should -Be 'main'
        $global:ReleasePrepCalls[2].Body | Should -Match '@owner-one @owner-two'
        Get-Content (Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj') -Raw |
            Should -Match '<Version>4\.0\.0-beta\.5</Version>'
        Should -Invoke Get-GitHubPullRequests -Times 1 -Exactly -ParameterFilter {
            $Head -eq 'azure-sdk:prepare-release-Azure.Mcp.Server-4.0.0-beta.5' -and $Base -eq 'main' -and $State -eq 'open'
        }
    }

    It 'leaves an existing open preparation PR and its branch unchanged' {
        Mock Get-GitHubPullRequests { [PSCustomObject]@{ html_url = 'https://github.com/microsoft/mcp/pull/1' } }
        Invoke-ReleasePrepPullRequest @script:PrepParameters
        $global:ReleasePrepCalls.Count | Should -Be 0
    }

    It 'does not treat a GitHub error as the absence of an existing PR' {
        Mock Get-GitHubPullRequests { throw 'GitHub unavailable' }
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*GitHub unavailable*'
        $global:ReleasePrepCalls.Count | Should -Be 0
    }

    It 'skips a version that has already been finalized' {
        Set-Content (Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\CHANGELOG.md') '## 4.0.0-beta.5 (2026-10-29)'
        Invoke-ReleasePrepPullRequest @script:PrepParameters
        $global:ReleasePrepCalls.Count | Should -Be 0
        Should -Invoke Get-GitHubPullRequests -Times 0
    }

    It 'skips runs with only the example entry or non-YAML files' {
        $entriesPath = Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\changelog-entries'
        Remove-Item (Join-Path $entriesPath 'test-entry.yaml')
        Set-Content (Join-Path $entriesPath 'username-example-brief-description.yaml') 'changes: []'
        Set-Content (Join-Path $entriesPath 'README.md') 'Entry documentation.'
        Invoke-ReleasePrepPullRequest @script:PrepParameters
        $global:ReleasePrepCalls.Count | Should -Be 0
        Should -Invoke Get-GitHubPullRequests -Times 0
    }

    It 'rejects a mismatched changelog version' {
        Set-Content (Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\CHANGELOG.md') '## 4.0.0-beta.4 (Unreleased)'
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*must match project version*'
        $global:ReleasePrepCalls.Count | Should -Be 0
    }

    It 'rejects a malformed changelog date instead of silently skipping it' {
        Set-Content (Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\CHANGELOG.md') '## 4.0.0-beta.5 (pending)'
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*valid release date*'
    }

    It 'rejects an invalid project version' {
        Set-Content (Join-Path $script:FixtureRoot 'servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj') '<Project><PropertyGroup><Version>invalid</Version></PropertyGroup></Project>'
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*valid release version*'
    }

    It 'refuses a dirty checkout' {
        $script:StatusOutput = @(' M unrelated.txt')
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*clean working tree*'
        $global:ReleasePrepCalls.Count | Should -Be 0
    }

    It 'fails explicitly if git status fails' {
        $script:StatusExitCode = 128
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*git exit code 128*'
    }

    It 'does not push after failed compilation' {
        $global:ReleasePrepCompileExitCode = 17
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*compilation failed with exit code 17*'
        ($global:ReleasePrepCalls.Kind -join ',') | Should -Be 'Compile'
    }

    It 'does not open a PR if compilation produces no changes' {
        $script:DiffExitCode = 0
        Invoke-ReleasePrepPullRequest @script:PrepParameters
        ($global:ReleasePrepCalls.Kind -join ',') | Should -Be 'Compile'
    }

    It 'does not mistake a git diff failure for release changes' {
        $script:DiffExitCode = 128
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*git exit code 128*'
        ($global:ReleasePrepCalls.Kind -join ',') | Should -Be 'Compile'
    }

    It 'does not submit a PR after a failed push' {
        $global:ReleasePrepPushExitCode = 23
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*branch failed with exit code 23*'
        ($global:ReleasePrepCalls.Kind -join ',') | Should -Be 'Compile,Push'
    }

    It 'surfaces PR submission failures' {
        $global:ReleasePrepSubmitExitCode = 29
        { Invoke-ReleasePrepPullRequest @script:PrepParameters } | Should -Throw '*PR failed with exit code 29*'
    }

    AfterAll {
        Remove-Variable -Scope Global -Name ReleasePrepCalls, ReleasePrepCompileExitCode, ReleasePrepPushExitCode, ReleasePrepSubmitExitCode
    }
}

Describe 'Release-preparation pipeline configuration' {
    It 'disables CI and PR triggers and schedules both UTC candidates on main' {
        $script:Pipeline.trigger | Should -Be 'none'
        $script:Pipeline.pr | Should -Be 'none'
        ($script:Pipeline.schedules.cron -join '|') | Should -Be '0 19 * * 2,4|0 20 * * 2,4'
        foreach ($schedule in $script:Pipeline.schedules) {
            $schedule.always | Should -BeTrue
            $schedule.branches.include | Should -Contain 'main'
        }
    }

    It 'gates GitHub access and PR preparation on the calendar decision' {
        $stage = $script:Pipeline.extends.parameters.stages[0]
        $stage.condition | Should -Match "eq\(variables\['System.TeamProject'\], 'internal'\)"
        $stage.condition | Should -Match "eq\(variables\['Build.Repository.Name'\], 'microsoft/mcp'\)"
        $stage.condition | Should -Match "eq\(variables\['Build.SourceBranch'\], 'refs/heads/main'\)"
        $stage.jobs[0].condition | Should -Match "eq\(variables\['Build.Reason'\], 'Schedule'\)"
        $stage.jobs[1].condition | Should -Match "dependencies.CheckSchedule.outputs\['schedule.ShouldPrepareRelease'\], 'true'"
        $stage.jobs[1].steps[-1].env.RELEASE_OWNERS | Should -Be '${{ convertToJson(parameters.ReleaseOwners) }}'
        $stage.jobs[1].steps[0].fetchDepth | Should -Be 0
    }
}
