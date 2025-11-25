@description('Location for all resources')
param location string = resourceGroup().location

@description('Application Insights connection string. If empty, creates new App Insights and returns its connection string. If provided, returns as-is.')
param appInsightsConnectionString string = ''

@description('Name for new Application Insights (only used if connection string is empty)')
param name string

var shouldCreate = empty(appInsightsConnectionString)
var isDisabled = appInsightsConnectionString == 'DISABLED'

// Create Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = if (shouldCreate) {
  name: '${name}-workspace'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Create Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = if (shouldCreate) {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Choose which connection string to return
// 1. If created  -> return the new connection string
// 2. If disabled -> return empty string
// 3. Otherwise   -> return provided (input) connection string
output connectionString string = shouldCreate 
  ? (applicationInsights.?properties.ConnectionString ?? '') 
  : (isDisabled ? '' : appInsightsConnectionString)
