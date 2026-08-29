targetScope = 'resourceGroup'

param baseName string = resourceGroup().name

// Deterministic, schema-valid names.
// Usage plan and enrollment names must match ^[a-zA-Z0-9-]{3,24}$.
var uniqueSuffix = uniqueString(resourceGroup().id, baseName)
var usagePlanName = take('up${uniqueSuffix}', 24)
var enrollmentName = take('en${uniqueSuffix}', 24)
var serviceGroupName = 'sgr${uniqueSuffix}'
var lifecycleEnrollmentName = take('el${uniqueSuffix}', 24)
var lifecycleServiceGroupName = take('sgl${uniqueSuffix}', 24)
var goalTemplateName = take('gt${uniqueSuffix}', 24)
var goalAssignmentName = take('ga${uniqueSuffix}', 24)
var recoveryPlanName = take('rp${uniqueSuffix}', 24)
var drillName = take('drd${uniqueSuffix}', 24)
var storageAccountName = toLower(take('st${uniqueSuffix}', 24))
var vnetName = take('vnet${uniqueSuffix}', 24)
var nicName = take('nic${uniqueSuffix}', 24)
var vmName = take('vm${uniqueSuffix}', 15)
// westus has no availability zones; a Zonal drill needs a zonal target, so place the VM and its
// network in a zone-capable, Chaos-supported region.
var vmLocation = 'eastus2'
var vmAdminPassword = 'Pw1!${uniqueString(resourceGroup().id, baseName)}'

// The test identity is automatically granted access to this resource group by the
// test harness (New-TestResources.ps1), so no explicit role assignment is created here.

// The following resilience resources are NOT created here because they are tenant-scoped
// or hang off the tenant-scoped service group, which cannot be expressed in this
// resource-group-scoped deployment. They are created via direct ARM REST PUTs in
// test-resources-post.ps1 (which only needs serviceGroups write, not tenant deployment write):
//  - Microsoft.Management/serviceGroups (the service group itself)
//  - the resource group -> service group membership
//  - the usage plan enrollment
//  - goal template, goal assignment and recovery plan (extension resources on the service group)

// Storage account (resource-group scoped) so the service group has a member resource that
// can surface as a goal/recovery resource target during live tests.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: resourceGroup().location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

// Usage plan (resource-group scoped). This resource type is only available in the 'global' location.
resource usagePlan 'Microsoft.AzureResilienceManagement/usagePlans@2026-04-01-preview' = {
  name: usagePlanName
  location: 'global'
  properties: {
    planType: 'Standard'
  }
}

// The drill is Zonal, so it needs a zonal target with a real native Chaos fault. A zonal VM
// (Microsoft.Compute/virtualMachines) is the simplest such target: it has the native shutdown
// fault (no agent required), so it can be force-included in the drill and used to start a run.
resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: vmLocation
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

resource nic 'Microsoft.Network/networkInterfaces@2023-09-01' = {
  name: nicName
  location: vmLocation
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: '${vnet.id}/subnets/default'
          }
          privateIPAllocationMethod: 'Dynamic'
        }
      }
    ]
  }
}

resource vm 'Microsoft.Compute/virtualMachines@2024-07-01' = {
  name: vmName
  location: vmLocation
  zones: [
    '1'
  ]
  properties: {
    hardwareProfile: {
      vmSize: 'Standard_D2als_v6'
    }
    osProfile: {
      computerName: vmName
      adminUsername: 'azureuser'
      adminPassword: vmAdminPassword
      linuxConfiguration: {
        disablePasswordAuthentication: false
      }
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
    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
        }
      ]
    }
  }
}

output usagePlanName string = usagePlanName
output enrollmentName string = enrollmentName
output serviceGroupName string = serviceGroupName
output lifecycleEnrollmentName string = lifecycleEnrollmentName
output lifecycleServiceGroupName string = lifecycleServiceGroupName
output goalTemplateName string = goalTemplateName
output goalAssignmentName string = goalAssignmentName
output recoveryPlanName string = recoveryPlanName
output drillName string = drillName
output storageAccountName string = storageAccountName
output storageAccountId string = storageAccount.id
output vmName string = vmName
output vmId string = vm.id
output location string = resourceGroup().location
