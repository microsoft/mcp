@minLength(3)
@maxLength(17)
@description('The base resource name. PostgreSQL Server names have a max length restriction.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location // resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string = '26ffb325-f480-419c-b7a9-2c8a018203a8' // azure-sdk-internal-devops-connections

var testDbName string = 'testdb'

// The deploying identity is also the test identity in CI and local runs. Service
// principals (CI, including sovereign clouds like usgovvirginia) only expose
// objectId/tenantId, while interactive user principals also expose userPrincipalName.
// Safe-dereference (.?) lets us detect which kind of principal is deploying without
// failing template evaluation when userPrincipalName is absent.
var knownDevOpsServicePrincipalOid = '26ffb325-f480-419c-b7a9-2c8a018203a8'
var deployerUserPrincipalName = deployer().?userPrincipalName
var isServicePrincipal = testApplicationOid == knownDevOpsServicePrincipalOid || deployerUserPrincipalName == null

// The PostgreSQL Entra admin login must match the username the test connects with, which is
// the deploying principal's name (app display name for service principals, UPN for users).
// deployer() exposes a display name for neither, so map the known CI service principals to
// their display names. Sovereign-cloud CI (e.g. usgovvirginia) uses a dedicated service
// principal whose objectId is not known ahead of time, so it is matched by cloud instead.
var isUSGovernment = environment().name == 'AzureUSGovernment'
var entraAdminLogin = testApplicationOid == knownDevOpsServicePrincipalOid
  ? 'azure-sdk-internal-devops-connections'
  : (deployerUserPrincipalName ?? (isServicePrincipal && isUSGovernment ? 'azure-mcp-gov-test' : testApplicationOid))

// PostgreSQL Flexible Server provisioning is offer/capacity-restricted for the test
// subscription in usgovvirginia (LocationIsOfferRestricted). Azure recommends retrying in a
// different location, so deploy the server to an alternate Azure US Government region where
// the offer is available. Public-cloud deployments keep using the resource group's location.
var postgresLocation = environment().name == 'AzureUSGovernment' ? 'usgovarizona' : location

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2025-06-01-preview' = {
  name: '${baseName}-postgres'
  location: postgresLocation
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '17'
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
      principalType: isServicePrincipal ? 'ServicePrincipal' : 'User'
      principalName: entraAdminLogin
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
output adminLogin string = entraAdminLogin
