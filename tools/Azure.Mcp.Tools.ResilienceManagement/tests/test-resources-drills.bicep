// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore mcpfixture

// Deployed by test-resources.bicep into the dedicated drills resource group created by
// test-resources-pre.ps1. Keeping these resources out of the main group stops the general
// service group from discovering the fault-injection VM.
targetScope = 'resourceGroup'

param baseName string
param adminUsername string = 'mcpfixture'
@secure()
param adminPassword string = 'P${newGuid()}!'
param location string = resourceGroup().location
param healthModelLocation string = 'centralus'
param deploySliWorkspace bool = false
param vmSize string = 'Standard_D2s_v5'
param drillName string = ''

var suffix = uniqueString(resourceGroup().id, baseName)
var effectiveDrillName = empty(drillName) ? 'dr-${suffix}' : drillName
var fixtureTags = {
  purpose: 'mcp-drills-ga-live-tests'
}

resource network 'Microsoft.Network/virtualNetworks@2024-05-01' = {
  name: 'vnet-${suffix}'
  location: location
  tags: fixtureTags
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.214.0.0/24'
      ]
    }
    subnets: [
      {
        name: 'fixture'
        properties: {
          addressPrefix: '10.214.0.0/24'
          defaultOutboundAccess: false
        }
      }
    ]
  }
}

resource nic 'Microsoft.Network/networkInterfaces@2024-05-01' = {
  name: 'nic-${suffix}'
  location: location
  tags: fixtureTags
  properties: {
    ipConfigurations: [
      {
        name: 'private'
        properties: {
          privateIPAllocationMethod: 'Dynamic'
          subnet: {
            id: '${network.id}/subnets/fixture'
          }
        }
      }
    ]
  }
}

// Only this private, dedicated VM may be included in lifecycle fault injection.
resource vm 'Microsoft.Compute/virtualMachines@2024-07-01' = {
  name: 'vm-${suffix}'
  location: location
  zones: [
    '1'
  ]
  tags: fixtureTags
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
        deleteOption: 'Delete'
        managedDisk: {
          storageAccountType: 'StandardSSD_LRS'
        }
      }
    }
    osProfile: {
      computerName: 'drill-fixture'
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
            deleteOption: 'Delete'
          }
        }
      ]
    }
  }
}

resource automation 'Microsoft.Automation/automationAccounts@2023-11-01' = {
  name: 'aa-${suffix}'
  location: location
  tags: fixtureTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    disableLocalAuth: true
    publicNetworkAccess: false
    sku: {
      name: 'Basic'
    }
  }
}

resource vmOperator 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(vm.id, automation.id, 'Virtual Machine Contributor')
  scope: vm
  properties: {
    principalId: automation.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '9980e02c-c2be-4d73-94e8-173b1dc7cf3c')
  }
}

resource usagePlan 'Microsoft.AzureResilienceManagement/usagePlans@2026-10-01' = {
  name: 'up-${suffix}'
  location: 'global'
  tags: fixtureTags
  properties: {
    planType: 'Standard'
  }
}

resource healthModel 'Microsoft.CloudHealth/healthmodels@2026-05-01-preview' = {
  name: 'hm-${suffix}'
  location: healthModelLocation
  tags: fixtureTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}

resource healthReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, healthModel.id, 'Reader')
  properties: {
    principalId: healthModel.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')
  }
}

resource workspace 'Microsoft.Monitor/accounts@2023-04-03' = if (deploySliWorkspace) {
  name: 'amw-${suffix}'
  location: location
  tags: fixtureTags
  properties: {
    publicNetworkAccess: 'Enabled'
  }
}

resource sliIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'sli-${suffix}'
  location: location
  tags: fixtureTags
}

output usagePlanId string = usagePlan.id
output usagePlanName string = usagePlan.name
output serviceGroupName string = 'sgd-${suffix}'
output lifecycleServiceGroupName string = 'sgl-${suffix}'
output recoveryPlanName string = 'rp-${suffix}'
output drillName string = effectiveDrillName
output vmId string = vm.id
output automationAccountId string = automation.id
output healthModelId string = healthModel.id
output monitorWorkspaceId string? = deploySliWorkspace ? workspace!.id : null
output sliIdentityId string = sliIdentity.id
output location string = location
