targetScope = 'resourceGroup'

@description('Base resource name for the PostgreSQL server.')
@minLength(3)
@maxLength(60)
param baseName string = toLower(replace(replace(replace(replace(resourceGroup().name, '-', ''), '_', ''), '.', ''), ' ', ''))

@description('Location for the PostgreSQL resources.')
param location string = 'westus3'

@description('Tenant ID used for Entra ID authentication.')
param tenantId string = tenant().tenantId

@description('Client (application) ID for the test principal that will administer the PostgreSQL server.')
param testApplicationId string

@description('Object ID for the test principal that will administer the PostgreSQL server.')
param testApplicationOid string

@description('Temporary administrator password. Local authentication remains disabled after deployment.')
@secure()
param administratorPassword string = newGuid()

var sanitizedBaseName = toLower(replace(replace(replace(replace(baseName, '-', ''), '_', ''), '.', ''), ' ', ''))
var fallbackName = 'pg${substring(uniqueString(resourceGroup().id), 0, 6)}'
var normalizedBaseName = empty(sanitizedBaseName) ? fallbackName : (startsWith(sanitizedBaseName, 'pg') ? sanitizedBaseName : 'pg${sanitizedBaseName}')
var trimmedLength = length(normalizedBaseName) > 60 ? 60 : length(normalizedBaseName)
var serverName = substring(normalizedBaseName, 0, trimmedLength)
var isServicePrincipal = !empty(testApplicationId) && toLower(testApplicationId) != toLower(testApplicationOid)
var adminPrincipalName = isServicePrincipal ? toLower(testApplicationId) : testApplicationOid
var adminPrincipalType = isServicePrincipal ? 'ServicePrincipal' : 'User'
var databaseName = 'sampledb'
var tableName = 'inventory'

resource server 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: serverName
  location: location
  sku: {
    name: 'Standard_D2s_v3'
    tier: 'GeneralPurpose'
    capacity: 2
  }
  properties: {
    version: '15'
    createMode: 'Default'
    administratorLogin: 'pgadmin'
    administratorLoginPassword: administratorPassword
    network: {
      publicNetworkAccess: 'Enabled'
    }
    storage: {
      storageSizeGB: 64
      autoGrow: 'Enabled'
      iops: 180
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    maintenanceWindow: {
      dayOfWeek: 0
      startHour: 0
      startMinute: 0
    }
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled'
      tenantId: tenantId
    }
  }
}

resource allowAzureIps 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = {
  name: '${server.name}/AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  name: '${server.name}/${databaseName}'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource aadAdministrator 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2022-12-01' = if (!empty(testApplicationOid)) {
  name: '${server.name}/activeDirectory'
  properties: {
    principalName: adminPrincipalName
    principalId: testApplicationOid
    principalType: adminPrincipalType
    tenantId: tenantId
  }
}

output POSTGRES_SERVER_NAME string = server.name
output POSTGRES_SERVER_FQDN string = server.properties.fullyQualifiedDomainName
output POSTGRES_DATABASE_NAME string = databaseName
output POSTGRES_AAD_PRINCIPAL string = adminPrincipalName
output POSTGRES_TABLE_NAME string = tableName
