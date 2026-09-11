targetScope = 'resourceGroup'

@minLength(4)
@maxLength(21)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

param healthModelsLocation string = 'swedencentral'

var logSearchBasicTableName = 'McpBasic_CL'
var logSearchAuxiliaryTableName = 'McpAuxiliary_CL'
var logSearchAnalyticsTableName = 'McpAnalytics_CL'
var logSearchDcrName = '${baseName}-log-search-dcr'
var logSearchDestinationName = 'log-search-workspace'
var logSearchColumns = [
  {
    name: 'TimeGenerated'
    type: 'dateTime'
  }
  {
    name: 'FixtureId'
    type: 'string'
  }
  {
    name: 'Message'
    type: 'string'
  }
  {
    name: 'Count'
    type: 'long'
  }
  {
    name: 'Enabled'
    type: 'boolean'
  }
  {
    name: 'OptionalValue'
    type: 'string'
  }
]
var logSearchStreamColumns = [
  for column in logSearchColumns: {
    name: column.name
    type: column.type == 'dateTime' ? 'datetime' : column.type
  }
]

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: baseName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
    features: {
      searchVersion: 1
      workspaceCapping: 'Off'
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource logSearchBasicTable 'Microsoft.OperationalInsights/workspaces/tables@2025-07-01' = {
  name: logSearchBasicTableName
  parent: workspace
  properties: {
    plan: 'Basic'
    schema: {
      name: logSearchBasicTableName
      columns: logSearchColumns
    }
  }
}

resource logSearchAuxiliaryTable 'Microsoft.OperationalInsights/workspaces/tables@2025-07-01' = {
  name: logSearchAuxiliaryTableName
  parent: workspace
  properties: {
    plan: 'Auxiliary'
    schema: {
      name: logSearchAuxiliaryTableName
      columns: logSearchColumns
    }
  }
}

resource logSearchAnalyticsTable 'Microsoft.OperationalInsights/workspaces/tables@2025-07-01' = {
  name: logSearchAnalyticsTableName
  parent: workspace
  properties: {
    plan: 'Analytics'
    schema: {
      name: logSearchAnalyticsTableName
      columns: logSearchColumns
    }
  }
}

resource logSearchDataCollectionRule 'Microsoft.Insights/dataCollectionRules@2024-03-11' = {
  name: logSearchDcrName
  location: location
  kind: 'Direct'
  properties: {
    streamDeclarations: {
      'Custom-${logSearchBasicTableName}': {
        columns: logSearchStreamColumns
      }
      'Custom-${logSearchAuxiliaryTableName}': {
        columns: logSearchStreamColumns
      }
    }
    destinations: {
      logAnalytics: [
        {
          name: logSearchDestinationName
          workspaceResourceId: workspace.id
        }
      ]
    }
    dataFlows: [
      {
        streams: [
          'Custom-${logSearchBasicTableName}'
        ]
        destinations: [
          logSearchDestinationName
        ]
        transformKql: 'source'
        outputStream: 'Custom-${logSearchBasicTableName}'
      }
      {
        streams: [
          'Custom-${logSearchAuxiliaryTableName}'
        ]
        destinations: [
          logSearchDestinationName
        ]
        transformKql: 'source'
        outputStream: 'Custom-${logSearchAuxiliaryTableName}'
      }
    ]
  }
  dependsOn: [
    logSearchBasicTable
    logSearchAuxiliaryTable
  ]
}

// Generic test infrastructure provides one service principal. The multi-identity OBO denial matrix
// remains a dedicated external validation.
resource logAnalyticsReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '73c42c96-874c-492b-b04d-ab87d138a893'
}

resource logSearchReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(workspace.id, testApplicationOid, logAnalyticsReaderRoleDefinition.id)
  scope: workspace
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: logAnalyticsReaderRoleDefinition.id
    description: 'Log Analytics Reader for Monitor live tests'
  }
}

resource monitoringMetricsPublisherRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '3913510d-42f4-4e42-8a64-420c390055eb'
}

resource logSearchIngestionRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(logSearchDataCollectionRule.id, testApplicationOid, monitoringMetricsPublisherRoleDefinition.id)
  scope: logSearchDataCollectionRule
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: monitoringMetricsPublisherRoleDefinition.id
    description: 'Monitoring Metrics Publisher for Monitor live-test fixtures'
  }
}

// Create a storage account to monitor
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}mon'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    resource fooContainer 'containers' = { name: 'foo' }
    resource barContainer 'containers' = { name: 'bar' }
    resource bazContainer 'containers' = { name: 'baz' }
  }

  resource tableServices 'tableServices' = {
    name: 'default'
    resource fooTable 'tables' = { name: 'foo' }
    resource barTable 'tables' = { name: 'bar' }
    resource bazTable 'tables' = { name: 'baz' }
  }
}

// Diagnostic settings for Storage Account (main account level)
resource storageAccountDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'storage-account-diagnostics'
  scope: storageAccount
  properties: {
    workspaceId: workspace.id
    metrics: [
      {
        category: 'Transaction'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'Capacity'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Diagnostic settings for Blob Service
resource blobServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'blob-service-diagnostics'
  scope: storageAccount::blobServices
  properties: {
    workspaceId: workspace.id
    logs: [
      {
        category: 'StorageRead'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'StorageWrite'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'StorageDelete'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'Transaction'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'Capacity'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Diagnostic settings for Table Service
resource tableServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'table-service-diagnostics'
  scope: storageAccount::tableServices
  properties: {
    workspaceId: workspace.id
    logs: [
      {
        category: 'StorageRead'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'StorageWrite'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'StorageDelete'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'Transaction'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'Capacity'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Role assignment for the test application to access the storage account
resource blobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Contributor role.
  // Read, write, and delete Azure Storage containers and blobs
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource appBlobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' =  {
  name: guid(blobContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: blobContributorRoleDefinition.id
    description: 'Blob Contributor for testApplicationOid'
  }
}

// Application Insights and Availability Tests module
module webTestsModule 'test-resources.webtests.module.bicep' = {
  name: 'webtests-module'
  params: {
    baseName: baseName
    location: location
    workspaceId: workspace.id
    testApplicationOid: testApplicationOid
  }
}

// Azure Monitor Health Models (Microsoft.CloudHealth)
module healthModelsModule 'test-resources.healthmodels.module.bicep' = {
  name: 'healthmodels-module'
  params: {
    baseName: baseName
    location: healthModelsLocation
  }
}

output healthModelParentName string = healthModelsModule.outputs.healthModelAName
output healthModelChildName string = healthModelsModule.outputs.healthModelBName
output logSearchWorkspaceName string = workspace.name
output logSearchWorkspaceCustomerId string = workspace.properties.customerId
output logSearchBasicTableName string = logSearchBasicTable.name
output logSearchAuxiliaryTableName string = logSearchAuxiliaryTable.name
output logSearchAnalyticsTableName string = logSearchAnalyticsTable.name
output logSearchAuxiliaryLastPlanModifiedDate string = logSearchAuxiliaryTable.properties.lastPlanModifiedDate
output logSearchDcrImmutableId string = logSearchDataCollectionRule.properties.immutableId
output logSearchIngestionEndpoint string = logSearchDataCollectionRule.properties.endpoints.logsIngestion
