targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'southeastasia'

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource migrateProject 'Microsoft.Migrate/migrateProjects@2020-06-01-preview' = {
  name: baseName
  location: location
  tags: {
    environment: 'test'
    purpose: 'mcp-livetests'
  }
  properties: {}
}

resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Contributor role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#contributor
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource appContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(contributorRoleDefinition.id, testApplicationOid, migrateProject.id)
  scope: migrateProject
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: contributorRoleDefinition.id
    description: 'Contributor for testApplicationOid'
  }
}

output AZURE_MIGRATE_PROJECT_NAME string = migrateProject.name
output AZURE_MIGRATE_PROJECT_ID string = migrateProject.id
