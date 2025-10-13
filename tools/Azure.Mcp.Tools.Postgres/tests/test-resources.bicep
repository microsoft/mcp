targetScope = 'resourceGroup'

// Deployment dependency chain:
// 1. PostgreSQL Server creation
// 2. Firewall rules configuration
// 3. Entra ID administrator setup (requires server to be accessible)
// 4. Database creation (best practice: after Entra ID config when using AD auth)
// 5. Role assignments

@minLength(3)
@maxLength(17)
@description('The base resource name. PostgreSQL Server names have a max length restriction.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'westus3'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('PostgreSQL Server administrator login name.')
param postgresAdminLogin string = 'mcptestadmin'

@description('PostgreSQL Server administrator password.')
@secure()
param postgresAdminPassword string = newGuid()

// PostgreSQL Flexible Server resource
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: '${baseName}-postgres'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    version: '15'
    storage: {
      storageSizeGB: 32
      iops: 120
      tier: 'P4'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'  // Keep both enabled during testing for flexibility
      tenantId: tenant().tenantId
    }
  }
}

// Configure Entra ID administrator for PostgreSQL server
// Must wait for server to be fully provisioned and firewall rules to be configured
resource postgresAdministrator 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2023-12-01-preview' = {
  parent: postgresServer
  name: testApplicationOid
  properties: {
    principalType: 'ServicePrincipal'
    principalName: testApplicationOid
    tenantId: tenant().tenantId
  }
  dependsOn: [
    allowAllAzureServicesRule
    allowAllIpsRule
  ]
}

// Firewall rule to allow all Azure services
resource allowAllAzureServicesRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Firewall rule to allow all IPs (for testing purposes)
resource allowAllIpsRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAllIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Test database
// Depends on Entra ID administrator being configured first (best practice for Entra ID deployments)
resource testDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: 'testdb'
  properties: {
    charset: 'utf8'
    collation: 'en_US.utf8'
  }
  dependsOn: [
    postgresAdministrator
  ]
}

// PostgreSQL Contributor role definition
resource postgresContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the PostgreSQL Contributor role
  // Lets you manage PostgreSQL servers, but not access to them
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#postgresql-contributor
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

// Role assignment for test application
resource appPostgresRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(postgresContributorRoleDefinition.id, testApplicationOid, postgresServer.id)
  scope: postgresServer
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: postgresContributorRoleDefinition.id
    description: 'PostgreSQL Contributor for testApplicationOid'
  }
}

// Output values for tests
output postgresServerName string = postgresServer.name
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output testDatabaseName string = testDatabase.name
output adminLogin string = postgresAdminLogin
output entraIdAdminObjectId string = testApplicationOid
