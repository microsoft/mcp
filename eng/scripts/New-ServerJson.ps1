#!/bin/env pwsh
#Requires -Version 7

param(
    [Parameter(Mandatory = $true)]
    [string] $ServerName,
    [string] $BuildInfoPath,

    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$exitCode = 0

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

# Exit early if there were parameter errors
if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable
$server = $buildInfo.servers | Where-Object { $_.name -ieq $ServerName }

if (!$server) {
    LogError "Server '$ServerName' not found in build info file $BuildInfoPath."
    exit 1
}

$serverJsonPath = "$RepoRoot/$($server.serverJsonPath)"

if (!(Test-Path $serverJsonPath)) {
    LogError "Server JSON file $serverJsonPath does not exist."
    exit 1
}

$jsonHashTable = Get-Content -Path $serverJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable

if (!$jsonHashTable.ContainsKey('version')) {
    LogError "server.json file $serverJsonPath does not have a version key."
    exit 1
}

$jsonHashTable.version = $server.version

foreach ($package in $jsonHashTable.packages) {
    if (!$package.ContainsKey('version')) {
        LogWarning "Package: $($package.registryType), identifier: $($package.identifier) does not have a version key."
        continue
    }

    $package.version = $server.version
}

$updatedJson = $jsonHashTable | ConvertTo-Json -Depth 10

if (!$OutputPath) {
    $serverOutputPath = "$RepoRoot/.work/build/$($server.artifactPath)/"
    $OutputPath = "$serverOutputPath/server.json"

    LogDebug "Output path not provided. Using default path: $serverOutputPath"

    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null
}

LogInfo "Writing updated server.json to $OutputPath"
Set-Content -Path $OutputPath -Value $updatedJson -Encoding UTF8 -NoNewLine