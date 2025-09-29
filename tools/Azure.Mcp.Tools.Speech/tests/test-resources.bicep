targetScope = 'resourceGroup'

@minLength(3)
@maxLength(64)
@description('The base resource name. Azure AI Services names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Azure AI Services resource (multi-service account that includes Speech)
resource aiServices 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: baseName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'AIServices'
  properties: {
    customSubDomainName: baseName
    networkAcls: {
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
}

// Role assignment for test application - Cognitive Services User
resource aiServicesRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'a97b65f3-24c7-4388-baec-2e87135dc908' // Cognitive Services User
}

resource appAiServicesRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aiServicesRoleDefinition.id, testApplicationOid, aiServices.id)
  scope: aiServices
  properties: {
    roleDefinitionId: aiServicesRoleDefinition.id
    principalId: testApplicationOid
    description: 'Cognitive Services User for testApplicationOid'
  }
}

// Outputs for test consumption
output aiServicesName string = aiServices.name
output aiServicesEndpoint string = aiServices.properties.endpoint // Will be https://{baseName}.cognitiveservices.azure.com/
output aiServicesLocation string = aiServices.location
