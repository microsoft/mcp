targetScope = 'resourceGroup'

@minLength(5)
@maxLength(50)
@description('The base resource name. ACR names must be 5-50 characters and globally unique.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

// The test infrastructure will only provide baseName and testApplicationOid.
// Any additional parameters are for local deployments only and require default values.

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The SKU of the container registry. Use Basic for cost-effective testing.')
param acrSku string = 'Basic'

// Main ACR resource
resource acrRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: baseName
  location: location
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
  }
}

// Role assignment for test application - ACR Pull role
resource acrPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // AcrPull role - allows pulling images from the registry
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource appAcrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acrPullRoleDefinition.id, testApplicationOid, acrRegistry.id)
  scope: acrRegistry
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: acrPullRoleDefinition.id
    description: 'AcrPull role for testApplicationOid'
  }
}

// Role assignment for test application - ACR Push role (includes pull)
resource acrPushRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // AcrPush role - allows pushing and pulling images
  name: '8311e382-0749-4cb8-b61a-304f252e45ec'
}

resource appAcrPushRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acrPushRoleDefinition.id, testApplicationOid, acrRegistry.id)
  scope: acrRegistry
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: acrPushRoleDefinition.id
    description: 'AcrPush role for testApplicationOid'
  }
}

// Reader role ensures subscription-level list of registries returns this registry for the principal
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Reader role
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

resource appReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, acrRegistry.id)
  scope: acrRegistry
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
    description: 'Reader role for testApplicationOid to allow listing'
  }
}

// Outputs for test consumption
output acrRegistryName string = acrRegistry.name
output acrLoginServer string = acrRegistry.properties.loginServer
output acrResourceId string = acrRegistry.id
