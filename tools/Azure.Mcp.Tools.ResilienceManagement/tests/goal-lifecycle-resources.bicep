targetScope = 'resourceGroup'
// cspell:ignore mcpgoals

@description('An existing, explicitly approved service group. This deployment never creates or deletes it.')
param serviceGroupName string

@description('A globally unique name for the dedicated goal lifecycle storage account.')
param storageAccountName string

// The provisioning identity inherits subscription/resource-group RBAC. No tenant role is granted.
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    publicNetworkAccess: 'Disabled'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource membership 'Microsoft.Relationships/serviceGroupMember@2023-09-01-preview' = {
  name: 'mcpgoals${uniqueString(serviceGroupName)}'
  properties: {
    targetId: tenantResourceId('Microsoft.Management/serviceGroups', serviceGroupName)
  }
}

output serviceGroupName string = serviceGroupName
output storageAccountId string = storage.id
output storageAccountName string = storage.name
output membershipId string = membership.id
