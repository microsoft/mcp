#Requires -Version 7

<#
.SYNOPSIS
    Shared, side-effect-free helpers for the startup performance measurement scripts.

.DESCRIPTION
    These functions are extracted into their own file so they can be dot-sourced by
    Test-StartupPerformance.ps1 and covered by focused Pester tests without spawning
    the server or running the full measurement pipeline.
#>

Set-StrictMode -Version Latest

# ---------------------------------------------------------------------------
# Compute the p-th percentile of a sample array using linear interpolation
# between closest ranks (the inclusive method, equivalent to Excel's
# PERCENTILE.INC). p is expressed as a fraction in [0, 1].
# A single sample returns that sample for any percentile.
# ---------------------------------------------------------------------------
function Get-Percentile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][long[]]  $Samples,
        [Parameter(Mandatory)][double]  $Percentile
    )

    if ($null -eq $Samples -or $Samples.Count -eq 0) {
        throw "Get-Percentile requires at least one sample."
    }
    if ($Percentile -lt 0 -or $Percentile -gt 1) {
        throw "Get-Percentile requires Percentile in the range [0, 1]."
    }

    $sorted = @($Samples | Sort-Object)
    $count  = $sorted.Count
    if ($count -eq 1) { return [double]$sorted[0] }

    $rank  = $Percentile * ($count - 1)
    $lower = [int][math]::Floor($rank)
    $upper = [int][math]::Ceiling($rank)
    if ($lower -eq $upper) { return [double]$sorted[$lower] }

    $frac = $rank - $lower
    return [double]$sorted[$lower] + ($sorted[$upper] - $sorted[$lower]) * $frac
}

# ---------------------------------------------------------------------------
# Compute first/average/median/p50/p95/p99 timing stats from a sample array.
# For an even number of samples the median is the average of the two middle
# values (the statistically correct definition); for an odd count it is the
# single middle value. p50/p95/p99 are computed via Get-Percentile (linear
# interpolation) so they align with the budgets in the acceptance criteria.
# ---------------------------------------------------------------------------
function Get-TimingStats {
    [CmdletBinding()]
    param([Parameter(Mandatory)][long[]] $Samples)

    if ($null -eq $Samples -or $Samples.Count -eq 0) {
        throw "Get-TimingStats requires at least one sample."
    }

    $sorted = @($Samples | Sort-Object)
    $count  = $sorted.Count

    if ($count % 2 -eq 1) {
        $median = $sorted[[math]::Floor($count / 2)]
    }
    else {
        $upper  = $count / 2
        $lower  = $upper - 1
        $median = ($sorted[$lower] + $sorted[$upper]) / 2
    }

    return [ordered]@{
        first   = $Samples[0]
        average = [math]::Round(($Samples | Measure-Object -Average).Average, 1)
        median  = $median
        p50     = [math]::Round((Get-Percentile -Samples $Samples -Percentile 0.50), 1)
        p95     = [math]::Round((Get-Percentile -Samples $Samples -Percentile 0.95), 1)
        p99     = [math]::Round((Get-Percentile -Samples $Samples -Percentile 0.99), 1)
        all     = $Samples
    }
}

# ---------------------------------------------------------------------------
# Split a sample array into cold (first invocation) and warm (all subsequent
# invocations) timing stats. Process-level cold vs warm startup is captured by
# treating the first sample as cold (OS/file-cache/JIT cold) and the remaining
# samples as warm. When only one sample exists, warm falls back to that sample.
# ---------------------------------------------------------------------------
function Split-ColdWarm {
    [CmdletBinding()]
    param([Parameter(Mandatory)][long[]] $Samples)

    if ($null -eq $Samples -or $Samples.Count -eq 0) {
        throw "Split-ColdWarm requires at least one sample."
    }

    $cold = [long]$Samples[0]
    $warmSamples = if ($Samples.Count -gt 1) { @($Samples[1..($Samples.Count - 1)]) } else { @($Samples[0]) }

    return [ordered]@{
        cold_ms = $cold
        warm    = Get-TimingStats -Samples $warmSamples
    }
}

# ---------------------------------------------------------------------------
# Reads a named member from either an ordered hashtable (results built in-process)
# or a PSCustomObject (parsed from JSON). Returns $null when the member is absent
# so callers can gracefully skip metrics missing from an older baseline.
# ---------------------------------------------------------------------------
function Get-MemberValue {
    [CmdletBinding()]
    param($InputObject, [Parameter(Mandatory)][string] $Name)

    if ($null -eq $InputObject) { return $null }
    if ($InputObject -is [System.Collections.IDictionary]) {
        if ($InputObject.Contains($Name)) { return $InputObject[$Name] }
        return $null
    }
    $prop = $InputObject.PSObject.Properties[$Name]
    if ($prop) { return $prop.Value }
    return $null
}

