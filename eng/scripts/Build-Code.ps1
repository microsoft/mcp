#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [switch] $SelfContained,
    [switch] $SingleFile,
    [switch] $ReadyToRun,
    [switch] $Trimmed,
    [switch] $DebugBuild,
    [switch] $CleanBuild,
    [switch] $BuildNative,
    [string] $ServerName,

    [Parameter(Mandatory, ParameterSetName='SpecificPlatform')]
    [Alias('OS')]
    [ValidateSet('windows','linux','macOS')]
    [string[]] $OperatingSystem,

    [Parameter(Mandatory, ParameterSetName='SpecificPlatform')]
    [ValidateSet('x64','arm64')]
    [string[]] $Architecture,

    [Parameter(ParameterSetName='AllPlatforms')]
    [switch] $AllPlatforms
)

$ErrorActionPreference = 'Continue'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$exitCode = 0

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    if ($env:TF_BUILD -ne 'true') {
        & "$PSScriptRoot/New-BuildInfo.ps1" -PublishTarget none -BuildId 12345 -OutputPath $BuildInfoPath
    } else {
        LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
        $exitCode = 1
    }
}

# normalize OperatingSystem and Architecture
$runtime = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier.Split('-')

if($OperatingSystem) {
    $OperatingSystem = $OperatingSystem | Select-Object -Unique
} else {
    if($AllPlatforms -and $BuildNative) {
        LogWarning "Native Builds do not support Cross OS builds. Only building for the current OS."
    }

    if ($AllPlatforms -and !$BuildNative) {
        $OperatingSystem = @('windows', 'linux', 'macOS')
    } else {
        if ($IsWindows) {
            $OperatingSystem = @('windows')
        } elseif ($IsLinux) {
            $OperatingSystem = @('linux')
        } elseif ($IsMacOS) {
            $OperatingSystem = @('macOS')
        } else {
            LogError "Unsupported OS detected. Supported OS are Windows, Linux and macOS."
            $exitCode = 1
        }
    }
}

if($Architecture) {
    $Architecture = $Architecture | Select-Object -Unique
} else {
    $Architecture = $AllPlatforms ? @('x64', 'arm64') : @($runtime[1])
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable
if (!$?) {
    LogError "Failed to read build info from $BuildInfoPath"
    $exitCode = 1
}

function BuildServer($server) {
    $serverName = $server.name

    if(!(Test-Path $server.path)) {
        LogError "No project file found for $serverName"
        $script:exitCode = 1
        return
    }

    $projectPath = $server.path
    $version = $server.version

    $serverOutputDirectory = "$OutputPath/$($server.artifactPath)"

    New-Item -Path $serverOutputDirectory -ItemType Directory -Force | Out-Null

    foreach ($os in $OperatingSystem) {
        foreach ($arch in $Architecture) {
            $platform = $server.platforms
            | Where-Object {
                ($_.operatingSystem -eq $os) -and
                ($_.architecture -eq $arch) -and
                ($_.native -eq $BuildNative) }

            $dotnetOs = $platform.dotnetOs
            $runtime = "$dotnetOs-$arch"
            $configuration = if ($DebugBuild) { 'Debug' } else { 'Release' }

            $outputDir = "$OutputPath/$($platform.artifactPath)"
            Write-Host "Building $configuration $runtime, version $version in $outputDir" -ForegroundColor Green

            if ($CleanBuild) {
                # Clean up any previous build artifacts.
                Invoke-LoggedCommand "dotnet clean '$projectPath' --configuration $configuration" -GroupOutput
            }

            # Clear and recreate the package output directory
            Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            New-Item -Path $outputDir -ItemType Directory -Force | Out-Null

            $command = "dotnet publish '$projectPath' --runtime '$runtime' --output '$outputDir' /p:Version=$version /p:Configuration=$configuration"

            if($SelfContained) {
                $command += " --self-contained"
            }

            if($ReadyToRun) {
                $command += " /p:PublishReadyToRun=true"
            }

            if($Trimmed) {
                $command += " /p:PublishTrimmed=true"
            }

            if($BuildNative) {
                $command += " /p:BuildNative=true"
            }

            if($SingleFile) {
                $command += " /p:PublishSingleFile=true"
            }

            Invoke-LoggedCommand $command -GroupOutput

            Write-Host "`nBuild completed successfully!" -ForegroundColor Green
        }
    }
}

# Exit early if there were parameter errors
if($exitCode -ne 0) {
    exit $exitCode
}

Push-Location $RepoRoot
try {
    foreach ($server in $buildInfo.servers) {
        BuildServer $server
    }
}
finally {
    Pop-Location
}

exit $exitCode
