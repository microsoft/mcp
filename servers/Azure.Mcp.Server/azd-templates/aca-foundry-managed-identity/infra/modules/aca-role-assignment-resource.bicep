@description('Full resource ID of the Storage Account')
@metadata({
  example: '/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/myResourceGroup/providers/Microsoft.Storage/storageAccounts/contosostorage'
})
param storageResourceId string

@description('Azure Container App Managed Identity principal/object ID (GUID)')
param acaPrincipalId string

@description('Azure RBAC role definition ID (GUID) to grant the Container App managed identity on the storage account')
param roleDefinitionId string

// Expected format: /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{accountName}
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
