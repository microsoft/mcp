#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Side-effect-free helpers for the command dispatch / transport overhead gate (issue #3119).

.DESCRIPTION
    Dot-source this file to reuse the pure functions in Measure-DispatchOverhead.ps1 and
    Check-DispatchOverheadRegression.ps1 and to unit-test them without launching a server.
    Reuses Get-MemberValue from StartupPerformance.Common.ps1.
#>

. (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')

# ---------------------------------------------------------------------------
# Tiered dispatch/transport regression gate shared by Measure-DispatchOverhead.ps1 and
# Check-DispatchOverheadRegression.ps1. For each transport (stdio, http) and operation
# (ping, tools_list) it applies the acceptance-criteria budgets from issue #3119:
#   * p50 and p95 may not regress by more than FailP50P95 (default +10%)
#   * p99 may not regress by more than FailP99 (default +20%)
#   * any change above Warn (default +5%) is reported as a warning
# It also reports transport-specific overhead (http minus stdio tools/list p50) changes above Warn.
#
# Returns [ordered]@{ Failures = @(...); Warnings = @(...) } and writes a line per check.
# ---------------------------------------------------------------------------
function Invoke-DispatchRegressionGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $ResultScenarios,
        [Parameter(Mandatory)] $BaselineScenarios,
        [double] $FailP50P95 = 1.10,
        [double] $FailP99    = 1.20,
        [double] $Warn       = 1.05
    )

    $failures = @()
    $warnings = @()
    $evaluated = 0

    $transports = @('stdio', 'http')
    $operations = @('tools_list')
    $percentileBudgets = @(
        @{ Metric = 'p50'; Limit = $FailP50P95 }
        @{ Metric = 'p95'; Limit = $FailP50P95 }
        @{ Metric = 'p99'; Limit = $FailP99 }
    )

    foreach ($transport in $transports) {
        $curTransport  = Get-MemberValue $ResultScenarios $transport
        $baseTransport = Get-MemberValue $BaselineScenarios $transport
        if ($null -eq $curTransport -or $null -eq $baseTransport) {
            Write-Host ("  [SKIP] {0}: absent in results or baseline" -f $transport)
            continue
        }

        foreach ($operation in $operations) {
            $curStats  = Get-MemberValue $curTransport $operation
            $baseStats = Get-MemberValue $baseTransport $operation
            if ($null -eq $curStats -or $null -eq $baseStats) {
                Write-Host ("  [SKIP] {0}/{1}: value absent" -f $transport, $operation)
                continue
            }

            foreach ($budget in $percentileBudgets) {
                $metric   = $budget.Metric
                $current  = Get-MemberValue $curStats $metric
                $baseline = Get-MemberValue $baseStats $metric
                if ($null -eq $baseline) {
                    continue
                }
                $label = "$transport/$operation ($metric)"
                if ($null -eq $current) {
                    $failures += "$label (missing in results)"
                    Write-Host ("  [FAIL] {0}: expected by baseline but absent in results" -f $label)
                    continue
                }

                $evaluated++
                if ($baseline -gt 0) {
                    $ratio = $current / $baseline
                    if ($ratio -gt $budget.Limit) { $failures += $label; $status = 'FAIL' }
                    elseif ($ratio -gt $Warn)     { $warnings += $label; $status = 'WARN' }
                    else                          { $status = 'PASS' }
                    Write-Host ("  [{0}] {1}: current={2}ms  baseline={3}ms  (+{4}%)" -f `
                        $status, $label, $current, $baseline, [math]::Round(($ratio - 1) * 100, 1))
                }
                elseif ($current -gt 0) {
                    $failures += $label
                    Write-Host ("  [FAIL] {0}: current={1}ms vs a zero baseline (not comparable)" -f $label, $current)
                }
                else {
                    Write-Host ("  [PASS] {0}: current=0ms  baseline=0ms (unchanged)" -f $label)
                }
            }
        }
    }

    # Transport-specific overhead: the extra tools/list p50 that HTTP adds over stdio.
    $curOverhead  = Get-TransportOverhead $ResultScenarios
    $baseOverhead = Get-TransportOverhead $BaselineScenarios
    if ($null -ne $curOverhead -and $null -ne $baseOverhead -and $baseOverhead -gt 0) {
        $ratio = $curOverhead / $baseOverhead
        if ($ratio -gt $Warn) {
            $warnings += 'transport_overhead_tools_list_p50'
            $status = 'WARN'
        }
        else {
            $status = 'PASS'
        }
        Write-Host ("  [{0}] transport_overhead (http-stdio tools/list p50): current={1}ms  baseline={2}ms  (+{3}%)" -f `
            $status, [math]::Round($curOverhead, 4), [math]::Round($baseOverhead, 4), [math]::Round(($ratio - 1) * 100, 1))
    }

    if ($evaluated -eq 0) {
        $failures += 'no_comparable_metrics'
        Write-Host "  [FAIL] no comparable dispatch metrics between results and baseline (incompatible or empty baseline)"
    }

    return [ordered]@{ Failures = $failures; Warnings = $warnings }
}

# ---------------------------------------------------------------------------
# The HTTP-minus-stdio tools/list p50 delta — the transport-specific overhead for a real
# request. Returns $null when either transport's tools/list p50 is absent.
# ---------------------------------------------------------------------------
function Get-TransportOverhead {
    [CmdletBinding()]
    param($Scenarios)

    $stdio = Get-MemberValue (Get-MemberValue (Get-MemberValue $Scenarios 'stdio') 'tools_list') 'p50'
    $http  = Get-MemberValue (Get-MemberValue (Get-MemberValue $Scenarios 'http') 'tools_list') 'p50'
    if ($null -eq $stdio -or $null -eq $http) {
        return $null
    }
    return $http - $stdio
}
