# Install-Module -Name Pester -Force -SkipPublisherCheck
# Invoke-Pester -Path eng/scripts/StartupPerformance.Common.Tests.ps1
#
# Focused tests for the pure helpers used by Test-StartupPerformance.ps1.

BeforeAll {
    . $PSScriptRoot/StartupPerformance.Common.ps1

    $script:RunFolder = Join-Path $PSScriptRoot '.perf-common-testruns'
    if (Test-Path $script:RunFolder) {
        Remove-Item -Recurse -Force $script:RunFolder
    }
    New-Item -ItemType Directory -Path $script:RunFolder | Out-Null
}

AfterAll {
    if (Test-Path $script:RunFolder) {
        Remove-Item -Recurse -Force $script:RunFolder
    }
}

Describe "Get-TimingStats" {
    It "returns the single middle value as the median for an odd number of samples" {
        # sorted: 3,5,9 -> middle = 5
        $stats = Get-TimingStats -Samples @(9, 3, 5)
        $stats.median | Should -Be 5
    }

    It "averages the two middle values as the median for an even number of samples" {
        # sorted: 10,20,30,40 -> (20+30)/2 = 25
        $stats = Get-TimingStats -Samples @(40, 10, 30, 20)
        $stats.median | Should -Be 25
    }

    It "produces a fractional median when the two middle values differ by an odd amount" {
        # sorted: 10,11 -> (10+11)/2 = 10.5
        $stats = Get-TimingStats -Samples @(11, 10)
        $stats.median | Should -Be 10.5
    }

    It "reports the first sample in original (unsorted) order" {
        $stats = Get-TimingStats -Samples @(40, 10, 30, 20)
        $stats.first | Should -Be 40
    }

    It "computes the average rounded to one decimal place" {
        # (1+2+2)/3 = 1.666... -> 1.7
        $stats = Get-TimingStats -Samples @(1, 2, 2)
        $stats.average | Should -Be 1.7
    }

    It "handles a single sample" {
        $stats = Get-TimingStats -Samples @(42)
        $stats.median | Should -Be 42
        $stats.first | Should -Be 42
        $stats.average | Should -Be 42
    }

    It "throws when no samples are provided" {
        { Get-TimingStats -Samples @() } | Should -Throw
    }
}

Describe "Resolve-PerfOutputPath" {
    It "resolves a bare filename against the base directory and returns an absolute path" {
        $result = Resolve-PerfOutputPath -Path 'startup-e2e.json' -BaseDirectory $script:RunFolder
        [System.IO.Path]::IsPathRooted($result) | Should -Be $true
        Split-Path -Parent $result | Should -Be ([System.IO.Path]::GetFullPath($script:RunFolder))
    }

    It "creates a missing parent directory for a relative nested path" {
        $nested = Join-Path 'nested-a' 'nested-b/out.json'
        $result = Resolve-PerfOutputPath -Path $nested -BaseDirectory $script:RunFolder
        Test-Path (Split-Path -Parent $result) | Should -Be $true
    }

    It "creates a missing parent directory for an absolute path" {
        $abs = Join-Path $script:RunFolder 'abs-dir/out.json'
        $result = Resolve-PerfOutputPath -Path $abs
        Test-Path (Split-Path -Parent $result) | Should -Be $true
        $result | Should -Be ([System.IO.Path]::GetFullPath($abs))
    }

    It "does not throw for a bare filename (regression for empty Split-Path parent)" {
        { Resolve-PerfOutputPath -Path 'only-a-filename.json' -BaseDirectory $script:RunFolder } | Should -Not -Throw
    }
}
