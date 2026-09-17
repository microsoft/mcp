# Pester tests for changelog management scripts
# Run: Invoke-Pester eng/scripts/tests/Changelog.Tests.ps1

BeforeAll {
    $scriptsDir = (Resolve-Path "$PSScriptRoot/..").Path
    $newEntryScript = Join-Path $scriptsDir "New-ChangelogEntry.ps1"
    $compileScript = Join-Path $scriptsDir "Compile-Changelog.ps1"
    $schemaPath = (Resolve-Path "$PSScriptRoot/../../schemas/changelog-entry.schema.json").Path
    $repoRoot = (Resolve-Path "$PSScriptRoot/../../..").Path

    $testRootRelative = "eng/scripts/tests/.testruns"
    $testRootDir = Join-Path $repoRoot $testRootRelative

    if (Test-Path $testRootDir) {
        Remove-Item -Recurse -Force $testRootDir
    }
    New-Item -ItemType Directory -Path $testRootDir -Force | Out-Null

    # Load helper functions from Compile-Changelog.ps1 into test scope
    $compileScriptContent = Get-Content -Path $compileScript -Raw
    $normMatch = [regex]::Match($compileScriptContent, '(?s)(function Normalize-Indentation\s*\{.*?\n\})')
    if ($normMatch.Success) {
        Invoke-Expression $normMatch.Value
    }
    $funcMatch = [regex]::Match($compileScriptContent, '(?s)(function Format-ChangelogEntry\s*\{.*?\n\})')
    if ($funcMatch.Success) {
        Invoke-Expression $funcMatch.Value
    }
}

AfterAll {
    if (Test-Path $testRootDir) {
        Remove-Item -Recurse -Force $testRootDir
    }
}

