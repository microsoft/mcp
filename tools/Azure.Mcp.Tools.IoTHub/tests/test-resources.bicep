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
  properties: {
    routing: {
      endpoints: {
        serviceBusQueues: [
          {
            name: 'sbqueue-endpoint'
            endpointUri: 'sb://${routingNamespace.name}.servicebus.windows.net'
            entityPath: routingQueue.name
            subscriptionId: subscription().subscriptionId
            resourceGroup: resourceGroup().name
            authenticationType: 'identityBased'
            identity: {
              userAssignedIdentity: routingIdentity.id
            }
          }
        ]
        serviceBusTopics: [
          {
            name: 'sbtopic-endpoint'
            endpointUri: 'sb://${routingNamespace.name}.servicebus.windows.net'
            entityPath: routingTopic.name
            subscriptionId: subscription().subscriptionId
            resourceGroup: resourceGroup().name
            authenticationType: 'identityBased'
            identity: {
              userAssignedIdentity: routingIdentity.id
            }
          }
        ]
      }
      routes: [
        for endpointName in ['sbqueue-endpoint', 'sbtopic-endpoint']: {
          name: endpointName
          source: 'DeviceMessages'
          condition: 'true'
          endpointNames: [
            endpointName
          ]
          isEnabled: true
        }
      ]
    }
  }
  dependsOn: [
    routingSender
  ]
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

// Data-plane IoT Hub Data Reader role: required to read the device registry over Microsoft Entra ID.
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#iot-hub-data-reader
resource iotHubDataReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'b447c946-2db7-41ec-983d-d8bf3b1c77e3'
}

resource iotHubDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(iotHubDataReaderRoleDefinition.id, testApplicationOid, iotHub.id)
  scope: iotHub
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: iotHubDataReaderRoleDefinition.id
  }
}

output IOTHUB_NAME string = iotHub.name
