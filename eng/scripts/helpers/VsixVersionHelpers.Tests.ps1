BeforeAll {
    . "$PSScriptRoot/../../common/scripts/SemVer.ps1"
    . "$PSScriptRoot/VsixVersionHelpers.ps1"
}

Describe "Resolve-PublicVsixVersion" {
    BeforeEach {
        $script:packageJsonPath = Join-Path $TestDrive "package.json"
        @{
            publisher = "ms-azuretools"
            name = "vscode-azure-mcp-server"
        } | ConvertTo-Json | Set-Content $script:packageJsonPath
    }

    It "maps beta server versions without querying the marketplace" {
        $marketplaceProvider = { throw "Marketplace should not be queried." }

        $result = Resolve-PublicVsixVersion `
            -ServerName "Azure.Mcp.Server" `
            -ServerVersion "3.0.0-beta.49" `
            -PackageJsonPath $script:packageJsonPath `
            -MarketplaceVersionProvider $marketplaceProvider

        $result.Version | Should -Be "3.0.49"
        $result.IsPrerelease | Should -BeTrue
        $result.Source | Should -Be "BetaMapping"
    }

    It "uses the next marketplace patch for stable Azure MCP releases" {
        $marketplaceProvider = {
            param($PublisherId, $ExtensionId, $MajorVersion)

            $PublisherId | Should -Be "ms-azuretools"
            $ExtensionId | Should -Be "vscode-azure-mcp-server"
            $MajorVersion | Should -Be 2
            [PSCustomObject]@{
                ExtensionPublished = $true
                LatestVersion = "2.0.42"
                MaxPatch = 42
                NextPatch = 43
            }
        }

        $result = Resolve-PublicVsixVersion `
            -ServerName "Azure.Mcp.Server" `
            -ServerVersion "2.0.2" `
            -PackageJsonPath $script:packageJsonPath `
            -MarketplaceVersionProvider $marketplaceProvider

        $result.Version | Should -Be "2.0.43"
        $result.IsPrerelease | Should -BeFalse
        $result.Source | Should -Be "Marketplace"
        $result.MarketplaceLatestVersion | Should -Be "2.0.42"
    }

    It "uses the server version for stable Fabric releases" {
        $marketplaceProvider = { throw "Marketplace should not be queried." }

        $result = Resolve-PublicVsixVersion `
            -ServerName "Fabric.Mcp.Server" `
            -ServerVersion "1.2.0" `
            -PackageJsonPath $script:packageJsonPath `
            -MarketplaceVersionProvider $marketplaceProvider

        $result.Version | Should -Be "1.2.0"
        $result.IsPrerelease | Should -BeFalse
        $result.Source | Should -Be "ServerVersion"
    }

    It "uses 1.0.0 only for a first stable release of an unpublished extension" {
        $marketplaceProvider = {
            [PSCustomObject]@{ ExtensionPublished = $false; LatestVersion = $null; MaxPatch = $null; NextPatch = $null }
        }

        $result = Resolve-PublicVsixVersion `
            -ServerName "Azure.Mcp.Server" `
            -ServerVersion "1.0.0" `
            -PackageJsonPath $script:packageJsonPath `
            -MarketplaceVersionProvider $marketplaceProvider

        $result.Version | Should -Be "1.0.0"
        $result.Source | Should -Be "FirstGaRelease"
    }

    It "uses the next marketplace patch for 1.0.0 when the 1.0.X series is already published" {
        $marketplaceProvider = {
            [PSCustomObject]@{ ExtensionPublished = $true; LatestVersion = "1.0.4"; MaxPatch = 4; NextPatch = 5 }
        }

        $result = Resolve-PublicVsixVersion `
            -ServerName "Azure.Mcp.Server" `
            -ServerVersion "1.0.0" `
            -PackageJsonPath $script:packageJsonPath `
            -MarketplaceVersionProvider $marketplaceProvider

        $result.Version | Should -Be "1.0.5"
        $result.Source | Should -Be "Marketplace"
    }

    It "does not use the first-GA exception when only other major series are published" {
        $marketplaceProvider = {
            [PSCustomObject]@{ ExtensionPublished = $true; LatestVersion = $null; MaxPatch = $null; NextPatch = $null }
        }

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "1.0.0" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*first-GA exception*applies only when the extension has no published versions*"
    }

    It "fails a later stable release of an unpublished extension" {
        $marketplaceProvider = {
            [PSCustomObject]@{ ExtensionPublished = $false; LatestVersion = $null; MaxPatch = $null; NextPatch = $null }
        }

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "2.0.2" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*No marketplace versions were found for the 2.0.X series*"
    }

    It "fails a later stable release when its major series has no marketplace history" {
        $marketplaceProvider = {
            [PSCustomObject]@{ ExtensionPublished = $true; LatestVersion = $null; MaxPatch = $null; NextPatch = $null }
        }

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "2.0.2" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*No marketplace versions were found for the 2.0.X series*"
    }

    It "rejects a marketplace provider result without the expected shape" -ForEach @(
        @{ Name = "no result"; Result = $null }
        @{ Name = "missing ExtensionPublished"; Result = [PSCustomObject]@{ NextPatch = 43 } }
        @{ Name = "missing NextPatch"; Result = [PSCustomObject]@{ ExtensionPublished = $true } }
    ) {
        $marketplaceProvider = { $Result }.GetNewClosure()

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "2.0.2" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*invalid result*"
    }

    It "fails when stable release package metadata is incomplete" {
        Set-StrictMode -Version Latest
        @{ name = "vscode-azure-mcp-server" } |
            ConvertTo-Json |
            Set-Content $script:packageJsonPath

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "2.0.2" `
                -PackageJsonPath $script:packageJsonPath
        } | Should -Throw "*Publisher or extension name was not found*"
    }

    It "does not treat a marketplace failure as a first GA release" {
        $marketplaceProvider = { throw "Marketplace is unavailable." }

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "1.0.0" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*Marketplace is unavailable*"
    }

    It "rejects an invalid server version" {
        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "not-a-version" `
                -PackageJsonPath $script:packageJsonPath
        } | Should -Throw "*not a supported semantic version*"
    }

    It "rejects an invalid next marketplace patch" {
        $marketplaceProvider = {
            [PSCustomObject]@{
                ExtensionPublished = $true
                LatestVersion = "2.0.42"
                NextPatch = "invalid"
            }
        }

        {
            Resolve-PublicVsixVersion `
                -ServerName "Azure.Mcp.Server" `
                -ServerVersion "2.0.2" `
                -PackageJsonPath $script:packageJsonPath `
                -MarketplaceVersionProvider $marketplaceProvider
        } | Should -Throw "*invalid next patch*"
    }
}

