targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The storage account name for cloud endpoint.')
param storageAccountName string = 'sa${uniqueString(resourceGroup().id, baseName)}'

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
  name: '${baseName}'
  parent: storageSyncService
  properties: {
  }
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

// Role Assignment - Reader and Data Access
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, '9469b9f5-6722-4481-a2b2-14ed560b706f')
  scope: storageAccount
  properties: {
    principalId: '9469b9f5-6722-4481-a2b2-14ed560b706f'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'c12c1c16-33a1-487b-954d-41c89c60f349')
    principalType: 'ServicePrincipal'
  }
}

// File Share
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${storageAccount.name}/default/${baseName}-share'
  properties: {
    accessTier: 'TransactionOptimized'
    shareQuota: 100
  }
}

// Cloud Endpoint
resource cloudEndpoint 'Microsoft.StorageSync/storageSyncServices/syncGroups/cloudEndpoints@2022-06-01' = {
  name: '${baseName}'
  parent: syncGroup
  properties: {
    storageAccountResourceId: storageAccount.id
    azureFileShareName: '${baseName}-share'
    storageAccountTenantId: subscription().tenantId
  }
  dependsOn: [
    fileShare
  ]
}

// Outputs for testing
output storageSyncServiceName string = storageSyncService.name
output storageSyncServiceId string = storageSyncService.id
output syncGroupName string = syncGroup.name
output syncGroupId string = syncGroup.id
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output fileShareName string = '${baseName}-share'
output cloudEndpointName string = cloudEndpoint.name
output cloudEndpointId string = cloudEndpoint.id
