targetScope = 'resourceGroup'

@minLength(3)
@maxLength(50)
@description('The base resource name. Event Grid topic names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Event Grid Topic
resource eventGridTopic 'Microsoft.EventGrid/topics@2023-12-15-preview' = {
  name: baseName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: 'Basic'
  }
}

// Role assignment for Event Grid Data Sender
resource eventGridDataSenderRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'd5a91429-5739-47e2-a06b-3470a27159e7' // EventGrid Data Sender
}

resource eventGridDataSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(eventGridDataSenderRoleDefinition.id, testApplicationOid, eventGridTopic.id)
  scope: eventGridTopic
  properties: {
    roleDefinitionId: eventGridDataSenderRoleDefinition.id
    principalId: testApplicationOid
  }
}

// Role assignment for Reader role for listing topics
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7' // Reader
}

resource readerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: readerRoleDefinition.id
    principalId: testApplicationOid
  }
}

// Outputs for test consumption
output Event_Grid_Topic_Name string = eventGridTopic.name
output Event_Grid_Topic_Endpoint string = eventGridTopic.properties.endpoint
