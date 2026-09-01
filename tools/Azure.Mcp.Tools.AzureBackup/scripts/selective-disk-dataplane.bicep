// Data-plane E2E infra: 3 Linux VMs sharing VNet + one RSV.
// Each VM gets 3 data disks (LUNs 0/1/2) so we can prove:
//   VM1: include LUN 0     → RP should contain OS + LUN 0
//   VM2: exclude LUN 1,2   → RP should contain OS + LUN 0
//   VM3: exclude-all       → RP should contain OS only
param location string = resourceGroup().location
param rsvName string
param vmNames array
param adminUser string
@secure()
param adminPass string

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: 'sd-vnet'
  location: location
  properties: {
    addressSpace: { addressPrefixes: [ '10.0.0.0/24' ] }
    subnets: [
      {
        name: 'default'
        properties: { addressPrefix: '10.0.0.0/26' }
      }
    ]
  }
}

resource nics 'Microsoft.Network/networkInterfaces@2024-01-01' = [for name in vmNames: {
  name: '${name}-nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: { id: vnet.properties.subnets[0].id }
          privateIPAllocationMethod: 'Dynamic'
        }
      }
    ]
  }
}]

resource vms 'Microsoft.Compute/virtualMachines@2024-03-01' = [for (name, i) in vmNames: {
  name: name
  location: location
  properties: {
    hardwareProfile: { vmSize: 'Standard_D2als_v7' }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-server-jammy'
        sku: '22_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        managedDisk: { storageAccountType: 'Standard_LRS' }
      }
      dataDisks: [
        {
          lun: 0
          name: '${name}-dd0'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
        {
          lun: 1
          name: '${name}-dd1'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
        {
          lun: 2
          name: '${name}-dd2'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
      ]
    }
    osProfile: {
      computerName: take(name, 15)
      adminUsername: adminUser
      adminPassword: adminPass
      linuxConfiguration: { disablePasswordAuthentication: false }
    }
    networkProfile: {
      networkInterfaces: [ { id: nics[i].id } ]
    }
  }
  tags: {
    Owner: 'azurebackup-mcp-selectivedisk-e2e'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}]

resource rsv 'Microsoft.RecoveryServices/vaults@2024-04-01' = {
  name: rsvName
  location: location
  sku: { name: 'RS0', tier: 'Standard' }
  identity: { type: 'SystemAssigned' }
  properties: {
    publicNetworkAccess: 'Enabled'
  }
  tags: {
    Owner: 'azurebackup-mcp-selectivedisk-e2e'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

resource rsvConfig 'Microsoft.RecoveryServices/vaults/backupconfig@2024-04-01' = {
  parent: rsv
  name: 'vaultconfig'
  properties: {
    storageModelType: 'LocallyRedundant'
  }
}

output rsvName string = rsv.name
output vmIds array = [for (name, i) in vmNames: vms[i].id]
