targetScope = 'resourceGroup'

@minLength(1)
@maxLength(22)
param baseName string = resourceGroup().name

@description('The location for the resources. Must be a region where Microsoft.CloudHealth is available (e.g. swedencentral, uksouth).')
param location string = resourceGroup().location

var readerRoleId = 'acdd72a7-3385-48ef-bd42-f606fba81ae7'

var modelAName = '${baseName}-hm-a'
var modelBName = '${baseName}-hm-b'

resource emptyStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: toLower('${baseName}hm')
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
  }
}

resource healthModelIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-hm-id'
  location: location
}

resource readerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, healthModelIdentity.id, readerRoleId)
  scope: resourceGroup()
  properties: {
    principalId: healthModelIdentity.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', readerRoleId)
    principalType: 'ServicePrincipal'
  }
}

// ============================================================
// Model B (child) — monitors the empty storage account via the Azure Resource Health (service health) signal.
// ============================================================
resource healthModelB 'Microsoft.CloudHealth/healthmodels@2026-05-01-preview' = {
  name: modelBName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${healthModelIdentity.id}': {}
    }
  }
  properties: {}
}

resource authB 'Microsoft.CloudHealth/healthmodels/authenticationsettings@2026-05-01-preview' = {
  parent: healthModelB
  name: 'default'
  properties: {
    authenticationKind: 'ManagedIdentity'
    displayName: 'Health model managed identity'
    managedIdentityName: healthModelIdentity.id
  }
}

resource rootEntityB 'Microsoft.CloudHealth/healthmodels/entities@2026-05-01-preview' = {
  parent: healthModelB
  name: modelBName
  properties: {
    displayName: 'Storage account (Azure Resource Health)'
    impact: 'Standard'
    signalGroups: {
      azureResource: {
        authenticationSetting: authB.name
        azureResourceId: emptyStorage.id
        azureResourceKind: 'Microsoft.Storage/storageAccounts'
        // The Azure Resource Health / service health availability signal — no metric signals needed.
        resourceHealth: {
          enabled: 'Enabled'
        }
      }
    }
  }
}

// ============================================================
// Model A (parent) — embeds Model B as a nested health model.
// The service uses Model B's root-entity health state as this entity's signal.
// ============================================================
resource healthModelA 'Microsoft.CloudHealth/healthmodels@2026-05-01-preview' = {
  name: modelAName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${healthModelIdentity.id}': {}
    }
  }
  properties: {}
}

resource authA 'Microsoft.CloudHealth/healthmodels/authenticationsettings@2026-05-01-preview' = {
  parent: healthModelA
  name: 'default'
  properties: {
    authenticationKind: 'ManagedIdentity'
    displayName: 'Health model managed identity'
    managedIdentityName: healthModelIdentity.id
  }
}

resource rootEntityA 'Microsoft.CloudHealth/healthmodels/entities@2026-05-01-preview' = {
  parent: healthModelA
  name: modelAName
  properties: {
    displayName: 'Nested health model (embeds ${modelBName})'
    impact: 'Standard'
    signalGroups: {
      azureResource: {
        authenticationSetting: authA.name
        azureResourceId: healthModelB.id
        azureResourceKind: 'Microsoft.CloudHealth/healthmodels'
        resourceHealth: {
          enabled: 'Disabled'
        }
      }
    }
  }
}

output healthModelAName string = modelAName
output healthModelBName string = modelBName
output emptyStorageAccountName string = emptyStorage.name
