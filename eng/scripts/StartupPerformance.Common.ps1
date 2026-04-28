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
# Compute first/average/median timing stats from a sample array.
# For an even number of samples the median is the average of the two middle
# values (the statistically correct definition); for an odd count it is the
# single middle value.
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
        all     = $Samples
    }
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
