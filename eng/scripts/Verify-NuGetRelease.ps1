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
.PARAMETER NuGetFeedIndexUrl
    The NuGet feed index URL to query for package availability. Default is "https://api.nuget.org/v3/index.json".
.PARAMETER TimeoutInSeconds
    Maximum time to wait for the package to become available, in seconds. Default is 300 seconds.
.PARAMETER SleepIntervalInSeconds
    Time to wait between checks for package availability, in seconds. Default is 10 seconds.
.EXAMPLE
    Verify-NuGetRelease.ps1 -ServerName Azure.Mcp.Server -BuildInfoPath ".work/build_info.json"
    Verifies the NuGet package for Azure.Mcp.Server is available on nuget.org.

    Verify-NuGetRelease.ps1 -ServerName Azure.Mcp.Server -NuGetFeedIndexUrl 'https://pkgs.dev.azure.com/azure-sdk/public/_packaging/azure-sdk-for-net/nuget/v3/index.json' -TimeoutInSeconds 15
    Verifies the NuGet package for Azure.Mcp.Server is available on the NuGet feed at the specified URL, with a timeout of 15 seconds.
#>
[CmdletBinding(DefaultParameterSetName='default')]
param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [string] $BuildInfoPath,
    [string] $NuGetFeedIndexUrl = "https://api.nuget.org/v3/index.json",
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

# If a NuGet feed index URL is provided, we will find the nuspec rather than the README.
# The README is preferred, but not all package feeds may have it.
# Approach is based on NuGet API documentation: https://learn.microsoft.com/nuget/api/overview#resources-and-schema
$feedIndex = Invoke-RestMethod -Uri $NuGetFeedIndexUrl
$readmeTemplateResource = $feedIndex.resources | Where-Object { $($_.'@type') -eq "ReadmeUriTemplate/6.13.0" } | Select-Object -First 1 -ExpandProperty @id

if ($readmeTemplateResource) {
    # https://learn.microsoft.com/nuget/api/readme-template-resource#url-placeholders
    $readmeTemplateUrl = $readmeTemplateResource.'@id'
    $url = $readmeTemplateUrl -replace "{lower_id}", $packageId -replace "{lower_version}", $packageVersion
} else {
    # https://learn.microsoft.com/nuget/api/package-base-address-resource#download-package-content-nupkg
    $packageBaseResource = $feedIndex.resources | Where-Object { $($_.'@type') -eq "PackageBaseAddress/3.0.0" } | Select-Object -First 1 -ExpandProperty @id
    $packageBaseUrl = $packageBaseResource.'@id'

    # GET {@id}/{PACKAGE_ID}/{PACKAGE_VERSION}/{PACKAGE_ID}.nuspec
    $url = "$($packageBaseUrl)/$($packageId)/$($packageVersion)/$($packageId).nuspec"
}

if (!$url) {
    LogError "Could not construct NuGet package URL for server '$ServerName'."
    exit 1
}

$elapsed = 0

Write-Host "Checking for package. URL: $url"

while ($true) {
    if ($elapsed -gt $TimeoutInSeconds) {
        Write-Error "Package README is not available after $elapsed seconds. Timeout: $TimeoutInSeconds. URL: $url"
        exit 1
    }

    try { 
        Invoke-WebRequest -Uri $url -ErrorAction Stop | Out-Null
        Write-Host "Package is now available."
        break
    } catch {
        Write-Host "Package is not yet available. Elapsed time: $elapsed seconds."
        Start-Sleep -Seconds $SleepIntervalInSeconds
        $elapsed += $SleepIntervalInSeconds
        continue
    }
}