Describe "New-ChangelogEntry.ps1" {
    BeforeEach {
        $serverRelative = "$testRootRelative/MockServer"
        $serverDir = Join-Path $repoRoot $serverRelative
        $entriesDir = Join-Path $serverDir "changelog-entries"
        if (Test-Path $serverDir) {
            Remove-Item -Recurse -Force $serverDir
        }
        New-Item -ItemType Directory -Path $entriesDir -Force | Out-Null
        $changelogPath = Join-Path $serverDir "CHANGELOG.md"
        Set-Content -Path $changelogPath -Value "# CHANGELOG`n`n## 1.0.0 (2026-01-01)`n`n### Features Added`n`n- Initial release."
    }

    It "Generates entry with Contributor parameter" {
        $relChangelog = "$testRootRelative/MockServer/CHANGELOG.md"
        & $newEntryScript -ChangelogPath $relChangelog `
            -Description "Added support for contributor attribution in changelog" `
            -Section "Features Added" `
            -PR 156 `
            -Contributor "community-dev" `
            -Filename "test-contributor.yaml"

        $entryFile = Join-Path $repoRoot "$testRootRelative/MockServer/changelog-entries/test-contributor.yaml"
        Test-Path $entryFile | Should -Be $true

        $content = Get-Content -Path $entryFile -Raw
        $content | Should -Match "pr: 156"
        $content | Should -Match 'contributor: "community-dev"'
        $content | Should -Match 'section: "Features Added"'
    }

    It "Normalizes Contributor parameter by stripping leading @" {
        $relChangelog = "$testRootRelative/MockServer/CHANGELOG.md"
        & $newEntryScript -ChangelogPath $relChangelog `
            -Description "Added leading at normalization test entry" `
            -Section "Features Added" `
            -Contributor "@octocat" `
            -Filename "test-at-strip.yaml"

        $entryFile = Join-Path $repoRoot "$testRootRelative/MockServer/changelog-entries/test-at-strip.yaml"
        Test-Path $entryFile | Should -Be $true

        $content = Get-Content -Path $entryFile -Raw
        $content | Should -Match 'contributor: "octocat"'
        $content | Should -Not -Match 'contributor: "@octocat"'
    }

    It "Rejects invalid Contributor username with special characters" {
        $relChangelog = "$testRootRelative/MockServer/CHANGELOG.md"
        $res = pwsh -File $newEntryScript -ChangelogPath $relChangelog `
            -Description "Attempting invalid contributor name" `
            -Section "Features Added" `
            -Contributor "invalid user name!" `
            -Filename "test-invalid.yaml" 2>&1
        $LASTEXITCODE | Should -Be 1
    }

    It "Generates backward-compatible entry without Contributor" {
        $relChangelog = "$testRootRelative/MockServer/CHANGELOG.md"
        & $newEntryScript -ChangelogPath $relChangelog `
            -Description "Entry without contributor parameter specified" `
            -Section "Bugs Fixed" `
            -PR 200 `
            -Filename "test-no-contributor.yaml"

        $entryFile = Join-Path $repoRoot "$testRootRelative/MockServer/changelog-entries/test-no-contributor.yaml"
        Test-Path $entryFile | Should -Be $true

        $content = Get-Content -Path $entryFile -Raw
        $content | Should -Match "pr: 200"
        $content | Should -Not -Match "contributor:"
    }
}

Describe "Changelog Entry Schema Validation" {
    It "Accepts entry with root-level contributor" {
        $schema = Get-Content -Path $schemaPath -Raw | ConvertFrom-Json
        $schema.properties.contributor | Should -Not -BeNullOrEmpty
        $schema.properties.contributor.type | Should -Be "string"
        $schema.properties.contributor.pattern | Should -Be "^@?[a-zA-Z0-9-]+$"
    }

    It "Accepts entry with change-level contributor" {
        $schema = Get-Content -Path $schemaPath -Raw | ConvertFrom-Json
        $changeContributor = $schema.properties.changes.items.properties.contributor
        $changeContributor | Should -Not -BeNullOrEmpty
        $changeContributor.type | Should -Be "string"
        $changeContributor.pattern | Should -Be "^@?[a-zA-Z0-9-]+$"
    }
}

Describe "Compile-Changelog.ps1 Formatting Function" {
    It "Formats single-line entry with Contributor and PR" {
        $result = Format-ChangelogEntry -Description "Added new security scanning tool" -PR 1234 -Contributor "octocat"
        $expected = "- Added new security scanning tool. (contributed by [@octocat](https://github.com/octocat)) [[#1234](https://github.com/microsoft/mcp/pull/1234)]"
        $result | Should -Be $expected
    }

    It "Formats single-line entry with Contributor having @" {
        $result = Format-ChangelogEntry -Description "Added new feature" -PR 1234 -Contributor "@octocat"
        $expected = "- Added new feature. (contributed by [@octocat](https://github.com/octocat)) [[#1234](https://github.com/microsoft/mcp/pull/1234)]"
        $result | Should -Be $expected
    }

    It "Formats single-line entry with Contributor without PR" {
        $result = Format-ChangelogEntry -Description "Fixed edge case in parsing" -PR 0 -Contributor "contributor1"
        $expected = "- Fixed edge case in parsing. (contributed by [@contributor1](https://github.com/contributor1))"
        $result | Should -Be $expected
    }

    It "Formats single-line entry without Contributor (backward compatibility)" {
        $result = Format-ChangelogEntry -Description "Existing entry without contributor" -PR 5678 -Contributor ""
        $expected = "- Existing entry without contributor. [[#5678](https://github.com/microsoft/mcp/pull/5678)]"
        $result | Should -Be $expected
    }

    It "Formats multi-line list entry with Contributor and PR" {
        $description = @"
Added new subcommands:
- subcmd_one: First command
- subcmd_two: Second command
"@
        $result = Format-ChangelogEntry -Description $description -PR 999 -Contributor "alice"
        $lines = $result -split "`n"
        $lines[0] | Should -Be "- Added new subcommands: (contributed by [@alice](https://github.com/alice)) [[#999](https://github.com/microsoft/mcp/pull/999)]"
        $lines[1] | Should -Be "  - subcmd_one: First command"
        $lines[2] | Should -Be "  - subcmd_two: Second command"
    }

    It "Formats regular multi-line entry with Contributor and PR on last line" {
        $description = @"
First line of descriptive text.
Second line of descriptive text explaining the fix
"@
        $result = Format-ChangelogEntry -Description $description -PR 888 -Contributor "bob"
        $lines = $result -split "`n"
        $lines[0] | Should -Be "- First line of descriptive text."
        $lines[1] | Should -Be "  Second line of descriptive text explaining the fix. (contributed by [@bob](https://github.com/bob)) [[#888](https://github.com/microsoft/mcp/pull/888)]"
    }
}

