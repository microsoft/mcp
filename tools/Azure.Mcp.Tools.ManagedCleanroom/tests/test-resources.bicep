targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Must be between 3 and 24 characters.')
param baseName string

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// NOTE: Azure Cleanroom Analytics Frontend is not a first-class ARM resource type.
// Live tests expect the CLEANROOM_ENDPOINT output to be supplied externally
// (e.g., via the post-deployment script reading an existing service endpoint).
@description('The Azure Cleanroom Analytics Frontend endpoint URL to test against.')
param cleanroomEndpoint string = ''

@description('A known collaboration ID to use in live tests (collaborations get, analytics get, oidc issuer-info).')
param cleanroomCollaborationId string = ''

resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
	scope: subscription()
	name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

resource testAppReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
	name: guid(resourceGroup().id, testApplicationOid, readerRoleDefinition.id)
	scope: resourceGroup()
	properties: {
		principalId: testApplicationOid
		roleDefinitionId: readerRoleDefinition.id
		description: 'Reader role assignment for managed cleanroom test application identity'
	}
}

output CLEANROOM_ENDPOINT string = cleanroomEndpoint
output CLEANROOM_COLLABORATION_ID string = cleanroomCollaborationId
output CLEANROOM_BASE_NAME string = baseName
output CLEANROOM_LOCATION string = location
output CLEANROOM_TENANT_ID string = tenantId
