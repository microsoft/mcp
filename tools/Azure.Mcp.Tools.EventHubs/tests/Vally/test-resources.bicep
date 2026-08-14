// Provisions the Azure Event Hubs resources used by the vally evaluations.
// New-EventHubsResources.ps1 delegates resource-group creation and deployment
// to the repository's shared New-TestResources.ps1 harness.

targetScope = 'resourceGroup'

@description('Azure region for the Event Hubs namespaces.')
param location string = resourceGroup().location

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

var tags = union(resourceGroup().tags, {
  Purpose: 'vally-eval'
})
var hasOrders = contains(eventHubNames, 'orders')
var otherEventHubNames = filter(eventHubNames, name => name != 'orders')

resource eventHubsNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    disableLocalAuth: true
  }
}

resource ordersHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = if (hasOrders) {
  parent: eventHubsNamespace
  name: 'orders'
  properties: {
    partitionCount: 2
    messageRetentionInDays: 1
  }
}

resource otherHubs 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = [for hubName in otherEventHubNames: {
  parent: eventHubsNamespace
  name: hubName
  properties: {
    partitionCount: 2
    messageRetentionInDays: 1
  }
}]

resource ordersConsumerGroupResources 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = [for consumerGroup in ordersConsumerGroups: if (hasOrders) {
  parent: ordersHub
  name: consumerGroup
}]

resource deletableEventHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = if (!empty(deletableEventHubName)) {
  parent: eventHubsNamespace
  name: deletableEventHubName
  properties: {
    partitionCount: 2
    messageRetentionInDays: 1
  }
}

resource deletableConsumerGroup 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = if (hasOrders && !empty(deletableConsumerGroupName)) {
  parent: ordersHub
  name: deletableConsumerGroupName
}

resource deletableNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = if (!empty(deletableNamespaceName)) {
  name: deletableNamespaceName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    disableLocalAuth: true
  }
}

output resourceGroupName string = resourceGroup().name
output namespaceName string = eventHubsNamespace.name
output eventHubNames array = eventHubNames
output deletableNamespaceName string = deletableNamespaceName
output deletableEventHubName string = deletableEventHubName
output deletableConsumerGroupName string = deletableConsumerGroupName