targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Static resource names for consistent testing
var staticOpenAIAccount = 'azmcp-test'
var staticOpenAIDeploymentName = 'gpt-4o-mini'
var staticOpenAIAccountResourceGroup = 'static-test-resources'
var staticEmbeddingModel = 'embedding-model'

var cognitiveServicesContributorRoleId = '25fbc0a9-bd7c-42a3-aa1a-3b75d497ee68' // Cognitive Services Contributor role

resource aiServicesAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: baseName
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  properties: {
    isAiFoundryType: true
    customSubDomainName: baseName
    dynamicThrottlingEnabled: false
    networkAcls: {
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
    allowProjectManagement: true
    encryption: {
      keySource: 'Microsoft.CognitiveServices'
    }
  }
}

resource contributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cognitiveServicesContributorRoleId, testApplicationOid, aiServicesAccount.id)
  scope: aiServicesAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesContributorRoleId)
    principalId: testApplicationOid
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}foundry'
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
    resource foundryContainer 'containers' = { name: 'foundry' }
  }
}

resource aiProjects 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  parent: aiServicesAccount
  name: '${baseName}-ai-projects'
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: '${baseName}-ai-projects'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
  }
  sku: {
    name: 'S0'
  }
}

resource aiProjectsRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cognitiveServicesContributorRoleId, testApplicationOid, aiProjects.id)
  scope: aiProjects
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesContributorRoleId)
    principalId: testApplicationOid
  }
}

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = {
  parent: aiServicesAccount
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 30
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
    }
  }
}

resource bingGroundingSearch 'Microsoft.Bing/accounts@2020-06-10' = {
  name: '${baseName}-bing-grounding'
  location: 'Global'
  sku: {
    name: 'G1'
  }
  kind: 'Bing.Grounding'
}

resource bingGroundingConnection 'Microsoft.CognitiveServices/accounts/projects/connections@2025-04-01-preview' = {
  parent: aiProjects
  name: '${baseName}-bing-connection'
  properties: {
    authType: 'ApiKey'
    metadata: {
      type: 'bing_grounding'
      ApiType: 'Azure'
      ResourceId: bingGroundingSearch.id
    }
    target: 'https://api.bing.microsoft.com/'
    category: 'GroundingWithBingSearch'
    group: 'AzureAI'
    credentials: {
      key: bingGroundingSearch.listKeys().key1
    }
  }
}

resource searchService 'Microsoft.Search/searchServices@2023-11-01' = {
  name: '${baseName}-search'
  location: location
  sku: {
    name: 'basic'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    networkRuleSet: {
      ipRules: []
    }
    encryptionWithCmk: {
      enforcement: 'Unspecified'
    }
    disableLocalAuth: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource searchServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('8ebe5a00-799e-43f5-93ac-243d3dce84a7', testApplicationOid, searchService.id) // Search Index Data Contributor role
  scope: searchService
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '8ebe5a00-799e-43f5-93ac-243d3dce84a7')
    principalId: testApplicationOid
  }
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-deployment-identity'
  location: location
}

resource managedIdentityRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('7ca78c08-252a-4471-8644-bb5ff32d4ba0', managedIdentity.id, searchService.id) // Search Service Contributor role
  scope: searchService
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7ca78c08-252a-4471-8644-bb5ff32d4ba0')
    principalId: managedIdentity.properties.principalId
  }
}

output searchServiceName string = searchService.name
output searchServiceEndpoint string = 'https://${searchService.name}.search.windows.net'
output knowledgeIndexName string = '${baseName}-knowledge-index'
output aiProjectsEndpoint string = 'https://${aiServicesAccount.name}.services.ai.azure.com/api/projects/${aiProjects.name}'

// Static resource outputs for test configuration
output openAIAccount string = staticOpenAIAccount
output openAIDeploymentName string = staticOpenAIDeploymentName
output openAIAccountResourceGroup string = staticOpenAIAccountResourceGroup
output embeddingDeploymentName string = staticEmbeddingModel
