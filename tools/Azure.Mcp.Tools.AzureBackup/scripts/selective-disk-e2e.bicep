// Inline test infra for selective-disk backup E2E validation.
// Deployed by scripts/test-selective-disk-e2e.ps1 into a throwaway RG.
//   * Ubuntu 22.04 Standard_B1s VM with 3 data disks (LUNs 0, 1, 2)
//   * RSV LRS (fastest, no CRR replication overhead)
//   * No public IP / no NSG — pure ARM/RSV-plane test, no SSH needed.
param location string = resourceGroup().location
param vmName string
param rsvName string
param adminUser string
@secure()
param adminPass string

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: '${vmName}-vnet'
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

resource nic 'Microsoft.Network/networkInterfaces@2024-01-01' = {
  name: '${vmName}-nic'
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
}

resource vm 'Microsoft.Compute/virtualMachines@2024-03-01' = {
  name: vmName
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
          name: '${vmName}-dd0'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
        {
          lun: 1
          name: '${vmName}-dd1'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
        {
          lun: 2
          name: '${vmName}-dd2'
          createOption: 'Empty'
          diskSizeGB: 4
          managedDisk: { storageAccountType: 'Standard_LRS' }
        }
      ]
    }
    osProfile: {
      computerName: 'sdvm'
      adminUsername: adminUser
      adminPassword: adminPass
      linuxConfiguration: { disablePasswordAuthentication: false }
    }
    networkProfile: {
      networkInterfaces: [ { id: nic.id } ]
    }
  }
  tags: {
    Owner: 'azurebackup-mcp-selectivedisk-e2e'
    ServiceName: 'AzureBackup'
    Environment: 'Test'
  }
}

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

output vmId string = vm.id
output vmName string = vm.name
output rsvId string = rsv.id
output rsvName string = rsv.name
