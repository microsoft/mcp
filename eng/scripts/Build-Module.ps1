#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $OutputPath,
    [string] $VersionSuffix,
    [switch] $SelfContained,
    [switch] $SingleFile,
    [switch] $ReadyToRun,
    [switch] $Trimmed,
    [switch] $DebugBuild,
    [switch] $CleanBuild,
    [switch] $BuildNative,
    [string] $ServerName,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('windows','linux','macOS')]
    [string] $OperatingSystem,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('x64','arm64')]
    [string] $Architecture,
    [Parameter(ParameterSetName='AllPlatforms')]
    [switch] $AllPlatforms
)

$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build"
}

if($AllPlatforms -and $BuildNative) {
    Write-Warning "Native Builds do not support Cross OS builds. Only building for the current OS."
}

#normalize OperatingSystem and Architecture
$runtime = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier.Split('-')
if($OperatingSystem) {
    switch($OperatingSystem) {
        'windows' { $operatingSystems = @('win') }
        'linux' { $operatingSystems = @('linux') }
        'macos' { $operatingSystems = @('osx') }
        default { Write-Error "Unsupported operating system: $OperatingSystem"; return }
    }
} else {
    $operatingSystems = ($AllPlatforms -and !$BuildNative) ? @('win', 'linux', 'osx') : @($runtime[0])
}

if($Architecture) {
    if ($Architecture -notin @('x64', 'arm64')) {
        Write-Error "Unsupported architecture: $Architecture"
        return
    }
    $architectures = $($Architecture)
} else {
    $architectures = $AllPlatforms ? @('x64', 'arm64') : @($runtime[1])
}

function BuildServer($serverName) {
    $projectFile = Get-Item "$RepoRoot/servers/$serverName/src/$serverName.csproj"

    if(!$projectFile) {
        Write-Error "No project file found for $serverName"
        return
    }


    $version = & "$PSScriptRoot/Get-Version.ps1" -ServerName $serverName
    $version = "$version$VersionSuffix"

    [xml]$project = Get-Content $projectFile -Raw
    $cliName = ($project.Project.PropertyGroup.CliName | Select-Object -First 1).Trim()

    foreach ($os in $operatingSystems) {
        foreach ($arch in $architectures) {
            switch($os) {
                'win' { $node_os = 'win32'; $extension = '.exe' }
                'osx' { $node_os = 'darwin'; $extension = '' }
                default { $node_os = $os; $extension = '' }
            }

            $outputDir = "$OutputPath/$ServerName$($BuildNative ? '-native' : '')/$os-$arch"
            Write-Host "Building version $version, $os-$arch in $outputDir" -ForegroundColor Green

            $configuration = if ($DebugBuild) { 'Debug' } else { 'Release' }

            if ($CleanBuild) {
                # Clean up any previous azmcp build artifacts.
                Invoke-LoggedCommand "dotnet clean '$projectFile' --configuration $configuration" -GroupOutput
            }

            # Clear and recreate the package output directory
            Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            New-Item -Path "$outputDir/dist" -ItemType Directory -Force | Out-Null

            Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination "$outputDir/dist" -Force

            $command = "dotnet publish '$projectFile' --runtime '$os-$arch' --output '$outputDir/dist' /p:Version=$version /p:Configuration=$configuration"

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

            $package = Get-Content "$RepoRoot/servers/$serverName/package.json" -Raw
            | ConvertFrom-Json -AsHashtable

            $package.name += "$($BuildNative ? '-native' : '')-$node_os-$arch"
            $package.version = $version
            $package.description += ", for $node_os on $arch"
            $package.bin = @{ "$cliName-$node_os-$arch" = "./dist/$cliName$extension" }
            $package.os = @($node_os)
            $package.cpu = @($arch)
            $package.Remove('scripts')
            $package.Remove('optionalDependencies')

            $package
            | ConvertTo-Json
            | Out-File -FilePath "$outputDir/package.json" -Encoding utf8

            Write-Host "Updated package.json in $outputDir" -ForegroundColor Yellow

            Write-Host "`nBuild completed successfully!" -ForegroundColor Green
        }
    }
}


Push-Location $RepoRoot
try {
    $serverNames = @(if($ServerName) {
        $ServerName
    } else {
        Get-ChildItem -Path "$RepoRoot/servers" -Directory | Select-Object -ExpandProperty Name
    })

    foreach ($serverName in $serverNames) {
        BuildServer $serverName
    }
}
finally {
    Pop-Location
}
