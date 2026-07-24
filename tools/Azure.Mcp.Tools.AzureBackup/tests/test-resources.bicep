targetScope = 'resourceGroup'

@minLength(3)
@maxLength(20)
@description('The base resource name.')
param baseName string = take(resourceGroup().name, 20)

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The location for the Cosmos DB account and the DPP backup vault. Azure Backup for Cosmos DB (DPP, preview) requires both to be in the same region (and in a preview-enabled region such as eastus2euap or centraluseuap). Defaults to the resource group location; override (along with the RG location) when targeting a preview region.')
param cosmosLocation string = location

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
  identity: {
    type: 'SystemAssigned'
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
// Note: Cosmos DB (preview) DPP backup requires the vault to be in the same region as the Cosmos DB primary write region.
// `cosmosLocation` defaults to `location`, so by default the DPP vault and the Cosmos DB account are co-located.
// When overriding `cosmosLocation`, set `location` to the same preview-enabled region.
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

// ─── Resources for Undelete Tests ───

// Managed Disk for DPP backup + undelete testing
resource testDisk 'Microsoft.Compute/disks@2023-10-02' = {
  name: '${baseName}-disk'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    creationData: {
      createOption: 'Empty'
    }
    diskSizeGB: 4
  }
}

// Disk Snapshot Contributor role for DPP vault MSI on the resource group
// Required for DPP to take disk snapshots
resource diskSnapshotContributorRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '7efff54f-a5b4-42b5-a1c5-5411624893ce' // Disk Snapshot Contributor
}

resource dppDiskSnapshotContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(diskSnapshotContributorRoleDef.id, dppVault.id, resourceGroup().id)
  properties: {
    principalId: dppVault.identity.principalId
    roleDefinitionId: diskSnapshotContributorRoleDef.id
    principalType: 'ServicePrincipal'
    description: 'Disk Snapshot Contributor for DPP vault MSI'
  }
}

// Disk Backup Reader role for DPP vault MSI on the disk
resource diskBackupReaderRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '3e5e47e6-65f7-47ef-90b5-e5dd4d455f24' // Disk Backup Reader
}

resource dppDiskBackupReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(diskBackupReaderRoleDef.id, dppVault.id, testDisk.id)
  scope: testDisk
  properties: {
    principalId: dppVault.identity.principalId
    roleDefinitionId: diskBackupReaderRoleDef.id
    principalType: 'ServicePrincipal'
    description: 'Disk Backup Reader for DPP vault MSI'
  }
}

// Output the resource IDs for the post-deployment script
output diskId string = testDisk.id
output diskName string = testDisk.name

// ─── RSV Undelete Test Resources (Storage Account + File Share) ───

// Storage Account for RSV file share backup + undelete testing
// allowSharedKeyAccess=true is required for RSV file share backup integration.
// Policy requires Reason, ETA, and DisableLocalAuth tags when shared key is enabled.
resource testStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: take('${replace(baseName, '-', '')}sa', 24)
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowSharedKeyAccess: true
  }
  tags: {
    DisableLocalAuth: 'false'
    Reason: 'RSV file share backup requires shared key auth'
    ETA: '2027-01-01'
    Owner: 'azurebackup-mcp-tests'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

resource testFileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: testStorageAccount
  name: 'default'
}

resource testFileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: testFileService
  name: '${baseName}-share'
  properties: {
    shareQuota: 1
  }
}

// Storage Account Backup Contributor for the test app on the storage account
resource storageBackupContributorRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'e5e2a7ff-d759-4cd2-bb51-3152d37e2eb1' // Storage Account Backup Contributor
}

resource appStorageBackupContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageBackupContributorRoleDef.id, testApplicationOid, testStorageAccount.id)
  scope: testStorageAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: storageBackupContributorRoleDef.id
    description: 'Storage Account Backup Contributor for ${testApplicationOid}'
  }
}

output storageAccountId string = testStorageAccount.id
output storageAccountName string = testStorageAccount.name
output fileShareName string = testFileShare.name

// ─── Resources for CosmosDB Backup E2E Tests ───

