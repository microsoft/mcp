# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Build-Local.ps1'
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$null, [ref]$parseErrors)
    if ($parseErrors.Count -ne 0) {
        throw "Unable to parse $scriptPath"
    }
    $verification = $ast.Find({
        param($node)
        $node -is [System.Management.Automation.Language.IfStatementAst] -and
        $node.Clauses[0].Item1.Extent.Text -eq '$VerifyNpx -and !$NoUsePaths'
    }, $true)
    if (-not $verification) {
        throw "Unable to find NPX verification in $scriptPath"
    }
    $verifyPackages = [scriptblock]::Create($verification.Extent.Text)

    function Invoke-LoggedCommand {
        param([string] $Command, [switch] $DoNotExitOnFailedExitCode)
        throw 'Verification must be mocked in this fixture.'
    }
}

Describe 'Build-Local NPX verification' {
    BeforeEach {
        $RepoRoot = Join-Path $TestDrive ([Guid]::NewGuid().ToString('N') + ' with spaces')
        $packageOutputPath = Join-Path $RepoRoot 'packages'
        foreach ($server in @('azure', 'fabric')) {
            $wrapper = Join-Path $packageOutputPath "$server\wrapper"
            New-Item -ItemType Directory -Path $wrapper -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $wrapper "$server.tgz") -Value 'fixture'
        }
        $VerifyNpx = $true
        $NoUsePaths = $false
        $originalLocation = (Get-Location).Path
        $originalCache = $env:NPM_CONFIG_CACHE
        $originalExitCode = $global:LASTEXITCODE
        $env:NPM_CONFIG_CACHE = Join-Path $TestDrive 'caller-cache'
        New-Item -ItemType Directory -Path $env:NPM_CONFIG_CACHE -Force | Out-Null
        $sentinel = Join-Path $env:NPM_CONFIG_CACHE 'keep.txt'
        Set-Content -LiteralPath $sentinel -Value 'caller cache'
        $script:observedCaches = [Collections.Generic.List[string]]::new()
        $script:verificationExitCode = 0
        $script:verificationThrows = $false
        Mock Invoke-LoggedCommand {
            param($Command, $DoNotExitOnFailedExitCode)

            $DoNotExitOnFailedExitCode | Should -BeTrue
            $match = [regex]::Match($Command, "^npx --cache '((?:[^']|'')+)' -y 'file:((?:[^']|'')+)' tools list$")
            $match.Success | Should -BeTrue
            $cache = $match.Groups[1].Value.Replace("''", "'")
            Test-Path -LiteralPath $cache -PathType Container | Should -BeTrue
            $cache | Should -Not -Be $env:NPM_CONFIG_CACHE
            $package = $match.Groups[2].Value.Replace("''", "'")
            Test-Path -LiteralPath $package | Should -BeTrue
            Set-Content -LiteralPath (Join-Path $cache 'downloaded.dat') -Value 'cache fixture'
            $script:observedCaches.Add($cache)
            if ($script:verificationThrows) {
                throw 'Fixture command failure'
            }
            $global:LASTEXITCODE = $script:verificationExitCode
        }
    }

    AfterEach {
        try {
            (Get-Location).Path | Should -Be $originalLocation
            Get-Content -LiteralPath $sentinel | Should -Be 'caller cache'
            foreach ($cache in $script:observedCaches) {
                Test-Path -LiteralPath $cache | Should -BeFalse
            }
        }
        finally {
            $env:NPM_CONFIG_CACHE = $originalCache
            $global:LASTEXITCODE = $originalExitCode
        }
    }

    It 'uses a fresh cache per wrapper without clearing the caller cache' {
        & $verifyPackages

        $script:observedCaches.Count | Should -Be 2
        @($script:observedCaches | Sort-Object -Unique).Count | Should -Be 2
        Should -Invoke Invoke-LoggedCommand -Times 2 -Exactly
    }

    It 'passes a quoted file package spec for <packagePath>' -TestCases @(
        @{
            packagePath = "/mnt/vss/_work/1/s/package space #100% 'quoted'.tgz"
            packageSpec = "file:/mnt/vss/_work/1/s/package space #100% ''quoted''.tgz"
        }
        @{
            packagePath = "C:\work\package space #100% 'quoted'.tgz"
            packageSpec = "file:C:\work\package space #100% ''quoted''.tgz"
        }
    ) {
        param($packagePath, $packageSpec)

        Mock Get-ChildItem {
            [pscustomobject]@{
                FullName = $packagePath
                Directory = [pscustomobject]@{ Name = 'wrapper' }
            }
        }
        Mock Invoke-LoggedCommand {
            param($Command, $DoNotExitOnFailedExitCode)
            $Command | Should -Match (" -y '" + [regex]::Escape($packageSpec) + "' tools list$")
            $global:LASTEXITCODE = 0
        }

        & $verifyPackages

        Should -Invoke Invoke-LoggedCommand -Times 1 -Exactly
    }

    It 'reports a failed package invocation and still cleans up' {
        $script:verificationExitCode = 17

        { & $verifyPackages } | Should -Throw '*NPX verification failed*exit code 17*'

        $script:observedCaches.Count | Should -Be 1
    }

    It 'preserves command exceptions and still cleans up' {
        $script:verificationThrows = $true

        { & $verifyPackages } | Should -Throw '*Fixture command failure*'

        $script:observedCaches.Count | Should -Be 1
    }

    It 'does not verify packages when path-based packaging is disabled' {
        $NoUsePaths = $true

        & $verifyPackages

        Should -Invoke Invoke-LoggedCommand -Times 0
    }

    It 'does not verify packages unless requested' {
        $VerifyNpx = $false

        & $verifyPackages

        Should -Invoke Invoke-LoggedCommand -Times 0
    }
}
