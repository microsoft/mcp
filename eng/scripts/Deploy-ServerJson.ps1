#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Updates the server.json file in the official MCP repository.
.DESCRIPTION
    Updates the server.json in the official MCP repository following guidance in:
    https://eng.ms/docs/coreai/devdiv/one-engineering-system-1es/1es-gadecast/ospoost/ai-guidance-for-microsoft-developers/mcp/publishing-to-the-official-mcp-registry
.PARAMETER ServerName
    Name of the MCP server under "./servers/" folder whose server.json will be deployed.
.PARAMETER BuildInfoPath
    Path to the build_info.json file containing build metadata. If not provided, defaults to ".work/build_info.json" in the repo root.
.PARAMETER TemporaryDirectory
    Path to a temporary directory for building and deploying the server.json.
.PARAMETER ReleaseType
    Type of release to deploy: 'staging' or 'production'. If 'staging' is specified, the server.json will be deployed to the staging MCP registry.
.PARAMETER KeyVaultName
    Name of the Azure Key Vault containing the credentials for MCP registry login.
.PARAMETER KeyVaultKeyName
    Name of the Key Vault key to use for MCP registry login.
.PARAMETER BuildOnly
    If specified, the script will only build the publisher tool and not deploy the server.json.
.EXAMPLE
    Deploy-ServerJson.ps1 -ServerName "Azure.Mcp.Server" -BuildInfoPath ".work/build_info.json" -TemporaryDirectory "C:\Temp\" -ReleaseType "production" -KeyVaultName "my-key-vault" -KeyVaultKeyName "mcp-registry-key"
    Updates the server.json for the Azure.Mcp.Server to the production MCP registry using credentials from the specified Key Vault.
#>
[CmdletBinding(DefaultParameterSetName='default')]
param(
    [Parameter(Mandatory=$true)]
    [string] $ServerName,
    [Parameter(Mandatory=$true)]
    [string] $BuildInfoPath,
    [Parameter(Mandatory=$true)]
    [string] $TemporaryDirectory,

    [Parameter()]
    [ValidateSet('staging','production')]
    [string] $ReleaseType,

    [string] $KeyVaultName,
    [string] $KeyVaultKeyName,
    [switch] $BuildOnly
)

$ErrorActionPreference = "Stop" 
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    exit 1
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

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

# log in using Key Vault
$StagingRegistry = "-registry https://staging.registry.modelcontextprotocol.io"

$loginArguments = "login dns azure-key-vault -vault $KeyVaultName -key $KeyVaultKeyName -domain microsoft.com";
$publishArguments = "publish $($buildInfo.serverJsonPath)"

if ($ReleaseType -eq 'staging') {
    Write-Host "Deploying server.json to staging instance: $StagingRegistry"

    $loginArguments += " $StagingRegistry"
    $publishArguments += " $StagingRegistry"
} else {
    Write-Host "Deploying server.json to production instance."

& $TemporaryDirectory/mcp-publisher.exe $loginArguments

# publish the server.json
& $TemporaryDirectory/mcp-publisher.exe $publishArguments