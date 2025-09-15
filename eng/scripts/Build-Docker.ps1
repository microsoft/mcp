#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding(DefaultParameterSetName='none')]
param(
    [string] $VersionSuffix,
    [string] $ServerName,
    [switch] $Trimmed,
    [switch] $DebugBuild
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$root = $RepoRoot.Path.Replace('\', '/')
$distPath = "$root/.work"
$dockerFile = "$root/Dockerfile"
$properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$ServerName.csproj"
$dockerImageName = $properties.DockerImageName

if(!$Version) {
    $Version = $properties.Version
}

# Will fix this when we update Dockerfile to multi-platform
$os = "linux"
$arch = "x64"
$SingleFile = $Trimmed
$tag = "$dockerImageName`:$Version$VersionSuffix";

& "$root/eng/scripts/Build-Module.ps1" -ServerName $ServerName -VersionSuffix $VersionSuffix -SelfContained -Trimmed:$Trimmed -SingleFile:$SingleFile -DebugBuild:$DebugBuild -OperatingSystem $os -Architecture $arch

[string]$publishDirectory = $([System.IO.Path]::Combine($distPath, "build", $ServerName, "$os-$arch", "dist"))
$relativeDirectory = $(Resolve-Path $publishDirectory -Relative).Replace('\', '/')

Write-Host "Building Docker image ($tag). PATH: [$relativeDirectory]. Absolute: [$publishDirectory]."

if (!(Test-Path $publishDirectory)) {
    Write-Error "Build output directory does not exist: $publishDirectory"
    return
}

& docker build --build-arg PUBLISH_DIR="$relativeDirectory" --file $dockerFile --tag $tag .
