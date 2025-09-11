targetScope = 'resourceGroup'

@minLength(3)
// Can be up to 24, but needs to be 22 for the storage account name addition of 'sa'
@maxLength(22)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('Virtual network address prefix')
param vnetAddressPrefix string = '10.20.0.0/16'

@description('Subnet prefix for AMLFS (must be at least /24 per RP requirement)')
param amlfsSubnetPrefix string = '10.20.1.0/24'

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('Object ID of the HPC Cache Resource Provider (service principal) that needs Storage roles on the storage account.')
param hpcCacheRpObjectId string


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
          // Allow this subnet to reach Storage via service endpoint (used by storage account network rules below)
          serviceEndpoints: [
            {
              service: 'Microsoft.Storage'
            }
          ]
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

// Storage account used for HSM hydration and logging containers
@minLength(3)
@maxLength(24)
@description('Storage account name for HSM hydration and logging containers')
param storageAccountName string = toLower('${baseName}sa')

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    // Restrict network access to the specified subnet (default Deny others)
    networkAcls: {
      bypass: 'AzureServices'
      virtualNetworkRules: [
        {
          id: filesystemSubnetId
          action: 'Allow'
        }
      ]
      ipRules: []
      defaultAction: 'Deny'
    }
    publicNetworkAccess: 'Enabled'
  }
}

// Role assignments granting the HPC Cache RP required access to the storage account for HSM (imports/exports)
// Storage Account Contributor
resource storageAccountContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, '17d1049b-9a84-46fb-8f53-869881c3d3ab', hpcCacheRpObjectId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab')
    principalId: hpcCacheRpObjectId
    principalType: 'ServicePrincipal'
  }
}

// Storage Blob Data Contributor
resource storageBlobDataContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe', hpcCacheRpObjectId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: hpcCacheRpObjectId
    principalType: 'ServicePrincipal'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {}
}

resource dataContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  parent: blobService
  name: 'data'
  properties: {
    publicAccess: 'None'
  }
}

resource loggingContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  parent: blobService
  name: 'logging'
  properties: {
    publicAccess: 'None'
  }
}

var filesystemSubnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnet.name, 'amlfs')

resource amlfs 'Microsoft.StorageCache/amlFilesystems@2024-07-01' = {
  name: baseName
  location: location
  sku: {
    name: amlfsSku
  }
  properties: {
    storageCapacityTiB: amlfsCapacityTiB
    filesystemSubnet: filesystemSubnetId
    hsm: {
      settings: {
        // Resource IDs for the blob containers used by HSM
        // Use symbolic resource IDs so deployment engine creates dependency on containers
        container: dataContainer.id
        loggingContainer: loggingContainer.id
        // Only blobs prefixed with one of these paths will be imported during initial creation
        importPrefixesInitial: [
          '/'
        ]
      }
    }
    maintenanceWindow: {
      dayOfWeek: 'Sunday'
      timeOfDayUTC: '02:00'
    }
  }
}

output amlfsId string = amlfs.id
output amlfsSubnetId string = filesystemSubnetId
