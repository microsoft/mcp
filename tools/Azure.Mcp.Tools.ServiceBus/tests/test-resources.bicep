targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  location: location
  name: baseName
  properties: {
    disableLocalAuth: true
  }
  sku: {
    capacity: 1
    name: 'Standard'
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'queue1'
  properties: {
    deadLetteringOnMessageExpiration: true
  }
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'topic1'
  properties: {
    defaultMessageTimeToLive: 'PT1H'
  }
}

resource serviceBusSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2024-01-01' = {
  parent: serviceBusTopic
  name: 'subscription1'
  properties: {
    defaultMessageTimeToLive: 'PT1H'
    lockDuration: 'PT5M'
  }
}

resource dataOwnerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Service Bus Data Owner role.
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
  name: '090c5cfd-751d-490a-894a-3ce6f1109419'
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(dataOwnerRoleDefinition.id, testApplicationOid, serviceBusNamespace.id)
  scope: serviceBusNamespace
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: dataOwnerRoleDefinition.id
  }
}
