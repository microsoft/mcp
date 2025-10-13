targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('Virtual network address prefix')
param vnetAddressPrefix string = '10.20.0.0/16'

@description('Subnet prefix for AMLFS (must be at least /24 per RP requirement)')
param amlfsSubnetPrefix string = '10.20.1.0/24'

@description('Subnet prefix for AMLFS small, for subnet validation live tests.')
param amlfsSubnetSmallPrefix string = '10.20.2.0/28'

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId


@description('AMLFS SKU name')
@allowed([
  'AMLFS-Durable-Premium-40'
  'AMLFS-Durable-Premium-125'
  'AMLFS-Durable-Premium-250'
  'AMLFS-Durable-Premium-500'
])
param amlfsSku string = 'AMLFS-Durable-Premium-500'

@description('AMLFS capacity in TiB')
@minValue(4)
param amlfsCapacityTiB int = 4

var kvCryptoUserRoleDefinitionId = '14b46e9e-c2b7-41b4-b07b-48a6ebf60603'

var userAssignedName = '${baseName}-uai'


resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: '${baseName}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [vnetAddressPrefix]
    }
    subnets: [
      {
        name: 'amlfs'
        properties: {
          addressPrefix: amlfsSubnetPrefix
          natGateway: {
            id: natGateway.id
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
      {
        name: 'amlfs-small'
        properties: {
          addressPrefix: amlfsSubnetSmallPrefix
          natGateway: {
            id: natGateway.id
          }
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

resource natGateway 'Microsoft.Network/natGateways@2024-07-01' = {
  name: '${baseName}-nat'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    idleTimeoutInMinutes: 10
    publicIpAddresses: [
      {
        id: natPublicIp.id
      }
    ]
  }
}

resource natPublicIp 'Microsoft.Network/publicIPAddresses@2024-07-01' = {
  name: '${baseName}-nat-pip'
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
  }
}

var filesystemSubnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, 'amlfs')
var filesystemSmallSubnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, 'amlfs-small')

resource amlfs 'Microsoft.StorageCache/amlFilesystems@2024-07-01' = {
  name: baseName
  location: location
  sku: {
    name: amlfsSku
  }
  properties: {
    storageCapacityTiB: amlfsCapacityTiB
    filesystemSubnet: filesystemSubnetId
    maintenanceWindow: {
      dayOfWeek: 'Sunday'
      timeOfDayUTC: '02:00'
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-01-01' existing = {
  parent: storageAccount
  name: 'default'
}

resource hsmContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-01-01' = {
  parent : blobService
  name: 'hsm-data'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [ blobService ]
}

resource hsmLogsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-01-01' = {
  parent : blobService
  name: 'hsm-logs'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [ blobService ]
}


resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: userAssignedName
  location: location
}


resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: baseName
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enablePurgeProtection: true
    softDeleteRetentionInDays: 90
    publicNetworkAccess: 'Enabled'
  }
}

resource keyVaultCryptoUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, 'kv-crypto-user', userAssignedIdentity.id)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', kvCryptoUserRoleDefinitionId)
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultKey 'Microsoft.KeyVault/vaults/keys@2024-11-01' = {
  parent: keyVault
  name: 'encryption-key'
  properties: {
    kty: 'RSA'
    keySize: 2048
  }
}

// Outputs for tests
output STORAGE_ACCOUNT_ID string = storageAccount.id
output HSM_CONTAINER_ID string = hsmContainer.id
output HSM_LOGS_CONTAINER_ID string = hsmLogsContainer.id
output KEY_VAULT_RESOURCE_ID string = keyVault.id
output KEY_VAULT_NAME string = keyVault.name
output USER_ASSIGNED_IDENTITY_RESOURCE_ID string = userAssignedIdentity.id
output KEY_URI_WITH_VERSION string = keyVaultKey.properties.keyUriWithVersion
output AMLFS_ID string = amlfs.id
output AMLFS_SUBNET_ID string = filesystemSubnetId
output AMLFS_SUBNET_SMALL_ID string = filesystemSmallSubnetId
output LOCATION string = location
