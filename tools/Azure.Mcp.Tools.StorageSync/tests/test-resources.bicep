targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Storage Sync Service
resource storageSyncService 'Microsoft.StorageSync/storageSyncServices@2022-06-01' = {
  name: baseName
  location: location
  properties: {
    incomingTrafficPolicy: 'AllowAllTraffic'
  }
}

// Sync Group
resource syncGroup 'Microsoft.StorageSync/storageSyncServices/syncGroups@2022-06-01' = {
  name: '${baseName}-sg'
  parent: storageSyncService
  properties: {
  }
}

// Outputs for testing
output storageSyncServiceName string = storageSyncService.name
output storageSyncServiceId string = storageSyncService.id
output syncGroupName string = syncGroup.name
output syncGroupId string = syncGroup.id
