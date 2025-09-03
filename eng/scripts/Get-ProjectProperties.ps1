#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, ParameterSetName='ByProjectName')]
    [string] $ProjectName,
    [Parameter(Mandatory=$true, ParameterSetName='ByPath')]
    [string] $Path
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if ($ProjectName) {
    $projectFiles = Get-ChildItem $RepoRoot -Filter "$ProjectName" -Recurse

    if ($projectFiles.Count -eq 0) {
        Write-Error "No project files found matching '$ProjectName'."
        exit 1
    }

    if ($projectFiles.Count -gt 1) {
        Write-Error "Multiple project files found matching '$ProjectName'."
        exit 1
    }
} elseif ($Path) {
    $projectFiles = @(Get-Item $Path -ErrorAction SilentlyContinue)
    if (-not $projectFiles) {
        Write-Error "No project file found at path '$Path'."
        exit 1
    }
}

$project = [xml](Get-Content $projectFiles[0])
$propertyGroups = $project.Project.PropertyGroup
$properties = [ordered]@{}

foreach($propertyGroup in $propertyGroups) {
    $nodes = $propertyGroup.ChildNodes
    foreach($node in $nodes) {
        $properties[$node.Name] = $node.InnerText
    }
}

[pscustomobject]$properties