Describe "Get-LatestMarketplaceVersion" {
    It "increments the numeric maximum across release channels and platform duplicates" {
        Mock Invoke-RestMethod {
            @'
{
  "results": [
    {
      "extensions": [
        {
          "versions": [
            { "version": "2.0.9", "targetPlatform": "win32-x64" },
            { "version": "3.0.99", "targetPlatform": "win32-x64" },
            { "version": "2.0.42", "targetPlatform": "win32-x64", "properties": [{ "key": "Microsoft.VisualStudio.Code.PreRelease", "value": "true" }] },
            { "version": "2.0.42", "targetPlatform": "linux-x64" },
            { "version": "2.1.80", "targetPlatform": "win32-x64" },
            { "version": "2.0.10", "targetPlatform": "win32-x64" }
          ]
        }
      ]
    }
  ]
}
'@ | ConvertFrom-Json
        }

        $result = Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 2

        $result.ExtensionPublished | Should -BeTrue
        $result.LatestVersion | Should -Be "2.0.42"
        $result.MaxPatch | Should -Be 42
        $result.NextPatch | Should -Be 43
        Should -Invoke Invoke-RestMethod -Times 1 -Exactly -ParameterFilter {
            $Method -eq "Post" -and
            $ErrorAction -eq "Stop" -and
            ($Body | ConvertFrom-Json).filters[0].criteria[0].value -eq "ms-azuretools.vscode-azure-mcp-server"
        }
    }

    It "requests the full version history so older major series are found" {
        Mock Invoke-RestMethod {
            $flags = ($Body | ConvertFrom-Json).flags
            if (($flags -band 512) -ne 0) {
                '{"results":[{"extensions":[{"versions":[{"version":"3.0.50","targetPlatform":"win32-x64"}]}]}]}' | ConvertFrom-Json
            }
            elseif (($flags -band 1) -ne 0) {
                '{"results":[{"extensions":[{"versions":[{"version":"3.0.50"},{"version":"2.0.46"},{"version":"2.0.45"},{"version":"1.0.4"}]}]}]}' | ConvertFrom-Json
            }
            else {
                '{"results":[{"extensions":[{"versions":[]}]}]}' | ConvertFrom-Json
            }
        }

        $result = Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 2

        $result.LatestVersion | Should -Be "2.0.46"
        $result.NextPatch | Should -Be 47
        Should -Invoke Invoke-RestMethod -Times 1 -Exactly -ParameterFilter {
            $flags = ($Body | ConvertFrom-Json).flags
            ($flags -band 1) -eq 1 -and ($flags -band 512) -eq 0
        }
    }

    It "reports an unpublished extension as having no versions in strict mode" {
        Set-StrictMode -Version Latest
        Mock Invoke-RestMethod { '{"results":[{"extensions":[]}]}' | ConvertFrom-Json }

        $result = Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 1

        $result.ExtensionPublished | Should -BeFalse
        $result.LatestVersion | Should -BeNullOrEmpty
        $result.NextPatch | Should -BeNullOrEmpty
    }

    It "distinguishes an unpublished major series from an unpublished extension" {
        Mock Invoke-RestMethod {
            '{"results":[{"extensions":[{"versions":[{"version":"1.0.42"}]}]}]}' | ConvertFrom-Json
        }

        $result = Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 2

        $result.ExtensionPublished | Should -BeTrue
        $result.LatestVersion | Should -BeNullOrEmpty
        $result.NextPatch | Should -BeNullOrEmpty
    }

    It "surfaces network errors instead of returning empty history" {
        Mock Invoke-RestMethod { throw "Connection failed." }

        {
            Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 2
        } | Should -Throw "*Unable to query VS Code Marketplace*Connection failed*"
    }

    It "rejects malformed marketplace responses" {
        Mock Invoke-RestMethod { [PSCustomObject]@{ unexpected = "response" } }

        {
            Get-LatestMarketplaceVersion -PublisherId "ms-azuretools" -ExtensionId "vscode-azure-mcp-server" -MajorVersion 2
        } | Should -Throw "*Marketplace returned an invalid response*"
    }
}

