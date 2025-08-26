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
    [switch] $SkipBuildServer,
    [string] $ServerName,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('windows','linux','macOS')]
    [string] $OperatingSystem,
    [Parameter(Mandatory=$true, ParameterSetName='Named')]
    [ValidateSet('x64','arm64')]
    [string] $Architecture,
    [Parameter(ParameterSetName='AllPlatforms')]
    [switch] $AllPlatforms,
    [ValidateSet('none','runtime','agnostic')]
    [string] $ProduceNugetPackages = 'none'


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


function Get-ProjectInfo ($serverName) {
    $serverDirectory = Join-Path $RepoRoot "servers" $serverName
    $projectFile = Get-Item (Join-Path $serverDirectory "src" "$serverName.csproj")
    $serverOutputDir = Join-Path $OutputPath $serverName
    New-Item -Path $serverOutputDir -ItemType Directory -Force | Out-Null

    if(!$projectFile) {
        Write-Error "No project file found for $serverName"
        return $null
    }

    return [PSCustomObject]@{
        ServerDirectory = $serverDirectory
        ProjectFile = $projectFile
        ServerOutputDir = $serverOutputDir
    }
}

function BuildServer($serverName, $serverDirectory, $projectFile, $packageOutputDir) {
    if (-not (Test-Path -Path $packageOutputDir)) {
        New-Item -Path $packageOutputDir -ItemType Directory -Force | Out-Null
    }
    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$serverName.csproj"

    $cliName = $properties.CliName
    $version = "$($properties.Version)$VersionSuffix"
    $description = $properties.Description
    $packageName = $properties.NpmPackageName
    $keywords = $properties.NpmPackageKeywords -split ','
    $readmeUrl = $properties.ReadmeUrl

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

    $wrapperPackage | ConvertTo-Json | Out-File -FilePath "$packageOutputDir/wrapper.json" -Encoding utf8
    Write-Host "Created wrapper.json in $packageOutputDir" -ForegroundColor Yellow

    Copy-Item "$serverDirectory/README.md" -Destination $packageOutputDir -Force
    Write-Host "Copied README.md to $packageOutputDir" -ForegroundColor Yellow

    foreach ($os in $operatingSystems) {
        foreach ($arch in $architectures) {
            switch($os) {
                'win' { $node_os = 'win32'; $extension = '.exe' }
                'osx' { $node_os = 'darwin'; $extension = '' }
                default { $node_os = $os; $extension = '' }
            }

            $outputDir = "$packageOutputDir$($BuildNative ? '-native' : '')/$os-$arch"
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
                name = "$packageName$($BuildNative ? '-native' : '')-$node_os-$arch"
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

function CreateRuntimeNugetPackage($serverName, $serverDirectory, $projectFile, $packageOutputDir) {
    if (-not (Test-Path -Path $packageOutputDir)) {
        New-Item -Path $packageOutputDir -ItemType Directory -Force | Out-Null
    }
    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$serverName.csproj"
    $version = $properties.Version
    foreach ($os in $operatingSystems) {
        foreach ($arch in $architectures) {
            $outputDir = "$packageOutputDir/$os-$arch"
            Write-Host "Packing version $version, $os-$arch in $outputDir" -ForegroundColor Green

            $configuration = if ($DebugBuild) { 'Debug' } else { 'Release' }

            if ($CleanBuild) {
                # Clean up any previous build artifacts.
                Invoke-LoggedCommand "dotnet clean '$projectFile' --configuration $configuration" -GroupOutput
            }

            # Clear and recreate the package output directory
            Remove-Item -Path $outputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            New-Item -Path "$outputDir" -ItemType Directory -Force | Out-Null
            $command = "dotnet pack '$projectFile' --runtime '$os-$arch' --output '$outputDir' /p:Version=$version /p:Configuration=$configuration /p:BuildNative=true"
            Invoke-LoggedCommand $command -GroupOutput
        }
    }
}

function CreateAgnosticNugetPackage($serverName, $serverDirectory, $projectFile, $packageOutputDir) {
    if (-not (Test-Path -Path $packageOutputDir)) {
        New-Item -Path $packageOutputDir -ItemType Directory -Force | Out-Null
    }
    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$serverName.csproj"
    $version = $properties.Version
    Write-Host "Packing version $version, agnostic in $packageOutputDir" -ForegroundColor Green

    $configuration = if ($DebugBuild) { 'Debug' } else { 'Release' }

    if ($CleanBuild) {
        # Clean up any previous build artifacts.
        Invoke-LoggedCommand "dotnet clean '$projectFile' --configuration $configuration" -GroupOutput
    }

    # Clear and recreate the package output directory
    Remove-Item -Path $packageOutputDir -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    $command = "dotnet pack '$projectFile' --output '$packageOutputDir' /p:Version=$version /p:Configuration=$configuration /p:BuildNative=true"
    Invoke-LoggedCommand $command -GroupOutput
}

Push-Location $RepoRoot
try {
    $serverNames = @(if($ServerName) {
        $ServerName
    } else {
        Get-ChildItem -Path "$RepoRoot/servers" -Directory | Select-Object -ExpandProperty Name
    })

    foreach ($serverName in $serverNames) {
        $info = Get-ProjectInfo -serverName $serverName
        if(-not $info) {
            Write-Warning "Skipping $serverName (project file missing)"
            continue
        }

        if (-not $SkipBuildServer) {
            BuildServer -serverName $serverName `
                -serverDirectory $info.ServerDirectory `
                -projectFile $info.ProjectFile `
                -packageOutputDir (Join-Path $info.ServerOutputDir "npm")
        }

        if ($ProduceNugetPackages -eq 'runtime') {
            CreateRuntimeNugetPackage -serverName $serverName `
                -serverDirectory $info.ServerDirectory `
                -projectFile $info.ProjectFile `
                -packageOutputDir (Join-Path $info.ServerOutputDir "nuget")
        } elseif ($ProduceNugetPackages -eq 'agnostic') {
            CreateAgnosticNugetPackage -serverName $serverName `
                -serverDirectory $info.ServerDirectory `
                -projectFile $info.ProjectFile `
                -packageOutputDir (Join-Path $info.ServerOutputDir "nuget")
        }
    }
}
finally {
    Pop-Location
}
