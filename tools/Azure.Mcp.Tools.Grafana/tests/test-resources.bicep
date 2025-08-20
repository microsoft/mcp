targetScope = 'resourceGroup'

@minLength(4)
@maxLength(63)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Generate a unique name for the Grafana workspace. The name must be between 2 to 23 characters long.
var grafanaName = '${baseName}-amg'

resource grafanaWorkspace 'Microsoft.Dashboard/grafana@2023-09-01' = {
  name: grafanaName
  location: location
  sku: {
    name: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    zoneRedundancy: 'Disabled'
    publicNetworkAccess: 'Enabled'
    deterministicOutboundIP: 'Disabled'
    grafanaIntegrations: {
      azureMonitorWorkspaceIntegrations: []
    }
  }
  tags: {
    'test-resource': 'true'
    'created-by': 'azure-mcp'
  }
}

// Role assignment to grant the test application admin access to the Grafana workspace
resource grafanaAdminRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(grafanaWorkspace.id, testApplicationOid, 'Grafana Admin')
  scope: grafanaWorkspace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '22926164-76b3-42b3-bc55-97df8dab3e41') // Grafana Admin role
    principalId: testApplicationOid
  }
}

output grafanaWorkspaceName string = grafanaWorkspace.name
output grafanaWorkspaceId string = grafanaWorkspace.id
output grafanaEndpoint string = grafanaWorkspace.properties.endpoint
output grafanaResourceGroupName string = resourceGroup().name
output grafanaLocation string = grafanaWorkspace.location
