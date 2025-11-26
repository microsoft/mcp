@description('Location for all resources')
param location string = resourceGroup().location

@description('Name for the managed identity')
param managedIdentityName string

// Create User Assigned Managed Identity
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: {
    product: 'azmcp'
  }
}

// Outputs
output managedIdentityId string = userAssignedIdentity.id
output managedIdentityName string = userAssignedIdentity.name
output managedIdentityPrincipalId string = userAssignedIdentity.properties.principalId
output managedIdentityClientId string = userAssignedIdentity.properties.clientId
