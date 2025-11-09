@description('Full resource ID of the Storage Account')
param storageResourceId string

@description('Azure Container App Managed Identity *principal/object* ID (GUID)')
param acaPrincipalId string

@description('Role definition ID (GUID) for the Azure RBAC role (e.g., Storage Blob Data Contributor = ba92f5b4-2d11-453d-a403-e96b0029c9fe)')
param roleDefinitionId string

var resourceIdParts = split(storageResourceId, '/')
var resourceGroupName = resourceIdParts[4]

module storageRoleAssignment './aca-role-assignment-resource-storage.bicep' = {
  name: 'aca-role-assignment-storage-${roleDefinitionId}'
  scope: resourceGroup(resourceGroupName)
  params: {
    storageResourceId: storageResourceId
    acaPrincipalId: acaPrincipalId
    roleDefinitionId: roleDefinitionId
  }
}

output roleAssignmentId string = storageRoleAssignment.outputs.roleAssignmentId
output roleAssignmentName string = storageRoleAssignment.outputs.roleAssignmentName
