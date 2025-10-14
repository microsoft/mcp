@minLength(3)
@maxLength(17)
@description('The base resource name. PostgreSQL Server names have a max length restriction.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'northeurope' // resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string = '26ffb325-f480-419c-b7a9-2c8a018203a8' // azure-sdk-internal-devops-connections

var testDbName string = 'testdb'

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2025-06-01-preview' = {
  name: '${baseName}-postgres'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '15'
    storage: {
      storageSizeGB: 32
      iops: 120
      tier: 'P4'
    }
    backup: {
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled'  // S360 compliant
      tenantId: tenant().tenantId
    }
  }

  resource firewallAzure 'firewallRules' = {
    name: 'allow-all-azure-internal-IPs'
    properties: {
        startIpAddress: '0.0.0.0'
        endIpAddress: '0.0.0.0'
    }
  }

  resource firewallSingle 'firewallRules' = {
    name: 'allow-all'
    properties: {
        startIpAddress: '0.0.0.0'
        endIpAddress: '255.255.255.255'
    }
  }

  resource postgresAdministrator 'administrators' = {
    name: testApplicationOid
    properties: {
      principalType: testApplicationOid == '26ffb325-f480-419c-b7a9-2c8a018203a8' ? 'ServicePrincipal' : 'User'
      principalName: deployer().userPrincipalName
      tenantId: tenant().tenantId
    }
    dependsOn: [
      firewallAzure
      firewallSingle
    ]
  }

  resource testDatabase 'databases' = {
    name: testDbName
    properties: {
      charset: 'utf8'
      collation: 'en_US.utf8'
    }
  }
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
output testDatabaseName string = testDbName
output entraIdAdminObjectId string = testApplicationOid
output adminLogin string = testApplicationOid
