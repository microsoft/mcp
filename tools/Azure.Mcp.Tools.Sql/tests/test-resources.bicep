targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base resource name. SQL Server names have a max length restriction.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location == 'westus' ? 'westus2' : resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('SQL Server administrator login name.')
param sqlAdminLogin string = 'mcptestadmin'

@description('SQL Server administrator password.')
@secure()
param sqlAdminPassword string = newGuid()

// SQL Server resource
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: baseName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }

  // Test elastic pool
  resource testElasticPool 'elasticPools@2023-05-01-preview' = {
    name: 'testpool'
    location: location
    sku: {
      name: 'BasicPool'
      tier: 'Basic'
      capacity: 50
    }
    properties: {
      perDatabaseSettings: {
        minCapacity: 0
        maxCapacity: 5
      }
      zoneRedundant: false
    }
  }

  // Test database
  resource testDatabase 'databases@2023-05-01-preview' = {
    name: 'testdb'
    location: location
    sku: {
      name: 'Basic'
      tier: 'Basic'
      capacity: 5
    }
    properties: {
      collation: 'SQL_Latin1_General_CP1_CI_AS'
      maxSizeBytes: 2147483648 // 2 GB
      catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
      zoneRedundant: false
      readScale: 'Disabled'
      requestedBackupStorageRedundancy: 'Local'
    }
  }
}

// SQL DB Contributor role definition
resource sqlDbContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the SQL DB Contributor role
  // Lets you manage SQL databases, but not access to them
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#sql-db-contributor
  name: '9b7fa17d-e63e-47b0-bb0a-15c516ac86ec'
}

// Role assignment for test application
resource appSqlRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sqlDbContributorRoleDefinition.id, testApplicationOid, sqlServer.id)
  scope: sqlServer
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: sqlDbContributorRoleDefinition.id
    description: 'SQL DB Contributor for testApplicationOid'
  }
}

// Output values for tests
output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output testDatabaseName string = sqlServer::testDatabase.name
output testElasticPoolName string = sqlServer::testElasticPool.name
