#!/bin/env pwsh
#Requires -Version 7
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
Invoke-WebRequest https://raw.githubusercontent.com/microsoft/go-infra/refs/heads/main/goinstallscript/powershell/go-install.ps1 -OutFile go-install.ps1
./go-install.ps1

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