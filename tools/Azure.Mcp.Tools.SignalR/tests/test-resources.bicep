targetScope = 'resourceGroup'

@description('The name of the SignalR service')
param baseName string = resourceGroup().name

@description('The location for all resources')
param location string = resourceGroup().location

// SignalR Service
resource signalr 'Microsoft.SignalRService/signalR@2024-03-01' = {
  name: baseName
  location: location
  sku: {
    name: 'Standard_S1'
    tier: 'Standard'
    capacity: 1
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
  }
}

// Basic outputs for tests
output baseName string = signalr.name
output signalRId string = signalr.id
