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

// Confidential Ledger requires a globally unique name
// Use a hash-based suffix to ensure uniqueness
@description('A seed value used to calculate the unique string to append to the ledger name. Defaults to utcNow().')
param suffixSeed string = utcNow()
var ledgerName = take('mcptestacl${uniqueString(suffixSeed)}', 24)

resource confidentialLedger 'Microsoft.ConfidentialLedger/ledgers@2023-01-26-preview' = {
  name: ledgerName
  location: location
  properties: {
    ledgerType: 'Public'
    aadBasedSecurityPrincipals: [
      {
        principalId: testApplicationOid
        tenantId: tenantId
        ledgerRoleName: 'Administrator'
      }
    ]
  }
}

output CONFIDENTIAL_LEDGER_NAME string = confidentialLedger.name
output CONFIDENTIAL_LEDGER_URL string = 'https://${confidentialLedger.name}.confidential-ledger.azure.com'
