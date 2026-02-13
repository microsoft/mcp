targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'southeastasia'

// Azure Migrate project for testing
resource migrateProject 'Microsoft.Migrate/migrateProjects@2020-06-01-preview' = {
  name: baseName
  location: location
  tags: {
    environment: 'test'
    purpose: 'mcp-livetests'
  }
  properties: {}
}

output AZURE_MIGRATE_PROJECT_NAME string = migrateProject.name
output AZURE_MIGRATE_PROJECT_ID string = migrateProject.id
