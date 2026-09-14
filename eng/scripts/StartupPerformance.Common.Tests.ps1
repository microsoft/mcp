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

Describe "Get-TimingStats percentiles" {
    It "reports p50 equal to the median for an odd number of samples" {
        $stats = Get-TimingStats -Samples @(9, 3, 5)
        $stats.p50 | Should -Be 5
    }

    It "orders p50 <= p95 <= p99 and keeps them within the sample range" {
        $stats = Get-TimingStats -Samples @(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
        $stats.p50 | Should -BeLessOrEqual $stats.p95
        $stats.p95 | Should -BeLessOrEqual $stats.p99
        $stats.p99 | Should -BeLessOrEqual 10
        $stats.p95 | Should -BeGreaterThan 9   # rank 8.55 lands between 9 and 10
    }

    It "returns the single value for all percentiles when one sample" {
        $stats = Get-TimingStats -Samples @(42)
        $stats.p50 | Should -Be 42
        $stats.p95 | Should -Be 42
        $stats.p99 | Should -Be 42
    }
}

Describe "Get-Percentile" {
    It "returns the min at p0 and max at p100" {
        (Get-Percentile -Samples @(10, 20, 30, 40) -Percentile 0.0)  | Should -Be 10
        (Get-Percentile -Samples @(10, 20, 30, 40) -Percentile 1.0)  | Should -Be 40
    }

    It "interpolates between ranks" {
        # sorted 10,20,30,40; rank = 0.5*3 = 1.5 -> 20 + 0.5*(30-20) = 25
        (Get-Percentile -Samples @(40, 10, 30, 20) -Percentile 0.5) | Should -Be 25
    }

    It "returns an exact sample when the rank is an integer" {
        # sorted 1..11, rank = 0.9*(11-1) = 9 -> sorted[9] = 10
        (Get-Percentile -Samples @(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11) -Percentile 0.9) | Should -Be 10
    }

    It "throws for out-of-range percentiles" {
        { Get-Percentile -Samples @(1) -Percentile 1.5 } | Should -Throw
        { Get-Percentile -Samples @(1) -Percentile -0.1 } | Should -Throw
    }

    It "throws when no samples are provided" {
        { Get-Percentile -Samples @() -Percentile 0.5 } | Should -Throw
    }
}

Describe "Split-ColdWarm" {
    It "treats the first sample as cold and the rest as warm" {
        $split = Split-ColdWarm -Samples @(100, 40, 50, 60)
        $split.cold_ms | Should -Be 100
        # warm samples 40,50,60 -> median 50
        $split.warm.median | Should -Be 50
    }

    It "falls back to the single sample for warm when only one sample exists" {
        $split = Split-ColdWarm -Samples @(77)
        $split.cold_ms | Should -Be 77
        $split.warm.median | Should -Be 77
    }

    It "throws when no samples are provided" {
        { Split-ColdWarm -Samples @() } | Should -Throw
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

Describe "Get-PerfRunMetadata" {
    It "records all controlled-environment dimensions required by issue #3120" {
        $m = Get-PerfRunMetadata
        foreach ($key in @('commit', 'timestamp', 'runtime', 'os', 'os_architecture', 'cpu_cores', 'machine_memory_gb', 'workload_catalog_version')) {
            $m.Contains($key) | Should -BeTrue -Because "metadata must include '$key'"
        }
    }

    It "reports a positive processor count" {
        (Get-PerfRunMetadata).cpu_cores | Should -BeGreaterThan 0
    }
}

Describe "Get-WorkloadCatalogVersion" {
    It "returns the catalog version from the committed workload catalog" {
        Get-WorkloadCatalogVersion | Should -Not -BeNullOrEmpty
    }

    It "returns 'unknown' when the catalog file is absent" {
        Get-WorkloadCatalogVersion -CatalogPath (Join-Path $script:RunFolder 'no-such-catalog.json') | Should -Be 'unknown'
    }
}
