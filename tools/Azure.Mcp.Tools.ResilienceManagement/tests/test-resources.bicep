targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Usage plans are the only Resilience Management resource that is resource-group scoped and therefore
// provisionable here. They are global resources under the Microsoft.AzureResilienceManagement provider.
resource usagePlan 'Microsoft.AzureResilienceManagement/usagePlans@2026-04-01-preview' = {
  name: baseName
  location: 'global'
  properties: {
    planType: 'Standard'
  }
}

// Reader role so the test application identity can list/get the usage plan during recording.
resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Reader - View all resources, but does not allow you to make any changes.
  // See https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#reader
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

// principalType is intentionally omitted so the assignment works whether testApplicationOid is a User
// (local deployments) or a ServicePrincipal (CI). Both are existing principals, so ARM does not need
// the type hint to avoid AAD replication races.
resource appReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, resourceGroup().id)
  scope: resourceGroup()
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
  }
}

// The goal, drill, and recovery commands operate against a tenant-level
// Microsoft.Management/serviceGroups hierarchy that cannot be created from a resource-group-scoped
// deployment. Provision the service group and its goal templates out-of-band (or in a separate
// tenant-scoped deployment) and surface its name via test-resources-post.ps1 so the live tests can
// reference it through Settings.ResourceBaseName.

output usagePlanName string = usagePlan.name
