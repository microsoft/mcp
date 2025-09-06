targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('SQL Server administrator login name.')
param sqlAdminLogin string = 'mcptestadmin'

@description('SQL Server administrator password.')
@secure()
param sqlAdminPassword string = newGuid()

@description('MySQL administrator login name.')
param mysqlAdminLogin string = 'mcptestadmin'

@description('MySQL administrator password.')
@secure()
param mysqlAdminPassword string = newGuid()

@description('PostgreSQL administrator login name.')
param postgresAdminLogin string = 'mcptestadmin'

@description('PostgreSQL administrator password.')
@secure()
param postgresAdminPassword string = newGuid()

@description('The SKU for the App Service Plan.')
param appServicePlanSku string = 'B1'

// Variables
var webAppName = '${baseName}-webapp'
var appServicePlanName = '${baseName}-plan'
var sqlServerName = '${baseName}-sql'
var sqlDatabaseName = '${baseName}db'
var mysqlServerName = '${baseName}-mysql'
var mysqlDatabaseName = '${baseName}mysqldb'
var postgresServerName = '${baseName}-postgres'
var postgresDatabaseName = '${baseName}postgresdb'
var cosmosAccountName = '${baseName}-cosmos'
var cosmosDatabaseName = '${baseName}cosmosdb'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: 'Basic'
    size: appServicePlanSku
    family: 'B'
    capacity: 1
  }
  properties: {
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: false
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  kind: 'app'
  properties: {
    enabled: true
    hostNameSslStates: [
      {
        name: '${webAppName}.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Standard'
      }
      {
        name: '${webAppName}.scm.azurewebsites.net'
        sslState: 'Disabled'
        hostType: 'Repository'
      }
    ]
    serverFarmId: appServicePlan.id
    reserved: false
    isXenon: false
    hyperV: false
    vnetRouteAllEnabled: false
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    siteConfig: {
      numberOfWorkers: 1
      acrUseManagedIdentityCreds: false
      alwaysOn: false
      http20Enabled: false
      functionAppScaleLimit: 0
      minimumElasticInstanceCount: 0
    }
    scmSiteAlsoStopped: false
    clientAffinityEnabled: true
    clientCertEnabled: false
    clientCertMode: 'Required'
    hostNamesDisabled: false
    containerSize: 0
    dailyMemoryTimeQuota: 0
    httpsOnly: false
    redundancyMode: 'None'
    storageAccountRequired: false
    keyVaultReferenceIdentity: 'SystemAssigned'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }

  // Firewall rule to allow Azure services
  resource allowAzureServices 'firewallRules@2023-05-01-preview' = {
    name: 'AllowAllWindowsAzureIps'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  // Test SQL Database
  resource testDatabase 'databases@2023-05-01-preview' = {
    name: sqlDatabaseName
    location: location
    sku: {
      name: 'Basic'
      tier: 'Basic'
      capacity: 5
    }
    properties: {
      collation: 'SQL_Latin1_General_CP1_CI_AS'
      maxSizeBytes: 2147483648
      catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
      zoneRedundant: false
      readScale: 'Disabled'
      requestedBackupStorageRedundancy: 'Local'
      isLedgerOn: false
    }
  }
}

// MySQL Flexible Server
resource mysqlServer 'Microsoft.DBforMySQL/flexibleServers@2023-06-30' = {
  name: mysqlServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: mysqlAdminLogin
    administratorLoginPassword: mysqlAdminPassword
    version: '8.0.21'
    storage: {
      storageSizeGB: 20
      iops: 360
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }

  // Firewall rule to allow Azure services
  resource allowAzureServices 'firewallRules@2023-06-30' = {
    name: 'AllowAllWindowsAzureIps'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  // Test MySQL Database
  resource testDatabase 'databases@2023-06-30' = {
    name: mysqlDatabaseName
    properties: {
      charset: 'utf8'
      collation: 'utf8_general_ci'
    }
  }
}

// PostgreSQL Flexible Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-06-01-preview' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    version: '15'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }

  // Firewall rule to allow Azure services
  resource allowAzureServices 'firewallRules@2023-06-01-preview' = {
    name: 'AllowAllWindowsAzureIps'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  // Test PostgreSQL Database
  resource testDatabase 'databases@2023-06-01-preview' = {
    name: postgresDatabaseName
    properties: {
      charset: 'utf8'
      collation: 'en_US.utf8'
    }
  }
}

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    publicNetworkAccess: 'Enabled'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    isVirtualNetworkFilterEnabled: false
    virtualNetworkRules: []
    disableKeyBasedMetadataWriteAccess: false
    enableFreeTier: false
    enableAnalyticalStorage: false
    analyticalStorageConfiguration: {
      schemaType: 'WellDefined'
    }
    databaseAccountOfferType: 'Standard'
    defaultIdentity: 'FirstPartyIdentity'
    networkAclBypass: 'None'
    disableLocalAuth: false
    enablePartitionMerge: false
    minimalTlsVersion: 'Tls12'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: []
    ipRules: []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
    networkAclBypassResourceIds: []
  }

  // Test Cosmos Database
  resource testDatabase 'sqlDatabases@2023-04-15' = {
    name: cosmosDatabaseName
    properties: {
      resource: {
        id: cosmosDatabaseName
      }
      options: {
        throughput: 400
      }
    }

    // Test container
    resource testContainer 'containers@2023-04-15' = {
      name: 'testcontainer'
      properties: {
        resource: {
          id: 'testcontainer'
          partitionKey: {
            paths: [
              '/id'
            ]
            kind: 'Hash'
          }
        }
      }
    }
  }
}

// Role assignment for test application - Web App Contributor
resource webAppContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(webApp.id, testApplicationOid, 'de139f84-1756-47ae-9be6-808fbbe84772')
  scope: webApp
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772') // Website Contributor
  }
}

// Role assignment for test application - SQL DB Contributor
resource sqlContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sqlServer.id, testApplicationOid, '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec')
  scope: sqlServer
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec') // SQL DB Contributor
  }
}

// Role assignment for test application - Cosmos DB Contributor
resource cosmosContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cosmosAccount.id, testApplicationOid, '5bd9cd88-fe45-4216-938b-f97437e15450')
  scope: cosmosAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '5bd9cd88-fe45-4216-938b-f97437e15450') // DocumentDB Account Contributor
  }
}

// Outputs for test usage
output webAppName string = webApp.name
output webAppResourceGroup string = resourceGroup().name
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabaseName
output sqlConnectionString string = 'Server=${sqlServer.properties.fullyQualifiedDomainName};Database=${sqlDatabaseName};Authentication=Active Directory Default;TrustServerCertificate=True;'
output mysqlServerName string = mysqlServer.name
output mysqlDatabaseName string = mysqlDatabaseName
output mysqlConnectionString string = 'Server=${mysqlServer.properties.fullyQualifiedDomainName};Database=${mysqlDatabaseName};Uid=${mysqlAdminLogin};Pwd=${mysqlAdminPassword};'
output postgresServerName string = postgresServer.name
output postgresDatabaseName string = postgresDatabaseName
output postgresConnectionString string = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=${postgresDatabaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};'
output cosmosAccountName string = cosmosAccount.name
output cosmosDatabaseName string = cosmosDatabaseName
output cosmosConnectionString string = 'AccountEndpoint=${cosmosAccount.properties.documentEndpoint};AccountKey=${cosmosAccount.listKeys().primaryMasterKey};Database=${cosmosDatabaseName};'
output baseName string = baseName
output location string = location
