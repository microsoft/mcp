@description('Location for all resources')
param location string = resourceGroup().location

@description('Name for the Azure Container App')
param acaName string

@description('Display name for the Entra App')
param entraAppDisplayName string

@description('Full resource ID of the Storage Account that the MCP server will have access to through storage tools')
param storageResourceId string

@description('AI Foundry project resource ID for assigning Entra App role to AIF project managed identity')
param aifProjectResourceId string

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
var entraAppUniqueName = '${replace(toLower(entraAppDisplayName), ' ', '-')}-${uniqueString(resourceGroup().id)}'
//
module entraApp 'modules/entra-app.bicep' = {
  name: 'entra-app-deployment'
  params: {
    entraAppDisplayName: entraAppDisplayName
    entraAppUniqueName: entraAppUniqueName
  }
}

// Deploy ACA Infrastructure
module acaInfrastructure 'modules/aca-infrastructure.bicep' = {
  name: 'aca-infrastructure-deployment'
  params: {
    name: acaName
    location: location
    appInsightsConnectionString: appInsights.outputs.connectionString
    azureMcpCollectTelemetry: string(!empty(appInsights.outputs.connectionString))
    azureAdTenantId: tenant().tenantId
    azureAdClientId: entraApp.outputs.entraAppClientId
  }
}

// Storage role definitions (read-only roles for the --read-only Azure MCP Server flag)
var storageBlobDataReaderRoleId = '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
var readerRoleId = 'acdd72a7-3385-48ef-bd42-f606fba81ae7'

// Deploy Storage Blob Data Reader role assignment for ACA
module acaStorageBlobRoleAssignment './modules/aca-role-assignment-resource.bicep' = {
  name: 'aca-storage-blob-role-assignment'
  params: {
    storageResourceId: storageResourceId
    acaPrincipalId: acaInfrastructure.outputs.containerAppPrincipalId
    roleDefinitionId: storageBlobDataReaderRoleId
  }
}

// Deploy Reader role assignment for ACA (read storage account properties)
module acaStorageAccountRoleAssignment './modules/aca-role-assignment-resource.bicep' = {
  name: 'aca-storage-account-role-assignment'
  params: {
    storageResourceId: storageResourceId
    acaPrincipalId: acaInfrastructure.outputs.containerAppPrincipalId
    roleDefinitionId: readerRoleId
  }
}

// Deploy Entra App role assignment for AIF project MI to access ACA
module aifRoleAssignment './modules/aif-role-assignment-entraapp.bicep' = {
  name: 'aif-role-assignment'
  params: {
    aifProjectResourceId: aifProjectResourceId
    entraAppServicePrincipalObjectId: entraApp.outputs.entraAppServicePrincipalObjectId
    entraAppRoleId: entraApp.outputs.entraAppRoleId
  }
}

// Outputs for azd and other consumers
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId
output AZURE_RESOURCE_GROUP string = resourceGroup().name
output AZURE_LOCATION string = location

// Entra App outputs
output ENTRA_APP_CLIENT_ID string = entraApp.outputs.entraAppClientId
output ENTRA_APP_OBJECT_ID string = entraApp.outputs.entraAppObjectId
output ENTRA_APP_SERVICE_PRINCIPAL_ID string = entraApp.outputs.entraAppServicePrincipalObjectId
output ENTRA_APP_ROLE_ID string = entraApp.outputs.entraAppRoleId
output ENTRA_APP_IDENTIFIER_URI string = entraApp.outputs.entraAppIdentifierUri

// ACA Infrastructure outputs
output CONTAINER_APP_URL string = acaInfrastructure.outputs.containerAppUrl
output CONTAINER_APP_NAME string = acaInfrastructure.outputs.containerAppName
output CONTAINER_APP_PRINCIPAL_ID string = acaInfrastructure.outputs.containerAppPrincipalId
output AZURE_CONTAINER_APP_ENVIRONMENT_ID string = acaInfrastructure.outputs.containerAppEnvironmentId

// Application Insights outputs
output APPLICATION_INSIGHTS_NAME string = appInsightsName
output APPLICATION_INSIGHTS_CONNECTION_STRING string = appInsights.outputs.connectionString
output AZURE_MCP_COLLECT_TELEMETRY string = string(!empty(appInsights.outputs.connectionString))
