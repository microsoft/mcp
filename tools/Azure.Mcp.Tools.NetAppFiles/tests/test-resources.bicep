targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name used by the live tests.')
param baseName string = resourceGroup().name

@description('The client object ID to grant access to the test resource group.')
param testApplicationOid string

resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource netAppFilesContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testApplicationOid, contributorRoleDefinition.id)
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: contributorRoleDefinition.id
    description: 'Contributor for Azure NetApp Files live tests'
  }
}

output NETAPP_ACCOUNT_NAME string = baseName
