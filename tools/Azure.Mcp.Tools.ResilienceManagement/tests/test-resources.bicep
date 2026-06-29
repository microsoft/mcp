targetScope = 'resourceGroup'

// Deterministic, schema-valid names.
// Usage plan and enrollment names must match ^[a-zA-Z0-9-]{3,24}$.
var uniqueSuffix = uniqueString(resourceGroup().id)
var usagePlanName = take('up${uniqueSuffix}', 24)
var enrollmentName = take('en${uniqueSuffix}', 24)
var serviceGroupName = 'sgr${uniqueSuffix}'
var goalTemplateName = take('gt${uniqueSuffix}', 24)
var goalAssignmentName = take('ga${uniqueSuffix}', 24)
var recoveryPlanName = take('rp${uniqueSuffix}', 24)
var drillName = take('dr${uniqueSuffix}', 24)
var storageAccountName = toLower(take('st${uniqueSuffix}', 24))

// The test identity is automatically granted access to this resource group by the
// test harness (New-TestResources.ps1), so no explicit role assignment is created here.

// The following resilience resources are NOT created here because they are tenant-scoped
// or hang off the tenant-scoped service group, which cannot be expressed in this
// resource-group-scoped deployment. They are created via direct ARM REST PUTs in
// test-resources-post.ps1 (which only needs serviceGroups write, not tenant deployment write):
//  - Microsoft.Management/serviceGroups (the service group itself)
//  - the resource group -> service group membership
//  - the usage plan enrollment
//  - goal template, goal assignment, recovery plan and drill (extension resources on the service group)

// Storage account (resource-group scoped) so the service group has a member resource that
// can surface as a goal/recovery/drill resource target during live tests.
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

output usagePlanName string = usagePlanName
output enrollmentName string = enrollmentName
output serviceGroupName string = serviceGroupName
output goalTemplateName string = goalTemplateName
output goalAssignmentName string = goalAssignmentName
output recoveryPlanName string = recoveryPlanName
output drillName string = drillName
output storageAccountName string = storageAccountName
output location string = resourceGroup().location
