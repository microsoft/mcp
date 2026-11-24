targetScope = 'resourceGroup'

@minLength(3)
@maxLength(64)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

resource provisioningService 'Microsoft.Devices/provisioningServices@2022-12-12' = {
  name: baseName
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {}
}

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

resource dataReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '10745317-c249-44a1-a5ce-3a4353c0bbd8'
}

resource dataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(dataReaderRoleDefinition.id, testApplicationOid, provisioningService.id)
  scope: provisioningService
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: dataReaderRoleDefinition.id
  }
}

output DEVICE_PROVISIONING_SERVICE_NAME string = provisioningService.name
