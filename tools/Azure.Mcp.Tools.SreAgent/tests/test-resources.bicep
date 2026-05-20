targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Used as the agent name.')
param baseName string = resourceGroup().name

@description('The location of the resource.')
@allowed([
  'swedencentral'
  'uksouth'
  'eastus2'
  'australiaeast'
])
param location string = 'eastus2'

// ─────────────────────────────────────────────────────────────────────────────
// SRE Agent (system-assigned identity).
// ─────────────────────────────────────────────────────────────────────────────

#disable-next-line BCP081
resource sreAgent 'Microsoft.App/agents@2025-05-01-preview' = {
  name: baseName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    upgradeChannel: 'Preview'
    monthlyAgentUnitLimit: 10000
    defaultModel: {
      provider: 'Anthropic'
      name: 'Automatic'
    }
  }
}

output AZURE_MCP_SREAGENT_NAME string = sreAgent.name
output AZURE_MCP_SREAGENT_RESOURCE_GROUP string = resourceGroup().name
#disable-next-line BCP081
output AZURE_MCP_SREAGENT_ENDPOINT string = sreAgent.properties.agentEndpoint
