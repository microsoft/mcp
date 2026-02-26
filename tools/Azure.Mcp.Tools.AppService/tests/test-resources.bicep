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

@description('The SKU for the App Service Plan.')
param appServicePlanSku string = 'S1'

// Variables
var webAppName = '${baseName}-webapp'
var appServicePlanName = '${baseName}-plan'
var logAnalyticsWorkspaceName = '${baseName}-law'
var appInsightsName = '${baseName}-ai'
var sqlServerName = '${baseName}-sql'
var sqlDatabaseName = '${baseName}db'
var cosmosAccountName = '${baseName}-cosmos'
var cosmosDatabaseName = '${baseName}cosmosdb'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: 'westus2'
  sku: {
    name: appServicePlanSku
    tier: 'Standard'
    size: appServicePlanSku
    family: 'S'
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
  location: 'westus2'
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
      alwaysOn: true
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

  resource logs 'config' = {
    name: 'logs'
    properties: {
      applicationLogs: {
        fileSystem: {
          level: 'Warning'
        }
      }
      detailedErrorMessages: {
        enabled: true
      }
      failedRequestsTracing: {
        enabled: true
      }
      httpLogs: {
        fileSystem: {
          enabled: true
          retentionInDays: 7
          retentionInMb: 35
        }
      }
    }
  }
}

// Log Analytics Workspace (required by Application Insights)
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: 'westus2'
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: 'westus2'
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Connect Application Insights to the Web App
resource webAppAppSettings 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: 'westus2'
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }

  // Test SQL Database
  resource testDatabase 'databases@2023-05-01-preview' = {
    name: sqlDatabaseName
    location: 'westus2'
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

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: cosmosAccountName
  location: 'westus2'
  tags: {
    defaultExperience: 'Core (SQL)'
    CosmosAccountType: 'Non-Production'
  }
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
    disableLocalAuth: true
    enablePartitionMerge: false
    minimalTlsVersion: 'Tls12'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
      maxIntervalInSeconds: 5
      maxStalenessPrefix: 100
    }
    locations: [
      {
        locationName: 'westus2'
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    cors: []
    capabilities: []
    ipRules: []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Geo'
      }
    }
    networkAclBypassResourceIds: []
  }

  // Test Cosmos Database
  resource testDatabase 'sqlDatabases@2024-11-15' = {
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
    resource testContainer 'containers@2024-11-15' = {
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
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#website-contributor
resource webAppContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(webApp.id, testApplicationOid, 'de139f84-1756-47ae-9be6-808fbbe84772')
  scope: webApp
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772') // Website Contributor
  }
}

// Role assignment for test application - SQL DB Contributor
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#sql-db-contributor
resource sqlContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sqlServer.id, testApplicationOid, '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec')
  scope: sqlServer
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec') // SQL DB Contributor
  }
}

// Role assignment for test application - Monitoring Contributor
// See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#monitoring-contributor
resource appInsightsContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(appInsights.id, testApplicationOid, '749f88d5-cbae-40b8-bcfc-e573ddc772fa')
  scope: appInsights
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '749f88d5-cbae-40b8-bcfc-e573ddc772fa') // Monitoring Contributor
  }
}

// Outputs for test usage
output webAppName string = webApp.name
output webAppResourceGroup string = resourceGroup().name
output appInsightsName string = appInsights.name
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDatabaseName
output sqlConnectionString string = 'Server=${sqlServer.properties.fullyQualifiedDomainName};Database=${sqlDatabaseName};Authentication=Active Directory Default;TrustServerCertificate=True;'

output cosmosAccountName string = cosmosAccount.name
output cosmosDatabaseName string = cosmosDatabaseName
output cosmosConnectionString string = 'AccountEndpoint=${cosmosAccount.properties.documentEndpoint};AccountKey=${cosmosAccount.listKeys().primaryMasterKey};Database=${cosmosDatabaseName};'
output baseName string = baseName
