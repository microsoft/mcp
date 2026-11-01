targetScope = 'resourceGroup'

@minLength(1)
@maxLength(22)
param baseName string = resourceGroup().name

@description('The location for the resources. Must be a region where Microsoft.CloudHealth is available (e.g. swedencentral, uksouth).')
param location string = resourceGroup().location

var readerRoleId = 'acdd72a7-3385-48ef-bd42-f606fba81ae7'

var modelAName = '${baseName}-hm-a'
var modelBName = '${baseName}-hm-b'
var modelCName = '${baseName}-hm-c'

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
// Model B (child): monitors the empty storage account via the Azure Resource Health (service health) signal.
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
        // The Azure Resource Health / service health availability signal: no metric signals needed.
        resourceHealth: {
          enabled: 'Enabled'
        }
      }
    }
  }
}

// ============================================================
// Model A (parent): embeds Model B as a nested health model.
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

// ============================================================
// Model C (topology): the fixture for the health model inspect walk test.
// Deliberately separate from models A and B so declaring relationships and a signal
// definition cannot change the health rollup the other recorded tests depend on.
// ============================================================
resource healthModelC 'Microsoft.CloudHealth/healthmodels@2026-05-01-preview' = {
  name: modelCName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${healthModelIdentity.id}': {}
    }
  }
  properties: {}
}

resource authC 'Microsoft.CloudHealth/healthmodels/authenticationsettings@2026-05-01-preview' = {
  parent: healthModelC
  name: 'default'
  properties: {
    authenticationKind: 'ManagedIdentity'
    displayName: 'Health model managed identity'
    managedIdentityName: healthModelIdentity.id
  }
}

// The RP auto-creates a root entity named like the model; declaring it with that name manages the built-in root.
resource rootEntityC 'Microsoft.CloudHealth/healthmodels/entities@2026-05-01-preview' = {
  parent: healthModelC
  name: modelCName
  properties: {
    displayName: 'Topology root'
    impact: 'Standard'
    canvasPosition: {
      x: 400
      y: 100
    }
    signalGroups: {
      dependencies: {
        aggregationType: 'WorstOf'
        ignoreUnknown: true
      }
    }
  }
}

resource leafFrontendC 'Microsoft.CloudHealth/healthmodels/entities@2026-05-01-preview' = {
  parent: healthModelC
  name: '${modelCName}-leaf-frontend'
  properties: {
    displayName: 'Frontend leaf'
    impact: 'Standard'
    canvasPosition: {
      x: 250
      y: 300
    }
  }
}

resource leafBackendC 'Microsoft.CloudHealth/healthmodels/entities@2026-05-01-preview' = {
  parent: healthModelC
  name: '${modelCName}-leaf-backend'
  properties: {
    displayName: 'Backend leaf'
    impact: 'Standard'
    canvasPosition: {
      x: 550
      y: 300
    }
  }
}

// The two edges the inspect dependency walk follows: they are what turns the root's WorstOf
// dependency rollup into an explainable statement about named children.
resource relationshipFrontendC 'Microsoft.CloudHealth/healthmodels/relationships@2026-05-01-preview' = {
  parent: healthModelC
  name: 'r-${modelCName}-leaf-frontend'
  properties: {
    displayName: 'Root depends on frontend'
    parentEntityName: rootEntityC.name
    childEntityName: leafFrontendC.name
  }
}

resource relationshipBackendC 'Microsoft.CloudHealth/healthmodels/relationships@2026-05-01-preview' = {
  parent: healthModelC
  name: 'r-${modelCName}-leaf-backend'
  properties: {
    displayName: 'Root depends on backend'
    parentEntityName: rootEntityC.name
    childEntityName: leafBackendC.name
  }
}

output healthModelAName string = modelAName
output healthModelBName string = modelBName
output healthModelCName string = modelCName
output emptyStorageAccountName string = emptyStorage.name
