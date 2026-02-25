// Live test runs require a resource file, so we use an empty one here.
targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Provided by the test framework when running live tests.')
param baseName string

@description('The client OID to grant access to test resources. Defaults to the OID of the user running the deployment.')
param testApplicationOid string = deployer().objectId

@description('The location to deploy test resources to. Defaults to the resource group location.')
var location string = resourceGroup().location

@description('The tenant ID to use for test resources. Defaults to the subscription tenant ID.')
var tenantId string = subscription().tenantId

// Add any additional resources and role assignments needed for live tests here.

// Outputs will be available in test-resources-post.ps1
output location string = location

// Output keys will be in uppercase
// $DeploymentOutputs.LOCATION
