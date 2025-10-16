#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_npm"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if(!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    $exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

New-Item -Path $OutputPath -ItemType Directory | Out-Null

foreach($server in $buildInfo.servers) {
    foreach($platform in $server.platforms) {
        $nameParts = @($server.name, $platform.dotnetOs, $platform.architecture)
        if ($platform.native) {
            $nameParts += 'native'
        }

        $zipName = "$($nameParts -join '-').zip"
        $platformPath = "$ArtifactsPath/$($platform.artifactPath)"
        if(!(Test-Path $platformPath)) {
            $message = "Artifact path $ArtifactsPath/$($platform.artifactPath) does not exist."
            $exitCode = 1

            if ($ignoreMissingArtifacts) {
                LogWarning $message
                continue
            } else {
                LogError $message
                continue
            }
        }

        Compress-Archive -Path "$platformPath/*", "$RepoRoot/LICENSE", "$RepoRoot/NOTICE.txt" -DestinationPath "$OutputPath/$zipName" -Force | Out-Null
        Write-Host "Created $OutputPath/$zipName" -ForegroundColor Green
    }
}

exit $exitCode
