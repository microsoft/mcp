// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Bicep template for Azure Load Testing infrastructure.
// Provisions:
//   - Azure Load Testing resource (long-lived, reused across runs)
//   - Log Analytics workspace (long-lived)
//   - Container App Environment (long-lived)
//   - Container App running the MCP Server with auth disabled (ephemeral, per-run)

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Name of the Azure Load Testing resource.')
param loadTestName string = 'mcp-load-test'

@description('Fully qualified Docker image reference for the MCP Server.')
param mcpServerImage string

@description('Name of the Container App (unique per pipeline run).')
param containerAppName string

@description('Name of the Azure Container Registry (without .azurecr.io).')
param acrName string

// ---------- Log Analytics ----------
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${loadTestName}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ---------- Container App Environment ----------
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${loadTestName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// ---------- ACR + AcrPull role assignment ----------
// Reference the existing Container Registry so we can grant AcrPull to the
// Container App's managed identity. A user-assigned identity is used (instead
// of system-assigned) so the role assignment can be created BEFORE the
// Container App attempts its first image pull.
resource acrRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' existing = {
  name: acrName
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${containerAppName}-uami'
  location: location
}

// AcrPull built-in role:
// https://learn.microsoft.com/azure/role-based-access-control/built-in-roles#acrpull
resource acrPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acrRegistry.id, containerAppIdentity.id, acrPullRoleDefinition.id)
  scope: acrRegistry
  properties: {
    principalId: containerAppIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: acrPullRoleDefinition.id
    description: 'AcrPull for MCP load-test Container App user-assigned identity'
  }
}

// ---------- Container App (MCP Server – auth disabled) ----------
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: '${acrName}.azurecr.io'
          identity: containerAppIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'azmcp'
          image: mcpServerImage
          command: [
            './azmcp'
            'server'
            'start'
            '--transport'
            'http'
            '--port'
            '8080'
            '--dangerously-disable-http-incoming-auth'
          ]
          resources: {
            cpu: json('1.0')
            memory: '2Gi'
          }
          probes: [
            {
              // Startup probe gives the .NET runtime time to JIT, initialize
              // the MCP server, and start serving /health before readiness /
              // liveness probes begin. failureThreshold * periodSeconds =
              // 30 * 5s = 150s max startup window.
              type: 'Startup'
              httpGet: {
                path: '/health'
                port: 8080
              }
              periodSeconds: 5
              failureThreshold: 30
              timeoutSeconds: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 0
              periodSeconds: 10
            }
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              initialDelaySeconds: 0
              periodSeconds: 30
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  dependsOn: [
    acrPullRoleAssignment
  ]
}

// ---------- Azure Load Testing ----------
resource loadTest 'Microsoft.LoadTestService/loadTests@2022-12-01' = {
  name: loadTestName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
}

// ---------- Outputs ----------
@description('FQDN of the deployed MCP Server Container App.')
output mcpServerFqdn string = containerApp.properties.configuration.ingress.fqdn

@description('Resource ID of the Azure Load Testing resource.')
output loadTestResourceId string = loadTest.id