# ---------------------------------------------------------------------------
# Tiered startup-regression gate shared by Test-StartupPerformance.ps1 and
# Check-StartupPerformanceRegression.ps1. Compares p50/p95/p99 of each tracked
# latency scenario against the baseline using the acceptance-criteria budgets
# from issue #3118:
#   * p50 and p95 may not regress by more than 10%  (FailP50P95 = 1.10)
#   * p99 may not regress by more than 20%          (FailP99   = 1.20)
#   * any change above 5% is reported as a warning  (Warn      = 1.05)
# Discovery scaling efficiency (per-tool startup cost for the all-tools surface)
# may not degrade by more than 15% (ScalingDegrade = 1.15).
#
# Returns [ordered]@{ Failures = @(...); Warnings = @(...) } and writes a
# human-readable line per check via Write-Host.
# ---------------------------------------------------------------------------
function Invoke-StartupRegressionGate {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $ResultScenarios,
        [Parameter(Mandatory)] $BaselineScenarios,
        [double] $FailP50P95    = 1.10,
        [double] $FailP99       = 1.20,
        [double] $Warn          = 1.05,
        [double] $ScalingDegrade = 1.15
    )

    $failures = @()
    $warnings = @()

    # Latency scenarios: display name -> scenario property holding a stats object.
    $latencyScenarios = [ordered]@{
        'cli_cold_start'    = 'cli_cold_start_ms'
        'mcp_stdio_default' = 'mcp_stdio_to_tools_list_ms'
        'mcp_namespace'     = 'mcp_namespace_mode_startup_ms'
        'mcp_all'           = 'mcp_all_mode_startup_ms'
        'mcp_http_default'  = 'mcp_http_default_startup_ms'
    }

    $percentileBudgets = @(
        @{ Metric = 'p50'; Limit = $FailP50P95 }
        @{ Metric = 'p95'; Limit = $FailP50P95 }
        @{ Metric = 'p99'; Limit = $FailP99 }
    )

    foreach ($name in $latencyScenarios.Keys) {
        $key         = $latencyScenarios[$name]
        $curStats    = Get-MemberValue $ResultScenarios $key
        $baseStats   = Get-MemberValue $BaselineScenarios $key
        if ($null -eq $curStats -or $null -eq $baseStats) {
            Write-Host ("  [SKIP] {0}: metric absent in results or baseline" -f $name)
            continue
        }

        foreach ($budget in $percentileBudgets) {
            $metric   = $budget.Metric
            $current  = Get-MemberValue $curStats $metric
            $baseline = Get-MemberValue $baseStats $metric
            if ($null -eq $current -or $null -eq $baseline) {
                Write-Host ("  [SKIP] {0} ({1}): value absent" -f $name, $metric)
                continue
            }

            $ratio  = if ($baseline -gt 0) { $current / $baseline } else { 1 }
            $limit  = [math]::Round($baseline * $budget.Limit, 1)
            $label  = "$name ($metric)"
            if ($ratio -gt $budget.Limit) {
                $failures += $label
                $status = 'FAIL'
            }
            elseif ($ratio -gt $Warn) {
                $warnings += $label
                $status = 'WARN'
            }
            else {
                $status = 'PASS'
            }
            Write-Host ("  [{0}] {1}: current={2}ms  baseline={3}ms  limit={4}ms  (+{5}%)" -f `
                $status, $label, $current, $baseline, $limit, [math]::Round(($ratio - 1) * 100, 1))
        }
    }

    # Discovery scaling efficiency: per-tool startup cost for the all-tools surface.
    $curAll   = Get-MemberValue $ResultScenarios 'mcp_all_mode_startup_ms'
    $curAllPl = Get-MemberValue $ResultScenarios 'all_mode_tools_list_payload'
    $baseAll   = Get-MemberValue $BaselineScenarios 'mcp_all_mode_startup_ms'
    $baseAllPl = Get-MemberValue $BaselineScenarios 'all_mode_tools_list_payload'
    $curP50    = Get-MemberValue $curAll 'p50'
    $curCount  = Get-MemberValue $curAllPl 'tool_count'
    $baseP50   = Get-MemberValue $baseAll 'p50'
    $baseCount = Get-MemberValue $baseAllPl 'tool_count'
    if ($curP50 -and $curCount -and $baseP50 -and $baseCount -and $curCount -gt 0 -and $baseCount -gt 0) {
        $curPerTool  = $curP50 / $curCount
        $basePerTool = $baseP50 / $baseCount
        $ratio       = if ($basePerTool -gt 0) { $curPerTool / $basePerTool } else { 1 }
        if ($ratio -gt $ScalingDegrade) {
            $failures += 'discovery_scaling_efficiency'
            $status = 'FAIL'
        }
        elseif ($ratio -gt $Warn) {
            $warnings += 'discovery_scaling_efficiency'
            $status = 'WARN'
        }
        else {
            $status = 'PASS'
        }
        Write-Host ("  [{0}] discovery_scaling_efficiency: current={1}ms/tool  baseline={2}ms/tool  (+{3}%)" -f `
            $status, [math]::Round($curPerTool, 3), [math]::Round($basePerTool, 3), [math]::Round(($ratio - 1) * 100, 1))
    }
    else {
        Write-Host "  [SKIP] discovery_scaling_efficiency: all-mode metrics absent"
    }

    return [ordered]@{ Failures = $failures; Warnings = $warnings }
}

