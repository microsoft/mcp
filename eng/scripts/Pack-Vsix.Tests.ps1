Describe "Pack-Vsix release changelog validation" {
    BeforeAll {
        $sourceScript = Join-Path $PSScriptRoot "Pack-Vsix.ps1"
        $sourceHelper = Join-Path $PSScriptRoot "helpers\VsixVersionHelpers.ps1"
        $originalSetDevVersion = $env:SETDEVVERSION
    }

    AfterAll {
        $env:SETDEVVERSION = $originalSetDevVersion
    }

    BeforeEach {
        $env:SETDEVVERSION = "false"
        $fixtureRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString("N"))
        $scriptsDirectory = Join-Path $fixtureRoot "eng\scripts"
        $commonDirectory = Join-Path $fixtureRoot "eng\common\scripts"
        $vscodeDirectory = Join-Path $fixtureRoot "servers\Azure.Mcp.Server\vscode"
        $artifactsDirectory = Join-Path $fixtureRoot "artifacts"
        New-Item -ItemType Directory -Path $scriptsDirectory, $commonDirectory, $vscodeDirectory, $artifactsDirectory -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $scriptsDirectory "helpers") -Force | Out-Null
        $scriptPath = Join-Path $scriptsDirectory "Pack-Vsix.ps1"
        Copy-Item -LiteralPath $sourceScript -Destination $scriptPath
        Copy-Item -LiteralPath $sourceHelper -Destination (Join-Path $scriptsDirectory "helpers\VsixVersionHelpers.ps1")

        @'
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
function LogWarning { param([string]$Message) Write-Warning $Message }
function LogError { param([string]$Message) throw $Message }
function Invoke-LoggedCommand {
    param([string]$Command)
    Add-Content -LiteralPath (Join-Path $RepoRoot "commands.txt") -Value $Command
}
'@ | Set-Content -LiteralPath (Join-Path $commonDirectory "common.ps1")
        @'
param($Command, $InputReadMePath, $PackageType, $InsertPayload, $OutputDirectory)
'@ | Set-Content -LiteralPath (Join-Path $scriptsDirectory "Process-PackageReadMe.ps1")
        "Fixture license" | Set-Content -LiteralPath (Join-Path $fixtureRoot "LICENSE")
        "Fixture notices" | Set-Content -LiteralPath (Join-Path $fixtureRoot "NOTICE.txt")
        "Fixture icon" | Set-Content -LiteralPath (Join-Path $fixtureRoot "icon.png")

        $sourceChangelogPath = Join-Path $vscodeDirectory "CHANGELOG.md"
        @'
# Release History

## 2.0.43 (2026-04-24)

### Fixed

- Fixture release notes.

## 2.0.42 (2026-04-23) (pre-release)

### Added

