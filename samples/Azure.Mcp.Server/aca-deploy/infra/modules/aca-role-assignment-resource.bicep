@description('Full resource ID of the Postgres resource')
param postgresResourceId string

@description('Azure Container App Managed Identity *principal/object* ID (GUID)')
param acaPrincipalId string

@description('Role definition ID (GUID) for the Azure RBAC role (e.g., Contributor = b24988ac-6180-42a0-ab88-20f7382dd24c)')
param roleDefinitionId string

var resourceIdParts = split(postgresResourceId, '/')
var resourceGroupName = resourceIdParts[4]

module postgresRoleAssignment './aca-role-assignment-resource-postgres.bicep' = {
  name: 'aca-role-assignment-module'
  scope: resourceGroup(resourceGroupName)
  params: {
    postgresResourceId: postgresResourceId
    acaPrincipalId: acaPrincipalId
    roleDefinitionId: roleDefinitionId
  }
}

output roleAssignmentId string = postgresRoleAssignment.outputs.roleAssignmentId
output roleAssignmentName string = postgresRoleAssignment.outputs.roleAssignmentName