# ---------------------------------------------------------------------------
# Normalize an output file path so its parent directory always exists.
# A bare filename (no directory component) otherwise makes Split-Path return an
# empty string, which causes New-Item -Path '' to fail. Relative paths are
# resolved against BaseDirectory so the parent directory is unambiguous.
# Returns the absolute file path.
# ---------------------------------------------------------------------------
function Resolve-PerfOutputPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Path,
        [string] $BaseDirectory = (Get-Location).Path
    )

    if (-not [System.IO.Path]::IsPathRooted($Path)) {
        $Path = Join-Path $BaseDirectory $Path
    }
    $Path = [System.IO.Path]::GetFullPath($Path)

    $parent = Split-Path -Parent $Path
    if ($parent -and -not (Test-Path $parent)) {
        $null = New-Item -ItemType Directory -Force -Path $parent
    }

    return $Path
}

# ---------------------------------------------------------------------------
# Reads the catalog_version from the versioned workload catalog (eng/perf-workloads.json),
# used to stamp every result so a number can be tied to the workload definition that produced
# it (issue #3120). Returns 'unknown' if the catalog is missing or unreadable.
# ---------------------------------------------------------------------------
function Get-WorkloadCatalogVersion {
    [CmdletBinding()]
    param(
        [string] $CatalogPath = (Join-Path $PSScriptRoot '../perf-workloads.json')
    )

    try {
        if (Test-Path $CatalogPath) {
            return (Get-Content $CatalogPath -Raw | ConvertFrom-Json).catalog_version
        }
    }
    catch {
        # Fall through to the default below.
    }
    return 'unknown'
}

# ---------------------------------------------------------------------------
# Captures the controlled-environment metadata every perf result must record (issue #3120):
# commit, runtime, OS, CPU, machine memory, plus the workload catalog version. Harnesses embed
# this under an "environment" key so results are self-describing and comparable across runs.
# Memory is best-effort and cross-platform; unresolved values are null rather than fatal.
# ---------------------------------------------------------------------------
function Get-PerfRunMetadata {
    [CmdletBinding()]
    param()

    $commit = try { (& git rev-parse --short HEAD 2>$null).Trim() } catch { '' }

    $memoryGb = $null
    try {
        if ($IsWindows) {
            $bytes = (Get-CimInstance -ClassName Win32_ComputerSystem -ErrorAction Stop).TotalPhysicalMemory
            $memoryGb = [math]::Round($bytes / 1GB, 1)
        }
        elseif ($IsLinux -and (Test-Path '/proc/meminfo')) {
            $kb = ((Get-Content '/proc/meminfo' | Where-Object { $_ -match '^MemTotal:' }) -replace '[^\d]', '')
            if ($kb) { $memoryGb = [math]::Round([double]$kb / 1048576, 1) }
        }
    }
    catch {
        $memoryGb = $null
    }

    return [ordered]@{
        commit                   = $commit
        timestamp                = (Get-Date).ToUniversalTime().ToString('o')
        runtime                  = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        os                       = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        os_architecture          = "$([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture)"
        cpu_cores                = [Environment]::ProcessorCount
        machine_memory_gb        = $memoryGb
        workload_catalog_version = Get-WorkloadCatalogVersion
    }
}
