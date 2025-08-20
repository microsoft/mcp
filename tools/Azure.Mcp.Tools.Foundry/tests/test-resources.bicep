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
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
    }
  }
}
