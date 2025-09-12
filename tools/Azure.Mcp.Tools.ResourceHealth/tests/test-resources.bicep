targetScope = 'resourceGroup'

@minLength(3)
@maxLength(18) // Reduced to allow for 'rhtest' suffix (max 24 total)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// For ResourceHealth, we don't need to create specific resources since:
// 1. Service Health Events are subscription-level and don't require specific resources
// 2. Availability Status queries work on existing Azure resources in the subscription
// 3. The ResourceHealth APIs work at the subscription level

// However, we'll create a simple storage account to have a resource to test availability status against
resource testStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${baseName}rhtest'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Cool'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
  tags: {
    'azd-env-name': baseName
    'test-application-oid': testApplicationOid
  }
}

// Output the storage account resource ID for availability status testing
output storageAccountResourceId string = testStorageAccount.id
output storageAccountName string = testStorageAccount.name
