targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The client OID to grant access to test resources.')
param testApplicationOid string

@description('Admin username for the VM.')
@secure()
param adminUsername string = 'azureuser'

@description('Admin password for the VM.')
@secure()
param adminPassword string = newGuid()

@description('The VM size to use for testing.')
param vmSize string = 'Standard_A1_v2'

// Virtual Network
resource vnet 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: '${baseName}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'default'
        properties: {
          addressPrefix: '10.0.0.0/24'
        }
      }
    ]
  }
}

// Network Interface for VM
resource nic 'Microsoft.Network/networkInterfaces@2023-05-01' = {
  name: '${baseName}-nic'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: vnet.properties.subnets[0].id
          }
          privateIPAllocationMethod: 'Dynamic'
        }
      }
    ]
  }
}

// Test Virtual Machine (Linux)
resource vm 'Microsoft.Compute/virtualMachines@2023-09-01' = {
  name: '${baseName}-vm'
  location: location
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-server-jammy'
        sku: '22_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        managedDisk: {
          storageAccountType: 'Standard_LRS'
        }
      }
    }
    osProfile: {
      computerName: '${baseName}-vm'
      adminUsername: adminUsername
      adminPassword: adminPassword
      linuxConfiguration: {
        disablePasswordAuthentication: false
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
          properties: {
            primary: true
          }
        }
      ]
    }
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// Second VM for multi-VM list testing
resource vm2 'Microsoft.Compute/virtualMachines@2023-09-01' = {
  name: '${baseName}-vm2'
  location: location
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_D2s_v3'
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer: '0001-com-ubuntu-server-jammy'
        sku: '22_04-lts-gen2'
        version: 'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        managedDisk: {
          storageAccountType: 'Standard_LRS'
        }
      }
    }
    osProfile: {
      computerName: '${baseName}-vm2'
      adminUsername: adminUsername
      adminPassword: adminPassword
      linuxConfiguration: {
        disablePasswordAuthentication: false
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: nic2.id
          properties: {
            primary: true
          }
        }
      ]
    }
  }
  tags: {
    environment: 'test'
    purpose: 'mcp-testing'
  }
}

// Network Interface for second VM
resource nic2 'Microsoft.Network/networkInterfaces@2023-05-01' = {
  name: '${baseName}-nic2'
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: vnet.properties.subnets[0].id
          }
          privateIPAllocationMethod: 'Dynamic'
        }
      }
    ]
  }
}

// Virtual Machine Scale Set for VMSS testing
resource vmss 'Microsoft.Compute/virtualMachineScaleSets@2023-09-01' = {
  name: '${baseName}-vmss'
  location: location
  sku: {
    name: vmSize
    tier: 'Standard'
    capacity: 2
  }
  properties: {
    overprovision: false
    upgradePolicy: {
      mode: 'Manual'
    }
    virtualMachineProfile: {
      storageProfile: {
        imageReference: {
          publisher: 'Canonical'
          offer: '0001-com-ubuntu-server-jammy'
          sku: '22_04-lts-gen2'
          version: 'latest'
        }
        osDisk: {
          createOption: 'FromImage'
          managedDisk: {
            storageAccountType: 'Standard_LRS'
          }
        }
      }
      osProfile: {
        computerNamePrefix: '${baseName}-'
        adminUsername: adminUsername
        adminPassword: adminPassword
        linuxConfiguration: {
          disablePasswordAuthentication: false
        }
      }
      networkProfile: {
        networkInterfaceConfigurations: [
          {
            name: 'vmssnic'
            properties: {
              primary: true
              ipConfigurations: [
                {
                  name: 'vmssipconfig'
                  properties: {
                    subnet: {
                      id: vnet.properties.subnets[0].id
                    }
                  }
                }
              ]
            }
          }
        ]
      }
    }
  }
}

// Virtual Machine Contributor role for managing VMs
resource vmContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Virtual Machine Contributor role
  // Lets you manage virtual machines, but not access to them, and not the virtual network or storage account they're connected to
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#virtual-machine-contributor
  name: '9980e02c-c2be-4d73-94e8-173b1dc7cf3c'
}

// Assign Virtual Machine Contributor role to test application
resource appVmContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vmContributorRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: vmContributorRoleDefinition.id
    description: 'Virtual Machine Contributor for testApplicationOid'
  }
}

// Reader role for querying VM information
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // This is the Reader role
  // View all resources, but does not allow you to make any changes
  // See https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#reader
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

// Assign Reader role to test application for resource group
resource appReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
    description: 'Reader for testApplicationOid'
  }
}

// Output values for test consumption
output vmName string = vm.name
output vm2Name string = vm2.name
output vmssName string = vmss.name
output vnetName string = vnet.name
output resourceGroupName string = resourceGroup().name
