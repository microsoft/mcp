targetScope = 'subscription'

@description('Resource group name for live test resources.')
param resourceGroupName string

@description('Azure location for the test resources.')
param location string

@description('ARM resource ID of the service group these resources belong to.')
param serviceGroupId string

@description('Storage account name. Must be globally unique, lowercase, 3-24 chars.')
param storageAccountName string

@description('Usage plan name. Must match ^[a-zA-Z0-9-]{3,24}$')
param usagePlanName string

@description('Enrollment name. Must match ^[a-zA-Z0-9-]{3,24}$')
param enrollmentName string

// Create the test resource group.
resource testRg 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    purpose: 'rhub-live-test'
    createdBy: 'bicep'
  }
}

// Deploy the resource-group-scoped resources into the new resource group.
module rgResources 'resource-group-resources.bicep' = {
  name: 'rhub-rg-resources'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    serviceGroupId: serviceGroupId
    storageAccountName: storageAccountName
    usagePlanName: usagePlanName
    enrollmentName: enrollmentName
  }
  dependsOn: [
    testRg
  ]
}

output resourceGroupId string = testRg.id
output storageAccountId string = rgResources.outputs.storageAccountId
output usagePlanId string = rgResources.outputs.usagePlanId
output enrollmentId string = rgResources.outputs.enrollmentId
