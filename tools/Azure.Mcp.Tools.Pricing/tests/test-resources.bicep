// Live test runs require a resource file, so we use an empty one here.
targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name.')
param baseName string

@description('The client OID to grant access to test resources.')
param testApplicationOid string = deployer().objectId

var location string = resourceGroup().location
var tenantId string = subscription().tenantId

// The Azure Retail Prices API is public and does not require any Azure resources.
// This file is kept minimal to satisfy the test resource deployment requirements.


// Outputs will be available in test-resources-post.ps1
output location string = location

// Their keys will be uppercase
// $DeploymentOutputs.LOCATION
