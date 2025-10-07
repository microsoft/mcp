targetScope = 'resourceGroup'

@minLength(3)
@maxLength(63)
@description('The base resource name. PostgreSQL Flexible Server names have a max length restriction.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'westus3'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('The application (client) ID for the service principal')
param testApplicationId string

@description('PostgreSQL version.')
param postgresqlVersion string = '16'

// PostgreSQL Flexible Server resource
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: postgresqlVersion
    administratorLogin: testApplicationId
    administratorLoginPassword: null // Use Entra ID authentication only
    storage: {
      storageSizeGB: 32
      autoGrow: 'Disabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    availabilityZone: '1'
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled' // Disable local auth, only use Entra ID
      tenantId: subscription().tenantId
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }

  // Firewall rule to allow Azure services
  resource firewallRuleAzure 'firewallRules@2023-12-01-preview' = {
    name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  // Firewall rule to allow all IPs for testing (remove in production)
  resource firewallRuleAll 'firewallRules@2023-12-01-preview' = {
    name: 'AllowAllIPs'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '255.255.255.255'
    }
  }

  // Test database
  resource testDatabase 'databases@2023-12-01-preview' = {
    name: 'testdb'
    properties: {
      charset: 'UTF8'
      collation: 'en_US.utf8'
    }
  }

  // Additional test database
  resource testDatabase2 'databases@2023-12-01-preview' = {
    name: 'testdb2'
    properties: {
      charset: 'UTF8'
      collation: 'en_US.utf8'
    }
  }
}

// Entra ID administrator configuration
resource postgresAdmin 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2023-12-01-preview' = {
  parent: postgresServer
  name: testApplicationOid
  properties: {
    principalName: testApplicationId
    principalType: 'ServicePrincipal'
    tenantId: subscription().tenantId
  }
}

// Reader role definition for PostgreSQL
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Reader role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#reader
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

// Role assignment for test application
resource appReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, postgresServer.id)
  scope: postgresServer
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
    description: 'Reader role for testApplicationOid to read PostgreSQL server metadata'
  }
}

// Output values for tests
output postgresServerName string = postgresServer.name
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output testDatabaseName string = postgresServer::testDatabase.name
output testDatabase2Name string = postgresServer::testDatabase2.name
output adminUsername string = testApplicationId
output location string = location
