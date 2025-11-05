@description('Full resource ID of the Postgres resource')
param postgresResourceId string

@description('Azure Container App Managed Identity Principal ID to assign the role to')
param acaPrincipalId string

@description('Role definition ID (GUID) for the Azure RBAC role')
param roleDefinitionId string

// postgresResourceId Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.DBforPostgreSQL/flexibleServers/{name}
var resourceIdParts = split(postgresResourceId, '/')
var postgresServerName = resourceIdParts[8]

resource targetResource 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
  name: postgresServerName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(postgresResourceId, acaPrincipalId, roleDefinitionId)
  scope: targetResource
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: acaPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output roleAssignmentId string = roleAssignment.id
output roleAssignmentName string = roleAssignment.name
