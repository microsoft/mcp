targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource vault 'Microsoft.RecoveryServices/vaults@2024-04-01' = {
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

resource backupContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '5e467623-bb1f-42f4-a55d-6e525e11384b'
}

resource appBackupContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(backupContributorRoleDefinition.id, testApplicationOid, vault.id)
  scope: vault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: backupContributorRoleDefinition.id
    description: 'Backup Contributor for ${testApplicationOid}'
  }
}
