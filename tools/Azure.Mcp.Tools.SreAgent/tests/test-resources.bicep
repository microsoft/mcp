targetScope = 'resourceGroup'

@minLength(3)
@maxLength(24)
@description('The base resource name. Used as the agent name and as a suffix for supporting resources.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
@allowed([
  'swedencentral'
  'uksouth'
  'eastus2'
  'australiaeast'
])
param location string = 'eastus2'

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// ─────────────────────────────────────────────────────────────────────────────
// Supporting observability resources — used as connector data sources and as
// the agent's own telemetry sink.
// ─────────────────────────────────────────────────────────────────────────────

var suffix = uniqueString(resourceGroup().id, baseName)

resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'law-${suffix}'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai-${suffix}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'SreAgent'
    WorkspaceResourceId: law.id
  }
}

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
    knowledgeGraphConfiguration: {
      managedResources: [
        resourceGroup().id
      ]
    }
    actionConfiguration: {
      accessLevel: 'Low'
      mode: 'Review'
    }
    logConfiguration: {
      applicationInsightsConfiguration: {
        appId: appInsights.properties.AppId
        connectionString: appInsights.properties.ConnectionString
      }
    }
    upgradeChannel: 'Preview'
    monthlyAgentUnitLimit: 10000
    defaultModel: {
      provider: 'Anthropic'
      name: 'Automatic'
    }
    experimentalSettings: {
      EnableWorkspaceTools: true
      EnableHttpTriggers: true
      EnableV2AgentLoop: true
    }
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// Built-in connectors so connectors_list returns non-empty results.
// Shape mirrors microsoft/sre-agent sreagent-templates/bicep/agent-extensions.bicep
// for api-version 2025-05-01-preview.
// ─────────────────────────────────────────────────────────────────────────────

#disable-next-line BCP081
resource appInsightsConnector 'Microsoft.App/agents/connectors@2025-05-01-preview' = {
  parent: sreAgent
  name: 'app-insights'
  properties: {
    dataConnectorType: 'AppInsights'
    dataSource: appInsights.id
    extendedProperties: {
      armResourceId: appInsights.id
      resource: { name: appInsights.name }
      appId: appInsights.properties.AppId
    }
    identity: 'system'
  }
}

#disable-next-line BCP081
resource logAnalyticsConnector 'Microsoft.App/agents/connectors@2025-05-01-preview' = {
  parent: sreAgent
  name: 'log-analytics'
  properties: {
    dataConnectorType: 'LogAnalytics'
    dataSource: law.id
    extendedProperties: {
      armResourceId: law.id
      resource: { name: law.name }
    }
    identity: 'system'
  }
  dependsOn: [
    appInsightsConnector
  ]
}

#disable-next-line BCP081
resource azureMonitorConnector 'Microsoft.App/agents/connectors@2025-05-01-preview' = {
  parent: sreAgent
  name: 'azure-monitor'
  properties: {
    dataConnectorType: 'AzureMonitor'
    dataSource: subscription().id
    extendedProperties: {
      armResourceId: subscription().id
      lookbackDays: 7
    }
    identity: 'system'
  }
  dependsOn: [
    logAnalyticsConnector
  ]
}

// ─────────────────────────────────────────────────────────────────────────────
// Built-in common prompt so commonprompts_list returns non-empty results.
// ARM sub-resource uses a base64 envelope (matches agent-extensions.bicep).
// ─────────────────────────────────────────────────────────────────────────────

#disable-next-line BCP081
resource safetyRulesPrompt 'Microsoft.App/agents/commonPrompts@2025-05-01-preview' = {
  parent: sreAgent
  name: 'safety-rules'
  properties: {
    value: base64(string({
      prompt: '## Safety rules\n\n- Never restart services without paging the on-call.\n- Always confirm subscription before destructive ops.\n- For any High accessLevel action, require human review even if actionMode=Automatic.'
    }))
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// RBAC for the test application:
//   - Reader on the agent (ARM control plane reads).
//   - SRE Agent Administrator on the agent (data-plane *.azuresre.ai access
//     required by all azmcp sreagent commands).
// scheduledTasks / incidentFilters are intentionally not provisioned here.
// agent-extensions.bicep documents that those sub-resources hit a different
// code path and are flaky when deployed via Bicep; tests that list them
// against an empty agent still pass since assertions only require non-null.
// ─────────────────────────────────────────────────────────────────────────────

resource readerRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // Reader
  // https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#reader
  name: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

resource sreAgentAdminRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  // SRE Agent Administrator — grants full data-plane access on the agent.
  name: 'e79298df-d852-4c6d-84f9-5d13249d1e55'
}

resource readerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(readerRoleDefinition.id, testApplicationOid, sreAgent.id)
  scope: sreAgent
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: readerRoleDefinition.id
    description: 'Reader on the SRE Agent for the test application OID.'
  }
}

resource sreAgentAdminAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sreAgentAdminRoleDefinition.id, testApplicationOid, sreAgent.id)
  scope: sreAgent
  properties: {
    principalId: testApplicationOid
    roleDefinitionId: sreAgentAdminRoleDefinition.id
    description: 'SRE Agent Administrator on the SRE Agent for the test application OID (data-plane access).'
  }
}

output AZURE_MCP_SREAGENT_NAME string = sreAgent.name
output AZURE_MCP_SREAGENT_RESOURCE_GROUP string = resourceGroup().name
#disable-next-line BCP081
output AZURE_MCP_SREAGENT_ENDPOINT string = sreAgent.properties.agentEndpoint
