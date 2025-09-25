targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  location: location
  name: baseName
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
  }
}

resource certificateOfficerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Key Vault Certificates Officer role. Allows user to read/write/delete certificates.
  name: 'a4417e6f-fecd-4de8-b567-7b0420556985'
}

resource keysOfficerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Key Vault Crypto Officer role.  Allows user to read/write/delete keys.
  name: '14b46e9e-c2b7-41b4-b07b-48a6ebf60603'
}

resource secretsOfficerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Key Vault Secrets Officer role. Allows user to read/write/delete secrets.
  name: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
}

// Key Vault Administrator role definition (full access including settings)
resource keyVaultAdminRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the built-in Key Vault Administrator role required to read vault settings
  name: '00482a5a-887f-4fb3-b363-3b7fe8e74483'
}

resource certificatesRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(certificateOfficerRoleDefinition.id, testApplicationOid, keyVault.id)
  scope: keyVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: certificateOfficerRoleDefinition.id
  }
}

resource keysRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keysOfficerRoleDefinition.id, testApplicationOid, keyVault.id)
  scope: keyVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: keysOfficerRoleDefinition.id
  }
}

resource secretsRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(secretsOfficerRoleDefinition.id, testApplicationOid, keyVault.id)
  scope: keyVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: secretsOfficerRoleDefinition.id
  }
}

// Assign Key Vault Administrator to test principal so admin settings API calls succeed
resource keyVaultAdminRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVaultAdminRoleDefinition.id, testApplicationOid, keyVault.id)
  scope: keyVault
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: keyVaultAdminRoleDefinition.id
  }
}

resource keyVaultKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: keyVault
  name: 'foo-bar'
  properties: {
    keySize: 2048
    keyOps: [
      'encrypt'
      'decrypt'
      'sign'
      'verify'
      'wrapKey'
      'unwrapKey'
    ]
    kty: 'RSA'
  }
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'foo-bar-secret'
  parent: keyVault
  properties: {
    value: 'foo-bar-value'
  }
}
