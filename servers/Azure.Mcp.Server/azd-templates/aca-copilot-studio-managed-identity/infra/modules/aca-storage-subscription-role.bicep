targetScope = 'subscription'

@description('The principal ID of the managed identity to assign the role to')
param managedIdentityPrincipalId string

@description('The role definition ID to assign. Defaults to the built-in Reader role (acdd72a7-3385-48ef-bd42-f606fba81ae7)')
param roleDefinitionId string = 'acdd72a7-3385-48ef-bd42-f606fba81ae7'

// Create role assignment at subscription level
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, managedIdentityPrincipalId, roleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output roleAssignmentId string = roleAssignment.id
output roleAssignmentName string = roleAssignment.name
