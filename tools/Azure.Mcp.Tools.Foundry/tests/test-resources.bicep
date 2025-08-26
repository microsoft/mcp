targetScope = 'resourceGroup'

@minLength(3)
@maxLength(17)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

var cognitiveServicesContributorRoleId = '25fbc0a9-bd7c-42a3-aa1a-3b75d497ee68' // Cognitive Services Contributor role

resource aiServicesAccount 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: baseName
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: 'S0'
  }
  properties: {
    isAiFoundryType: true
    customSubDomainName: baseName
    dynamicThrottlingEnabled: false
    networkAcls: {
      defaultAction: 'Allow'
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: true
    allowProjectManagement: true
    encryption: {
      keySource: 'Microsoft.CognitiveServices'
    }
  }
}

resource contributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cognitiveServicesContributorRoleId, testApplicationOid, aiServicesAccount.id)
  scope: aiServicesAccount
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesContributorRoleId)
    principalId: testApplicationOid
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}foundry'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowSharedKeyAccess: false
  }

  resource blobServices 'blobServices' = {
    name: 'default'
    resource foundryContainer 'containers' = { name: 'foundry' }
  }
}

resource aiProjects 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = {
  parent: aiServicesAccount
  name: '${baseName}-ai-projects'
  location: location
  kind: 'AIServices'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    customSubDomainName: '${baseName}-ai-projects'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      virtualNetworkRules: []
      ipRules: []
    }
  }
  sku: {
    name: 'S0'
  }
}

resource aiProjectsRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cognitiveServicesContributorRoleId, testApplicationOid, aiProjects.id)
  scope: aiProjects
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', cognitiveServicesContributorRoleId)
    principalId: testApplicationOid
  }
}

resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview' = {
  parent: aiServicesAccount
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
    }
  }
}

resource searchService 'Microsoft.Search/searchServices@2023-11-01' = {
  name: '${baseName}-search'
  location: location
  sku: {
    name: 'basic'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    networkRuleSet: {
      ipRules: []
    }
    encryptionWithCmk: {
      enforcement: 'Unspecified'
    }
    disableLocalAuth: false
    authOptions: {
      apiKeyOnly: {}
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource searchServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('8ebe5a00-799e-43f5-93ac-243d3dce84a7', testApplicationOid, searchService.id) // Search Index Data Contributor role
  scope: searchService
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '8ebe5a00-799e-43f5-93ac-243d3dce84a7')
    principalId: testApplicationOid
  }
}

resource searchIndexDeploymentScript 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: '${baseName}-create-search-index'
  location: location
  kind: 'AzurePowerShell'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    azPowerShellVersion: '11.0'
    retentionInterval: 'P1D'
    timeout: 'PT30M'
    cleanupPreference: 'OnSuccess'
    scriptContent: '''
      param(
        [string]$searchServiceName,
        [string]$resourceGroupName,
        [string]$subscriptionId
      )

      # Set the context
      Set-AzContext -SubscriptionId $subscriptionId

      # Get search service admin key
      $adminKeys = Get-AzSearchAdminKeyPair -ResourceGroupName $resourceGroupName -ServiceName $searchServiceName
      $adminKey = $adminKeys.Primary

      # Define the index schema
      $indexSchema = @{
        name = "test-knowledge-index"
        fields = @(
          @{
            name = "id"
            type = "Edm.String"
            key = $true
            searchable = $false
            filterable = $true
            retrievable = $true
          },
          @{
            name = "content"
            type = "Edm.String"
            searchable = $true
            filterable = $false
            retrievable = $true
            analyzer = "standard.lucene"
          },
          @{
            name = "title"
            type = "Edm.String"
            searchable = $true
            filterable = $true
            retrievable = $true
            sortable = $true
          },
          @{
            name = "metadata"
            type = "Edm.String"
            searchable = $false
            filterable = $true
            retrievable = $true
          }
        )
      } | ConvertTo-Json -Depth 10

      # Create the index
      $uri = "https://$searchServiceName.search.windows.net/indexes?api-version=2023-11-01"
      $headers = @{
        'Content-Type' = 'application/json'
        'api-key' = $adminKey
      }

      try {
        $response = Invoke-RestMethod -Uri $uri -Method POST -Body $indexSchema -Headers $headers
        Write-Output "Successfully created search index: test-knowledge-index"
        Write-Output "Index response: $($response | ConvertTo-Json)"
      } catch {
        if ($_.Exception.Response.StatusCode -eq 409) {
          Write-Output "Search index 'test-knowledge-index' already exists"
        } else {
          Write-Error "Failed to create search index: $($_.Exception.Message)"
          throw
        }
      }
    '''
    arguments: '-searchServiceName "${searchService.name}" -resourceGroupName "${resourceGroup().name}" -subscriptionId "${subscription().subscriptionId}"'
  }
  dependsOn: [
    searchServiceRoleAssignment
    managedIdentityRoleAssignment
  ]
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}-deployment-identity'
  location: location
}

resource managedIdentityRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid('7ca78c08-252a-4471-8644-bb5ff32d4ba0', managedIdentity.id, searchService.id) // Search Service Contributor role
  scope: searchService
  properties: {
    roleDefinitionId: resourceId('Microsoft.Authorization/roleDefinitions', '7ca78c08-252a-4471-8644-bb5ff32d4ba0')
    principalId: managedIdentity.properties.principalId
  }
}

output searchServiceName string = searchService.name
output searchServiceEndpoint string = 'https://${searchService.name}.search.windows.net'
output knowledgeIndexName string = 'test-knowledge-index'
output aiProjectsEndpoint string = 'https://${aiServicesAccount.name}.services.ai.azure.com/api/projects/${aiProjects.name}'
