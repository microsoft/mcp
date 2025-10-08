#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $VersionSuffix,
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [switch] $Trimmed,
    [switch] $DebugBuild,
    [Parameter(Mandatory=$true, ParameterSetName='SpecificPlatform')]
    [ValidateSet('windows','linux','macOS')]
    [string] $OperatingSystem,
    [Parameter(Mandatory=$true, ParameterSetName='SpecificPlatform')]
    [ValidateSet('x64','arm64')]
    [string] $Architecture,
    [Parameter(ParameterSetName='AllPlatforms')]
    [switch] $AllPlatforms
)

function Get-DockerOS([string]$dotnetName) {
    switch ($dotnetName) {
        'linux' { return 'linux' }
        'macOS' { return 'linux' }
        'windows' { return 'win' }
        default { return $dotnetName }
    }
}

function Get-DockerArchitecture([string]$dotnetName) {
    switch ($dotnetName) {
        'x64' { return 'amd64' }
        default { return $dotnetName }
    }
}

. "$PSScriptRoot/../common/scripts/common.ps1"
$root = $RepoRoot.Path.Replace('\', '/')
$distPath = "$root/.work"
$dockerFile = "$root/Dockerfile"
$properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$ServerName.csproj"
$dockerImageName = $properties.DockerImageName
$buildDirectory = $([System.IO.Path]::Combine($distPath, "build", $ServerName))

if(!$Version) {
    $Version = $properties.Version
}

$SingleFile = $Trimmed
$tag = "$dockerImageName`:$Version$VersionSuffix";

if ($AllPlatforms) {
    & "$root/eng/scripts/Build-Code.ps1" -ServerName $ServerName -VersionSuffix $VersionSuffix -SelfContained -Trimmed:$Trimmed -SingleFile:$SingleFile -DebugBuild:$DebugBuild -AllPlatforms

    $operatingSystems = @('linux','windows')
    $architectures = @('x64','arm64')

} elseif ($OperatingSystem -and $Architecture) {
    if ($OperatingSystem -eq 'macOS') {
        $OperatingSystem = 'linux'
    }

    & "$root/eng/scripts/Build-Code.ps1" -ServerName $ServerName -VersionSuffix $VersionSuffix -SelfContained -Trimmed:$Trimmed -SingleFile:$SingleFile -DebugBuild:$DebugBuild -OperatingSystem $OperatingSystem -Architecture $Architecture

    $operatingSystems = @($OperatingSystem)
    $architectures = @($Architecture)
} else {
    Write-Error "Either specify both OperatingSystem and Architecture, or use the AllPlatforms switch."
    exit 1
}

if ($LastExitCode -ne 0) {
    exit $LastExitCode
}

# Move x64 folder to amd64 to match Docker naming conventions
foreach ($os in $operatingSystems) {
    
}

[string[]]$identifiers = @()
foreach($os in $operatingSystems) {
    $dockerOS = Get-DockerOS $os

    foreach($arch in $architectures) {
        $dockerArchitecture = Get-DockerArchitecture $arch

        if ($arch -eq 'x64') {
            $x64Folder = "$buildDirectory/$dockerOS-x64"
            $destination = "$buildDirectory/$dockerOS-amd64"
            Write-Host "Moving $x64Folder to $destination" -ForegroundColor Yellow
            Move-Item -Path $x64Folder -Destination $destination -Force
        }

        $identifiers += "$dockerOS/$dockerArchitecture"
    }
}

# The COPY command needs to be within the scope of the "working directory". Usually where the ./Dockerfile is.  We need a relative path. 
$relativeDirectory = $(Resolve-Path $buildDirectory -Relative).Replace('\', '/')

if (!(Test-Path $buildDirectory)) {
    Write-Error "Build output directory does not exist: $buildDirectory"
    return
}

[string]$platformString = [string]::Join(',', $identifiers)

Write-Host "Building Docker image ($tag). PATH: [$relativeDirectory]. Absolute: [$buildDirectory].  Platforms: [$platformString]"

& docker build --platform $platformString --build-arg PUBLISH_DIR="$relativeDirectory" --file $dockerFile --tag $tag .
