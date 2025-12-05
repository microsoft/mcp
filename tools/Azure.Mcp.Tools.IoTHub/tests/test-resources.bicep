param location string = resourceGroup().location
param suffix string = uniqueString(resourceGroup().id)

resource iothub 'Microsoft.Devices/IotHubs@2023-06-30' = {
  name: 'iothub-${suffix}'
  location: location
  sku: {
    name: 'S1'
    capacity: 1
  }
  properties: {}
}

output IOTHUB_NAME string = iothub.name
