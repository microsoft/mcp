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
    [Parameter(Mandatory=$true, ParameterSetName='SpecificPlatform')]
    [ValidateSet('windows','linux','macOS')]
    [string] $OperatingSystem,
    [Parameter(Mandatory=$true, ParameterSetName='SpecificPlatform')]
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
    $serverDirectory = "$RepoRoot/servers/$serverName"
    $projectFile = Get-Item "$serverDirectory/src/$serverName.csproj"

    if(!$projectFile) {
        Write-Error "No project file found for $serverName"
        return
    }

    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName $projectFile.Name

    $cliName = $properties.CliName
    $version = "$($properties.Version)$VersionSuffix"
    $description = $properties.Description
    $packageName = $properties.NpmPackageName
    $keywords = $properties.NpmPackageKeywords -split ','
    $readmeUrl = $properties.ReadmeUrl

    $serverOutputDirectory = "$OutputPath/$serverName"

    if ($BuildNative) {
        $packageName += '-native'
        $serverOutputDirectory += '-native'
    }

    New-Item -Path $serverOutputDirectory -ItemType Directory -Force | Out-Null

    $wrapperPackage = [ordered]@{
        name = $packageName
        version = $version
        description = $description
        author = 'Microsoft Corporation'
        homepage = $readmeUrl
        license = 'MIT'
        keywords = $keywords
        bugs = @{ url = "https://github.com/microsoft/mcp/issues" }
        repository = @{ type = 'git'; url = 'https://github.com/microsoft/mcp.git' }
        engines = @{ node = '>=20.0.0' }
        bin = @{ $cliName = './index.js' }
        os = @()
        cpu = @()
        optionalDependencies = @{}
        scripts = @{ postinstall = "node ./scripts/post-install-script.js" }
    }

    $wrapperPackage | ConvertTo-Json | Out-File -FilePath "$serverOutputDirectory/wrapper.json" -Encoding utf8
    Write-Host "Created wrapper.json in $serverOutputDirectory" -ForegroundColor Yellow

    Copy-Item "$serverDirectory/README.md" -Destination $serverOutputDirectory -Force
    Write-Host "Copied README.md to $serverOutputDirectory" -ForegroundColor Yellow

    foreach ($os in $operatingSystems) {
        foreach ($arch in $architectures) {
            switch($os) {
                'win' { $node_os = 'win32'; $extension = '.exe' }
                'osx' { $node_os = 'darwin'; $extension = '' }
                default { $node_os = $os; $extension = '' }
            }

            $outputDir = "$serverOutputDirectory/$os-$arch"
            Write-Host "Building version $version, $os-$arch in $outputDir" -ForegroundColor Green

            $configuration = if ($DebugBuild) { 'Debug' } else { 'Release' }

            if ($CleanBuild) {
                # Clean up any previous build artifacts.
                Invoke-LoggedCommand "dotnet clean '$projectFile' --configuration $configuration" -GroupOutput
            }

            # Clear and recreate the package output directory
            Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            New-Item -Path "$outputDir/dist" -ItemType Directory -Force | Out-Null

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

            $package = [ordered]@{
                name = "$packageName-$node_os-$arch"
                version = $version
                description = "$description, for $node_os on $arch"
                author = 'Microsoft Corporation'
                homepage = $readmeUrl
                license = 'MIT'
                keywords = $properties.NpmPackageKeywords -split ','
                bugs = @{ url = "https://github.com/microsoft/mcp/issues" }
                repository = @{ type = 'git'; url = 'https://github.com/microsoft/mcp.git' }
                engines = @{ node = '>=20.0.0' }
                main = './index.js'
                bin = @{ "$cliName-$node_os-$arch" = "./dist/$cliName$extension" }
                os = @($node_os)
                cpu = @($arch)
            }

            $package
            | ConvertTo-Json
            | Out-File -FilePath "$outputDir/package.json" -Encoding utf8

            Write-Host "Created package.json in $outputDir" -ForegroundColor Yellow

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
