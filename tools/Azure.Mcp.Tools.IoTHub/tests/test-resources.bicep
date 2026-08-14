targetScope = 'resourceGroup'

@minLength(3)
@maxLength(50)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

resource iotHub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: baseName
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {}
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
