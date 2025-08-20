targetScope = 'resourceGroup'

@description('The base resource name')
param baseName string = resourceGroup().name

@description('The location for the Datadog monitor')
param location string = resourceGroup().location == 'westus' ? 'westus2' : resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string

@description('The client OID to grant access')
param testApplicationOid string

// This module is conditionally deployed only for the specific tenant ID.
resource datadogMonitor 'Microsoft.Datadog/monitors@2023-01-01' = if (tenantId == '888d76fa-54b2-4ced-8ee5-aac1585adee7') {
  name: baseName
  location: location
  sku: {
    name: 'payg_v3_Monthly'
  }
  properties: {
    datadogOrganizationProperties: {
      name: baseName
    }
    userInfo: {
      name: 'sample-user'
      phoneNumber: '555-555-5555'
    }
  }
  tags: {}
  identity: {
    type: 'SystemAssigned'
  }
}

resource datadogContributorRole 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
}

resource datadogRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (tenantId == '888d76fa-54b2-4ced-8ee5-aac1585adee7') {
  name: guid(testApplicationOid, datadogContributorRole.id, datadogMonitor.id)
  scope: datadogMonitor
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: datadogContributorRole.id
  }
}