Describe "End-to-End Changelog Compilation" {
    BeforeEach {
        $testServerRelative = "$testRootRelative/E2EServer"
        $testServerDir = Join-Path $repoRoot $testServerRelative
        $testEntriesDir = Join-Path $testServerDir "changelog-entries"
        $testVscodeDir = Join-Path $testServerDir "vscode"
        if (Test-Path $testServerDir) {
            Remove-Item -Recurse -Force $testServerDir
        }
        New-Item -ItemType Directory -Path $testEntriesDir -Force | Out-Null
        New-Item -ItemType Directory -Path $testVscodeDir -Force | Out-Null

        $testChangelogPath = Join-Path $testServerDir "CHANGELOG.md"
        $initialChangelog = @"
# CHANGELOG

## 1.0.0-beta.1 (Unreleased)

### Features Added

- Initial feature. [[#100](https://github.com/microsoft/mcp/pull/100)]
"@
        Set-Content -Path $testChangelogPath -Value $initialChangelog

        $testVscodeChangelogPath = Join-Path $testVscodeDir "CHANGELOG.md"
        $initialVscode = @"
# Release History

## 1.0.1 (2026-01-01)

### Added

- Initial feature. [[#100](https://github.com/microsoft/mcp/pull/100)]
"@
        Set-Content -Path $testVscodeChangelogPath -Value $initialVscode
    }

    It "Compiles entries with contributor attribution in DryRun mode" {
        $testEntriesDir = Join-Path $repoRoot "$testRootRelative/E2EServer/changelog-entries"
        
        # Entry 1: with contributor
        $entry1Content = @"
pr: 156
contributor: "octocat"
changes:
  - section: "Features Added"
    description: "Added contributor attribution to changelogs"
"@
        Set-Content -Path (Join-Path $testEntriesDir "entry1.yaml") -Value $entry1Content

        # Entry 2: without contributor
        $entry2Content = @"
pr: 157
changes:
  - section: "Bugs Fixed"
    description: "Fixed minor bug in release script"
"@
        Set-Content -Path (Join-Path $testEntriesDir "entry2.yaml") -Value $entry2Content

        $relChangelog = "$testRootRelative/E2EServer/CHANGELOG.md"
        $output = pwsh -File $compileScript -ChangelogPath $relChangelog -DryRun *>&1 | Out-String

        $output | Should -Match "\(contributed by \[@octocat\]\(https://github\.com/octocat\)\) \[\[#156\]\(https://github\.com/microsoft/mcp/pull/156\)\]"
        $output | Should -Match "Fixed minor bug in release script\. \[\[#157\]\(https://github\.com/microsoft/mcp/pull/157\)\]"
        $output | Should -Match "DRY RUN - No files were modified"
    }

    It "Compiles entries with contributor on individual change items" {
        $testEntriesDir = Join-Path $repoRoot "$testRootRelative/E2EServer/changelog-entries"
        
        $entryContent = @"
pr: 158
changes:
  - section: "Features Added"
    description: "Multi-author change item one"
    contributor: "alice"
  - section: "Features Added"
    description: "Multi-author change item two"
    contributor: "bob"
"@
        Set-Content -Path (Join-Path $testEntriesDir "per-change-entry.yaml") -Value $entryContent

        $relChangelog = "$testRootRelative/E2EServer/CHANGELOG.md"
        $output = pwsh -File $compileScript -ChangelogPath $relChangelog -DryRun *>&1 | Out-String

        $output | Should -Match "\(contributed by \[@alice\]\(https://github\.com/alice\)\) \[\[#158\]"
        $output | Should -Match "\(contributed by \[@bob\]\(https://github\.com/bob\)\) \[\[#158\]"
    }
}
