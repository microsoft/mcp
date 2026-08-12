// Module deployed at resource-group scope by eventhubs-resources.bicep.
// Creates the Event Hubs namespace, event hubs, and the "orders" hub's
// consumer groups.

@description('Event Hubs namespace to create.')
param namespaceName string

@description('Azure region for the namespace.')
param location string

@description('Tags to apply to all resources, including the DeleteAfter safety tag.')
param tags object

@description('Names of the event hubs to create.')
param eventHubNames array

@description('Consumer groups to create on the "orders" event hub, if present in eventHubNames.')
param ordersConsumerGroups array

@description('Name of a disposable event hub reserved for the eventhub-delete eval to delete. Empty string skips it.')
param deletableEventHubName string = ''

@description('Name of a disposable consumer group on the "orders" hub reserved for the consumergroup-delete eval to delete. Empty string skips it.')
param deletableConsumerGroupName string = ''

var hasOrders = contains(eventHubNames, 'orders')
var otherEventHubNames = filter(eventHubNames, name => name != 'orders')

// disableLocalAuth: SAS/local auth is blocked by the Safe Secrets Standard
// policy in many subscriptions. The Azure MCP tools authenticate with Entra ID
// (AzureCliCredential), so disabling local auth keeps the namespace both
// policy-compliant and fully usable by the eval.
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

// A couple of consumer groups on the "orders" hub so the resource looks
// realistic (the eval asks for its details).
resource ordersConsumerGroupResources 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = [for cg in ordersConsumerGroups: if (hasOrders) {
  parent: ordersHub
  name: cg
}]

// Reserved for the eventhub-delete eval so it has something disposable to
// delete without disturbing "orders"/"payments"/"shipments", which the
// get/update evals depend on.
resource deletableEventHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = if (!empty(deletableEventHubName)) {
  parent: eventHubsNamespace
  name: deletableEventHubName
  properties: {
    partitionCount: 2
    messageRetentionInDays: 1
  }
}

// Reserved for the consumergroup-delete eval so it has something disposable
// to delete without disturbing "billing"/"analytics", which the
// consumergroup-get/update evals depend on.
resource deletableConsumerGroup 'Microsoft.EventHub/namespaces/eventhubs/consumergroups@2024-01-01' = if (hasOrders && !empty(deletableConsumerGroupName)) {
  parent: ordersHub
  name: deletableConsumerGroupName
}

output namespaceName string = eventHubsNamespace.name