// Cosmos DB account (NoSQL API) for DPP backup testing
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: take('${replace(baseName, '-', '')}cosmos', 44)
  location: cosmosLocation
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: cosmosLocation
        failoverPriority: 0
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    // Continuous backup mode (required for Azure Backup DPP long-term protection)
    backupPolicy: {
      type: 'Continuous'
      continuousModeProperties: {
        tier: 'Continuous7Days'
      }
    }
  }
  tags: {
    Owner: 'azurebackup-mcp-tests'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

// Cosmos DB Operator role for DPP vault MSI on the Cosmos DB account
// Required for the backup vault to manage backup operations
resource cosmosDbOperatorRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '230815da-be43-4aae-9cb4-875f7bd000aa' // Cosmos DB Operator
}

resource dppCosmosDbOperatorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cosmosDbOperatorRoleDef.id, dppVault.id, cosmosDbAccount.id)
  scope: cosmosDbAccount
  properties: {
    principalId: dppVault.identity.principalId
    roleDefinitionId: cosmosDbOperatorRoleDef.id
    principalType: 'ServicePrincipal'
    description: 'Cosmos DB Operator for DPP vault MSI'
  }
}

// Reader role for DPP vault MSI on the Cosmos DB account resource group
// Some DPP datasources require Reader on the RG
resource readerRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7' // Reader
}

resource dppReaderOnRgForCosmosDb 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDef.id, dppVault.id, resourceGroup().id, 'cosmosdb')
  properties: {
    principalId: dppVault.identity.principalId
    roleDefinitionId: readerRoleDef.id
    principalType: 'ServicePrincipal'
    description: 'Reader for DPP vault MSI on RG (CosmosDB backup)'
  }
}

// Backup Contributor for the test app on the Cosmos DB account
resource appCosmosDbBackupContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(backupContributorRoleDefinition.id, testApplicationOid, cosmosDbAccount.id)
  scope: cosmosDbAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: backupContributorRoleDefinition.id
    description: 'Backup Contributor for ${testApplicationOid} on CosmosDB'
  }
}

output cosmosDbAccountId string = cosmosDbAccount.id
output cosmosDbAccountName string = cosmosDbAccount.name
output cosmosDbAccountLocation string = cosmosLocation

// ─── SQL VM for ARM-Level Discovery Testing ───
// Lowest-cost config: Standard_B2als_v2 + SQL 2022 Developer (free license) + no public IP.
// The find-unprotected command discovers this VM via ARM Resource Graph as an unprotected resource.
// No RSV container registration is needed — ARM-level discovery finds VMs directly.

resource testVnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: '${baseName}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/24'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.0.0.0/24'
        }
      }
    ]
  }
  tags: {
    Owner: 'azurebackup-mcp-tests'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

resource sqlVmNic 'Microsoft.Network/networkInterfaces@2024-01-01' = {
  name: '${baseName}-sqlvm-nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: testVnet.properties.subnets[0].id
          }
          privateIPAllocationMethod: 'Dynamic'
        }
      }
    ]
  }
  tags: {
    Owner: 'azurebackup-mcp-tests'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

// Test-only password — VM has no public IP or public inbound access.
#disable-next-line secure-secrets-in-params
var sqlVmAdminPwd = 'McpT3st!${uniqueString(resourceGroup().id, baseName)}'

resource sqlVm 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: '${baseName}-sqlvm'
  location: location
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_B2als_v2'
    }
    storageProfile: {
      imageReference: {
        publisher: 'MicrosoftSQLServer'
        offer: 'sql2022-ws2022'
        sku: 'sqldev-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        managedDisk: {
          storageAccountType: 'Standard_LRS'
        }
      }
    }
    osProfile: {
      computerName: take('${replace(baseName, '-', '')}sq', 15)
      adminUsername: 'mcptestadmin'
      adminPassword: sqlVmAdminPwd
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: sqlVmNic.id
        }
      ]
    }
  }
  tags: {
    Owner: 'azurebackup-mcp-tests'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

output sqlVmId string = sqlVm.id
output sqlVmName string = sqlVm.name
output resourceGroupLocation string = location