- Previous release notes.
'@ | Set-Content -LiteralPath $sourceChangelogPath
        @{
            name = "vscode-azure-mcp-server"
            publisher = "ms-azuretools"
            version = "0.0.0"
        } | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $vscodeDirectory "package.json")

        $buildInfo = @{
            publishTarget = "public"
            dynamicPrereleaseVersion = $false
            servers = @(
                @{
                    name = "Azure.Mcp.Server"
                    version = "2.0.2"
                    vsixVersion = "2.0.43"
                    vsixIsPrerelease = $false
                    packageIcon = "icon.png"
                    readmePath = "README.md"
                    platforms = @()
                }
            )
        }
        $buildInfoPath = Join-Path $fixtureRoot "build_info.json"
        $outputPath = Join-Path $fixtureRoot "packages"
        $commandLogPath = Join-Path $fixtureRoot "commands.txt"
    }

    It "rejects the original server-version heading before installing or packaging" {
        $content = (Get-Content -LiteralPath $sourceChangelogPath -Raw).Replace(
            "## 2.0.43 (2026-04-24)", "## 2.0.2 (2026-04-24)")
        Set-Content -LiteralPath $sourceChangelogPath -Value $content -NoNewline
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        {
            & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null
        } | Should -Throw "*does not match VSIX version '2.0.43'*"

        Test-Path -LiteralPath $commandLogPath | Should -BeFalse
        Get-Content -LiteralPath $sourceChangelogPath -Raw | Should -Be $content
    }

    It "rejects a version that advanced after changelog preparation" {
        $buildInfo.servers[0].vsixVersion = "2.0.44"
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        {
            & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null
        } | Should -Throw "*does not match VSIX version '2.0.44'*"

        Test-Path -LiteralPath $commandLogPath | Should -BeFalse
    }

    It "keeps the staged changelog version consistent with the package manifest" {
        $content = Get-Content -LiteralPath $sourceChangelogPath -Raw
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null

        $LASTEXITCODE | Should -Be 0
        $stagedDirectory = Join-Path $fixtureRoot ".work\temp"
        $packageJson = Get-Content -LiteralPath (Join-Path $stagedDirectory "package.json") -Raw | ConvertFrom-Json
        $packageJson.version | Should -Be "2.0.43"
        $stagedChangelog = Get-Content -LiteralPath (Join-Path $stagedDirectory "CHANGELOG.md") -Raw
        $stagedChangelog | Should -Match "(?m)^## $([regex]::Escape($packageJson.version)) \(2026-04-24\)\r?$"
        $stagedChangelog | Should -Be $content
        Get-Content -LiteralPath $sourceChangelogPath -Raw | Should -Be $content
    }

    It "rejects the wrong release channel even when the numeric version matches" {
        $buildInfo.servers[0].vsixIsPrerelease = $true
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        {
            & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null
        } | Should -Throw "*does not match VSIX release channel*"

        Test-Path -LiteralPath $commandLogPath | Should -BeFalse
    }

    It "preserves non-public placeholder builds" {
        $buildInfo.publishTarget = "none"
        $buildInfo.dynamicPrereleaseVersion = $true
        $buildInfo.servers[0].vsixVersion = "2.0.999"
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null

        $LASTEXITCODE | Should -Be 0
        $packageJson = Get-Content -LiteralPath (Join-Path $fixtureRoot ".work\temp\package.json") -Raw | ConvertFrom-Json
        $packageJson.version | Should -Be "2.0.999"
    }

    It "preserves public test-pipeline builds with dynamic server versions" {
        $buildInfo.dynamicPrereleaseVersion = $true
        $buildInfo.servers[0].version = "2.0.2-alpha.12345"
        $buildInfo.servers[0].vsixVersion = "2.0.999"
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null

        $LASTEXITCODE | Should -Be 0
    }

    It "preserves explicitly requested development versions" {
        $env:SETDEVVERSION = "true"
        $buildInfo.servers[0].vsixVersion = "2.0.12345"
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null

        $LASTEXITCODE | Should -Be 0
    }

    It "preserves other servers' release policies" -ForEach @(
        @{ ServerName = "Fabric.Mcp.Server" }
        @{ ServerName = "Template.Mcp.Server" }
    ) {
        $buildInfo.servers[0].name = $ServerName
        $buildInfo.servers[0].version = "1.0.0"
        $buildInfo.servers[0].vsixVersion = "1.0.0"
        $serverDirectory = Join-Path $fixtureRoot "servers\$ServerName"
        Move-Item -LiteralPath (Join-Path $fixtureRoot "servers\Azure.Mcp.Server") -Destination $serverDirectory
        if ($ServerName -eq "Template.Mcp.Server") {
            Remove-Item -LiteralPath (Join-Path $serverDirectory "vscode\CHANGELOG.md")
        }
        $buildInfo | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $buildInfoPath

        & $scriptPath -ArtifactsPath $artifactsDirectory -BuildInfoPath $buildInfoPath -OutputPath $outputPath 6>$null

        $LASTEXITCODE | Should -Be 0
    }
}
