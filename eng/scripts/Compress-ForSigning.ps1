#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $ArtifactsPath,
    [string] $ArtifactPrefix,
    [string] $ServerName,
    [string] $OutputPath
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work"
}

if(!$ArtifactPrefix) {
    $ArtifactPrefix = "build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/signed"
}

if(!(Test-Path -Path $ArtifactsPath -PathType Container)) {
    Write-Error "Artifacts path '$ArtifactsPath' does not exist or is not a directory."
    return
}

$entitlements = "$RepoRoot/eng/dotnet-executable-entitlements.plist"

$artifactDirectories = Get-ChildItem -Path $ArtifactsPath -Directory
| Where-Object { $_.Name -like "$ArtifactPrefix*" }
| Where-Object { $_.Name -notlike '*FailedAttempt*' }

Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
$OutputPath = (Resolve-Path $OutputPath).Path.Replace('\', '/')

if($env:TF_BUILD) {
    foreach ($artifactDirectory in $artifactDirectories) {
        Write-Host "`n##[group] Artifact directory '$artifactDirectory' contents:"
        Get-ChildItem -Path $artifactDirectory -File -Recurse | Select-Object -ExpandProperty FullName | Out-Host
        Write-Host "##[endgroup]`n"
    }
}

$serverJsonFiles = $artifactDirectories | Get-ChildItem -Filter "wrapper.json" -Recurse

if ($ServerName) {
    $serverJsonFiles = $serverJsonFiles | Where-Object { $_.Directory.Name -ieq $ServerName }
}

foreach ($serverJsonFile in $serverJsonFiles) {
    Write-Host "Processing $serverJsonFile" -ForegroundColor Yellow
    $serverDirectory = $serverJsonFile.Directory
    $serverOutputPath = "$OutputPath/$($serverDirectory.Name)"

    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null
    if(!(Test-Path -Path "$serverOutputPath/wrapper.Json")) {
        Copy-Item -Path $serverJsonFile.FullName -Destination $serverOutputPath -Force
        Copy-Item -Path "$serverDirectory/README.md" -Destination $serverOutputPath -Force
    }

    $packageJsonFiles = Get-ChildItem -Path $serverDirectory -Filter "package.json" -Recurse
    foreach($packageJsonFile in $packageJsonFiles) {
        $packageDirectory = $packageJsonFile.DirectoryName.Replace('\','/')
        Write-Host "Copying $packageDirectory to $serverOutputPath`n" -ForegroundColor Yellow
        Copy-Item -Path $packageDirectory -Destination $serverOutputPath -Recurse -Force

        $packageOutputDirectory = "$serverOutputPath/$($packageJsonFile.Directory.Name)"

        $package = Get-Content $packageJsonFile -Raw | ConvertFrom-Json -AsHashtable
        $os = $package.os[0]

        Write-Host "`nProcessing $os package in $packageOutputDirectory" -ForegroundColor Yellow
        if ($os -eq 'darwin') {
            # Only mac binaries need to be compressed. Linux binaries aren't signed and windows are signed uncompressed.

            # Mac requires code signing the binary with an entitlements file such that the signed and notarized binary will properly invoke on
            # a mac system. However, the `codesign` command is only available on a MacOS agent. With that being the case, we simply special case
            # this function here to ensure that the script does not fail outside of a MacOS agent.
            $binaryFilePath = Resolve-Path "$packageOutputDirectory/$($package.bin.Values[0])"

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
