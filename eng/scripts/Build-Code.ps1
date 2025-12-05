#!/bin/env pwsh
#Requires -Version 7

# When calling with BuildInfoPath or PlatformName, we build according to the platform definitions in build_info.json.
# Otherwise, we build based on the custom build parameters.
[CmdletBinding(DefaultParameterSetName='CustomPlatform')]
param (
    # Common Parameters
    [string] $OutputPath,
    [string] $ServerName,

    # Common Switches
    [switch] $SelfContained,
    [switch] $SingleFile,
    [switch] $ReadyToRun,
    [switch] $ReleaseBuild,
    [switch] $CleanBuild,

    # build_info.json based parameters
    [Parameter(ParameterSetName='BuildInfoPlatform')]
    [string] $BuildInfoPath,
    [Parameter(ParameterSetName='BuildInfoPlatform')]
    [string] $PlatformName,

    # Custom build parameters
    [Parameter(ParameterSetName='CustomPlatform')]
    [Alias('OS')]
    [string[]] $OperatingSystems,
    [Parameter(ParameterSetName='CustomPlatform')]
    [string[]] $Architectures,
    [Parameter(ParameterSetName='CustomPlatform')]
    [switch] $Trimmed,
    [Parameter(ParameterSetName='CustomPlatform')]
    [switch] $Native
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/helpers/BuildHelpers.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$exitCode = 0

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

function BuildServer($server) {
    $serverName = $server.name
    $projectPath = $server.path

    if (!(Test-Path $projectPath)) {
        LogError "No project file found for $serverName"
        $script:exitCode = 1
        return
    }

    $version = $server.version

    if ($PlatformName) {
        $platforms = $server.platforms | Where-Object { $_.name -eq $PlatformName }

        if ($platforms.Count -eq 0) {
            LogError "No build configuration found for $serverName with platform name $PlatformName"
            $script:exitCode = 1
            return
        }
    } else {
        $platforms = $server.platforms
    }

    foreach ($platform in $platforms) {
        $dotnetOs = $platform.dotnetOs
        $arch = $platform.architecture
        $configuration = if ($ReleaseBuild) { 'Release' } else { 'Debug' }
        $runtime = "$dotnetOs-$arch"

        $outputDir = "$OutputPath/$($platform.artifactPath)"
        Write-Host "Building $configuration $runtime, version $version in $outputDir" -ForegroundColor Green

        # Clear and recreate the package output directory
        Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -Path $outputDir -ItemType Directory -Force | Out-Null

        $command = "dotnet publish '$projectPath' --runtime '$runtime' --output '$outputDir' /p:Version=$version /p:Configuration=$configuration"

        if ($SelfContained -or $platform.trimmed) {
            $command += " --self-contained"
        }

        if ($platform.trimmed) {
            $command += " /p:PublishTrimmed=true"
        }

        if ($platform.native) {
            $command += " /p:BuildNative=true"
        }

        if ($ReadyToRun) {
            $command += " /p:PublishReadyToRun=true"
        }

        if ($SingleFile) {
            $command += " /p:PublishSingleFile=true"
        }

        Invoke-LoggedMsBuildCommand $command -GroupOutput
    }

    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
}

function CreateServersWithPlatforms {
    if (!$OperatingSystems) {
        if ($IsWindows) {
            $OperatingSystems = @('windows')
        } elseif ($IsLinux) {
            $OperatingSystems = @('linux')
        } elseif ($IsMacOS) {
            $OperatingSystems = @('macos')
        } else {
            LogError "Unsupported OS detected. Supported OS are Windows, Linux and macOS."
            exit 1
        }
    }

    if (!$Architectures) {
        $currentArch = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier.Split('-')[1]
        $Architectures = @($currentArch)
    }

    $OperatingSystems = $OperatingSystems | Sort-Object -Unique
    $Architectures = $Architectures | Sort-Object -Unique
    $osDetails = Get-OperatingSystems

    $serverDirectories = Get-ChildItem "$RepoRoot/servers" -Directory
    $serverProjects = $serverDirectories | Get-ChildItem -Filter "src/*.csproj"
    if ($ServerName) {
        $serverProjects = $serverProjects | Where-Object { $_.BaseName -eq $ServerName }
        if ($serverProjects.Count -eq 0) {
            LogError "No server project found with name '$ServerName'."
            exit 1
        }
    }

    $serverProjects | ForEach-Object {
        $serverName = $_.BaseName
        $projectPath = $_.FullName
        $properties = . "$PSScriptRoot/Get-ProjectProperties.ps1" -Path $projectPath

        $platforms = @($OperatingSystems | ForEach-Object {
            $os = $osDetails | Where-Object name -eq $_

            if (-not $os) {
                LogError "Unsupported operating system specified: '$_'. Supported OS are: $($osDetails.name -join ', ')"
                exit 1
            }

            $Architectures | ForEach-Object {
                $platform = "$($os.name)-$_"
                [ordered]@{
                    name = $platform
                    dotnetOs = $os.dotnetName
                    architecture = $_
                    artifactPath = "$serverName/$platform"
                    native = $Native
                    trimmed = $Trimmed
                }
            }
        })

        [ordered]@{
            name = $serverName
            path = $projectPath.Replace('\', '/')
            version = $properties.Version
            platforms = $platforms
        }
    }
}

function GetServersFromBuildInfo {
    if (!$BuildInfoPath) {
        $BuildInfoPath = "$RepoRoot/.work/build_info.json"
    }

    if (!(Test-Path $BuildInfoPath)) {
        LogError "Build info file not found at path '$BuildInfoPath'. Please provide a valid path using -BuildInfoPath."
        exit 1
    } else {
        $buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable
        $servers = $buildInfo.servers
    }

    if ($ServerName) {
        $servers = $servers | Where-Object { $_.name -eq $ServerName }
        if ($servers.Count -eq 0) {
            LogError "No build configuration found for server named '$ServerName' in build info file."
            exit 1
        }
    }

    return $servers
}

$servers = @()

if ($PSCmdlet.ParameterSetName -eq 'CustomPlatform') {
    $servers = CreateServersWithPlatforms
} else {
    $servers = GetServersFromBuildInfo
}

# Exit early if there were parameter errors
if ($exitCode -ne 0) {
    exit $exitCode
}

Push-Location $RepoRoot
try {
    if ($CleanBuild) {
        # Clean up any previous build artifacts.
        Write-Host "Removing existing bin and obj folders"
        Remove-Item * -Recurse -Include 'obj', 'bin' -Force -ProgressAction SilentlyContinue
    }

    foreach ($server in $servers) {
        BuildServer $server

        if ($LastExitCode -ne 0) {
            $exitCode = $LastExitCode
        }
    }
}
finally {
    Pop-Location
}

exit $exitCode
