#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Provisions the Azure Event Hubs resources used by the vally "get Event Hub"
evaluation (eventhub-get.eval.yaml and namespace-get.eval.yaml).

.DESCRIPTION
Deploys eventhubs-resources.bicep, which creates - and is safe to re-run
against - the resources that the eval prompts reference:

  - a resource group (default: contoso-rg), tagged with `DeleteAfter` so the
    Azure clean-up tooling always removes it even if teardown is skipped,
  - an Event Hubs namespace (default: contoso-ehns), and
  - several event hubs (including "orders", which one prompt asks for by name),
    with a couple of consumer groups on the "orders" hub,
  - a disposable event hub ("contoso-temp-hub") and a disposable consumer
    group on "orders" ("contoso-temp-cg"), reserved for the eventhub-delete
    and consumergroup-delete evals so they have something to delete without
    disturbing the resources the other evals depend on, and
  - a second, minimal Event Hubs namespace ("contoso-ehns-delete"), reserved
    for the namespace-delete eval to delete.

Uses the Azure CLI (`az`) to run a subscription-scoped Bicep deployment, which
creates the resource group and everything inside it in a single deployment.
Sign in first with `az login`.

The companion Remove-EventHubsResources.ps1 deletes the resource group. As a
belt-and-braces safety net, the `DeleteAfter` tag added here guarantees the group
is eventually reclaimed by the standard resource-cleanup job even if teardown
never runs.

Reports the identifiers it actually provisioned back to Invoke-VallyEval.ps1 as
a PSCustomObject of `KEY = value` pairs - the LAST object written to the output
stream (Write-Info/Write-Host logging above does not interfere, since it never
touches that stream). The runner forwards each entry to vally as
`--param KEY=value`, resolving the matching `${KEY}`/`${KEY=default}`
placeholder each eval prompt references - see Vally's parameter resolution:
https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution.
This is what lets -ResourceGroup/-Namespace (or a future caller randomizing
either to avoid colliding with a concurrent run) always flow through to the
evals actually run, without ever having to hand-edit an eval spec.

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

.EXAMPLE
# Provision against a non-default resource group directly (e.g. to inspect it
# by hand). If Invoke-VallyEval.ps1 later runs with -SkipProvisioning against
# this same resource group, pass -ResourceGroup so the eval prompts match.
./New-EventHubsResources.ps1 -ResourceGroup my-rg -Namespace my-ns
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

$templateFile = Join-Path $PSScriptRoot 'eventhubs-resources.bicep'
$deploymentName = "eventhubs-vally-$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())"

# Bicep params are passed as a single JSON parameters file so the array-typed
# eventHubNames parameter survives Azure CLI's argument parsing unambiguously.
$parameters = @{
    '$schema'      = 'https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#'
    contentVersion = '1.0.0.0'
    parameters     = @{
        resourceGroupName = @{ value = $ResourceGroup }
        location          = @{ value = $Location }
        namespaceName     = @{ value = $Namespace }
        eventHubNames     = @{ value = $EventHubs }
        deleteAfter       = @{ value = $deleteAfter }
    }
} | ConvertTo-Json -Depth 5 -Compress

$parametersFile = New-TemporaryFile
try {
    Set-Content -Path $parametersFile -Value $parameters -NoNewline

    Write-Info "Deploying Event Hubs resources via Bicep (resource group '$ResourceGroup', namespace '$Namespace', DeleteAfter=$deleteAfter) ..."
    $deployment = az deployment sub create `
        --name $deploymentName `
        --location $Location `
        --template-file $templateFile `
        --parameters "@$parametersFile" `
        @subArgs | ConvertFrom-Json
    Assert-Az "deploy eventhubs-resources.bicep"
}
finally {
    Remove-Item -Path $parametersFile -Force -ErrorAction SilentlyContinue
}

$provisionedNamespace = $deployment.properties.outputs.namespaceName.value
$provisionedResourceGroup = $deployment.properties.outputs.resourceGroupName.value
$provisionedDeletableNamespace = $deployment.properties.outputs.deletableNamespaceName.value
$provisionedDeletableEventHub = $deployment.properties.outputs.deletableEventHubName.value
$provisionedDeletableConsumerGroup = $deployment.properties.outputs.deletableConsumerGroupName.value

Write-Info "Provisioning complete. Resource group '$provisionedResourceGroup' (namespace '$provisionedNamespace') will auto-delete after $deleteAfter if not removed sooner."

# Report the actually-provisioned resource identifiers back to
# Invoke-VallyEval.ps1 as the LAST object on the output stream. It forwards
# each entry to vally as `--param KEY=value`, resolving the matching
# `${KEY}`/`${KEY=default}` placeholder each eval prompt references - so the
# evals always target what was truly provisioned here (this resource
# group/namespace could be renamed, or even randomized, by a caller without
# ever having to edit an eval spec). See the "Vally Parameter resolution"
# feature: https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution
[pscustomobject]@{
    RESOURCE_GROUP             = $provisionedResourceGroup
    EVENTHUBS_NAMESPACE        = $provisionedNamespace
    EVENTHUBS_NAMESPACE_DELETE = $provisionedDeletableNamespace
    EVENTHUB_ORDERS            = 'orders'
    EVENTHUB_PAYMENTS          = 'payments'
    EVENTHUB_TEMP              = $provisionedDeletableEventHub
    CONSUMER_GROUP_BILLING     = 'billing'
    CONSUMER_GROUP_TEMP        = $provisionedDeletableConsumerGroup
}
