extension microsoftGraphV1

@description('AI Foundry project resource ID')
param aifProjectResourceId string

@description('Entra App Service Principal Object ID (resourceId in Graph API)')
param entraAppServicePrincipalObjectId string

@description('Entra App Role ID to assign')
param entraAppRoleId string

var resourceIdParts = split(aifProjectResourceId, '/')
var projectResourceGroup = resourceIdParts[4]
var accountName = resourceIdParts[8]
var projectName = resourceIdParts[10]

resource aifProject 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' existing = {
  scope: resourceGroup(projectResourceGroup)
  name: '${accountName}/${projectName}'
}

var aifProjectMIPrincipalId = aifProject.identity.principalId

resource appRoleAssignment 'Microsoft.Graph/appRoleAssignedTo@v1.0' = {
  principalId: aifProjectMIPrincipalId
  resourceId: entraAppServicePrincipalObjectId
  appRoleId: entraAppRoleId
}

output roleAssignmentId string = appRoleAssignment.id
output aifProjectMIPrincipalId string = aifProjectMIPrincipalId
