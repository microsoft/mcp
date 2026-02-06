targetScope = 'resourceGroup'

@minLength(3)
@maxLength(40)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = 'westus' == resourceGroup().location ? 'westus2' : resourceGroup().location

var administratorLogin = 'testadmin'
// Use a password without special characters that need URL encoding (! and @ cause issues)
var administratorLoginPassword = 'Pass${uniqueString(resourceGroup().id)}0rd'

// DocumentDB (Azure Cosmos DB for MongoDB vCore) account
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

// Allow access from anywhere (for development/testing)
// Note: This is insecure. In production, restrict to specific IPs
resource allowAllIPs 'Microsoft.DocumentDB/mongoClusters/firewallRules@2024-03-01-preview' = {
  parent: documentDbAccount
  name: 'AllowAllIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Output the connection string (will be sanitized in tests)
// The connectionString property returns a template like: mongodb+srv://<user>:<password>@host...
// We need to replace <user> and <password> with actual credentials
output DOCUMENTDB_ENDPOINT string = documentDbAccount.properties.connectionString
output DOCUMENTDB_CONNECTION_STRING string = replace(replace(documentDbAccount.properties.connectionString, '<user>', administratorLogin), '<password>', administratorLoginPassword)
