targetScope = 'resourceGroup'

@minLength(6)
@maxLength(50)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location
resource eventHubsNamespace 'Microsoft.EventHub/namespaces@2024-01-01' = {
  location: location
  name: baseName
  properties: {
    disableLocalAuth: true
  }
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
}

resource eventHubsNamespaceEmpty 'Microsoft.EventHub/namespaces@2024-01-01' = {
  location: location
  name: '${substring(baseName, 0, min(length(baseName), 49))}2'
  properties: {
    disableLocalAuth: true
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 1
  }
}

resource eventHub 'Microsoft.EventHub/namespaces/eventhubs@2024-01-01' = {
  parent: eventHubsNamespace
  name: 'test-hub'
  properties: {
    messageRetentionInDays: 1
  }
}

resource dataOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Azure Event Hubs Data Owner role.
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
  name: 'f526a384-b230-433a-b45c-95f59c4a2dec'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(dataOwnerRoleDefinition.id, testApplicationOid, eventHubsNamespace.id)
  scope: eventHubsNamespace
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: dataOwnerRoleDefinition.id
  }
}

resource roleAssignmentEmpty 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(dataOwnerRoleDefinition.id, testApplicationOid, eventHubsNamespaceEmpty.id)
  scope: eventHubsNamespaceEmpty
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: dataOwnerRoleDefinition.id
  }
}
// Outputs for test consumption
output eventHubsNamespaceName string = eventHubsNamespace.name
output eventHubsNamespaceEmptyName string = eventHubsNamespaceEmpty.name
output eventHubName string = eventHub.name
