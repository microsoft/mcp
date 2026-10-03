#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $FullCodeModel,
    [Parameter(Mandatory)] [string] $FullGeneratedDirectory,
    [Parameter(Mandatory)] [string] $ProjectedCodeModel,
    [Parameter(Mandatory)] [string] $ProjectedGeneratedDirectory,
    [Parameter(Mandatory)] [string[]] $SelectedResourceType
)

$ErrorActionPreference = 'Stop'

function Get-SchemaResources {
    param([string] $Path)
    $model = Get-Content $Path -Raw | ConvertFrom-Json -Depth 100
    foreach ($client in $model.clients) {
        foreach ($decorator in @($client.decorators)) {
            if ($decorator.name -eq 'Azure.ClientGenerator.Core.@armProviderSchema') {
                return @($decorator.arguments.resources)
            }
        }
    }
    throw "ARM provider schema not found in $Path"
}

function Get-GeneratedNameMap {
    param([string] $Directory)
    if (-not (Test-Path $Directory)) { throw "Generated directory not found: $Directory" }
    $map = @{}
    foreach ($file in Get-ChildItem $Directory -Filter '*Resource.cs' -File) {
        $text = Get-Content $file.FullName -Raw
        $match = [regex]::Match($text, 'public\s+static\s+readonly\s+ResourceType\s+ResourceType\s*=\s*"([^"]+)"\s*;')
        if (-not $match.Success) { continue }
        $resourceType = $match.Groups[1].Value
        if ($map.ContainsKey($resourceType)) { throw "Duplicate generated name mapping for $resourceType" }
        $map[$resourceType] = $file.BaseName
    }
    return $map
}

function ConvertTo-StrictEntryMap {
    param([object[]] $Resources, [hashtable] $Names, [string[]] $Filter)
    $map = @{}
    foreach ($resource in $Resources) {
        $type = [string] $resource.resourceType
        if ($Filter.Count -gt 0 -and $Filter -cnotcontains $type) { continue }
        if (-not $Names.ContainsKey($type)) { throw "Generated name mapping unavailable for $type" }
        if ($map.ContainsKey($type)) { throw "Duplicate resource metadata for $type" }
        $parent = if ($resource.parentResourceId) { [string] $resource.parentResourceId } else { '' }
        $scope = if ($resource.scope) { [string] $resource.scope.kind } else { '' }
        $map[$type] = [pscustomobject]@{
            Name = [string] $Names[$type]
            ResourceType = $type
            ResourceId = [string] $resource.resourceIdPattern
            Parent = $parent
            Scope = $scope
            IsSingleton = -not [string]::IsNullOrEmpty([string] $resource.singletonResourceName)
        }
    }
    return $map
}

function Assert-OrdinalEqual {
    param([string] $Label, [string] $Expected, [string] $Actual)
    if (-not [string]::Equals($Expected, $Actual, [StringComparison]::Ordinal)) {
        throw "$Label differs. Expected '$Expected'; actual '$Actual'."
    }
}

$fullResources = Get-SchemaResources $FullCodeModel
$projectedResources = Get-SchemaResources $ProjectedCodeModel
$fullNames = Get-GeneratedNameMap $FullGeneratedDirectory
$projectedNames = Get-GeneratedNameMap $ProjectedGeneratedDirectory
$expected = ConvertTo-StrictEntryMap $fullResources $fullNames $SelectedResourceType
$actual = ConvertTo-StrictEntryMap $projectedResources $projectedNames @()

$expectedTypes = @($expected.Keys | Sort-Object -CaseSensitive)
$actualTypes = @($actual.Keys | Sort-Object -CaseSensitive)
if (($expectedTypes -join "`n") -cne ($actualTypes -join "`n")) {
    throw "Resource sets differ. Expected [$($expectedTypes -join ', ')]; actual [$($actualTypes -join ', ')]."
}

foreach ($type in $expectedTypes) {
    $left = $expected[$type]
    $right = $actual[$type]
    Assert-OrdinalEqual "$type generated name" $left.Name $right.Name
    Assert-OrdinalEqual "$type resource type" $left.ResourceType $right.ResourceType
    Assert-OrdinalEqual "$type resource ID" $left.ResourceId $right.ResourceId
    Assert-OrdinalEqual "$type parent" $left.Parent $right.Parent
    Assert-OrdinalEqual "$type scope" $left.Scope $right.Scope
    if ($left.IsSingleton -ne $right.IsSingleton) {
        throw "$type singleton differs. Expected $($left.IsSingleton); actual $($right.IsSingleton)."
    }
}

Write-Host "Strict hierarchy validation passed for $($expectedTypes.Count) resource(s)." -ForegroundColor Green
