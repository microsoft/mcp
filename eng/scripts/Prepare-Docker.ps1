#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [switch] $CI
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/docker_staged"
}

$dockerFile = "$RepoRoot/Dockerfile"
if(!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    $exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if(!(Test-Path $dockerFile)) {
    LogError "Dockerfile not found at $dockerFile."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

# Clear the output directory
Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

Push-Location $RepoRoot
try {
    $buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json
    $platformName = "linux-x64"
    $cliNames = @()
    foreach($server in $buildInfo.servers) {
        $platform = $server.platforms | Where-Object { $_.name -eq $platformName -and -not $_.native }
        $platformOutputPath = "$OutputPath/$($server.artifactPath)/$platformName"

        New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null

        $platformArtifactPath = "$ArtifactsPath/$($platform.artifactPath)"
        if(!(Test-Path $platformArtifactPath)) {
            if ($ignoreMissingArtifacts) {
                LogWarning "Artifact path $platformArtifactPath does not exist. Skipping $($server.name)."
                LogWarning "To build, run 'eng/scripts/Build-Code.ps1 -ServerName $($server.name) -OS linux -Architecture x64'"
            } else {
                LogError "Artifact path $platformArtifactPath does not exist."
                $exitCode = 1
            }
            continue
        }

        # Copy the server artifact to the output path
        Write-Host "`nCopying $platformName artifact from $platformArtifactPath to $platformOutputPath/dist"
        Copy-Item -Path $platformArtifactPath -Destination "$platformOutputPath/dist" -Recurse -Force

        # Copy the Dockerfile to the output path
        Write-Host "Copying Dockerfile to $platformOutputPath"
        Copy-Item -Path $dockerFile -Destination $platformOutputPath -Force

        # Set output variables for downstream jobs
        # DockerImageName-<cliName>, DockerImageVersion-<cliName>, DockerExecutableName-<cliName>, DockerLocalTag-<cliName>
        # Example: DockerImageName-azmcp, DockerImageVersion-azmcp, DockerExecutableName-azmcp, DockerLocalTag-azmcp
        $imageName = $server.dockerImageName
        Write-Host "Setting variable DockerImageName-$($server.cliName) to $imageName"
        Write-Host "##vso[task.setvariable variable=DockerImageName-$($server.cliName);isOutput=true]$imageName"

        $version = $server.version
        Write-Host "Setting variable DockerImageVersion-$($server.cliName) to $version"
        Write-Host "##vso[task.setvariable variable=DockerImageVersion-$($server.cliName);isOutput=true]$version"

        $cliName = $server.cliName
        $cliNames += $cliName
        $extension = $server.extension
        $executableName = "$cliName$extension"
        Write-Host "Setting variable DockerExecutableName-$($server.cliName) to $executableName"
        Write-Host "##vso[task.setvariable variable=DockerExecutableName-$($server.cliName);isOutput=true]$executableName"

        $dockerLocalTag = "$(Build.Repository.Name)/$($server.cliName):$(Build.BuildId)"
        Write-Host "Setting variable DockerLocalTag-$($server.cliName) to $dockerLocalTag"
        # Need this also as a local variable for the next steps in the template
        Write-Host "##vso[task.setvariable variable=DockerLocalTag-$($server.cliName)]$dockerLocalTag"
        Write-Host "##vso[task.setvariable variable=DockerLocalTag-$($server.cliName);isOutput=true]$dockerLocalTag"
    }
}
finally {
    Pop-Location
}

exit $exitCode
