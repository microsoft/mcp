// Provisions the Azure Event Hubs resources used by the vally "get Event Hub"
// evaluation (eventhub-get.eval.yaml and namespace-get.eval.yaml).
//
// Deployed at subscription scope so a single `az deployment sub create` call
// creates - and is safe to re-run against - both the resource group and
// everything inside it:
//   - a resource group (default: contoso-rg), tagged with `DeleteAfter` so the
//     Azure clean-up tooling always removes it even if teardown is skipped,
//   - an Event Hubs namespace (default: contoso-ehns), and
//   - the requested event hubs (including "orders", which one prompt asks for
//     by name), with a couple of consumer groups on the "orders" hub.
//
// See New-EventHubsResources.ps1 for the wrapper that deploys this template,
// and Remove-EventHubsResources.ps1 for the (CLI-based) teardown.

targetScope = 'subscription'

@description('Resource group to create. Must match the resource group used in the eval prompts.')
param resourceGroupName string = 'contoso-rg'

@description('Azure region for the resource group and Event Hubs namespace.')
param location string = 'eastus'

@description('Event Hubs namespace to create. Must match the namespace used in the eval prompts.')
param namespaceName string = 'contoso-ehns'

@description('Names of the event hubs to create. Must include "orders" for the eval to pass.')
param eventHubNames array = [
  'orders'
  'payments'
  'shipments'
]

@description('Consumer groups to create on the "orders" event hub, if present in eventHubNames.')
param ordersConsumerGroups array = [
  'billing'
  'analytics'
]

@description('Name of a disposable event hub reserved for the eventhub-delete eval. Empty string skips it.')
param deletableEventHubName string = 'contoso-temp-hub'

@description('Name of a disposable consumer group on the "orders" hub reserved for the consumergroup-delete eval. Empty string skips it.')
param deletableConsumerGroupName string = 'contoso-temp-cg'

@description('Name of a second, disposable Event Hubs namespace reserved for the namespace-delete eval to delete. Empty string skips it.')
param deletableNamespaceName string = 'contoso-ehns-delete'

@description('ISO 8601 UTC timestamp stamped as a DeleteAfter safety tag, honored by the repo cleanup job.')
param deleteAfter string

var tags = {
  DeleteAfter: deleteAfter
  Purpose: 'vally-eval'
}

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// The namespace, event hubs, and consumer groups are resource-group-scoped
// resources; a module is required to deploy them into the resource group
// created above from this subscription-scoped template.
module eventHubsResources 'eventhubs-namespace.bicep' = {
  name: 'eventHubsNamespaceDeployment'
  scope: rg
  params: {
    namespaceName: namespaceName
    location: location
    tags: tags
    eventHubNames: eventHubNames
    ordersConsumerGroups: ordersConsumerGroups
    deletableEventHubName: deletableEventHubName
    deletableConsumerGroupName: deletableConsumerGroupName
  }
}

// A second, minimal Event Hubs namespace reserved for the namespace-delete
// eval so it has something disposable to delete without touching the primary
// `namespaceName` namespace, which the other evals depend on.
module deletableNamespaceResources 'eventhubs-namespace.bicep' = if (!empty(deletableNamespaceName)) {
  name: 'eventHubsDeletableNamespaceDeployment'
  scope: rg
  params: {
    namespaceName: deletableNamespaceName
    location: location
    tags: tags
    eventHubNames: []
    ordersConsumerGroups: []
  }
}

output resourceGroupName string = rg.name
output namespaceName string = eventHubsResources.outputs.namespaceName
output eventHubNames array = eventHubNames
output deletableNamespaceName string = deletableNamespaceName
output deletableEventHubName string = deletableEventHubName
output deletableConsumerGroupName string = deletableConsumerGroupName