Describe "Assert-VsixChangelogVersion" {
    BeforeEach {
        $changelogPath = Join-Path $TestDrive "CHANGELOG.md"
    }

    It "accepts matching stable and pre-release headings with either line ending" -ForEach @(
        @{ Suffix = ""; IsPrerelease = $false; Newline = "`n" }
        @{ Suffix = ""; IsPrerelease = $false; Newline = "`r`n" }
        @{ Suffix = " (pre-release)"; IsPrerelease = $true; Newline = "`n" }
        @{ Suffix = " (pre-release)"; IsPrerelease = $true; Newline = "`r`n" }
    ) {
        $content = "# Release History${Newline}${Newline}## 2.0.43 (2026-04-24)$Suffix${Newline}"
        Set-Content -LiteralPath $changelogPath -Value $content -NoNewline

        {
            Assert-VsixChangelogVersion -ChangelogPath $changelogPath -VsixVersion "2.0.43" -IsPrerelease $IsPrerelease
        } | Should -Not -Throw
        Get-Content -LiteralPath $changelogPath -Raw | Should -Be $content
    }

    It "does not mistake a matching older heading for the current release" {
        "# Release History`n`n## 2.0.44 (2026-04-25)`n`n## 2.0.43 (2026-04-24)" |
            Set-Content -LiteralPath $changelogPath

        {
            Assert-VsixChangelogVersion -ChangelogPath $changelogPath -VsixVersion "2.0.43" -IsPrerelease $false
        } | Should -Throw "*does not match VSIX version '2.0.43'*"
    }

    It "does not skip a malformed or unreleased first heading" -ForEach @(
        @{ Header = "2.0.43 (Unreleased)" }
        @{ Header = "2.0.2-beta.43 (2026-04-24)" }
        @{ Header = "invalid" }
    ) {
        "# Release History`n`n## $Header`n`n## 2.0.43 (2026-04-24)" |
            Set-Content -LiteralPath $changelogPath

        {
            Assert-VsixChangelogVersion -ChangelogPath $changelogPath -VsixVersion "2.0.43" -IsPrerelease $false
        } | Should -Throw "*must start with a finalized*"
    }

    It "fails when release notes are missing" {
        "# Release History" | Set-Content -LiteralPath $changelogPath

        {
            Assert-VsixChangelogVersion -ChangelogPath $changelogPath -VsixVersion "2.0.43" -IsPrerelease $false
        } | Should -Throw "*has no release heading*"
    }
}
