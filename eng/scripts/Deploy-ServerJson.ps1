#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Deploys the server.json file in the official MCP repository.
.DESCRIPTION
    Deploys the server.json in the official MCP repository following guidance in:
    https://eng.ms/docs/coreai/devdiv/one-engineering-system-1es/1es-gadecast/ospoost/ai-guidance-for-microsoft-developers/mcp/publishing-to-the-official-mcp-registry

    Supported publish targets are: 'public' and 'public_staging'.
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
.PARAMETER BuildOnly
    If specified, only builds the MCP publisher tool without deploying the server.json.
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
    [string] $KeyVaultKeyName,
    [switch] $BuildOnly
)

$ErrorActionPreference = "Stop" 
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$StagingRegistry = "-registry https://staging.registry.modelcontextprotocol.io"
$TemporaryDirectory = "$RepoRoot/.work/temp_deploy_server_json"

$PublishTargetInternal = "internal"
$PublishTargetStaging = "public_staging"
$PublishTargetProduction = "public"
$KnownPublishTargets = @($PublishTargetInternal, $PublishTargetStaging, $PublishTargetProduction)

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

if (!(Test-Path $TemporaryDirectory)) {
    New-Item -ItemType Directory -Path $TemporaryDirectory | Out-Null
}

Set-Location $TemporaryDirectory

# Install Microsoft Go
Write-Host "Installing Microsoft Go and building MCP publishing tool..."

# This go-install.ps1 script could be checked into your source repository
$goInstallScriptPath = "$PSScriptRoot/Install-Go.ps1"
if (!(Test-Path $goInstallScriptPath)) {
    LogError "Go install script not found at $goInstallScriptPath"
    exit 1
}

. $goInstallScriptPath

# Enable compliant crypto
$env:GOEXPERIMENT = "systemcrypto"

# Clone and build the publisher tool
git clone --branch "v1.3.7" https://github.com/modelcontextprotocol/registry

Set-Location registry

go build -o $TemporaryDirectory/mcp-publisher.exe ./cmd/publisher

# show help for the tool to ensure it's working
& $TemporaryDirectory/mcp-publisher.exe --help

if ($BuildOnly) {
    Write-Host "Build only flag specified. Exiting before deployment."
    exit 0
}

try {
    if ($publishTarget -eq $PublishTargetStaging) {
        Write-Host "$($serverInfo.name): Deploying server.json to staging instance: $StagingRegistry"
        & $TemporaryDirectory/mcp-publisher.exe login dns azure-key-vault -vault $KeyVaultName -key $KeyVaultKeyName -domain microsoft.com -registry https://staging.registry.modelcontextprotocol.io
        & $TemporaryDirectory/mcp-publisher.exe publish $serverJsonPath -registry https://staging.registry.modelcontextprotocol.io
    } elseif ($publishTarget -eq $PublishTargetProduction) {
        Write-Host "$($serverInfo.name): Deploying server.json to production instance."
        & $TemporaryDirectory/mcp-publisher.exe login dns azure-key-vault -vault $KeyVaultName -key $KeyVaultKeyName -domain microsoft.com
        & $TemporaryDirectory/mcp-publisher.exe publish $serverJsonPath
    } elseif ($publishTarget -eq $PublishTargetInternal) { 
        LogInfo "$($serverInfo.name): Internal publish target specified. Skipping deployment to public MCP registry."
    } else {
        LogError "$($serverInfo.name): Unsupported publish target: $publishTarget"
        exit 1
    }
}
finally {
    Pop-Location
}
