targetScope = 'resourceGroup'

@minLength(3)
@maxLength(50)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'westus' == resourceGroup().location ? 'westus2' : resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

var cosmosContributorRoleId = '00000000-0000-0000-0000-000000000002' // Built-in Contributor role

// Hybrid OpenAI provisioning: in the TME tenant, reference a pre-existing static
// OpenAI account; otherwise, deploy and tear down a per-run account alongside the
// rest of the Cosmos test infrastructure.
var staticSuffix = toLower(substring(subscription().subscriptionId, 0, 4))
var staticBaseName = 'mcp${staticSuffix}'
var staticResourceGroupName = 'mcp-static-${staticSuffix}'
var isTmeTenant = tenantId == '70a036f6-8e4d-4615-bad6-149c02e7720d'
var embeddingDeploymentName = 'embedding-model'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: baseName
  location: location
  tags: {
    defaultExperience: 'Core (SQL)'
    CosmosAccountType: 'Non-Production'
  }
  kind: 'GlobalDocumentDB'
  identity: {
    type: 'None'
  }
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
    enablePerRegionPerPartitionAutoscale: false
    enableBurstCapacity: false
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
    cors: []
    capabilities: [
      {
        name: 'EnableNoSQLVectorSearch'
      }
      {
        name: 'EnableNoSQLFullTextSearch'
      }
    ]
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
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-11-15' = {
  parent: cosmosAccount
  name: 'ToDoList'
  properties: {
    resource: {
      id: 'ToDoList'
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: cosmosDatabase
  name: 'Items'
  properties: {
    resource: {
      id: 'Items'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      uniqueKeyPolicy: {
        uniqueKeys: []
      }
      conflictResolutionPolicy: {
        mode: 'LastWriterWins'
        conflictResolutionPath: '/_ts'
      }
    }
  }
}

// Container with a full-text policy + index for exercising the
// `cosmos_database_container_item_text-search` tool.
resource cosmosTextContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-12-01-preview' = {
  parent: cosmosDatabase
  name: 'TextItems'
  properties: {
    resource: {
      id: 'TextItems'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
        fullTextIndexes: [
          {
            path: '/description'
          }
        ]
      }
      fullTextPolicy: {
        defaultLanguage: 'en-US'
        fullTextPaths: [
          {
            path: '/description'
            language: 'en-US'
          }
        ]
      }
    }
  }
}

// Container with a vector embedding policy + index for exercising the
// `cosmos_database_container_item_vector-search` tool. Uses 1536-dimensional
// Float32 vectors to match the output of Azure OpenAI `text-embedding-3-small`.
resource cosmosVectorContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-12-01-preview' = {
  parent: cosmosDatabase
  name: 'VectorItems'
  properties: {
    resource: {
      id: 'VectorItems'
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
          {
            path: '/vector/*'
          }
        ]
        vectorIndexes: [
          {
            path: '/vector'
            type: 'quantizedFlat'
          }
        ]
      }
      vectorEmbeddingPolicy: {
        vectorEmbeddings: [
          {
            path: '/vector'
            dataType: 'float32'
            distanceFunction: 'cosine'
            dimensions: 1536
          }
        ]
      }
    }
  }
}

// Azure OpenAI account deployed only outside the TME tenant. In TME we reuse
// the static account named `mcp<suffix>` from the `mcp-static-<suffix>` RG.
resource openai 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (!isTmeTenant) {
  name: toLower(baseName)
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: toLower(baseName)
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }

  resource openaiDeployment 'deployments' = {
    name: embeddingDeploymentName
    sku: {
      name: 'Standard'
      capacity: 50
    }
    properties: {
      model: {
        format: 'OpenAI'
        name: 'text-embedding-3-small'
      }
    }
  }
}

resource staticOpenai 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = if (isTmeTenant) {
  name: staticBaseName
  scope: resourceGroup(staticResourceGroupName)
}

// Cognitive Services OpenAI User role: grants data-plane access to invoke the
// embedding deployment. Only assigned when we deployed the OpenAI account; the
// static TME account is expected to grant access out of band.
resource openaiUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Cognitive Services OpenAI User
  // https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#cognitive-services-openai-user
  name: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
}

resource testApp_openai_roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!isTmeTenant) {
  name: guid(openaiUserRoleDefinition.id, testApplicationOid, openai.id)
  scope: openai
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: openaiUserRoleDefinition.id
  }
}

output openAIEndpoint string = isTmeTenant ? staticOpenai!.properties.endpoint : openai!.properties.endpoint
output embeddingDeploymentName string = embeddingDeploymentName

// Assign CosmosDB Contributor role for the Web App on the Cosmos Account
resource sqlRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-11-15' = {
  name: guid(cosmosContributorRoleId, testApplicationOid, cosmosAccount.id)
  parent: cosmosAccount
  properties:{
    principalId: testApplicationOid
    roleDefinitionId: '${resourceGroup().id}/providers/Microsoft.DocumentDB/databaseAccounts/${cosmosAccount.name}/sqlRoleDefinitions/${cosmosContributorRoleId}'
    scope: cosmosAccount.id
  }
}
