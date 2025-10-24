#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $BuildInfoPath,
    [string] $ArtifactsDirectory,
    [string] $TargetOs,
    [string] $TargetArch,
    [switch] $CI
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$exitCode = 0
$isPipelineRun = $CI -or $env:TF_BUILD -eq "true"
$ignoreMissingArtifacts = -not $isPipelineRun

$tempDirectory = "$RepoRoot/.work/temp"

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$ArtifactsDirectory) {
    $ArtifactsDirectory = "$RepoRoot/.work"
}

if (!$TargetOs) {
    $osPlatform = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
    $TargetOs = switch -Regex ($osPlatform) {
        "Windows" { "windows" }
        "Linux"   { "linux" }
        "Darwin"  { "macOs" }
        default { LogError "Unknown OS Platform: $osPlatform"; $exitCode = 1}
    }
}

if (!$TargetArch) {
    $archPlatform = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture
    $TargetArch = switch ($archPlatform) {
        "X64" { "x64" }
        "Arm64" { "arm64" }
        default { LogError "Unknown Architecture Platform: $archPlatform"; $exitCode = 1}
    }
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if (!(Test-Path $ArtifactsDirectory)) {
    LogError "Artifacts directory $ArtifactsDirectory does not exist."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    Write-Host "Exiting with code $exitCode"
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

function Test-NugetPackages {
    param (
        [hashtable] $Server
    )

    Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Path $tempDirectory | Out-Null

    $artifactsPath = "$ArtifactsDirectory/packages_dnx/$($Server.artifactPath)"
    if( -not (Test-Path $artifactsPath) ) {
        $message = "Artifacts path $artifactsPath does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            return $true
        } else {
            LogError $message
            return $false
        }
    }

    Copy-Item -Path (Join-Path $artifactsPath '*') -Destination $tempDirectory -Recurse -Force

    Write-Host "Copied from $artifactsPath to $tempDirectory"
    Write-Host "Validating NuGet package for server $($Server.name)"

    $output = dnx $server.dnxPackageId -y --source $tempDirectory --prerelease -- $server.dnxToolCommandName tools list
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Server tools list command completed successfully for server $($Server.name)."
    } else {
        Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
        Write-Host $output
        return $false
    }

    return $true
}

function Test-NpmPackages {
    param (
        [hashtable] $Server
    )

    switch ($TargetOs) {
        "linux" { $artifactOs = "linux" }
        "macOs"   { $artifactOs = "darwin" }
        "windows"   { $artifactOs = "win32" }
        default { throw "Unknown TargetOs: $TargetOs" }
    }

    Remove-Item -Path $tempDirectory -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Path $tempDirectory | Out-Null

    $artifactsPath = "$ArtifactsDirectory/packages_npm/$($Server.artifactPath)"
    if( -not (Test-Path $artifactsPath) ) {
        $message = "Artifacts path $artifactsPath does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            return $true
        } else {
            LogError $message
            return $false
        }
    }

    $mainPackage = Get-ChildItem -Path $artifactsPath -Filter "*.tgz" | Where-Object { $_.Name -notmatch '-(linux|darwin|win32)-' } | Select-Object -First 1
    $platformPackage = Get-ChildItem -Path $artifactsPath -Filter "*-$artifactOs-$TargetArch-*.tgz" | Select-Object -First 1

    $originalLocation = Get-Location
    Set-Location $tempDirectory
    try {
        Write-Host "Installing Platform Package: $($platformPackage.FullName)"
        npm install $platformPackage.FullName | Out-Null

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to install platform package $($platformPackage.FullName) with exit code $LASTEXITCODE"
            return $false
        }

        Write-Host "Installing Wrapper Package: $($mainPackage.FullName)"
        npm install $mainPackage.FullName | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to install platform package $($platformPackage.FullName) with exit code $LASTEXITCODE"
            return $false
        }

        $output = npx --no $server.cliName tools list
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Server tools list command completed successfully for $($server.name)"
        } else {
            Write-Host "Server tools list command failed with exit code $LASTEXITCODE"
            Write-Host $output
            return $false
        }
    } finally {
        Set-Location $originalLocation
    }

    return $true
}

function Test-DockerImages {
    param (
        [hashtable] $Server
    )

    $artifactsPath = "$ArtifactsDirectory/docker_output"
    if( -not (Test-Path $artifactsPath) ) {
        $message = "Artifacts path $artifactsPath does not exist."
        if ($ignoreMissingArtifacts) {
            LogWarning $message
            return $true
        } else {
            LogError $message
            return $false
        }
    }

    $imageTar = Get-ChildItem -Path $artifactsPath -Filter "$($Server.cliName)-image.tar" -ErrorAction SilentlyContinue | Select-Object -First 1
    Write-Host "Verifying Docker image for $($Server.name)"
    
    if (-not $imageTar) {
        LogError "Docker image tar file $($Server.cliName)-image.tar not found in $artifactsPath"
        return $false
    }

    Write-Host "Loading Docker image from $($imageTar.FullName)"
    $loadOutput = docker load -i $imageTar.FullName 2>&1
    if ($LASTEXITCODE -ne 0) {
        LogError "Failed to load Docker image with exit code $LASTEXITCODE"
        Write-Host $loadOutput
        return $false
    }
    Write-Host "Image loaded: $loadOutput"

    # Extract the loaded image tag from the docker load output
    $loadedImage = $loadOutput | Select-String -Pattern 'Loaded image: (.+)' | ForEach-Object { $_.Matches.Groups[1].Value }
    if (-not $loadedImage) {
        LogError "Could not parse loaded image name from docker load output"
        Write-Host "Load output was: $loadOutput"
        return $false
    }
    Write-Host "Loaded image tag: $loadedImage"
    
    # Verify the image exists after loading
    $imageExists = docker image inspect $loadedImage 2>&1
    if ($LASTEXITCODE -ne 0) {
        LogError "Docker image $loadedImage not found after loading"
        Write-Host $imageExists
        return $false
    }
    Write-Host "Verified image exists: $loadedImage"

    # Run the smoke test command
    Write-Host "Running smoke test: docker run --rm --entrypoint ./server-binary $loadedImage tools list"
    $output = docker run --rm --entrypoint ./server-binary $loadedImage tools list 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Server tools list command completed successfully for $($Server.name)"
        Write-Host "Output: $output"
    } else {
        LogError "Server tools list command failed with exit code $LASTEXITCODE"
        Write-Host "Output: $output"
        return $false
    }

    # Cleanup: Remove the loaded image to save space
    Write-Host "Cleaning up Docker image $loadedImage"
    docker rmi $loadedImage 2>&1 | Out-Null

    return $true
}

$servers = $buildInfo.servers

foreach($server in $servers) {
    Write-Host "Validating packages for server $($server.name) version $($server.version) for OS $TargetOs and Arch $TargetArch"

    $nugetValid = Test-NugetPackages -Server $server
    $npmValid = Test-NpmPackages -Server $server
    $dockerValid = Test-DockerImages -Server $server

    if (!$nugetValid -or !$npmValid -or !$dockerValid) {
        $exitCode = 1
    }
}

exit $exitCode
