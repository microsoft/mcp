targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Storage account for file shares
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: toLower(replace(baseName, '-', ''))
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_GRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// File share in storage account
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  name: '${storageAccount.name}/default/testshare'
  properties: {
    shareQuota: 100
    enabledProtocols: 'SMB'
  }
}

// Role assignment for storage account (Storage File Data SMB Share Contributor)
resource storageRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '0c867c2a-1d8c-454a-a3db-ab2ea1bdc8bb'
}

resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageRoleDefinition.id
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
  }
}

// Outputs for test consumption
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output fileShareName string = 'testshare'
