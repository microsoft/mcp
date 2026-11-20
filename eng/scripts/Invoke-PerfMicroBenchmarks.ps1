#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Runs the BenchmarkDotNet micro-benchmarks for the perf pipeline and guards against a silent
    no-op run.

.DESCRIPTION
    Extracted from the pipeline YAML so CI behavior can be reproduced locally. --buildTimeout is
    raised because the generated boilerplate references the whole server (60+ tool assemblies) and
    exceeds BenchmarkDotNet's default 2-minute build timeout. BenchmarkDotNet can exit 0 even when
    the boilerplate build fails and nothing runs, so this fails explicitly if no benchmarks executed.

.PARAMETER BenchmarkExe
    Path to the built performance-benchmark executable. Defaults to the Release build.

.PARAMETER ArtifactsPath
    Directory for BenchmarkDotNet artifacts and the captured run log.

.PARAMETER Filter
    Glob passed to BenchmarkDotNet --filter (e.g. '*ServerStartup*').

.PARAMETER BuildTimeoutSeconds
    --buildTimeout for the generated boilerplate build.
#>
param(
    [string] $BenchmarkExe,
    [Parameter(Mandatory)][string] $ArtifactsPath,
    [string] $Filter = '*',
    [int]    $BuildTimeoutSeconds = 600
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $BenchmarkExe) {
    . (Join-Path $PSScriptRoot 'StartupPerformance.Common.ps1')
    $BenchmarkExe = Resolve-PerfBenchmarkExe -RepoRoot (Join-Path $PSScriptRoot '../..')
}

New-Item -ItemType Directory -Force -Path $ArtifactsPath | Out-Null
$logPath = Join-Path $ArtifactsPath 'run.log'

& $BenchmarkExe --filter $Filter --artifacts $ArtifactsPath --buildTimeout $BuildTimeoutSeconds 2>&1 |
    Tee-Object -FilePath $logPath
$code = $LASTEXITCODE
if ($code -ne 0) { exit $code }

$log = Get-Content $logPath -Raw
if ($log -match 'failed to build the auto-generated boilerplate' -or $log -match 'executed benchmarks: 0') {
    Write-Error 'BenchmarkDotNet executed no benchmarks (boilerplate build failure or empty filter).'
    exit 1
}
