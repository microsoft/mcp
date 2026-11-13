targetScope = 'resourceGroup'

@minLength(3)
@maxLength(50)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

resource routingIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-routing'
  location: location
}

resource routingNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: '${baseName}-routing'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
  }
}

resource routingQueue 'Microsoft.ServiceBus/namespaces/queues@2024-01-01' = {
  parent: routingNamespace
  name: 'telemetry-queue'
}

resource routingTopic 'Microsoft.ServiceBus/namespaces/topics@2024-01-01' = {
  parent: routingNamespace
  name: 'telemetry'
}

resource routingSender 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(routingNamespace.id, routingIdentity.id, 'sender')
  scope: routingNamespace
  properties: {
    principalId: routingIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
    )
  }
}

// Routing endpoints and routes are added by test-resources-post.ps1. IoT Hub validates identity-based endpoints
// when they are created, and ARM cannot wait for the routingSender assignment above to take effect.
resource iotHub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: baseName
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${routingIdentity.id}': {}
    }
  }
}

// Control-plane Reader role: required to resolve the IoT Hub hostname via the ARM hub GET.
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#reader
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
  }
}

// Data-plane IoT Hub Data Contributor role: required to read the device registry over Microsoft Entra ID and to
// create the test devices in test-resources-post.ps1 without shared access keys or the azure-iot CLI extension.
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#iot-hub-data-contributor
resource iotHubDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4fc6c259-987e-4a07-842e-c321cc9d413f'
}

resource iotHubDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(iotHubDataContributorRoleDefinition.id, testApplicationOid, iotHub.id)
  scope: iotHub
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: iotHubDataContributorRoleDefinition.id
  }
}

output IOTHUB_NAME string = iotHub.name
output IOTHUB_HOST_NAME string = iotHub.properties.hostName
output ROUTING_IDENTITY_ID string = routingIdentity.id
output ROUTING_ENDPOINT_URI string = 'sb://${routingNamespace.name}.servicebus.windows.net'
output ROUTING_QUEUE_NAME string = routingQueue.name
output ROUTING_TOPIC_NAME string = routingTopic.name
