targetScope = 'resourceGroup'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

var roleDefinitionID = 'acdd72a7-3385-48ef-bd42-f606fba81ae7' // Built-in Reader role

var roleAssignmentName= guid(testApplicationOid, roleDefinitionID, resourceGroup().id)
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: roleAssignmentName
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionID)
    principalId: testApplicationOid
    description: 'Role assignment for azmcp test'
  }
}
