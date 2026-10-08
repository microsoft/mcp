targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name used by the live tests.')
param baseName string = resourceGroup().name

@description('The client object ID to grant access to the test resource group.')
param testApplicationOid string

param location string = resourceGroup().location

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: '${baseName}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'anf'
        properties: {
          addressPrefix: '10.0.0.0/24'
          delegations: [
            {
              name: 'Microsoft.NetApp-volumes'
              properties: {
                serviceName: 'Microsoft.NetApp/volumes'
              }
            }
          ]
        }
      }
    ]
  }
}

resource netAppAccount 'Microsoft.NetApp/netAppAccounts@2024-09-01' = {
  name: '${baseName}-volume'
  location: location
}

resource capacityPool 'Microsoft.NetApp/netAppAccounts/capacityPools@2024-09-01' = {
  parent: netAppAccount
  name: 'pool'
  location: location
  properties: {
    serviceLevel: 'Standard'
    size: 4398046511104
  }
}

resource backupVault 'Microsoft.NetApp/netAppAccounts/backupVaults@2024-09-01' = {
  parent: netAppAccount
  name: 'vault'
  location: location
}

resource backupVolume 'Microsoft.NetApp/netAppAccounts/capacityPools/volumes@2024-09-01' = {
  parent: capacityPool
  name: '${baseName}-backup-volume'
  location: location
  properties: {
    creationToken: '${baseName}-backup-volume'
    dataProtection: {
      backup: {
        backupVaultId: backupVault.id
        policyEnforced: false
      }
    }
    protocolTypes: [
      'NFSv3'
    ]
    serviceLevel: 'Standard'
    subnetId: virtualNetwork.properties.subnets[0].id
    usageThreshold: 107374182400
  }
}

resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource netAppFilesContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testApplicationOid, contributorRoleDefinition.id)
  properties: {
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
    roleDefinitionId: contributorRoleDefinition.id
    description: 'Contributor for Azure NetApp Files live tests'
  }
}

output NETAPP_ACCOUNT_NAME string = netAppAccount.name
output NETAPP_BACKUP_VAULT_NAME string = backupVault.name
output NETAPP_BACKUP_VOLUME_ID string = backupVolume.id
output NETAPP_POOL_NAME string = capacityPool.name
output NETAPP_SUBNET_ID string = virtualNetwork.properties.subnets[0].id
output LOCATION string = location