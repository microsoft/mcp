// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base name for resources.')
param baseName string = resourceGroup().name

@description('The principal ID of the test application.')
param testApplicationOid string = deployer().objectId

@description('The location for the resources.')
param location string = resourceGroup().location

// NOTE: An Azure IoT Operations instance (Microsoft.IoTOperations/instances) is a projection of
// a deployment onto an Azure Arc-enabled Kubernetes cluster and cannot be provisioned directly
// via a standalone Bicep/ARM template. The list/get commands query Azure Resource Graph, so the
// only infrastructure required for the recorded tests is read access to the target subscription.
// This template assigns the Reader role to the test application at the resource group scope.

// Reader (acdd72a7-3385-48ef-bd42-f606fba81ae7)
resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testApplicationOid, 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
  }
}

output IOTOPERATIONS_RESOURCE_GROUP string = resourceGroup().name
output IOTOPERATIONS_LOCATION string = location
output IOTOPERATIONS_BASE_NAME string = baseName
