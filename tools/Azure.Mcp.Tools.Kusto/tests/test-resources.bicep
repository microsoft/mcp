targetScope = 'resourceGroup'

@minLength(3)
@maxLength(50)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. Defaults to westus2 because the required Kusto SKU can be restricted in westus.')
param location string = 'westus2'

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('The SKU to use for the Kusto cluster.')
param clusterSku string = 'Standard_E2a_v4'

resource kustoCluster 'Microsoft.Kusto/clusters@2024-04-13' = {
  name: baseName
  location: location
  sku: {
    name: clusterSku
    tier: 'Standard'
    capacity: 2
  }
  identity: {
    type: 'SystemAssigned'
  }

  resource kustoDatabase 'databases' = {
    location: location
    name: 'ToDoLists'
    kind: 'ReadWrite'
  }
}
