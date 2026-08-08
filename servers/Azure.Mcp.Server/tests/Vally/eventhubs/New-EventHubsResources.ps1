#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Provisions the Azure Event Hubs resources used by the vally "get Event Hub"
evaluation (eventhub-get.eval.yaml and namespace-get.eval.yaml).

.DESCRIPTION
Creates - and is safe to re-run against - the resources that the eval prompts
reference:

  - a resource group (default: contoso-rg), tagged with `DeleteAfter` so the
    Azure clean-up tooling always removes it even if teardown is skipped,
  - an Event Hubs namespace (default: contoso-ehns), and
  - several event hubs (including "orders", which one prompt asks for by name),
    with a couple of consumer groups on the "orders" hub.

Uses the Azure CLI (`az`). Sign in first with `az login`.

The companion Remove-EventHubsResources.ps1 deletes the resource group. As a
belt-and-braces safety net, the `DeleteAfter` tag added here guarantees the group
is eventually reclaimed by the standard resource-cleanup job even if teardown
never runs.

.PARAMETER ResourceGroup
Resource group to create. Must match the resource group used in the eval prompts.
Default: contoso-rg.

.PARAMETER Namespace
Event Hubs namespace to create. Must match the namespace used in the eval prompts.
Default: contoso-ehns.

.PARAMETER Location
Azure region for the resource group and namespace. Default: eastus.

.PARAMETER Subscription
Azure subscription (id or name) to target. Defaults to the current `az` context.

.PARAMETER DeleteAfterHours
Number of hours from now to stamp into the `DeleteAfter` safety tag. Default: 4.

.PARAMETER EventHubs
Names of the event hubs to create. Must include "orders" for the eval to pass.
Default: orders, payments, shipments.

.EXAMPLE
./New-EventHubsResources.ps1

.EXAMPLE
./New-EventHubsResources.ps1 -Subscription <subscription-id> -Location westus2
#>

[CmdletBinding()]
param(
    [string] $ResourceGroup = 'contoso-rg',
    [string] $Namespace = 'contoso-ehns',
    [string] $Location = 'eastus',
    [string] $Subscription,
    [int] $DeleteAfterHours = 4,
    [string[]] $EventHubs = @('orders', 'payments', 'shipments')
)

$ErrorActionPreference = 'Stop'

function Write-Info($Message) { Write-Host "[provision] $Message" -ForegroundColor Cyan }

# Fail fast on Azure CLI errors. `$ErrorActionPreference = 'Stop'` does NOT trap
# non-zero exit codes from native commands like `az`, so every `az` call must be
# followed by this check or a failure (e.g. a policy denial) would be silently
# swallowed and the script would still exit 0.
function Assert-Az([string] $Action) {
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI failed while trying to $Action (exit code $LASTEXITCODE)."
    }
}

# Verify the Azure CLI is available.
if (-not (Get-Command 'az' -ErrorAction SilentlyContinue)) {
    throw "The Azure CLI ('az') was not found on PATH. Install it: https://learn.microsoft.com/cli/azure/install-azure-cli"
}

# Common args to pin every call to the requested subscription (if provided).
$subArgs = $Subscription ? @('--subscription', $Subscription) : @()

# Ensure a subscription context exists.
$account = az account show @subArgs 2>$null | ConvertFrom-Json
if (-not $account) {
    throw "No Azure CLI session found. Run 'az login' (and optionally pass -Subscription) first."
}
Write-Info "Using subscription: $($account.name) ($($account.id))"

# The DeleteAfter tag mirrors the repo's TestResources convention: an ISO 8601
# (round-trip 'o') UTC timestamp that the resource clean-up job honors.
$deleteAfter = [DateTime]::UtcNow.AddHours($DeleteAfterHours).ToString('o')

Write-Info "Creating resource group '$ResourceGroup' in '$Location' (DeleteAfter=$deleteAfter) ..."
az group create `
    --name $ResourceGroup `
    --location $Location `
    --tags "DeleteAfter=$deleteAfter" "Purpose=vally-eval" `
    @subArgs | Out-Null
Assert-Az "create resource group '$ResourceGroup'"

Write-Info "Creating Event Hubs namespace '$Namespace' (Standard) ..."
# --disable-local-auth true: SAS/local auth is blocked by the Safe Secrets Standard
# policy in many subscriptions. The Azure MCP tools authenticate with Entra ID
# (AzureCliCredential), so disabling local auth keeps the namespace both
# policy-compliant and fully usable by the eval.
az eventhubs namespace create `
    --resource-group $ResourceGroup `
    --name $Namespace `
    --location $Location `
    --sku Standard `
    --disable-local-auth true `
    --tags "DeleteAfter=$deleteAfter" "Purpose=vally-eval" `
    @subArgs | Out-Null
Assert-Az "create Event Hubs namespace '$Namespace'"

foreach ($hub in $EventHubs) {
    Write-Info "Creating event hub '$hub' ..."
    az eventhubs eventhub create `
        --resource-group $ResourceGroup `
        --namespace-name $Namespace `
        --name $hub `
        --partition-count 2 `
        --cleanup-policy Delete `
        --retention-time-in-hours 24 `
        @subArgs | Out-Null
    Assert-Az "create event hub '$hub'"
}

# Add a couple of consumer groups to the "orders" hub so the resource looks
# realistic (the eval asks for its details).
if ($EventHubs -contains 'orders') {
    foreach ($cg in @('billing', 'analytics')) {
        Write-Info "Creating consumer group '$cg' on event hub 'orders' ..."
        az eventhubs eventhub consumer-group create `
            --resource-group $ResourceGroup `
            --namespace-name $Namespace `
            --eventhub-name orders `
            --name $cg `
            @subArgs | Out-Null
        Assert-Az "create consumer group '$cg' on event hub 'orders'"
    }
}

Write-Info "Provisioning complete. Resource group '$ResourceGroup' will auto-delete after $deleteAfter if not removed sooner."
