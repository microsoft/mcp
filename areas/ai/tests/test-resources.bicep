targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Service names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Azure OpenAI service
resource openaiService 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: baseName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'OpenAI'
  properties: {
    customSubDomainName: baseName
    publicNetworkAccess: 'Enabled'
  }
}

// Test deployment - GPT-3.5 Turbo
resource testDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openaiService
  name: 'test-gpt-35-turbo'
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'
    }
    raiPolicyName: 'Microsoft.Default'
  }
  sku: {
    name: 'Standard'
    capacity: 10
  }
}

// Cognitive Services OpenAI Contributor role definition
resource openaiContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Cognitive Services OpenAI Contributor role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#cognitive-services-openai-contributor
  name: 'a001fd3d-188f-4b5d-821b-7da978bf7442'
}

// Assign Cognitive Services OpenAI Contributor role to testApplicationOid
resource testApp_openai_contributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(openaiContributorRoleDefinition.id, testApplicationOid, openaiService.id)
  scope: openaiService
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: openaiContributorRoleDefinition.id
  }
}

// Outputs for test consumption
output openaiServiceName string = openaiService.name
output testDeploymentName string = testDeployment.name
output endpoint string = openaiService.properties.endpoint
