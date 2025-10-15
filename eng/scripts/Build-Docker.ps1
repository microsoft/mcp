#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $ServerName,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$dockerFile = "$RepoRoot/Dockerfile"
$exitCode = 0

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_docker"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$supportedPlatforms = @(
    "linux/amd64"
    "linux/arm64"
    # "windows/amd64"
    # "windows/arm64"
)

$servers = $buildInfo.servers

if($ServerName) {
    $servers = $servers | Where-Object { $_.name -eq $ServerName }
}

$originalPath = Get-Location
Set-Location $RepoRoot
try {
    foreach($server in $servers) {
        $dockerImageName = $server.dockerImageName
        $version = $server.version
        $serverName = $server.name

        if(-not $dockerImageName) {
            LogWarning "Skipping server $serverName because it does not have a dockerImageName"
            continue
        }

        foreach($platform in $server.platforms) {
            $dockerOs = switch($platform.operatingSystem) {
                "linux" { "linux" }
                "osx" { "linux" }
                "windows" { "windows" }
                default {
                    LogWarning "Skipping unsupported operating system $($platform.operatingSystem) for server $serverName"
                    continue
                }
            }

            $dockerArch = switch($platform.architecture) {
                "x64" { "amd64" }
                "arm64" { "arm64" }
                default {
                    LogWarning "Skipping unsupported architecture $($platform.architecture) for server $serverName"
                    continue
                }
            }

            $dockerPlatformString = "$dockerOs/$dockerArch"

            $relativePublishDirectory = ".work/build/$($platform.artifactPath)"
            $publishDirectory = "$RepoRoot/$relativePublishDirectory"

            if($supportedPlatforms -notcontains $dockerPlatformString) {
                LogWarning "Skipping unsupported platform $dockerPlatformString"
                continue
            }

            if (!(Test-Path $publishDirectory)) {
                LogWarning "Build output directory does not exist: $publishDirectory, skipping"
                continue
            }

            $tempPath = "$RepoRoot/.work/temp"
            Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

            $tag = "$dockerImageName`:$version";

            Write-Host "Building Docker image ($tag). PATH: [$relativePublishDirectory]. Absolute: [$publishDirectory]."
            function quote($str) {
                return "`"$str`""
            }

            $dockerArgs = @(
                "--platform $(quote $dockerPlatformString)"
                "--build-arg PUBLISH_DIR=$(quote $relativePublishDirectory)"
                "--build-arg CLI_NAME=$(quote $server.cliName + $platform.extension)"
                "--file $(quote $dockerFile)"
                "--tag $(quote $tag)"
                "--no-cache"
                "--progress plain"
                "."
            )

            Invoke-LoggedCommand "docker build $($dockerArgs -join ' ')"
            if ($LASTEXITCODE -ne 0) {
                LogError "Docker build failed for $serverName on $dockerPlatformString"
                $exitCode = 1
                continue
            }

            # the dockerImageName will contain slashes, so consider the full path including tag when creating the directory
            $platformOutputPath = "$OutputPath/$($platform.artifactPath)/$dockerImageName.tar"
            New-Item -Path (Split-Path $platformOutputPath -Parent) -ItemType Directory -Force | Out-Null

            Invoke-LoggedCommand "docker save $tag -o $(quote $platformOutputPath)"
            if ($LASTEXITCODE -ne 0) {
                LogError "Docker save failed for $serverName on $dockerPlatformString"
                $exitCode = 1
            }
        }
    }
}
finally {
    Set-Location $originalPath
}

exit $exitCode
