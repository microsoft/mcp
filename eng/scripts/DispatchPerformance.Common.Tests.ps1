# Install-Module -Name Pester -Force -SkipPublisherCheck
# Invoke-Pester -Path eng/scripts/DispatchPerformance.Common.Tests.ps1
#
# Focused tests for the pure dispatch-gate helpers used by Measure-DispatchOverhead.ps1.

BeforeAll {
    . $PSScriptRoot/DispatchPerformance.Common.ps1

    function script:New-Stats {
        param($P50, $P95, $P99)
        return [ordered]@{ p50 = $P50; p95 = $P95; p99 = $P99 }
    }

    function script:New-DispatchScenarios {
        param($StdioList, $HttpList)
        return [ordered]@{
            stdio = [ordered]@{ tools_list = $StdioList }
            http  = [ordered]@{ tools_list = $HttpList }
        }
    }

    function script:New-UniformScenarios {
        param($StdioP50 = 5.0, $HttpP50 = 6.0)
        $stdio = New-Stats -P50 $StdioP50 -P95 ($StdioP50 * 1.5) -P99 ($StdioP50 * 2)
        $http  = New-Stats -P50 $HttpP50  -P95 ($HttpP50 * 1.5)  -P99 ($HttpP50 * 2)
        return New-DispatchScenarios -StdioList $stdio -HttpList $http
    }
}

Describe "Get-TransportOverhead" {
    It "returns http minus stdio tools/list p50" {
        $s = New-DispatchScenarios `
            -StdioList (New-Stats -P50 5 -P95 6 -P99 7) `
            -HttpList  (New-Stats -P50 6.5 -P95 8 -P99 9)
        Get-TransportOverhead $s | Should -Be 1.5
    }

    It "returns null when a tools/list stat is absent" {
        $s = [ordered]@{ stdio = [ordered]@{ } ; http = [ordered]@{ tools_list = (New-Stats -P50 6 -P95 8 -P99 9) } }
        Get-TransportOverhead $s | Should -Be $null
    }
}

Describe "Invoke-DispatchRegressionGate" {
    It "reports no failures or warnings for an identical baseline" {
        $s = New-UniformScenarios
        $b = New-UniformScenarios
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
        @($gate.Warnings).Count | Should -Be 0
    }

    It "fails when stdio tools/list p95 regresses beyond 10%" {
        $b = New-UniformScenarios
        $s = New-UniformScenarios
        $s.stdio.tools_list.p95 = $b.stdio.tools_list.p95 * 1.15
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'stdio/tools_list (p95)'
    }

    It "fails when http tools/list p99 regresses beyond 20%" {
        $b = New-UniformScenarios
        $s = New-UniformScenarios
        $s.http.tools_list.p99 = $b.http.tools_list.p99 * 1.25
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'http/tools_list (p99)'
    }

    It "warns but does not fail for a p50 change between 5% and 10%" {
        $b = New-UniformScenarios
        $s = New-UniformScenarios
        $s.stdio.tools_list.p50 = $b.stdio.tools_list.p50 * 1.07
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
        $gate.Warnings | Should -Contain 'stdio/tools_list (p50)'
    }

    It "warns when transport overhead grows beyond 5%" {
        # Baseline overhead = 6 - 5 = 1. Raise http p50 to 6.4 => overhead 1.4 (+40%),
        # while keeping the http p50 change (+6.7%) below the +10% fail threshold.
        $b = New-UniformScenarios -StdioP50 5.0 -HttpP50 6.0
        $s = New-UniformScenarios -StdioP50 5.0 -HttpP50 6.4
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Warnings | Should -Contain 'transport_overhead_tools_list_p50'
    }

    It "skips a transport absent from the baseline" {
        $s = New-UniformScenarios
        $b = [ordered]@{ stdio = $s.stdio }  # no http
        $gate = Invoke-DispatchRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
    }
}
