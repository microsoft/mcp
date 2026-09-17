#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $CodeModel,
    [Parameter(Mandatory)] [string] $Roots,
    [Parameter(Mandatory)] [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

function Get-ProviderSchema {
    param([string] $Path)
    $model = Get-Content $Path -Raw | ConvertFrom-Json -Depth 100
    foreach ($client in $model.clients) {
        foreach ($decorator in @($client.decorators)) {
            if ($decorator.name -eq 'Azure.ClientGenerator.Core.@armProviderSchema') {
                return $decorator.arguments
            }
        }
    }
    throw "ARM provider schema not found in $Path"
}

$schema = Get-ProviderSchema $CodeModel
$rootManifest = Get-Content $Roots -Raw | ConvertFrom-Json -Depth 20
$rootIds = @($rootManifest.operations.methodId)
if ($rootIds.Count -eq 0) { throw "No operation roots were provided in $Roots" }
if (@($rootIds | Sort-Object -Unique).Count -ne $rootIds.Count) { throw "Duplicate operation roots found in $Roots" }

$resourcesById = @{}
$resourceMethodOwners = @{}
function Get-ResourceKey([object] $Resource) {
    return @($Resource.methods.methodId | Sort-Object -CaseSensitive) -join "`n"
}
foreach ($resource in @($schema.resources)) {
    $resourceId = [string] $resource.resourceIdPattern
    if ($resourceId) {
        if (-not $resourcesById.ContainsKey($resourceId)) {
            $resourcesById[$resourceId] = [System.Collections.Generic.List[object]]::new()
        }
        [void] $resourcesById[$resourceId].Add($resource)
    }
    foreach ($method in @($resource.methods)) {
        $methodId = [string] $method.methodId
        if ($resourceMethodOwners.ContainsKey($methodId)) { throw "Operation has multiple resource owners: $methodId" }
        $resourceMethodOwners[$methodId] = $resource
    }
}

$nonResourceMethods = @{}
foreach ($method in @($schema.nonResourceMethods)) {
    $methodId = [string] $method.methodId
    if ($nonResourceMethods.ContainsKey($methodId) -or $resourceMethodOwners.ContainsKey($methodId)) {
        throw "Duplicate operation ID in provider schema: $methodId"
    }
    $nonResourceMethods[$methodId] = $method
}

$selectedResources = @{}
$selectedMethods = @{}
$methodReasons = @{}
$resourceReasons = @{}
$queue = [System.Collections.Generic.Queue[object]]::new()
$selectedNonResource = [System.Collections.Generic.List[object]]::new()

foreach ($rootId in $rootIds) {
    $id = [string] $rootId
    if ($resourceMethodOwners.ContainsKey($id)) {
        $resource = $resourceMethodOwners[$id]
        $selectedMethods[$id] = @($resource.methods | Where-Object { $_.methodId -ceq $id })[0]
        $methodReasons[$id] = 'Direct MCP usage'
        $queue.Enqueue($resource)
    }
    elseif ($nonResourceMethods.ContainsKey($id)) {
        $method = $nonResourceMethods[$id]
        [void] $selectedNonResource.Add([ordered]@{
            methodId = $id
            kind = [string] $method.kind
            operationPath = [string] $method.operationPath
            reason = 'Direct MCP usage'
        })
    }
    else {
        throw "Root operation not found in ARM provider schema: $id"
    }
}

while ($queue.Count -gt 0) {
    $resource = $queue.Dequeue()
    $resourceType = [string] $resource.resourceType
    $resourceKey = Get-ResourceKey $resource
    if ($selectedResources.ContainsKey($resourceKey)) { continue }

    $selectedResources[$resourceKey] = $resource
    $hasDirectRoot = @($resource.methods | Where-Object { $rootIds -ccontains $_.methodId }).Count -gt 0
    $resourceReasons[$resourceKey] = if ($hasDirectRoot) { 'Owns a direct MCP root operation' } else { 'Required in-service ancestor' }

    $readMethods = @($resource.methods | Where-Object { $_.kind -ceq 'Read' })
    if ($readMethods.Count -ne 1) {
        throw "Selected resource '$resourceType' must have exactly one Read operation; found $($readMethods.Count)."
    }
    $read = $readMethods[0]
    $readId = [string] $read.methodId
    if (-not $selectedMethods.ContainsKey($readId)) {
        $selectedMethods[$readId] = $read
        $methodReasons[$readId] = 'Required Read for selected resource hierarchy'
    }

    $parentId = [string] $resource.parentResourceId
    if ($parentId) {
        if (-not $resourcesById.ContainsKey($parentId)) {
            throw "In-service parent resource not found for '$resourceType': $parentId"
        }
        $parents = @($resourcesById[$parentId])
        if ($parents.Count -ne 1) {
            throw "In-service parent resource is ambiguous for '$resourceType': $parentId"
        }
        $queue.Enqueue($parents[0])
    }
}

$expandedResources = [System.Collections.Generic.List[object]]::new()
foreach ($resource in @($schema.resources)) {
    $resourceType = [string] $resource.resourceType
    $resourceKey = Get-ResourceKey $resource
    if (-not $selectedResources.ContainsKey($resourceKey)) { continue }

    $operations = [System.Collections.Generic.List[object]]::new()
    foreach ($method in @($resource.methods)) {
        $methodId = [string] $method.methodId
        if (-not $selectedMethods.ContainsKey($methodId)) { continue }
        [void] $operations.Add([ordered]@{
            methodId = $methodId
            kind = [string] $method.kind
            operationPath = [string] $method.operationPath
            reason = [string] $methodReasons[$methodId]
        })
    }
    [void] $expandedResources.Add([ordered]@{
        resourceType = $resourceType
        reason = [string] $resourceReasons[$resourceKey]
        operations = $operations.ToArray()
    })
}

$result = [ordered]@{
    selectionPolicy = 'minimumHierarchyClosure'
    directRootCount = $rootIds.Count
    resources = $expandedResources.ToArray()
    nonResourceOperations = $selectedNonResource.ToArray()
}
$result | ConvertTo-Json -Depth 20 | Set-Content $OutputPath

$operationCount = @($expandedResources | ForEach-Object { $_.operations }).Count + $selectedNonResource.Count
Write-Host "Minimum hierarchy closure: $($expandedResources.Count) resource(s), $operationCount operation(s)." -ForegroundColor Green
