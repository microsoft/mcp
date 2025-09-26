targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The name of the storage account.')
param baseName string = 'mystorageaccount'

@description('The name of the Azure resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = 'b4d27b47-fc60-4385-bdf8-40230117daee'

@description('The subscription ID to which the application and resources belong.')
param subscriptionId string = 'c74a3712-a4f5-4a18-aca4-6e8b861eda05'

@description('The source path to the directory containing the web content.')
param sourcePath string = './wwwroot'

@description('Overwrite existing files when deploying')
param overwrite bool = true

@description('The object ID of the application to assign roles to.')
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
}

resource blobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
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
