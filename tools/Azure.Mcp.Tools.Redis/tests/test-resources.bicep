targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

resource redisCache 'Microsoft.Cache/Redis@2024-11-01' = {
  name: baseName
  location: location
  properties: {
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    disableAccessKeyAuthentication: true
    sku: {
      capacity: 0
      family: 'C'
      name: 'Basic'
    }
    redisConfiguration: {
      'aad-enabled': 'true'
    }
  }
}

resource redisCacheAccessPolicyAssignment 'Microsoft.Cache/Redis/accessPolicyAssignments@2024-11-01' = {
  parent: redisCache
  name: baseName
  properties: {
    accessPolicyName: 'Data Owner'
    objectId: testApplicationOid
    objectIdAlias: testApplicationOid
  }
}

resource redisCluster 'Microsoft.Cache/redisEnterprise@2025-05-01-preview' = {
  location: location
  name: baseName
  sku: {
    name: 'Balanced_B0'
  }
  identity: {
    type: 'None'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

resource redisClusterDatabase 'Microsoft.Cache/redisEnterprise/databases@2025-05-01-preview' = {
  parent: redisCluster
  name: 'default'
  properties:{
    clientProtocol: 'Encrypted'
    port: 10000
    clusteringPolicy: 'OSSCluster'
    evictionPolicy: 'NoEviction'
    persistence:{
      aofEnabled: false 
      rdbEnabled: false
    }
  }
}
