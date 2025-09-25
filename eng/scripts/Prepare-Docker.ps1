#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [switch] $IgnoreMissingArtifacts # When running locally, ignore missing artifacts instead of failing
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
    if(!(Test-Path $BuildInfoPath)) {
        & "$PSScriptRoot/New-BuildInfo.ps1" -PublishTarget none -BuildId 12345 -OutputPath $BuildInfoPath
    }
} elseif (!(Test-Path $BuildInfoPath)) {
    Write-Error "Build info file $BuildInfoPath does not exist."
    return
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/docker_staged"
}

# Clear the output directory
Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

if(!(Test-Path $ArtifactsPath)) {
    Write-Error "Artifacts path $ArtifactsPath does not exist."
    return
}

$dockerFile = "$RepoRoot/Dockerfile"

if(!(Test-Path $dockerFile)) {
    Write-Error "Dockerfile not found at $dockerFile."
    return
}

Push-Location $RepoRoot
try {
    $buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json
    $platformName = "linux-x64"

    foreach($server in $buildInfo.servers) {
        $platform = $server.platforms | Where-Object { $_.name -eq $platformName -and -not $_.native }
        $platformOutputPath = "$OutputPath/$($server.artifactPath)/$platformName"

        New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null

        $platformArtifactPath = "$ArtifactsPath/$($platform.artifactPath)"
        if(!(Test-Path $platformArtifactPath)) {
            if ($IgnoreMissingArtifacts) {
                Write-Warning "Artifact path $platformArtifactPath does not exist. Skipping $($server.name)."
                Write-Warning "To build, run 'eng/scripts/Build-Code.ps1 -ServerName $($server.name) -OS linux -Architecture x64'"
                continue
            }

            Write-Error "Artifact path $platformArtifactPath does not exist."
            return
        }

        # Copy the server artifact to the output path
        Write-Host "`nCopying $platformName artifact from $platformArtifactPath to $platformOutputPath/dist"
        Copy-Item -Path $platformArtifactPath -Destination "$platformOutputPath/dist" -Recurse -Force

        # Copy the Dockerfile to the output path
        Write-Host "Copying Dockerfile to $platformOutputPath"
        Copy-Item -Path $dockerFile -Destination $platformOutputPath -Force
    }
}
finally {
    Pop-Location
}

