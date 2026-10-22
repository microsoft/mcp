targetScope = 'resourceGroup'

@minLength(4)
@maxLength(21)
@description('The base resource name.')
param baseName string

@description('The location for the Azure Monitor Workspace. Trace Store preview supports a limited set of regions; this fixture defaults to newzealandnorth.')
param location string = 'newzealandnorth'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Azure Monitor Workspace (Trace Store account)
resource monitorWorkspace 'Microsoft.Monitor/accounts@2025-10-03-preview' = {
  name: '${baseName}-traces'
  location: location
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

// traceContainers/default sub-resource — enables Trace Store on the AMW
resource traceContainer 'Microsoft.Monitor/accounts/traceContainers@2024-10-03-preview' = {
  parent: monitorWorkspace
  name: 'default'
  properties: {
    traceDurationWindowInSeconds: 600
  }
}

// Monitoring Reader — allows testApplicationOid to read the AMW resource
resource monitoringReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Monitoring Reader — read all monitoring data and settings.
  // See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#monitoring-reader
  name: '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
}

resource amwReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(monitoringReaderRoleDefinition.id, testApplicationOid, monitorWorkspace.id)
  scope: monitorWorkspace
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: monitoringReaderRoleDefinition.id
    description: 'Monitoring Reader for testApplicationOid on the trace AMW'
  }
}

// Outputs — used by the live test class to target this AMW
output traceAccountName string = monitorWorkspace.name
output traceAccountResourceId string = monitorWorkspace.id
output traceAccountLocation string = location
