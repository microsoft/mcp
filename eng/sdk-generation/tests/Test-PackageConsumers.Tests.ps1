#!/usr/bin/env pwsh
#Requires -Version 7

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$resolver = Join-Path $repoRoot 'eng/sdk-generation/scripts/Resolve-PackageConsumers.ps1'
$temp = Join-Path ([IO.Path]::GetTempPath()) 'azure-mcp-package-consumer-tests'
Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
New-Item (Join-Path $temp 'tools/PackageConsumer') -ItemType Directory -Force | Out-Null
New-Item (Join-Path $temp 'tools/GeneratedConsumer') -ItemType Directory -Force | Out-Null
New-Item (Join-Path $temp 'tests/TestConsumer') -ItemType Directory -Force | Out-Null
New-Item (Join-Path $temp 'eng/generated/Azure.ResourceManager.Test') -ItemType Directory -Force | Out-Null

@'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup><PackageReference Include="Azure.ResourceManager.Test" /></ItemGroup>
</Project>
'@ | Set-Content (Join-Path $temp 'tools/PackageConsumer/PackageConsumer.csproj')
@'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup><ProjectReference Include="$(RepoRoot)eng\generated\Azure.ResourceManager.Test\Azure.ResourceManager.Test.csproj" /></ItemGroup>
</Project>
'@ | Set-Content (Join-Path $temp 'tools/GeneratedConsumer/GeneratedConsumer.csproj')
@'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup><PackageReference Include="Azure.ResourceManager.Test" /></ItemGroup>
</Project>
'@ | Set-Content (Join-Path $temp 'tests/TestConsumer/TestConsumer.csproj')
'<Project Sdk="Microsoft.NET.Sdk" />' | Set-Content (Join-Path $temp 'eng/generated/Azure.ResourceManager.Test/Azure.ResourceManager.Test.csproj')

$output = Join-Path $temp 'consumers.json'
& $resolver `
    -PackageId Azure.ResourceManager.Test `
    -GeneratedProject (Join-Path $temp 'eng/generated/Azure.ResourceManager.Test/Azure.ResourceManager.Test.csproj') `
    -RepositoryRoot $temp `
    -OutputPath $output `
    -ProductionOnly
$actual = @(Get-Content $output -Raw | ConvertFrom-Json) | Sort-Object -CaseSensitive
$expected = @(
    'tools/GeneratedConsumer/GeneratedConsumer.csproj',
    'tools/PackageConsumer/PackageConsumer.csproj'
) | Sort-Object -CaseSensitive
if (($actual -join "`n") -cne ($expected -join "`n")) {
    throw "Unexpected package consumers. Expected [$($expected -join ', ')]; actual [$($actual -join ', ')]."
}
Write-Host 'Package consumer fixtures passed.' -ForegroundColor Green
