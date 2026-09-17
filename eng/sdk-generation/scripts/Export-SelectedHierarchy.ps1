#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $CodeModel,
    [Parameter(Mandatory)] [string] $GeneratedDirectory,
    [Parameter(Mandatory)] [string] $Closure,
    [Parameter(Mandatory)] [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

function Get-ProviderResources {
    param([string] $Path)
    $model = Get-Content $Path -Raw | ConvertFrom-Json -Depth 100
    foreach ($client in $model.clients) {
        foreach ($decorator in @($client.decorators)) {
            if ($decorator.name -eq 'Azure.ClientGenerator.Core.@armProviderSchema') { return @($decorator.arguments.resources) }
        }
    }
    throw "ARM provider schema not found in $Path"
}

$resources = Get-ProviderResources $CodeModel
$closureManifest = Get-Content $Closure -Raw | ConvertFrom-Json -Depth 100
$selectedTypes = @($closureManifest.resources.resourceType)
$nameByType = @{}
foreach ($file in Get-ChildItem $GeneratedDirectory -Filter '*Resource.cs' -File) {
    $text = Get-Content $file.FullName -Raw
    $match = [regex]::Match($text, 'public\s+static\s+readonly\s+ResourceType\s+ResourceType\s*=\s*"([^"]+)"\s*;')
    if (-not $match.Success) { continue }
    $resourceType = $match.Groups[1].Value
    if ($selectedTypes -cnotcontains $resourceType) { continue }
    if ($nameByType.ContainsKey($resourceType)) { throw "Duplicate generated resource name mapping: $resourceType" }
    $nameByType[$resourceType] = $file.BaseName
}

$typeById = @{}
foreach ($resource in $resources) {
    if ($resource.resourceIdPattern) { $typeById[[string] $resource.resourceIdPattern] = [string] $resource.resourceType }
}

$result = [System.Collections.Generic.List[object]]::new()
foreach ($resource in $resources) {
    $resourceType = [string] $resource.resourceType
    if ($selectedTypes -cnotcontains $resourceType) { continue }
    if (-not $nameByType.ContainsKey($resourceType)) { throw "Generated resource name mapping is missing: $resourceType" }
    $parents = @()
    $parentId = [string] $resource.parentResourceId
    if ($parentId) {
        if (-not $typeById.ContainsKey($parentId)) { throw "Parent resource ID is not in the provider schema: $parentId" }
        $parents = @($typeById[$parentId])
    }
    [void] $result.Add([ordered]@{
        Name = [string] $nameByType[$resourceType]
        ResourceType = $resourceType
        ResourceId = [string] $resource.resourceIdPattern
        IsSingleton = -not [string]::IsNullOrEmpty([string] $resource.singletonResourceName)
        ParentResourceTypes = $parents
        Scopes = @([string] $resource.scope.kind)
    })
}
if ($result.Count -ne $selectedTypes.Count) { throw "Expected $($selectedTypes.Count) hierarchy entries, generated $($result.Count)." }
$result.ToArray() | ConvertTo-Json -Depth 10 | Set-Content $OutputPath
Write-Host "Exported expected hierarchy for $($result.Count) resource(s): $OutputPath" -ForegroundColor Green
