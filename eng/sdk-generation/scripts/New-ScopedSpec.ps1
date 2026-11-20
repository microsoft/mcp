#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $SourceDirectory,
    [Parameter(Mandatory)] [string] $CodeModel,
    [Parameter(Mandatory)] [string] $Closure,
    [Parameter(Mandatory)] [string] $OutputDirectory
)

$ErrorActionPreference = 'Stop'

function Get-ProviderSchema {
    param([string] $Path)
    $model = Get-Content $Path -Raw | ConvertFrom-Json -Depth 100
    foreach ($client in $model.clients) {
        foreach ($decorator in @($client.decorators)) {
            if ($decorator.name -eq 'Azure.ClientGenerator.Core.@armProviderSchema') { return $decorator.arguments }
        }
    }
    throw "ARM provider schema not found in $Path"
}

$schema = Get-ProviderSchema $CodeModel
$closureManifest = Get-Content $Closure -Raw | ConvertFrom-Json -Depth 100
$retained = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($operation in @($closureManifest.resources.operations) + @($closureManifest.nonResourceOperations)) {
    [void] $retained.Add([string] $operation.methodId)
}
if ($retained.Count -eq 0) { throw "Closure contains no operations: $Closure" }

$all = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($resource in @($schema.resources)) {
    foreach ($method in @($resource.methods)) { [void] $all.Add([string] $method.methodId) }
}
foreach ($method in @($schema.nonResourceMethods)) { [void] $all.Add([string] $method.methodId) }
foreach ($methodId in $retained) {
    if (-not $all.Contains($methodId)) { throw "Retained operation is missing from the full code model: $methodId" }
}
$excluded = @($all | Where-Object { -not $retained.Contains($_) } | Sort-Object -CaseSensitive)

Remove-Item $OutputDirectory -Recurse -Force -ErrorAction SilentlyContinue
New-Item $OutputDirectory -ItemType Directory -Force | Out-Null
Copy-Item (Join-Path $SourceDirectory '*') $OutputDirectory -Recurse -Force
$clientTsp = Join-Path $OutputDirectory 'client.tsp'
if (-not (Test-Path $clientTsp)) { throw "client.tsp not found under $OutputDirectory" }

Add-Content $clientTsp "`n`n// BEGIN AZURE MCP GENERATED OPERATION SCOPES"
foreach ($methodId in $excluded) {
    Add-Content $clientTsp ('@@scope({0}, "!csharp");' -f $methodId)
}
Add-Content $clientTsp '// END AZURE MCP GENERATED OPERATION SCOPES'

Write-Host "Scoped specification: $($retained.Count) retained, $($excluded.Count) excluded operation(s)." -ForegroundColor Green
