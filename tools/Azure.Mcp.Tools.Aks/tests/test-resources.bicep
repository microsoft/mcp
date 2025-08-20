targetScope = 'resourceGroup'

@minLength(3)
@maxLength(63)
@description('The base resource name. AKS cluster names must be between 1 and 63 characters.')
param baseName string = resourceGroup().name

// AKS works more reliably in westus2
@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location == 'westus' ? 'westus2' : resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('The VM size for the AKS node pool. Default is Standard_F2 which is the smallest VM that meets AKS system node pool requirements (2 vCPUs, 4 GB RAM).')
param nodeVmSize string = 'Standard_D2lds_v5'

// Create a basic AKS cluster for testing
resource aksCluster 'Microsoft.ContainerService/managedClusters@2024-02-01' = {
  name: baseName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: '${baseName}-dns'
    agentPoolProfiles: [
      {
        name: 'agentpool'
        count: 1
        vmSize: nodeVmSize // Use parameterized VM size
        osType: 'Linux'
        mode: 'System'
      }
    ]
    disableLocalAccounts: false
    enableRBAC: true
  }
}

// Azure Kubernetes Service RBAC Cluster Admin role
resource aksClusterAdminRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Azure Kubernetes Service RBAC Cluster Admin role
  // Lets you manage all resources in the cluster.
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-kubernetes-service-rbac-cluster-admin
  name: 'b1ff04bb-8a4e-4dc4-8eb5-8693973ce19b'
}

// Assign the test application as cluster admin
resource appAksClusterAdminRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aksClusterAdminRoleDefinition.id, testApplicationOid, aksCluster.id)
  scope: aksCluster
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: aksClusterAdminRoleDefinition.id
    description: 'AKS Cluster Admin for testApplicationOid'
  }
}

// Azure Kubernetes Service Cluster User Role for basic cluster access
resource aksClusterUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Azure Kubernetes Service Cluster User Role
  // List cluster user credentials action.
  name: '4abbcc35-e782-43d8-92c5-2d3f1bd2253f'
}

// Assign the test application as cluster user
resource appAksClusterUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(aksClusterUserRoleDefinition.id, testApplicationOid, aksCluster.id)
  scope: aksCluster
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: aksClusterUserRoleDefinition.id
    description: 'AKS Cluster User for testApplicationOid'
  }
}

// Output the cluster name for test consumption
output aksClusterName string = aksCluster.name
output aksClusterFqdn string = aksCluster.properties.fqdn
output aksClusterResourceId string = aksCluster.id
