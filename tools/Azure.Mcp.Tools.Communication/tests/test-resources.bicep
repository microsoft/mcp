targetScope = 'resourceGroup'

@minLength(3)
@maxLength(63)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('The data location for the Communication Services resource.')
param dataLocation string = 'United States'

resource communicationService 'Microsoft.Communication/communicationServices@2020-08-20' = {
  name: baseName
  location: 'global'
  properties: {
    dataLocation: dataLocation
  }
}

// Communication Services supports both connection string and credential-based authentication
// For MCP tools, we use credential-based authentication with endpoint URL
// The endpoint URL and phone numbers will be provided via outputs for live tests

output COMMUNICATION_SERVICES_ENDPOINT string = 'https://${communicationService.properties.hostName}'
output COMMUNICATION_SERVICES_RESOURCE_NAME string = communicationService.name
