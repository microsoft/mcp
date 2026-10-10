#!/usr/bin/env pwsh
#Requires -Version 7

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$resolver = Join-Path $repoRoot 'eng/sdk-generation/scripts/Resolve-MinimumHierarchyClosure.ps1'
$temp = Join-Path $repoRoot '.work/minimum-hierarchy-closure-tests'
Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
New-Item $temp -ItemType Directory -Force | Out-Null

function New-Method([string] $Id, [string] $Kind, [string] $Path) {
    return @{ methodId = $Id; kind = $Kind; operationPath = $Path }
}

$parentId = '/subscriptions/{subscriptionId}/providers/Microsoft.Test/parents/{parentName}'
$childId = "$parentId/children/{childName}"
$resources = @(
    @{
        resourceType = 'Microsoft.Test/parents'
        resourceIdPattern = $parentId
        scope = @{ kind = 'Subscription' }
        methods = @(
            (New-Method 'Microsoft.Test.Parents.get' 'Read' $parentId),
            (New-Method 'Microsoft.Test.Parents.delete' 'Delete' $parentId),
            (New-Method 'Microsoft.Test.Parents.list' 'List' '/subscriptions/{subscriptionId}/providers/Microsoft.Test/parents')
        )
    },
    @{
        resourceType = 'Microsoft.Test/parents/children'
        resourceIdPattern = $childId
        parentResourceId = $parentId
        scope = @{ kind = 'Subscription' }
        methods = @(
            (New-Method 'Microsoft.Test.Children.get' 'Read' $childId),
            (New-Method 'Microsoft.Test.Children.action' 'Action' "$childId/action"),
            (New-Method 'Microsoft.Test.Children.delete' 'Delete' $childId)
        )
    }
)
$codeModel = @{
    clients = @(@{
        decorators = @(@{
            name = 'Azure.ClientGenerator.Core.@armProviderSchema'
            arguments = @{
                resources = $resources
                nonResourceMethods = @((New-Method 'Microsoft.Test.checkName' 'Action' '/subscriptions/{subscriptionId}/checkName'))
            }
        })
    })
}
$codeModelPath = Join-Path $temp 'code-model.json'
$codeModel | ConvertTo-Json -Depth 20 | Set-Content $codeModelPath
$rootsPath = Join-Path $temp 'roots.json'
@{
    operations = @(
        @{ methodId = 'Microsoft.Test.Children.action' },
        @{ methodId = 'Microsoft.Test.checkName' }
    )
} | ConvertTo-Json -Depth 10 | Set-Content $rootsPath
$outputPath = Join-Path $temp 'closure.json'
& $resolver -CodeModel $codeModelPath -Roots $rootsPath -OutputPath $outputPath
$closure = Get-Content $outputPath -Raw | ConvertFrom-Json -Depth 20
$actual = @($closure.resources.operations.methodId) + @($closure.nonResourceOperations.methodId) | Sort-Object -CaseSensitive
$expected = @(
    'Microsoft.Test.Children.action',
    'Microsoft.Test.Children.get',
    'Microsoft.Test.Parents.get',
    'Microsoft.Test.checkName'
) | Sort-Object -CaseSensitive
if (($actual -join "`n") -cne ($expected -join "`n")) {
    throw "Unexpected closure. Expected [$($expected -join ', ')]; actual [$($actual -join ', ')]."
}
if ($closure.selectionPolicy -cne 'minimumHierarchyClosure') { throw 'Unexpected closure policy.' }
if ($closure.resources.Count -ne 2) { throw 'Parent resource was not retained.' }
if (@($closure.resources.operations | Where-Object { $_.reason -eq 'Required Read for selected resource hierarchy' }).Count -ne 2) {
    throw 'Expected child and parent Reads to be hierarchy-required.'
}

@{ operations = @(@{ methodId = 'Microsoft.Test.missing' }) } | ConvertTo-Json -Depth 10 | Set-Content $rootsPath
& pwsh -NoProfile -File $resolver -CodeModel $codeModelPath -Roots $rootsPath -OutputPath $outputPath *> (Join-Path $temp 'missing.log')
if ($LASTEXITCODE -eq 0) { throw 'Missing root fixture unexpectedly succeeded.' }

Write-Host 'Minimum hierarchy closure fixtures passed.' -ForegroundColor Green
