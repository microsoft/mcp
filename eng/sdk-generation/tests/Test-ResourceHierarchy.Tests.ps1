#!/usr/bin/env pwsh
#Requires -Version 7

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$validator = Join-Path $repoRoot 'eng/sdk-generation/scripts/Test-ResourceHierarchy.ps1'
$temp = Join-Path $repoRoot '.work/resource-hierarchy-tests'
Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
New-Item $temp -ItemType Directory -Force | Out-Null

function New-Resource {
    param(
        [string] $Type = 'Microsoft.DocumentDB/databaseAccounts',
        [string] $Id = '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.DocumentDB/databaseAccounts/{accountName}',
        [string] $Parent = '',
        [string] $Scope = 'ResourceGroup',
        [string] $Singleton = ''
    )
    $resource = [ordered]@{
        resourceType = $Type
        resourceIdPattern = $Id
        methods = @()
        scope = @{ kind = $Scope }
        singletonResourceName = $Singleton
    }
    if ($Parent) { $resource.parentResourceId = $Parent }
    return $resource
}

function Write-CodeModel {
    param([string] $Path, [object[]] $Resources)
    $model = @{
        clients = @(@{
            decorators = @(@{
                name = 'Azure.ClientGenerator.Core.@armProviderSchema'
                arguments = @{ resources = $Resources; nonResourceMethods = @() }
            })
        })
    }
    $model | ConvertTo-Json -Depth 20 | Set-Content $Path
}

function Write-GeneratedResource {
    param([string] $Directory, [string] $Name, [string] $Type)
    New-Item $Directory -ItemType Directory -Force | Out-Null
    "public class $Name { public static readonly ResourceType ResourceType = `"$Type`"; }" | Set-Content (Join-Path $Directory "$Name.cs")
}

function Invoke-Fixture {
    param([string] $Name, [scriptblock] $Mutate, [bool] $ShouldPass = $false)
    $case = Join-Path $temp $Name
    $fullGenerated = Join-Path $case 'full-generated'
    $actualGenerated = Join-Path $case 'actual-generated'
    New-Item $case -ItemType Directory -Force | Out-Null
    $expected = @(New-Resource)
    $actual = @(New-Resource)
    Write-GeneratedResource $fullGenerated 'CosmosDBAccountResource' 'Microsoft.DocumentDB/databaseAccounts'
    Write-GeneratedResource $actualGenerated 'CosmosDBAccountResource' 'Microsoft.DocumentDB/databaseAccounts'
    & $Mutate ([ref] $actual) $actualGenerated
    Write-CodeModel (Join-Path $case 'full.json') $expected
    Write-CodeModel (Join-Path $case 'actual.json') $actual
    & pwsh -NoProfile -File $validator `
        -FullCodeModel (Join-Path $case 'full.json') `
        -FullGeneratedDirectory $fullGenerated `
        -ProjectedCodeModel (Join-Path $case 'actual.json') `
        -ProjectedGeneratedDirectory $actualGenerated `
        -SelectedResourceType 'Microsoft.DocumentDB/databaseAccounts' *> (Join-Path $case 'output.log')
    $passed = $LASTEXITCODE -eq 0
    if ($passed -ne $ShouldPass) {
        $output = Get-Content (Join-Path $case 'output.log') -Raw
        throw "Fixture '$Name' expected pass=$ShouldPass but pass=$passed.`n$output"
    }
    Write-Host "Fixture passed: $Name" -ForegroundColor Green
}

Invoke-Fixture 'unchanged' { param($actual, $generated) } $true
Invoke-Fixture 'resource-id-drift' { param($actual, $generated) $actual.Value[0].resourceIdPattern += '/unexpected' }
Invoke-Fixture 'additional-scope' { param($actual, $generated) $actual.Value[0].scope.kind = 'Subscription' }
Invoke-Fixture 'casing-only-name' {
    param($actual, $generated)
    Remove-Item (Join-Path $generated 'CosmosDBAccountResource.cs')
    Write-GeneratedResource $generated 'CosmosDbAccountResource' 'Microsoft.DocumentDB/databaseAccounts'
}
Invoke-Fixture 'additional-resource' {
    param($actual, $generated)
    $actual.Value += New-Resource -Type 'Microsoft.DocumentDB/unexpected' -Id '/subscriptions/{subscriptionId}/providers/Microsoft.DocumentDB/unexpected/{name}' -Scope 'Subscription'
    Write-GeneratedResource $generated 'UnexpectedResource' 'Microsoft.DocumentDB/unexpected'
}
Invoke-Fixture 'missing-resource' { param($actual, $generated) $actual.Value = @() }
Invoke-Fixture 'parent-drift' { param($actual, $generated) $actual.Value[0].parentResourceId = '/unexpected/parent' }
Invoke-Fixture 'singleton-drift' { param($actual, $generated) $actual.Value[0].singletonResourceName = 'default' }
Invoke-Fixture 'missing-generated-name' { param($actual, $generated) Remove-Item (Join-Path $generated 'CosmosDBAccountResource.cs') }

Write-Host 'All strict hierarchy validator fixtures passed.' -ForegroundColor Green
