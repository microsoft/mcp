#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Verifies the NuGet package for the specified server is available on nuget.org.
.DESCRIPTION
    This script checks the availability of the NuGet package README for the given server on nuget.org.
.PARAMETER ServerName
    Name of the MCP server under "./servers/" folder whose server.json will be deployed.
.PARAMETER BuildInfoPath
    Path to the build_info.json file containing build metadata. If not provided, defaults to ".work/build_info.json" in the repo root.
.PARAMETER TimeoutInSeconds
    Maximum time to wait for the package to become available, in seconds. Default is 300 seconds.
.PARAMETER SleepIntervalInSeconds
    Time to wait between checks for package availability, in seconds. Default is 10 seconds.
.EXAMPLE
    Verify-NuGetRelease.ps1 -ServerName "Azure.Mcp.Server" -BuildInfoPath ".work/build_info.json"
    Verifies the NuGet package for Azure.Mcp.Server is available on nuget.org.
#>
[CmdletBinding(DefaultParameterSetName='default')]
param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [string] $BuildInfoPath,

    [int] $TimeoutInSeconds = 300,
    [int] $SleepIntervalInSeconds = 10
)

$ErrorActionPreference = "Stop" 
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    exit 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable
$server = $buildInfo.servers | Where-Object { $_.name -ieq $ServerName }

if (!$server) {
    LogError "Server '$ServerName' not found in build info file $BuildInfoPath."
    exit 1
}

$packageId = $server.dnxPackageId.ToLowerInvariant()
$packageVersion = $server.version.ToLowerInvariant()

$url = "https://api.nuget.org/v3-flatcontainer/$packageId/$packageVersion/readme"
$elapsed = 0

Write-Host "Checking for package README. URL: $url"

while ($true) {
    if ($elapsed -gt $TimeoutInSeconds) {
        Write-Error "Package README is not available after $elapsed seconds. Timeout: $TimeoutInSeconds. URL: $url"
        exit 1
    }

    try { 
        Invoke-WebRequest -Uri $url -ErrorAction Stop | Out-Null
        Write-Host "Package README is now available."
        break
    } catch {
        Write-Host "Package README is not yet available. Elapsed time: $elapsed seconds."
        Start-Sleep -Seconds $SleepIntervalInSeconds
        $elapsed += $SleepIntervalInSeconds
        continue
    }
}