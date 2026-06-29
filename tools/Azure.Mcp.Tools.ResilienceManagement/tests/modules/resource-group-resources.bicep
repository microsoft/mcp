targetScope = 'resourceGroup'

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

// Simple ZRS storage account so the resource group has a member resource.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_ZRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    accessTier: 'Hot'
  }
}

// Add the resource group as a member of the service group.
resource resourceGroupMembership 'Microsoft.Relationships/serviceGroupMember@2023-09-01-preview' = {
  name: 'rhub-rg-member'
  properties: {
    targetId: serviceGroupId
  }
}

// Standard usage plan in the resource group.
resource usagePlan 'Microsoft.AzureResilienceManagement/usagePlans@2026-04-01-preview' = {
  name: usagePlanName
  location: location
  properties: {
    planType: 'Standard'
  }
  tags: {
    purpose: 'rhub-live-test'
    createdBy: 'bicep'
  }
  dependsOn: [
    resourceGroupMembership
  ]
}

// Enroll the service group using this usage plan.
resource enrollment 'Microsoft.AzureResilienceManagement/usagePlans/enrollments@2026-04-01-preview' = {
  parent: usagePlan
  name: enrollmentName
  properties: {
    serviceGroupId: serviceGroupId
  }
}

output storageAccountId string = storageAccount.id
output usagePlanId string = usagePlan.id
output enrollmentId string = enrollment.id
