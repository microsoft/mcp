@description('Full resource ID of the Storage Account')
param storageResourceId string

@description('Azure Container App Managed Identity principal/object ID (GUID)')
param acaPrincipalId string

@description('Azure RBAC role definition ID (GUID) to grant the Container App managed identity on the storage account')
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
