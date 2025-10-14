targetScope = 'resourceGroup'

@minLength(4)
@maxLength(21)
@description('The base resource name.')
param baseName string

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The Log Analytics workspace ID for Application Insights.')
param workspaceId string

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Application Insights resource with cheapest SKU
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${baseName}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: workspaceId
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    Request_Source: 'rest'
  }
}

// Standard availability test for Bing.com
resource bingAvailabilityTest 'Microsoft.Insights/webtests@2022-06-15' = {
  name: '${baseName}-bing-test'
  location: location
  kind: 'standard'
  properties: {
    SyntheticMonitorId: '${baseName}-bing-test'
    Name: 'Bing Availability Test'
    Description: 'Standard availability test for bing.com'
    Enabled: true
    Frequency: 300 // 5 minutes (cheapest frequency)
    Timeout: 30
    Kind: 'standard'
    RetryEnabled: true
    Locations: [
      {
        Id: 'us-ca-sjc-azr' // US West (San Jose)
      }
      {
        Id: 'us-tx-sn1-azr' // US South Central (San Antonio)
      }
    ]
    Request: {
      RequestUrl: 'https://www.bing.com'
      HttpVerb: 'GET'
      ParseDependentRequests: false
      FollowRedirects: true
    }
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      IgnoreHttpStatusCode: false
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
    }
  }
  tags: {
    'hidden-link:${appInsights.id}': 'Resource'
  }
}

// Role assignments for the test application to manage Application Insights and Web Tests
resource applicationInsightsComponentContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Application Insights Component Contributor role.
  // Can manage Application Insights components
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#application-insights-component-contributor
  name: 'ae349356-3a1b-4a5e-921d-050484c6347e'
}

resource monitoringContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Monitoring Contributor role.
  // Can read all monitoring data and update monitoring settings including web tests
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#monitoring-contributor
  name: '749f88d5-cbae-40b8-bcfc-e573ddc772fa'
}

// Resource group level role assignments for Application Insights and Web Tests management
resource appInsightsContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(applicationInsightsComponentContributorRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: applicationInsightsComponentContributorRoleDefinition.id
    description: 'Application Insights Component Contributor for testApplicationOid'
  }
}

resource monitoringContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(monitoringContributorRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: monitoringContributorRoleDefinition.id
    description: 'Monitoring Contributor for testApplicationOid'
  }
}

// Specific Application Insights resource level role assignment
resource appInsightsComponentRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(applicationInsightsComponentContributorRoleDefinition.id, testApplicationOid, appInsights.id)
  scope: appInsights
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: applicationInsightsComponentContributorRoleDefinition.id
    description: 'Application Insights Component Contributor for testApplicationOid on specific AI resource'
  }
}

// Standard availability test for Microsoft.com
resource microsoftAvailabilityTest 'Microsoft.Insights/webtests@2022-06-15' = {
  name: '${baseName}-microsoft-test'
  location: location
  kind: 'standard'
  properties: {
    SyntheticMonitorId: '${baseName}-microsoft-test'
    Name: 'Microsoft Availability Test'
    Description: 'Standard availability test for microsoft.com'
    Enabled: true
    Frequency: 300 // 5 minutes (cheapest frequency)
    Timeout: 30
    Kind: 'standard'
    RetryEnabled: true
    Locations: [
      {
        Id: 'us-ca-sjc-azr' // US West (San Jose)
      }
      {
        Id: 'us-tx-sn1-azr' // US South Central (San Antonio)
      }
    ]
    Request: {
      RequestUrl: 'https://www.microsoft.com'
      Headers: null
      HttpVerb: 'GET'
      ParseDependentRequests: false
      FollowRedirects: true
    }
    ValidationRules: {
      ExpectedHttpStatusCode: 200
      IgnoreHttpStatusCode: false
      SSLCheck: true
      SSLCertRemainingLifetimeCheck: 7
    }
  }
  tags: {
    'hidden-link:${appInsights.id}': 'Resource'
  }
}

// Outputs
output appInsightsId string = appInsights.id
output appInsightsName string = appInsights.name
