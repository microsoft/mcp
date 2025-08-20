targetScope = 'resourceGroup'

@minLength(3)
@maxLength(60)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: take('${baseName}storage', 24)
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: true
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// Consumption Plan for Windows Function App
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${baseName}-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: false
  }
}

// Function App (Windows)
resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${baseName}-func'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower('${baseName}-func')
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
}

// Flex Consumption Plan for Linux Function App
resource flexConsumptionPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${baseName}-flex-plan'
  location: location
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

// Linux Function App with Flex Consumption plan
resource linuxFunctionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${baseName}-linux-func'
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: flexConsumptionPlan.id
    httpsOnly: true
    reserved: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccount.properties.primaryEndpoints.blob}deployments'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'AzureWebJobsStorage'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: 100
        instanceMemoryMB: 2048
      }
      runtime: {
        name: 'python'
        version: '3.11'
      }
    }
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
  }
}

// Role definition for Website Contributor (allows managing Function Apps)
resource websiteContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Website Contributor role
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#website-contributor
  name: 'de139f84-1756-47ae-9be6-808fbbe84772'
}

// Role assignment for the test application
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(websiteContributorRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: websiteContributorRoleDefinition.id
    description: 'Website Contributor for Function App tests'
  }
}

// Storage Blob Data Contributor role for the storage account
resource blobContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Storage Blob Data Contributor role
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource storageBlobRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(blobContributorRoleDefinition.id, testApplicationOid, storageAccount.id)
  scope: storageAccount
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: blobContributorRoleDefinition.id
    description: 'Storage Blob Data Contributor for Function App tests'
  }
}
