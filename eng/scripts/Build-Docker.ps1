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

$workPath = "$root/.work"
$dockerFile = "$root/Dockerfile"

$serverNames = Get-ChildItem -Path "$root/servers" -Directory | Select-Object -ExpandProperty Name

if($ServerName) {
    $serverNames = $serverNames | Where-Object { $_ -like $ServerName }
    if (!$serverNames) {
        Write-Error "No server found matching name '$ServerName'."
        return
    }
}

foreach ($serverName in $serverNames) {
    $properties = & "$PSScriptRoot/Get-ProjectProperties.ps1" -ProjectName "$ServerName.csproj"
    $dockerImageName = $properties.DockerImageName
    $version = $properties.Version

    # Will fix this when we update Dockerfile to multi-platform
    $os = "linux"
    $arch = "x64"
    $tag = "$dockerImageName`:$version$VersionSuffix";
    [string]$publishDirectory = "$workPath/build/$ServerName/$os-$arch/dist"

    if (!(Test-Path $publishDirectory)) {
        Write-Host "Didn't find '$publishDirectory'. Building server now..."
        & "$root/eng/scripts/Build-Servers.ps1" -ServerName $ServerName -VersionSuffix $VersionSuffix -SelfContained -Trimmed:$Trimmed -DebugBuild:$DebugBuild -OperatingSystem $os -Architecture $arch
    }

    if (!(Test-Path $publishDirectory)) {
        Write-Error "Build output directory does not exist: $publishDirectory"
        return
    }

    $relativeDirectory = $(Resolve-Path $publishDirectory -Relative).Replace('\', '/')

    Write-Host "Building Docker image ($tag). PATH: [$relativeDirectory]. Absolute: [$publishDirectory]."

    & docker build --build-arg PUBLISH_DIR="$relativeDirectory" --file $dockerFile --tag $tag .
}
