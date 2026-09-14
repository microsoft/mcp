# Install-Module -Name Pester -Force -SkipPublisherCheck
# Invoke-Pester -Path eng/scripts/ConcurrencyPerformance.Common.Tests.ps1
#
# Focused tests for the pure concurrency-gate helpers used by Measure-ConcurrencyScaling.ps1.

BeforeAll {
    . $PSScriptRoot/ConcurrencyPerformance.Common.ps1

    function script:New-Level {
        param($Concurrency, $Throughput, $Scaling, $P95, $P99, $ErrorRate = 0, $Mismatches = 0)
        return [ordered]@{
            concurrency           = $Concurrency
            throughput_rps        = $Throughput
            scaling_efficiency    = $Scaling
            error_rate_pct        = $ErrorRate
            tool_count_mismatches = $Mismatches
            latency_ms            = [ordered]@{ p50 = ($P95 * 0.6); p95 = $P95; p99 = $P99 }
        }
    }

    function script:New-UniformLevels {
        return @(
            (New-Level -Concurrency 1 -Throughput 100 -Scaling 1.0 -P95 5  -P99 7),
            (New-Level -Concurrency 4 -Throughput 350 -Scaling 0.9 -P95 8  -P99 12)
        )
    }
}

Describe "Test-ConcurrencyBudget" {
    It "reports the fraction dropped for a higher-is-better metric" {
        $r = Test-ConcurrencyBudget -Current 85 -Baseline 100 -FailFraction 0.10 -WarnFraction 0.05 -Direction 'higher'
        $r.Status | Should -Be 'FAIL'
        [math]::Round($r.Change, 2) | Should -Be 0.15
    }

    It "reports the fraction risen for a lower-is-better metric" {
        # +20% rise is at the fail boundary (not > 0.20) but above the 5% warn threshold.
        $r = Test-ConcurrencyBudget -Current 12 -Baseline 10 -FailFraction 0.20 -WarnFraction 0.05 -Direction 'lower'
        $r.Status | Should -Be 'WARN'
        [math]::Round($r.Change, 2) | Should -Be 0.20
    }

    It "skips when the baseline is not positive" {
        (Test-ConcurrencyBudget -Current 5 -Baseline 0 -FailFraction 0.1 -WarnFraction 0.05 -Direction 'higher').Status | Should -Be 'SKIP'
    }
}

Describe "Invoke-ConcurrencyRegressionGate" {
    It "reports no failures or warnings for an identical baseline" {
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels (New-UniformLevels) -BaselineLevels (New-UniformLevels)
        @($gate.Failures).Count | Should -Be 0
        @($gate.Warnings).Count | Should -Be 0
    }

    It "fails when throughput drops more than 10%" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].throughput_rps = $b[1].throughput_rps * 0.85
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 throughput_rps'
    }

    It "fails when scaling efficiency degrades more than 15%" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].scaling_efficiency = $b[1].scaling_efficiency * 0.80
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 scaling_efficiency'
    }

    It "fails when p95 latency degrades more than 15%" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].latency_ms.p95 = $b[1].latency_ms.p95 * 1.20
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 latency_p95'
    }

    It "fails when p99 latency regresses more than 20%" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].latency_ms.p99 = $b[1].latency_ms.p99 * 1.25
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 latency_p99'
    }

    It "fails when the error rate increases more than 0.5 percentage points" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].error_rate_pct = $b[1].error_rate_pct + 0.6
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 error_rate_pct'
    }

    It "fails on any concurrent tool-count mismatch (state leakage)" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].tool_count_mismatches = 3
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 state_leakage'
    }

    It "warns but does not fail for a throughput drop between 5% and 10%" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].throughput_rps = $b[1].throughput_rps * 0.93
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        @($gate.Failures).Count | Should -Be 0
        $gate.Warnings | Should -Contain 'c4 throughput_rps'
    }

    It "skips a concurrency level absent from the baseline" {
        $s = New-UniformLevels
        $b = @($s[0])  # only concurrency 1
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        @($gate.Failures).Count | Should -Be 0
    }

    It "flags a state-leakage mismatch even when the level is absent from the baseline (F-008)" {
        $s = New-UniformLevels
        $s += (New-Level -Concurrency 16 -Throughput 900 -Scaling 0.7 -P95 20 -P99 30 -Mismatches 5)
        $b = New-UniformLevels   # no concurrency 16
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c16 state_leakage'
    }

    It "fails when no level overlaps the baseline (F-007)" {
        $s = @(New-Level -Concurrency 32 -Throughput 100 -Scaling 1.0 -P95 5 -P99 7)
        $b = New-UniformLevels   # concurrency 1 and 4 only
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'no_comparable_metrics'
    }

    It "fails when a latency metric the baseline expects is missing from results (F-007)" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].latency_ms.Remove('p95')   # results dropped p95 at concurrency 4
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 latency_p95 (missing in results)'
    }

    It "fails when the error-rate metric the baseline expects is missing from results (F-007)" {
        $s = New-UniformLevels
        $b = New-UniformLevels
        $s[1].Remove('error_rate_pct')   # results dropped error_rate_pct at concurrency 4
        $gate = Invoke-ConcurrencyRegressionGate -ResultLevels $s -BaselineLevels $b
        $gate.Failures | Should -Contain 'c4 error_rate_pct (missing in results)'
    }
}
