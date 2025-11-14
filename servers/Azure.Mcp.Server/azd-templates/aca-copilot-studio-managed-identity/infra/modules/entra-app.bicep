/*
  This template creates an Entra (Azure AD) application with the necessary components
  for secure authentication and authorization in Azure.

  What gets created:

  Entra Application Registration
     This is like a "blueprint" that defines what the Entra App can do. It includes
     API scopes, identifier URIs for OAuth validation,
     and basic app configuration.
*/

extension microsoftGraphV1

@description('Display name for the Entra Application')
param entraAppDisplayName string

@description('Unique name for the Entra Application')
param entraAppUniqueName string

param isServer bool

@description('Value of the app scope')
param entraAppScopeValue string = ''

@description('Display name of the app scope')
param entraAppScopeDisplayName string = ''

@description('Description of the app scope')
param entraAppScopeDescription string = ''

@description('Known client app id')
param knownClientAppId string = ''

var scopeId = guid(entraAppUniqueName, entraAppScopeValue)

resource entraApp 'Microsoft.Graph/applications@v1.0' = {
  uniqueName: entraAppUniqueName 
  displayName: entraAppDisplayName
  api: isServer ? {
    oauth2PermissionScopes: [
      {
        id: scopeId
        type: 'User'
        adminConsentDescription: entraAppScopeDescription
        adminConsentDisplayName: entraAppScopeDisplayName
        userConsentDescription: entraAppScopeDescription
        userConsentDisplayName: entraAppScopeDisplayName
        value: entraAppScopeValue
        isEnabled: true
      }
    ]
    preAuthorizedApplications: [
      {
        appId: knownClientAppId
        delegatedPermissionIds: [
          scopeId
        ]
      }
    ]
  } : null
}

output entraAppClientId string = entraApp.appId
output entraAppObjectId string = entraApp.id
output entraAppIdentifierUri string = 'api://${entraApp.appId}'
output entraAppScopeValue string = entraAppScopeValue
output entraAppScopeId string = isServer ? entraApp.api.oauth2PermissionScopes[0].id : ''
