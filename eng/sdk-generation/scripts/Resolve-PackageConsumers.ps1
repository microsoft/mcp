#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $PackageId,
    [string] $GeneratedProject,
    [string] $RepositoryRoot,
    [string] $OutputPath,
    [switch] $ProductionOnly
)

$ErrorActionPreference = 'Stop'
if (-not $RepositoryRoot) { $RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path }
$RepositoryRoot = [IO.Path]::GetFullPath($RepositoryRoot)
$consumers = [System.Collections.Generic.List[string]]::new()
foreach ($project in Get-ChildItem $RepositoryRoot -Filter '*.csproj' -File -Recurse) {
    if ($project.FullName -match '[\\/](bin|obj|\.work)[\\/]') { continue }
    $relative = [IO.Path]::GetRelativePath($RepositoryRoot, $project.FullName).Replace('\', '/')
    if ($ProductionOnly -and $relative -match '(^|/)tests?(/|$)') { continue }
    try { [xml] $xml = Get-Content $project.FullName -Raw }
    catch { throw "Unable to parse project '$($project.FullName)': $($_.Exception.Message)" }
    $packageReferences = @($xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -ceq $PackageId })
    $generatedReferences = @()
    if ($GeneratedProject) {
        $generatedFileName = [IO.Path]::GetFileName($GeneratedProject)
        $generatedReferences = @($xml.Project.ItemGroup.ProjectReference | Where-Object {
            [IO.Path]::GetFileName(([string] $_.Include).Replace('\', '/')) -ceq $generatedFileName
        })
    }
    if ($packageReferences.Count -gt 0 -or $generatedReferences.Count -gt 0) {
        [void] $consumers.Add($relative)
    }
}
$result = @($consumers | Sort-Object -CaseSensitive)
if ($OutputPath) {
    ConvertTo-Json -InputObject @($result) | Set-Content $OutputPath
    Write-Host "Found $($result.Count) consumer(s) of $PackageId or its generated project." -ForegroundColor Green
}
else {
    $result
}
