#!/usr/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
Provisions the Azure Event Hubs resources used by the vally "get Event Hub"
evaluation (eventhub-get.eval.yaml and namespace-get.eval.yaml).

.DESCRIPTION
Delegates resource-group creation and test-resources.bicep deployment to the
repository's shared eng/common/TestResources/New-TestResources.ps1 script. The
operation is safe to re-run against the resources that the eval prompts reference:

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

Uses the same Azure PowerShell provisioning harness as the repository's live
tests. Sign in first with `Connect-AzAccount`.

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
Azure subscription (id or name) to target. Defaults to the current Az context.

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

$context = Get-AzContext
if (-not $context) {
    throw "No Azure PowerShell session found. Run 'Connect-AzAccount' first."
}

$subscriptionId = $context.Subscription.Id
if ($Subscription) {
    $matchingSubscriptions = @(Get-AzSubscription | Where-Object {
        $_.Id -eq $Subscription -or $_.Name -eq $Subscription
    })
    if ($matchingSubscriptions.Count -ne 1) {
        throw "Subscription '$Subscription' did not resolve to exactly one accessible Azure subscription."
    }
    $subscriptionId = $matchingSubscriptions[0].Id
}

$newTestResources = Join-Path $PSScriptRoot '../../../../eng/common/TestResources/New-TestResources.ps1' -Resolve
$deletableNamespace = 'contoso-ehns-delete'
$deletableEventHub = 'contoso-temp-hub'
$deletableConsumerGroup = 'contoso-temp-cg'
$templateParameters = @{
    namespaceName                 = $Namespace
    eventHubNames                 = $EventHubs
    deletableNamespaceName        = $deletableNamespace
    deletableEventHubName         = $deletableEventHub
    deletableConsumerGroupName    = $deletableConsumerGroup
}

Write-Info "Provisioning resource group '$ResourceGroup' and namespace '$Namespace' with the shared test-resource harness ..."
& $newTestResources `
    -TestResourcesDirectory $PSScriptRoot `
    -BaseName $Namespace `
    -ResourceGroupName $ResourceGroup `
    -SubscriptionId $subscriptionId `
    -Location $Location `
    -DeleteAfterHours $DeleteAfterHours `
    -ArmTemplateParameters $templateParameters `
    -OutFile:$false `
    -Force | Out-Host

Write-Info "Provisioning complete. Resource group '$ResourceGroup' (namespace '$Namespace') is protected by the shared harness's DeleteAfter tag."

# Report the actually-provisioned resource identifiers back to
# Invoke-VallyEval.ps1 as the LAST object on the output stream. It forwards
# each entry to vally as `--param KEY=value`, resolving the matching
# `${KEY}`/`${KEY=default}` placeholder each eval prompt references - so the
# evals always target what was truly provisioned here (this resource
# group/namespace could be renamed, or even randomized, by a caller without
# ever having to edit an eval spec). See the "Vally Parameter resolution"
# feature: https://microsoft.github.io/vally/reference/cli/eval/#parameter-resolution
[pscustomobject]@{
    RESOURCE_GROUP             = $ResourceGroup
    EVENTHUBS_NAMESPACE        = $Namespace
    EVENTHUBS_NAMESPACE_DELETE = $deletableNamespace
    EVENTHUB_ORDERS            = 'orders'
    EVENTHUB_PAYMENTS          = 'payments'
    EVENTHUB_TEMP              = $deletableEventHub
    CONSUMER_GROUP_BILLING     = 'billing'
    CONSUMER_GROUP_TEMP        = $deletableConsumerGroup
}
