targetScope = 'resourceGroup'

@minLength(3)
@maxLength(20)
@description('The base resource name.')
param baseName string = take(resourceGroup().name, 20)

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Recovery Services Vault (RSV) - GeoRedundant for CRR support
resource rsvVault 'Microsoft.RecoveryServices/vaults@2024-04-01' = {
  name: '${baseName}-rsv'
  location: location
  sku: {
    name: 'RS0'
    tier: 'Standard'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

// Set RSV storage config to GeoRedundant (required for Cross-Region Restore)
resource rsvBackupConfig 'Microsoft.RecoveryServices/vaults/backupconfig@2024-04-01' = {
  parent: rsvVault
  name: 'vaultconfig'
  properties: {
    storageModelType: 'GeoRedundant'
  }
}

// Backup Vault (Data Protection / DPP) - GeoRedundant for CRR support
resource dppVault 'Microsoft.DataProtection/backupVaults@2024-04-01' = {
  name: '${baseName}-dpp'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    storageSettings: [
      {
        datastoreType: 'VaultStore'
        type: 'GeoRedundant'
      }
    ]
  }
}

// Backup Contributor role on RSV vault
resource backupContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '5e467623-bb1f-42f4-a55d-6e525e11384b'
}

resource appBackupContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(backupContributorRoleDefinition.id, testApplicationOid, rsvVault.id)
  scope: rsvVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: backupContributorRoleDefinition.id
    description: 'Backup Contributor for ${testApplicationOid}'
  }
}

// Backup Contributor role on DPP vault
resource appDppBackupContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(backupContributorRoleDefinition.id, testApplicationOid, dppVault.id)
  scope: dppVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: backupContributorRoleDefinition.id
    description: 'Backup Contributor for ${testApplicationOid}'
  }
}
