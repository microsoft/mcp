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

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
    isHnsEnabled: true
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    resource fooContainer 'containers' = { name: 'foo' }
    resource barContainer 'containers' = { name: 'bar' }
    resource bazContainer 'containers' = { name: 'baz' }
    resource testFileSystem 'containers' = { 
      name: 'testfilesystem'
      properties: {
        publicAccess: 'None'
      }
    }
  }

  resource tableServices 'tableServices' = {
    name: 'default'
    resource fooTable 'tables' = { name: 'foo' }
    resource barTable 'tables' = { name: 'bar' }
    resource bazTable 'tables' = { name: 'baz' }
  }

  resource fileServices 'fileServices' = {
    name: 'default'
    resource testShare 'shares' = { 
      name: 'testshare'
      properties: {
        shareQuota: 1024
      }
    }
    resource fooShare 'shares' = { 
      name: 'foo'
      properties: {
        shareQuota: 1024
      }
    }
    resource barShare 'shares' = { 
      name: 'bar'
      properties: {
        shareQuota: 1024
      }
    }
  }

  resource queueServices 'queueServices' = {
    name: 'default'
    resource testQueue 'queues' = { 
      name: 'testqueue'
    }
  }
}

resource blobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Contributor role.
  // Read, write, and delete Azure Storage containers and blobs
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource fileContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage File Data Privileged Contributor role.
  // Read, write, and delete Azure Storage files and directories
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage
  name: '69566ab7-960f-475b-8e7c-b3118f30c6bd'
}

resource queueDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Queue Data Contributor role.
  // Read, write, and delete Azure Storage queues and messages
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-queue-data-contributor
  name: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
}

resource appBlobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(blobContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: blobContributorRoleDefinition.id
    description: 'Blob Contributor for testApplicationOid'
  }
}

resource appFileRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(fileContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: fileContributorRoleDefinition.id
    description: 'File Share Contributor for testApplicationOid'
  }
}

resource appQueueRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(queueDataContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: queueDataContributorRoleDefinition.id
    description: 'Queue Data Contributor for testApplicationOid'
  }
}
