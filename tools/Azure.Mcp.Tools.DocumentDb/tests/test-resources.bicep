targetScope = 'resourceGroup'

@minLength(3)
@maxLength(40)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'westus' == resourceGroup().location ? 'westus2' : resourceGroup().location

@description('Enable an additional public firewall rule for local development. Leave disabled for CI and shared test environments.')
param enablePublicIpRule bool = false

@description('Start IP address for the optional public firewall rule used for local development.')
param allowedStartIpAddress string = '0.0.0.0'

@description('End IP address for the optional public firewall rule used for local development.')
param allowedEndIpAddress string = '255.255.255.255'

var administratorLogin = 'testadmin'
// Use a password without special characters that need URL encoding (! and @ cause issues)
var administratorLoginPassword = 'Pass${uniqueString(resourceGroup().id)}0rd'

// Azure DocumentDB (with MongoDB compatibility) cluster
resource documentDbAccount 'Microsoft.DocumentDB/mongoClusters@2024-03-01-preview' = {
  name: '${take(baseName, 30)}-ddb'
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    serverVersion: '5.0'
    nodeGroupSpecs: [
      {
        kind: 'Shard'
        sku: 'M30'
        diskSizeGB: 128
        enableHa: false
        nodeCount: 1
      }
    ]
    publicNetworkAccess: 'Enabled'
  }
}

// Allow access from Azure services (enables test proxy and Azure pipelines)
resource allowAzureServices 'Microsoft.DocumentDB/mongoClusters/firewallRules@2024-03-01-preview' = {
  parent: documentDbAccount
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Optional public IP rule for local development.
// Keep disabled for CI/shared environments and prefer a narrow caller IP range when enabled.
resource allowPublicIpRange 'Microsoft.DocumentDB/mongoClusters/firewallRules@2024-03-01-preview' = if (enablePublicIpRule) {
  parent: documentDbAccount
  name: 'AllowPublicIpRange'
  properties: {
    startIpAddress: allowedStartIpAddress
    endIpAddress: allowedEndIpAddress
  }
}

// Output the connection string (will be sanitized in tests)
// The connectionString property returns a template like: mongodb+srv://<user>:<password>@host...
// We need to replace <user> and <password> with actual credentials
output DOCUMENTDB_ENDPOINT string = documentDbAccount.properties.connectionString
output DOCUMENTDB_CONNECTION_STRING string = replace(replace(documentDbAccount.properties.connectionString, '<user>', administratorLogin), '<password>', administratorLoginPassword)