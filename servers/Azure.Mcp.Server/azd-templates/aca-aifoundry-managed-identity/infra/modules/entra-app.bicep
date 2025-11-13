/*
  This template creates an Entra (Azure AD) application with the necessary components
  for secure authentication and authorization in Azure.

  What gets created:

  Entra Application Registration
     This is like a "blueprint" that defines what the Entra App can do. It includes
     app roles (think of these as custom permissions), identifier URIs for OAuth validation,
     and basic app configuration.

  Service Principal
     This is the actual "identity" that represents the Entra App within the Azure
     tenant. This is what you'll assign Azure permissions to, not the app registration itself.
  
  The Entra App registration defines what the app could do, while the Service Principal
  defines what it can actually do in your specific environment.
*/

extension microsoftGraphV1

@description('Display name for the Entra Application')
param entraAppDisplayName string

@description('Unique name for the Entra Application')
param entraAppUniqueName string

var entraAppRoleValue = 'Mcp.Tools.ReadWrite.All'
var entraAppRoleId = guid(subscription().id, entraAppRoleValue)
var entraAppRoleDisplayName = 'Azure MCP Tools ReadWrite All'
var entraAppRoleDescription = 'Application permission for Azure MCP tool calls'

resource entraApp 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: entraAppUniqueName 
  displayName: entraAppDisplayName
  appRoles: [
    {
      id: entraAppRoleId
      displayName: entraAppRoleDisplayName
      description: entraAppRoleDescription
      value: entraAppRoleValue
      isEnabled: true
      allowedMemberTypes: ['Application']
    }
  ]
}

resource entraAppUpdate 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: entraAppUniqueName
  displayName: entraAppDisplayName
  appRoles: entraApp.appRoles
  identifierUris: ['api://${entraApp.appId}']
}

resource entraServicePrincipal 'Microsoft.Graph/servicePrincipals@v1.0' = {
  appId: entraApp.appId
}

output entraAppClientId string = entraApp.appId
output entraAppObjectId string = entraApp.id
output entraAppIdentifierUri string = 'api://${entraApp.appId}'
output entraAppRoleValue string = entraAppRoleValue
output entraAppRoleId string = entraApp.appRoles[0].id
output entraAppServicePrincipalObjectId string = entraServicePrincipal.id
