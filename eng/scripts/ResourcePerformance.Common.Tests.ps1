# Install-Module -Name Pester -Force -SkipPublisherCheck
# Invoke-Pester -Path eng/scripts/ResourcePerformance.Common.Tests.ps1
#
# Focused tests for the pure resource-gate helpers used by Measure-ResourceUsage.ps1.

BeforeAll {
    . $PSScriptRoot/ResourcePerformance.Common.ps1

    function script:New-ResourceScenario {
        param(
            [double] $SteadyMem,
            [double] $SteadyCpu,
            [double] $PeakMem,
            [switch] $WithSoak,
            [bool]   $Unbounded = $false
        )

        $scenario = [ordered]@{
            phases = [ordered]@{
                steady_state = [ordered]@{
                    working_set_mb = [ordered]@{ mean = $SteadyMem; max = $SteadyMem }
                    cpu_percent    = $SteadyCpu
                }
            }
            peak = [ordered]@{ working_set_mb = $PeakMem }
        }

        if ($WithSoak) {
            $scenario.soak = [ordered]@{
                unbounded_growth             = $Unbounded
                working_set_slope_mb_per_min = 1
                working_set_r2               = 0.9
                handle_slope_per_min         = 1
                handle_r2                    = 0.1
            }
        }

        return $scenario
    }
}

Describe "Get-MemberNames" {
    It "returns the keys of an ordered dictionary" {
        $names = Get-MemberNames ([ordered]@{ a = 1; b = 2 })
        $names | Should -Be @('a', 'b')
    }

    It "returns the property names of a PSCustomObject" {
        $obj = [pscustomobject]@{ x = 1; y = 2 }
        $names = Get-MemberNames $obj
        $names | Should -Be @('x', 'y')
    }

    It "returns an empty set for null" {
        @(Get-MemberNames $null).Count | Should -Be 0
    }
}

Describe "Invoke-ResourceRegressionGate" {
    It "reports no failures or warnings for an identical baseline" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
        @($gate.Warnings).Count | Should -Be 0
    }

    It "fails when steady-state memory regresses beyond 10%" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 115 -SteadyCpu 10 -PeakMem 150) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'default steady_working_set_mb'
    }

    It "warns but does not fail for a steady-state memory change between 5% and 10%" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 107 -SteadyCpu 10 -PeakMem 150) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
        $gate.Warnings | Should -Contain 'default steady_working_set_mb'
    }

    It "fails when steady-state CPU regresses beyond 10%" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 12 -PeakMem 150) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'default steady_cpu_percent'
    }

    It "allows peak memory growth up to 15% as a warning" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 168) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
        $gate.Warnings | Should -Contain 'default peak_working_set_mb'
    }

    It "fails when peak memory regresses beyond 15%" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 180) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'default peak_working_set_mb'
    }

    It "fails when a soak phase is flagged with unbounded growth" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150 -WithSoak -Unbounded $true) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'default soak_unbounded_growth'
    }

    It "passes a soak phase that is not flagged" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150 -WithSoak -Unbounded $false) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
    }

    It "flags an incompatible (empty) baseline instead of silently passing (F-007)" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 999 -SteadyCpu 99 -PeakMem 999) }
        $b = [ordered]@{ }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'no_comparable_metrics'
    }

    It "compares overlapping modes and skips a newly added mode without failing" {
        $s = [ordered]@{
            default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150)
            all     = (New-ResourceScenario -SteadyMem 200 -SteadyCpu 20 -PeakMem 280)
        }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        @($gate.Failures).Count | Should -Be 0
    }

    It "flags soak unbounded growth even when the mode is absent from the baseline (F-008)" {
        $s = [ordered]@{ all = (New-ResourceScenario -SteadyMem 200 -SteadyCpu 20 -PeakMem 280 -WithSoak -Unbounded $true) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 10 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'all soak_unbounded_growth'
    }

    It "treats positive CPU against a zero baseline as a failure, not a pass (F-010)" {
        $s = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 25 -PeakMem 150) }
        $b = [ordered]@{ default = (New-ResourceScenario -SteadyMem 100 -SteadyCpu 0 -PeakMem 150) }
        $gate = Invoke-ResourceRegressionGate -ResultScenarios $s -BaselineScenarios $b
        $gate.Failures | Should -Contain 'default steady_cpu_percent'
    }
}
