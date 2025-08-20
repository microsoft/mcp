// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

@description('The location where resources will be deployed')
param location string = resourceGroup().location

@description('The base name for resource naming')
param baseName string = resourceGroup().name

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

// Output the created resources for testing
output hostPoolName string = hostPool.name
output hostPoolId string = hostPool.id
output applicationGroupName string = appGroup.name
output workspaceName string = workspace.name
