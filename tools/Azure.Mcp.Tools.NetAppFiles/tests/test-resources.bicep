targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name used by the live tests.')
param baseName string = resourceGroup().name

@description('The client object ID to grant access to the test resource group.')
param testApplicationOid string

var location = 'eastus'
var capacityPoolName = '${baseName}-pool'
var virtualNetworkName = '${baseName}-vnet'
var subnetName = 'anf'

resource contributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

resource netAppFilesContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, testApplicationOid, contributorRoleDefinition.id)
  properties: {
    principalId: testApplicationOid
    principalType: 'ServicePrincipal'
    roleDefinitionId: contributorRoleDefinition.id
    description: 'Contributor for Azure NetApp Files live tests'
  }
}

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: virtualNetworkName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '10.0.0.0/24'
          delegations: [
            {
              name: 'Microsoft.NetApp-volumes'
              properties: {
                serviceName: 'Microsoft.NetApp/volumes'
              }
            }
          ]
        }
      }
    ]
  }
}

resource netAppAccount 'Microsoft.NetApp/netAppAccounts@2024-09-01' = {
  name: baseName
  location: location
}

resource capacityPool 'Microsoft.NetApp/netAppAccounts/capacityPools@2024-09-01' = {
  parent: netAppAccount
  name: capacityPoolName
  location: location
  properties: {
    serviceLevel: 'Premium'
    size: 4398046511104
    qosType: 'Auto'
  }
}

output NETAPP_ACCOUNT_NAME string = baseName
output NETAPP_CAPACITY_POOL_ID string = capacityPool.id
output NETAPP_SUBNET_ID string = virtualNetwork.properties.subnets[0].id
output LOCATION string = location