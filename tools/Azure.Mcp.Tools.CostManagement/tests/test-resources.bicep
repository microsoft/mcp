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

output location string = location
