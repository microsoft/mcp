#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [switch] $Flat
)

. "$PSScriptRoot/../common/scripts/common.ps1"

function Get-ProjectIds {
    param (
        [string] $slnFilePath
    )

    $projectIds = @{}
    if (Test-Path $slnFilePath) {
        $slnContent = Get-Content $slnFilePath
        foreach ($line in $slnContent) {
            if ($line -match 'Project\("(?<type>[^"]+)"\) = "(?<name>[^"]+)", "(?<path>[^"]+)", "(?<id>[^"]+)"') {
                $path = $matches['path']
                $id = $matches['id']
                if($path -like '*.csproj') {
                    $projectIds[$path] = $id
                }
            }
        }
    }
    return $projectIds
}

Push-Location $RepoRoot
try {
    $slnFile = Get-Item 'AzureMcp.sln' -ErrorAction SilentlyContinue
    $projectIds = @{}

    if ($slnFile) {
        $projectIds = Get-ProjectIds $slnFile
        Remove-Item $slnFile -Force
    }

    dotnet new sln -n 'AzureMcp'

    $projects = Get-ChildItem -Path "$RepoRoot" -Filter "*.csproj" -Recurse | Sort-Object -Property FullName

    if($Flat) {
        dotnet sln add --in-root $projects
    } else {
        foreach ($project in $projects) {
            dotnet sln add $project
        }
    }


    # Read the new project ids from the solution file
    $newProjectIds = Get-ProjectIds 'AzureMcp.sln'

    # Replace the new ids with the old ones
    $slnContent = Get-Content 'AzureMcp.sln' -Raw
    foreach ($path in $projectIds.Keys) {
        if ($newProjectIds.ContainsKey($path)) {
            $newId = $newProjectIds[$path]
            $originalId = $projectIds[$path]
            $slnContent = $slnContent.Replace($newId, $originalId)
        }
    }
    Set-Content 'AzureMcp.sln' -Value $slnContent
}
finally {
    Pop-Location
}
