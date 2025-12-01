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

function ReplacePropertyValue {
    param (
        [hashtable]$HashTable,
        [string]$PropertyName,
        [string]$NewValue
    )

    if ($HashTable.ContainsKey($PropertyName)) {
        $HashTable["$PropertyName"] = $NewValue
    } else {
        LogError "Hashtable does not have a property named '$PropertyName'."
    }
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

[hashtable]$jsonHashTable = Get-Content -Path $serverJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json -AsHashtable

ReplacePropertyValue -HashTable $jsonHashTable -PropertyName 'name' -NewValue $server.mcpRepositoryName
ReplacePropertyValue -HashTable $jsonHashTable -PropertyName 'version' -NewValue $server.version

foreach ($package in $jsonHashTable.packages) {
    ReplacePropertyValue -HashTable $package -PropertyName 'version' -NewValue $server.version

    switch ($package.registryType) {
        'nuget' {
            ReplacePropertyValue -HashTable $package -PropertyName 'identifier' -NewValue $server.dnxPackageId
        }
        'npm' {
            ReplacePropertyValue -HashTable $package -PropertyName 'identifier' -NewValue $server.npmPackageName
        }
        'docker' {
            ReplacePropertyValue -HashTable $package -PropertyName 'identifier' -NewValue $server.dockerImageName
        }
        Default {
            LogWarning "Unknown package registry type '$($package.registryType)' in server.json."
        }
    }
}

$updatedJson = $jsonHashTable | ConvertTo-Json -Depth 10

if (!$OutputPath) {
    $serverOutputPath = "$RepoRoot/.work/build/$($server.artifactPath)/"
    $OutputPath = "$serverOutputPath/server.json"

    LogInfo "Output path not provided. Using default path: $serverOutputPath"
}

LogInfo "Writing updated server.json to $OutputPath"

New-Item -ItemType File -Force -Path $OutputPath | Out-Null
Set-Content -Path $OutputPath -Value $updatedJson -Encoding UTF8 -NoNewLine