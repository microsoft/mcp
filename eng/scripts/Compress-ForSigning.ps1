#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $BuildInfoPath,
    [string] $ArtifactsPath,
    [string] $ArtifactPrefix,
    [string] $OutputPath
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$exitCode = 0

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work"
}

if(!$ArtifactPrefix) {
    $ArtifactPrefix = "build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/signed"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if(!(Test-Path -Path $ArtifactsPath -PathType Container)) {
    LogError "Artifacts path '$ArtifactsPath' does not exist or is not a directory."
    $exitCode = 1
}

if(!(Test-Path -Path $BuildInfoPath -PathType Leaf)) {
    LogError "Build info file '$BuildInfoPath' does not exist or is not a file."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$entitlements = "$RepoRoot/eng/dotnet-executable-entitlements.plist"

$artifactDirectories = Get-ChildItem -Path $ArtifactsPath -Directory
| Where-Object { $_.Name -like "$ArtifactPrefix*" }
| Where-Object { $_.Name -notlike '*FailedAttempt*' }

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$OutputPath = (Resolve-Path $OutputPath).Path.Replace('\', '/')

if($env:TF_BUILD) {
    foreach ($artifactDirectory in $artifactDirectories) {
        Write-Host "`n##[group] Artifact directory '$artifactDirectory' contents:"
        Get-ChildItem -Path $artifactDirectory -File -Recurse | Select-Object -ExpandProperty FullName | Out-Host
        Write-Host "##[endgroup]`n"
    }
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

foreach ($server in $buildInfo.servers) {
    Write-Host "Processing $($server.name)" -ForegroundColor Yellow
    foreach($platform in $server.platforms) {
        $artifactPath = $platform.artifactPath
        $platformOutputPath = "$outputPath/$artifactPath"
        # The relationship between artifacts and platform paths is loose
        # Locally, they'll all be in the .work/build subdirectory
        # In a pipeline, each server platform will be in its own artifact directory
        # We can ignore the artifact directory name and look for any matching <Server>/<Platform> path

        $platformSourcePath = $artifactDirectories
        | ForEach-Object { "$_/$artifactPath".Replace('\','/') }
        | Where-Object { Test-Path -Path $_ -PathType Container }
        | Select-Object -First 1

        New-Item -Path (Split-Path -Path $platformOutputPath -Parent) -ItemType Directory -Force | Out-Null

        Write-Host "Copying $platformSourcePath to $platformOutputPath`n" -ForegroundColor Yellow
        Copy-Item -Path $platformSourcePath -Destination $platformOutputPath -Recurse -Force -ProgressAction SilentlyContinue

        if ($platform.operatingSystem -eq 'macos') {
            # Only mac binaries need to be compressed. Linux binaries aren't signed and windows are signed uncompressed.

            # Mac requires code signing the binary with an entitlements file such that the signed and notarized binary will properly invoke on
            # a mac system. However, the `codesign` command is only available on a MacOS agent. With that being the case, we simply special case
            # this function here to ensure that the script does not fail outside of a MacOS agent.
            $binaryFilePath = Resolve-Path "$platformOutputPath/$($server.cliName)"

            if ($IsMacOS) {
                Invoke-LoggedCommand "chmod +x `"$binaryFilePath`""
                Invoke-LoggedCommand "codesign --deep -s - -f --options runtime --entitlements `"$entitlements`" `"$binaryFilePath`""
                Invoke-LoggedCommand "codesign -d --entitlements :- `"$binaryFilePath`""
            } else {
                Write-Warning "Mac binaries should be code signed with entitlements, but this is only possible on a mac agent."
            }

            $archivePath = "$binaryFilePath.zip"
            Write-Host "Creating $archivePath" -ForegroundColor Yellow
            # We only need to compress the single binary file.
            Compress-Archive -Path $binaryFilePath -DestinationPath $archivePath

            Write-Host "Deleting $binaryFilePath" -ForegroundColor Yellow
            Remove-Item -Path $binaryFilePath -Force -ProgressAction SilentlyContinue
        }
    }
}

if($env:TF_BUILD) {
    Write-Host "`n##[group] Output Path Contents:"
    Get-ChildItem -Path $OutputPath -File -Recurse | Select-Object -ExpandProperty FullName | Out-Host
    Write-Host "##[endgroup]`n"
}

exit $exitCode
