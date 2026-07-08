#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Tears down the Azure Event Hubs resources created by New-EventHubsResources.ps1.

.DESCRIPTION
Deletes the resource group (default: contoso-rg) and everything in it, including
the Event Hubs namespace and its event hubs. Uses the Azure CLI (`az`).

This is the companion teardown for the vally "get Event Hub" evaluation. Even if
this script is never run (or fails), the `DeleteAfter` tag applied by
New-EventHubsResources.ps1 ensures the standard Azure resource-cleanup job
reclaims the group later.

.PARAMETER ResourceGroup
Resource group to delete. Default: contoso-rg.

.PARAMETER Subscription
Azure subscription (id or name) to target. Defaults to the current `az` context.

.PARAMETER Wait
Block until the deletion completes. By default the deletion is started with
--no-wait and returns immediately.

.EXAMPLE
./Remove-EventHubsResources.ps1

.EXAMPLE
./Remove-EventHubsResources.ps1 -Subscription <subscription-id> -Wait
#>

[CmdletBinding()]
param(
    [string] $ResourceGroup = 'contoso-rg',
    [string] $Subscription,
    [switch] $Wait
)

$ErrorActionPreference = 'Stop'

function Write-Info($Message) { Write-Host "[teardown] $Message" -ForegroundColor Cyan }
function Write-Warn($Message) { Write-Host "[teardown] $Message" -ForegroundColor Yellow }

# Verify the Azure CLI is available.
if (-not (Get-Command 'az' -ErrorAction SilentlyContinue)) {
    throw "The Azure CLI ('az') was not found on PATH. Install it: https://learn.microsoft.com/cli/azure/install-azure-cli"
}

$subArgs = $Subscription ? @('--subscription', $Subscription) : @()

# Nothing to do if the group is already gone.
$exists = az group exists --name $ResourceGroup @subArgs 2>$null
if ($exists -ne 'true') {
    Write-Info "Resource group '$ResourceGroup' does not exist. Nothing to delete."
    return
}

$waitArgs = $Wait ? @() : @('--no-wait')
Write-Info "Deleting resource group '$ResourceGroup'$([string]($Wait ? ' (waiting for completion)' : ' (started, --no-wait)')) ..."
az group delete `
    --name $ResourceGroup `
    --yes `
    @waitArgs `
    @subArgs | Out-Null
# `$ErrorActionPreference = 'Stop'` does not trap native-command failures, so check
# the exit code explicitly to surface a failed deletion.
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI failed while deleting resource group '$ResourceGroup' (exit code $LASTEXITCODE)."
}

Write-Info "Teardown initiated for resource group '$ResourceGroup'."
