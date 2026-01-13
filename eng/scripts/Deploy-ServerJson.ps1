#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Deploys the server.json file in the official MCP repository.
.DESCRIPTION
    Deploys the server.json in the official MCP repository following guidance in:
    https://eng.ms/docs/coreai/devdiv/one-engineering-system-1es/1es-gadecast/ospoost/ai-guidance-for-microsoft-developers/mcp/publishing-to-the-official-mcp-registry

    Supported publish targets are: 'public' and 'public_staging'.
    
    This script uses native PowerShell to authenticate with Azure Key Vault and publish to the MCP registry.
.PARAMETER ServerName
    Name of the MCP server under "./servers/" folder whose server.json will be deployed.
.PARAMETER ServerJsonPath
    Path to the server.json file to be deployed.
.PARAMETER BuildInfoPath
    Path to the build_info.json file containing build metadata. If not provided, defaults to ".work/build_info.json" in the repo root.
.PARAMETER KeyVaultName
    Name of the Azure Key Vault containing the credentials for MCP registry login.
.PARAMETER KeyVaultKeyName
    Name of the Key Vault key to use for MCP registry login.
.EXAMPLE
    Deploy-ServerJson.ps1 -ServerName "Azure.Mcp.Server" -ServerJsonPath "./.work/Azure.Mcp.Server/server.json" -BuildInfoPath ".work/build_info.json" -KeyVaultName "my-key-vault" -KeyVaultKeyName "mcp-registry-key"
    Updates the server.json for the Azure.Mcp.Server to the production MCP registry using credentials from the specified Key Vault.
#>
[CmdletBinding(DefaultParameterSetName='default')]
param(
    [Parameter(Mandatory=$true)]
    [string] $ServerJsonPath,
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [string] $BuildInfoPath,
    [string] $KeyVaultName,
    [string] $KeyVaultKeyName
)

$ErrorActionPreference = "Stop" 
. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/Publish-McpRegistry.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$PublishTargetInternal = "internal"
$PublishTargetStaging = "public_staging"
$PublishTargetProduction = "public"
$KnownPublishTargets = @($PublishTargetInternal, $PublishTargetStaging, $PublishTargetProduction)

$ProductionRegistryUrl = "https://registry.modelcontextprotocol.io"
$StagingRegistryUrl = "https://staging.registry.modelcontextprotocol.io"
$Domain = "microsoft.com"

if (!(Test-Path $ServerJsonPath)) {
    LogError "Server JSON file $ServerJsonPath does not exist. Run eng/scripts/New-ServerJson.ps1 to create it."
    exit 1
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    exit 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable
$publishTarget = $buildInfo.publishTarget
$matchingServer = @($buildInfo.servers | Where-Object { $_.name -eq $ServerName })

if ($matchingServer.Count -eq 0) {
    LogError "No server found with name '$ServerName' in build info."
    exit 1
} elseif ($matchingServer.Count -gt 1) {
    LogError "Multiple servers found with name '$ServerName' in build info."
    exit 1
}

$serverInfo = $matchingServer[0]

if (-not $KnownPublishTargets.Contains($publishTarget)) {
    LogInfo "Unknown publish target '$publishTarget'. Known targets are: $($KnownPublishTargets -join ', ')"
    exit 0
}

Write-Host "Preparing to deploy '$ServerJsonPath' with type '$publishTarget'."

# Ensure Az.Accounts module is available for Azure authentication
if (-not (Get-Module -ListAvailable -Name Az.Accounts)) {
    LogError "Az.Accounts module is not installed. Please install it using: Install-Module -Name Az.Accounts"
    exit 1
}

Import-Module Az.Accounts -ErrorAction Stop

try {
    if ($publishTarget -eq $PublishTargetStaging) {
        Write-Host "$($serverInfo.name): Deploying server.json to staging instance: $StagingRegistryUrl"
        Publish-ToMcpRegistry `
            -ServerJsonPath $ServerJsonPath `
            -RegistryUrl $StagingRegistryUrl `
            -Domain $Domain `
            -KeyVaultName $KeyVaultName `
            -KeyVaultKeyName $KeyVaultKeyName
    } elseif ($publishTarget -eq $PublishTargetProduction) {
        Write-Host "$($serverInfo.name): Deploying server.json to production instance: $ProductionRegistryUrl"
        Publish-ToMcpRegistry `
            -ServerJsonPath $ServerJsonPath `
            -RegistryUrl $ProductionRegistryUrl `
            -Domain $Domain `
            -KeyVaultName $KeyVaultName `
            -KeyVaultKeyName $KeyVaultKeyName
    } elseif ($publishTarget -eq $PublishTargetInternal) { 
        LogInfo "$($serverInfo.name): Internal publish target specified. Skipping deployment to public MCP registry."
    } else {
        LogError "$($serverInfo.name): Unsupported publish target: $publishTarget"
        exit 1
    }
}
catch {
    LogError "Failed to publish to MCP registry: $_"
    throw
}
