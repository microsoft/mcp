targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base resource name. DPS names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Device Provisioning Service instance
resource dpsInstance 'Microsoft.Devices/provisioningServices@2022-12-12' = {
  name: baseName
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {
    allocationPolicy: 'Hashed'
  }
}

// Role assignment for test application - IoT Hub Data Contributor
resource dpsRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // IoT Hub Data Contributor role
  name: '4fc6c259-987e-4a07-842e-c321cc9d413f'
}

resource appDpsRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(dpsRoleDefinition.id, testApplicationOid, dpsInstance.id)
  scope: dpsInstance
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: dpsRoleDefinition.id
    description: 'IoT Hub Data Contributor for testApplicationOid'
  }
}

// Outputs for test consumption
output dpsInstanceName string = dpsInstance.name
output dpsIdScope string = dpsInstance.properties.idScope
output dpsServiceOperationsHostName string = dpsInstance.properties.serviceOperationsHostName
