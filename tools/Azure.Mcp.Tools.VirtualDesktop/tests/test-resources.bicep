// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

@description('The location where resources will be deployed')
param location string = resourceGroup().location

@description('The base name for resource naming')
param baseName string = resourceGroup().name

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Host Pool for Azure Virtual Desktop
resource hostPool 'Microsoft.DesktopVirtualization/hostPools@2023-09-05' = {
  name: 'hp-${baseName}'
  location: location
  properties: {
    hostPoolType: 'Pooled'
    loadBalancerType: 'BreadthFirst'
    maxSessionLimit: 5
    preferredAppGroupType: 'Desktop'
    startVMOnConnect: false
    validationEnvironment: false
  }
}

// Application Group for the Host Pool
resource appGroup 'Microsoft.DesktopVirtualization/applicationGroups@2023-09-05' = {
  name: 'ag-${baseName}'
  location: location
  properties: {
    applicationGroupType: 'Desktop'
    hostPoolArmPath: hostPool.id
  }
}

// Workspace to contain the Application Group
resource workspace 'Microsoft.DesktopVirtualization/workspaces@2023-09-05' = {
  name: 'ws-${baseName}'
  location: location
  properties: {
    applicationGroupReferences: [
      appGroup.id
    ]
  }
}

// Reference to Desktop Virtualization User role definition
resource desktopVirtualizationUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '1d18fff3-a72a-46b5-b4a9-0b38a3cd7e63' // Desktop Virtualization User
}

// Desktop Virtualization User role assignment for the test application
resource desktopUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(desktopVirtualizationUserRole.id, testApplicationOid, resourceGroup().id)
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: desktopVirtualizationUserRole.id
  }
}

// Reference to Reader role definition
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7' // Reader
}

// Reader role assignment on the resource group
resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, resourceGroup().id)
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
  }
}

// Output the created resources for testing
output hostPoolName string = hostPool.name
output hostPoolId string = hostPool.id
output applicationGroupName string = appGroup.name
output workspaceName string = workspace.name
