targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

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

// Storage Account for cloud endpoint
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'sa${replace(baseName, '-', '')}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// File Service
resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  name: 'default'
  parent: storageAccount
}

// File Share for cloud endpoint
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: 'cloudsync'
  parent: fileService
  properties: {
    accessTier: 'Hot'
    shareQuota: 100
  }
}

// Cloud Endpoint
resource cloudEndpoint 'Microsoft.StorageSync/storageSyncServices/syncGroups/cloudEndpoints@2022-06-01' = {
  name: '${baseName}-ce'
  parent: syncGroup
  properties: {
    storageAccountResourceId: storageAccount.id
    azureFileShareName: fileShare.name
    friendlyName: 'cloud-endpoint-${baseName}'
  }
  dependsOn: [
    fileShare
  ]
}

// Role assignment for Storage File Data SMB Share Contributor
resource storageFileShareRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage File Data SMB Share Contributor role
  name: '0c867c2a-1d8c-454a-a3db-ab2ea1bdc13b'
}

resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageFileShareRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: storageFileShareRoleDefinition.id
  }
}

// Outputs for testing
output storageSyncServiceName string = storageSyncService.name
output storageSyncServiceId string = storageSyncService.id
output syncGroupName string = syncGroup.name
output syncGroupId string = syncGroup.id
output storageAccountName string = storageAccount.name
output fileShareName string = fileShare.name
output cloudEndpointName string = cloudEndpoint.name
