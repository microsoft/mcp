targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('Virtual network name for private endpoints.')
param vnetName string = '${baseName}-vnet'

@description('Virtual network address space.')
param vnetAddressSpace string = '10.0.0.0/16'

@description('Subnet name for private endpoints.')
param subnetName string = '${baseName}-subnet'

@description('Subnet address space.')
param subnetAddressSpace string = '10.0.0.0/24'

// ============================================================================
// 1. Virtual Network and Subnet
// ============================================================================

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-06-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressSpace
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: subnetAddressSpace
          serviceEndpoints: [
            {
              service: 'Microsoft.FileShares'
            }
          ]
          privateLinkServiceNetworkPolicies: 'Disabled'
        }
      }
    ]
  }
}

// ============================================================================
// 2. FileShares with VNet Association
// ============================================================================

// FileShare 1 - Primary file share with VNet association
resource fileShare1 'Microsoft.FileShares/fileShares@2025-06-01-preview' = {
  name: '${baseName}-fileshare-01'
  location: location
  properties: {
    protocol: 'NFS'
    mediaTier: 'SSD'
    redundancy: 'Local'
    provisionedStorageGiB: 32
    provisionedIOPerSec: 9
    provisionedThroughputMiBPerSec: 5
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// FileShare 2 - Secondary file share with VNet association
resource fileShare2 'Microsoft.FileShares/fileShares@2025-06-01-preview' = {
  name: '${baseName}-fileshare-02'
  location: location
  properties: {
    protocol: 'NFS'
    mediaTier: 'Standard'
    redundancy: 'Local'
    provisionedStorageGiB: 32
    provisionedIOPerSec: 5
    provisionedThroughputMiBPerSec: 3
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// ============================================================================
// 3. FileShare Snapshots
// ============================================================================

// FileShare Snapshot 1 - Snapshot of primary file share
resource fileShareSnapshot1 'Microsoft.FileShares/fileShares/fileShareSnapshots@2025-06-01-preview' = {
  parent: fileShare1
  name: '${baseName}-snapshot-01'
  properties: {
    metadata: {
      environment: 'test'
      purpose: 'mcp-testing'
    }
  }
}

// FileShare Snapshot 2 - Snapshot of secondary file share
resource fileShareSnapshot2 'Microsoft.FileShares/fileShares/fileShareSnapshots@2025-06-01-preview' = {
  parent: fileShare2
  name: '${baseName}-snapshot-02'
  properties: {
    metadata: {
      environment: 'test'
      purpose: 'mcp-testing'
    }
  }
}

// ============================================================================
// 4. Private Endpoint for FileShare
// ============================================================================

resource fileSharePrivateEndpoint 'Microsoft.Network/privateEndpoints@2023-06-01' = {
  name: '${baseName}-fs-pe'
  location: location
  properties: {
    subnet: {
      id: '${virtualNetwork.id}/subnets/${subnetName}'
    }
    privateLinkServiceConnections: [
      {
        name: '${baseName}-fs-plsc'
        properties: {
          privateLinkServiceId: fileShare1.id
          groupIds: [
            'file'
          ]
        }
      }
    ]
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// ============================================================================
// Outputs for Test Consumption
// ============================================================================

// FileShare outputs
output fileShare1Name string = fileShare1.name
output fileShare1Id string = fileShare1.id
output fileShare2Name string = fileShare2.name
output fileShare2Id string = fileShare2.id

// FileShare Snapshot outputs
output fileShareSnapshot1Name string = fileShareSnapshot1.name
output fileShareSnapshot1Id string = fileShareSnapshot1.id
output fileShareSnapshot2Name string = fileShareSnapshot2.name
output fileShareSnapshot2Id string = fileShareSnapshot2.id

// Network outputs
output virtualNetworkName string = virtualNetwork.name
output virtualNetworkId string = virtualNetwork.id
output subnetName string = subnetName
output subnetId string = '${virtualNetwork.id}/subnets/${subnetName}'

// Private Endpoint outputs
output privateEndpointName string = fileSharePrivateEndpoint.name
output privateEndpointId string = fileSharePrivateEndpoint.id
