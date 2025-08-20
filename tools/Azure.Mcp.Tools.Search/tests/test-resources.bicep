targetScope = 'resourceGroup'

@minLength(4)
@maxLength(20)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = subscription().tenantId

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

var staticSuffix = toLower(substring(subscription().subscriptionId, 0, 4))
var staticBaseName = 'mcp${staticSuffix}'
var staticResourceGroupName = 'mcp-static-${staticSuffix}'

// Is this deployment to the TME tenant
var isTmeTenant = tenantId == '70a036f6-8e4d-4615-bad6-149c02e7720d'

// Azure OpenAI resource
resource openai 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (!isTmeTenant) {
  name:  toLower(baseName)
  location: location
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName:  toLower(baseName)
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
  }
  // Deployment of the text-embedding-3-small model
  resource openaiDeployment 'deployments' = {
    name: 'embedding-model'
    sku: {
        name: 'Standard'
        capacity: 50 // This is the Tokens Per Minute (TPM) capacity for the model
    }
    properties: {
        model: {
        format: 'OpenAI'
        name: 'text-embedding-3-small'
        }
    }
  }
}

// Managed identity to place on the search service
resource searchServiceIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = if (!isTmeTenant) {
  name: '${baseName}-search-service-identity'
  location: location
}

resource staticSearchServiceIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = if (isTmeTenant)  {
  name: '${staticBaseName}-search-service-identity'
  scope: resourceGroup(staticResourceGroupName)
}

// Azure AI Search service
resource search 'Microsoft.Search/searchServices@2025-02-01-preview' = {
  name: baseName
  location: location
  sku: {
    name: 'basic'
  }
  properties: {
    disableLocalAuth: true
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
        '${tenantId != '70a036f6-8e4d-4615-bad6-149c02e7720d' ? searchServiceIdentity.id : staticSearchServiceIdentity.id}': {}
    }
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}ais'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    resource searchDocsContainer 'containers' = {
        name: 'searchdocs'
        properties: {
            publicAccess: 'None'
        }
    }
  }
}

// Role assignments:
// Identity           | Resource         | Role
// -------------------------------------------------------------------------------
// search service      | storage account | Storage Blob Data Reader
// test application    | storage account | Storage Blob Data Contributor
// test application    | search service  | Search Index Data Reader
// test application    | search service  | Search Service Contributor
// search service      | openai account  | Cognitive Services OpenAI Contributor

// Storage Blob Data Reader role definition
resource storageBlobDataReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Reader role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-reader
  name: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
}

// Storage Blob Data Contributor role definition
resource storageBlobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Reader role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-contributor
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

// Search Index Data Reader role definition
resource searchIndexDataReaderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Search Index Data Reader role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#search-index-data-reader
  name: '1407120a-92aa-4202-b7e9-c0e197c71c8f'
}

// Search Service Contributor role definition
resource searchServiceContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Search Service Contributor role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#search-service-contributor
  name: '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
}

// Cognitive Services OpenAI Contributor role definition
resource openaiContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Cognitive Services OpenAI Contributor role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#cognitive-services-openai-contributor
  name: 'a001fd3d-188f-4b5d-821b-7da978bf7442'
}

// Assign Storage Blob Data Reader role for Azure Search service identity on the storage account
resource search_Storage_RoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageBlobDataReaderRoleDefinition.id, search.id, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: tenantId != '70a036f6-8e4d-4615-bad6-149c02e7720d' ? searchServiceIdentity.properties.principalId : staticSearchServiceIdentity.properties.principalId
    roleDefinitionId: storageBlobDataReaderRoleDefinition.id
  }
}

// Assign Storage Blob Data Reader role for Azure Search service identity on the storage account
resource testApp_Storage_RoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageBlobDataContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: storageBlobDataContributorRoleDefinition.id
  }
}

// Assign Search Index Data Reader role to testApplicationOid
resource testApp_search_indexDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(searchIndexDataReaderRoleDefinition.id, testApplicationOid, search.id)
  scope: search
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: searchIndexDataReaderRoleDefinition.id
  }
}

// Assign Search Service Contributor role to testApplicationOid
resource testApp_search_contributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(searchServiceContributorRoleDefinition.id, testApplicationOid, search.id)
  scope: search
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: searchServiceContributorRoleDefinition.id
  }
}

// Assign Cognitive Services OpenAI Contributor role to the search resource's identity if we created the OpenAI resource
resource search_openAi_roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!isTmeTenant) {
  name: guid(openaiContributorRoleDefinition.id, searchServiceIdentity.id, openai.id)
  scope: openai
  properties: {
    principalId: searchServiceIdentity.properties.principalId
    roleDefinitionId: openaiContributorRoleDefinition.id
  }
}

output staticBaseName string = staticBaseName
output staticResourceGroupName string = staticResourceGroupName
output storageAccountName string = storageAccount.name
output containerName string = storageAccount::blobServices::searchDocsContainer.name
