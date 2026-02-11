targetScope = 'resourceGroup'

@minLength(3)
@maxLength(23)
@description('The base resource name. Service Fabric managed cluster names must be between 4 and 23 characters.')
param baseName string = resourceGroup().name

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The admin username for the cluster.')
param adminUsername string = 'sfadmin'

@description('The admin password for the cluster.')
@secure()
param adminPassword string = newGuid()

// Create a basic Service Fabric managed cluster for testing
resource sfCluster 'Microsoft.ServiceFabric/managedClusters@2024-04-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    dnsName: baseName
    adminUserName: adminUsername
    adminPassword: adminPassword
    clientConnectionPort: 19000
    httpGatewayConnectionPort: 19080
    clients: [
      {
        isAdmin: true
        thumbprint: '0000000000000000000000000000000000000000'
      }
    ]
  }
}

// Create a primary node type
resource primaryNodeType 'Microsoft.ServiceFabric/managedClusters/nodeTypes@2024-04-01' = {
  parent: sfCluster
  name: 'primary'
  properties: {
    isPrimary: true
    vmSize: 'Standard_D2s_v3'
    vmInstanceCount: 3
    dataDiskSizeGB: 128
    dataDiskType: 'StandardSSD_LRS'
  }
}

// Service Fabric Managed Clusters Contributor role
resource sfContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Contributor role - allows read/write on Service Fabric managed clusters
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource appRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sfContributorRoleDefinition.id, testApplicationOid, sfCluster.id)
  scope: sfCluster
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: sfContributorRoleDefinition.id
    description: 'Contributor for testApplicationOid on Service Fabric managed cluster'
  }
}

// Outputs for test consumption
output clusterName string = sfCluster.name
output primaryNodeTypeName string = primaryNodeType.name
