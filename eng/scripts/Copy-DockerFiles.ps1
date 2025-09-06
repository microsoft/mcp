#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $OutputPath
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/docker_drops"
}

# Clear and recreate the output directory
Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

if(!(Test-Path $ArtifactsPath)) {
    Write-Error "Artifacts path $ArtifactsPath does not exist."
    return
}

Push-Location $RepoRoot
try {
    $serverJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse
    | Where-Object { $_.Directory.Name -notlike '*-native' }

    foreach($serverJsonFile in $serverJsonFiles) {
        $serverDirectory = $serverJsonFile.Directory
        $serverName = $serverDirectory.Name
        $serverOutputPath = "$OutputPath/$serverName"
        $serverJson = Get-Content -Path $serverJsonFile.FullName | ConvertFrom-Json

        $binariesPath = "$serverDirectory/linux-x64/dist"

        New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

        $projectProperties = & "$RepoRoot/eng/scripts/Get-ProjectProperties.ps1" -ProjectName "$serverName.csproj"

        $dockerVariables = @{
            DockerImageName = $projectProperties.DockerImageName
            DockerImageVersion = $serverJson.version
        } | ConvertTo-Json

        $variablesPath = "$serverOutputPath/variables.json"

        Write-Host "Writing Docker variables to $variablesPath`:`n$dockerVariables"
        $dockerVariables | Out-File -FilePath $variablesPath -Encoding utf8

        Copy-Item -Path "$RepoRoot/DockerFile" -Destination $serverOutputPath -Force
        Write-Host "Copied DockerFile to $serverOutputPath"

        Copy-Item -Path $binariesPath -Destination "$serverOutputPath/dist" -Recurse -Force
        Write-Host "Copied $binariesPath to $serverOutputPath/dist"
    }
}
finally {
    Pop-Location
}
