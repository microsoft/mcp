@description('Full resource ID of the Storage Account')
param storageResourceId string

@description('Azure Container App Managed Identity principal ID (GUID)')
param acaPrincipalId string

@description('Role definition ID (GUID) for the Azure RBAC role (e.g., Storage Blob Data Contributor = ba92f5b4-2d11-453d-a403-e96b0029c9fe)')
param roleDefinitionId string

var resourceIdParts = split(storageResourceId, '/')
var storageAccountName = resourceIdParts[8]

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, acaPrincipalId, roleDefinitionId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: acaPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output roleAssignmentId string = roleAssignment.id
output roleAssignmentName string = roleAssignment.name
