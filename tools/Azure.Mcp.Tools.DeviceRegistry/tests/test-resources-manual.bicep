// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

@description('The base name for resources.')
param baseName string

@description('The location for the resources.')
param location string = resourceGroup().location

@description('The principal ID of the test application.')
param testApplicationOid string

// Device Registry Namespace
resource deviceRegistryNamespace 'Microsoft.DeviceRegistry/namespaces@2025-10-01' = {
  name: '${baseName}-ns'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

// Assign Reader role to the test application for the resource group
resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testApplicationOid, 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
  }
}

output DEVICEREGISTRY_NAMESPACE_NAME string = deviceRegistryNamespace.name
output DEVICEREGISTRY_NAMESPACE_ID string = deviceRegistryNamespace.id
