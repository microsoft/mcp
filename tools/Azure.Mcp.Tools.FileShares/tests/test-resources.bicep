targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

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
// Virtual Network and Subnet for Private Endpoints
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
// Microsoft.FileShares Resources
// ============================================================================

// FileShare 1 - Primary file share for testing
resource fileShare1 'Microsoft.FileShares/fileShares@2025-06-01-preview' = {
  name: '${baseName}-fileshare-${substring(uniqueString(resourceGroup().id), 0, 6)}'
  location: location
  properties: {
    description: 'Primary file share for testing MFS operations'
    enabledProtocols: 'SMB'
    accessTier: 'Standard'
    shareQuota: 1024
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// FileShare 2 - Secondary file share for testing
resource fileShare2 'Microsoft.FileShares/fileShares@2025-06-01-preview' = {
  name: '${baseName}-fileshare-02-${substring(uniqueString(resourceGroup().id), 0, 6)}'
  location: location
  properties: {
    description: 'Secondary file share for testing MFS operations'
    enabledProtocols: 'SMB'
    accessTier: 'Standard'
    shareQuota: 512
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// FileShare Snapshot 1 - Snapshot of primary file share
resource fileShareSnapshot1 'Microsoft.FileShares/fileShares/fileShareSnapshots@2025-06-01-preview' = {
  name: '${fileShare1.name}/snapshot-${substring(uniqueString(resourceGroup().id, utcNow('u')), 0, 8)}'
  properties: {
    description: 'Snapshot for testing file share snapshot operations'
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// FileShare Snapshot 2 - Another snapshot of secondary file share
resource fileShareSnapshot2 'Microsoft.FileShares/fileShares/fileShareSnapshots@2025-06-01-preview' = {
  name: '${fileShare2.name}/snapshot-${substring(uniqueString(resourceGroup().id, utcNow('d')), 0, 8)}'
  properties: {
    description: 'Secondary snapshot for testing file share snapshot operations'
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// ============================================================================
// Private Endpoint for FileShare
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
// Role Assignments for Access Control
// ============================================================================

// Reference to Storage File Data SMB Share Contributor role
resource fileShareContributorRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '0c867c2a-1d8c-454a-a3db-ab2ea1bdc8bb' // Storage File Data SMB Share Contributor
}

// Role assignment for FileShare 1
resource fileShare1RoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(fileShareContributorRole.id, testApplicationOid, fileShare1.id)
  scope: fileShare1
  properties: {
    roleDefinitionId: fileShareContributorRole.id
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for FileShare 2
resource fileShare2RoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(fileShareContributorRole.id, testApplicationOid, fileShare2.id)
  scope: fileShare2
  properties: {
    roleDefinitionId: fileShareContributorRole.id
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
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

// Test application OID
output testApplicationOid string = testApplicationOid
