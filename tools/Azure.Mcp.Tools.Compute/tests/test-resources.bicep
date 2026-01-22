targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Disk names have specific length restrictions.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

// Create a test managed disk
resource testDisk 'Microsoft.Compute/disks@2023-10-02' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    creationData: {
      createOption: 'Empty'
    }
    diskSizeGB: 32
  }
  tags: {
    Environment: 'Test'
    Purpose: 'MCP-Testing'
  }
}

// Assign Contributor role for managing disks
resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Contributor role
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource diskContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(contributorRoleDefinition.id, testApplicationOid, testDisk.id)
  scope: testDisk
  properties: {
    roleDefinitionId: contributorRoleDefinition.id
    principalId: testApplicationOid
  }
}

// Outputs for test consumption
output diskName string = testDisk.name
output resourceGroupName string = resourceGroup().name
output location string = location
