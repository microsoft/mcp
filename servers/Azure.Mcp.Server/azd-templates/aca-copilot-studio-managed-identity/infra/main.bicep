@description('Location for all resources')
param location string = resourceGroup().location

@description('Name for the Azure Container App')
param acaName string

@description('Display name for the Server Entra App')
param entraAppServerDisplayName string

@description('Display name for the Client Entra App')
param entraAppClientDisplayName string

@description('Application Insights connection string. Use "DISABLED" to disable telemetry, or provide existing connection string. If omitted, new App Insights will be created.')
param appInsightsConnectionString string = ''

// Deploy Application Insights if appInsightsConnectionString is empty and not DISABLED
var appInsightsName = '${acaName}-insights'
//
module appInsights 'modules/application-insights.bicep' = {
  name: 'application-insights-deployment'
  params: {
    appInsightsConnectionString: appInsightsConnectionString
    name: appInsightsName
    location: location
  }
}

// Deploy Entra App
var entraAppClientUniqueName = '${replace(toLower(entraAppClientDisplayName), ' ', '-')}-${uniqueString(resourceGroup().id)}'
module entraAppClient 'modules/entra-app.bicep' = {
  name: 'entra-app-client-deployment'
  params: {
    entraAppDisplayName: entraAppClientDisplayName
    entraAppUniqueName: entraAppClientUniqueName
    isServer: false
  }
}

var entraAppServerUniqueName = '${replace(toLower(entraAppServerDisplayName), ' ', '-')}-${uniqueString(resourceGroup().id)}'
module entraAppServer 'modules/entra-app.bicep' = {
  name: 'entra-app-server-deployment'
  params: {
    entraAppDisplayName: entraAppServerDisplayName
    entraAppUniqueName: entraAppServerUniqueName
    isServer: true
    entraAppScopeValue: 'Mcp.Tools.ReadWrite'
    entraAppScopeDisplayName: 'Azure MCP Storage Tools ReadWrite'
    entraAppScopeDescription: 'Azure MCP Storage Tools Permission to call tools'
    knownClientAppId: entraAppClient.outputs.entraAppClientId
  }
}

module acaStorageManagedIdentity 'modules/aca-storage-managed-identity.bicep' = {
  name: 'aca-storage-managed-identity-deployment'
  params: {
    location: location
    managedIdentityName: '${acaName}-storage-managed-identity'
  }
}

// Deploy ACA Infrastructure to host Azure MCP Server
module acaInfrastructure 'modules/aca-infrastructure.bicep' = {
  name: 'aca-infrastructure-deployment'
  params: {
    name: acaName
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
    azureMcpCollectTelemetry: string(!empty(appInsights.outputs.connectionString))
    azureAdTenantId: tenant().tenantId
    azureAdClientId: entraAppServer.outputs.entraAppClientId
    azureAdInstance: environment().authentication.loginEndpoint
    namespaces: ['storage']
    userAssignedManagedIdentityId: acaStorageManagedIdentity.outputs.managedIdentityId
    userAssignedManagedIdentityClientId: acaStorageManagedIdentity.outputs.managedIdentityClientId
  }
}

module acaStorageRoleAssignment 'modules/aca-storage-subscription-role.bicep' = {
  name: 'aca-storage-subscription-role-deployment'
  scope: subscription()
  params: {
    managedIdentityPrincipalId: acaStorageManagedIdentity.outputs.managedIdentityPrincipalId
  }
}

// Outputs for azd and other consumers
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId
output AZURE_RESOURCE_GROUP string = resourceGroup().name
output AZURE_LOCATION string = location

// Entra App outputs
output ENTRA_APP_SERVER_CLIENT_ID string = entraAppServer.outputs.entraAppClientId
output ENTRA_APP_SERVER_SCOPE_ID string = entraAppServer.outputs.entraAppScopeId
output ENTRA_APP_SERVER_SCOPE_VALUE string = entraAppServer.outputs.entraAppScopeValue
output ENTRA_APP_CLIENT_CLIENT_ID string = entraAppClient.outputs.entraAppClientId

// ACA Infrastructure outputs
output CONTAINER_APP_URL string = acaInfrastructure.outputs.containerAppUrl
output CONTAINER_APP_NAME string = acaInfrastructure.outputs.containerAppName
output AZURE_CONTAINER_APP_ENVIRONMENT_ID string = acaInfrastructure.outputs.containerAppEnvironmentId

// ACA user assigned managed identity
output CONTAINER_APP_MANAGED_IDENTITY_CLIENT_ID string = acaStorageManagedIdentity.outputs.managedIdentityClientId

// Application Insights outputs
output APPLICATION_INSIGHTS_NAME string = appInsightsName
output APPLICATION_INSIGHTS_CONNECTION_STRING string = appInsights.outputs.connectionString
output AZURE_MCP_COLLECT_TELEMETRY string = string(!empty(appInsights.outputs.connectionString))
