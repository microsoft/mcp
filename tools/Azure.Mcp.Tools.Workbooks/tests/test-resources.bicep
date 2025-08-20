targetScope = 'resourceGroup'

@minLength(4)
@maxLength(24)
@description('The base resource name.')
param baseName string = resourceGroup().name

@description('The location of the resource. By default, this is the same as the resource group.')
param location string = resourceGroup().location

@description('The tenant ID to which the application and resources belong.')
param tenantId string = '72f988bf-86f1-41af-91ab-2d7cd011db47'

@description('The client OID to grant access to test resources.')
param testApplicationOid string

// Reference the existing Log Analytics workspace created by the monitor module
resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: baseName
}

// Basic Azure Monitor Workbook
resource basicMonitoringWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-basic-monitoring')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Basic Monitoring Dashboard'
    description: 'Basic monitoring workbook for testing Azure MCP workbook operations'
    category: 'workbook'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Basic Monitoring Dashboard\n\nThis workbook provides basic monitoring capabilities for testing purposes.'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'Heartbeat\n| where TimeGenerated > ago(1h)\n| summarize count() by Computer\n| order by count_ desc'
            size: 0
            title: 'Heartbeat Count by Computer (Last Hour)'
            timeContext: {
              durationMs: 3600000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'table'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'AzureActivity\n| where TimeGenerated > ago(24h)\n| summarize count() by ActivityStatus\n| render piechart'
            size: 0
            title: 'Azure Activity Status (Last 24 Hours)'
            timeContext: {
              durationMs: 86400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'piechart'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
    tags: {
      purpose: 'testing'
      environment: 'development'
      project: 'azure-mcp'
    }
  }
}

// Performance Monitoring Workbook
resource performanceWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-performance')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Performance Monitoring'
    description: 'Performance monitoring workbook for testing Azure MCP performance metrics'
    category: 'workbook'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Performance Monitoring\n\nThis workbook tracks performance metrics for testing purposes.'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'Perf\n| where TimeGenerated > ago(4h)\n| where CounterName == "% Processor Time"\n| summarize avg(CounterValue) by Computer, bin(TimeGenerated, 10m)\n| render timechart'
            size: 0
            title: 'CPU Usage Over Time'
            timeContext: {
              durationMs: 14400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'timechart'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'Perf\n| where TimeGenerated > ago(4h)\n| where CounterName == "Available MBytes"\n| summarize avg(CounterValue) by Computer, bin(TimeGenerated, 10m)\n| render timechart'
            size: 0
            title: 'Available Memory Over Time'
            timeContext: {
              durationMs: 14400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'timechart'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
    tags: {
      purpose: 'testing'
      environment: 'development'
      project: 'azure-mcp'
      category: 'performance'
    }
  }
}

// Security Monitoring Workbook
resource securityWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-security')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Security Monitoring'
    description: 'Security monitoring workbook for testing Azure MCP security insights'
    category: 'workbook'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Security Monitoring\n\nThis workbook provides security insights for testing purposes.'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'SecurityEvent\n| where TimeGenerated > ago(24h)\n| summarize count() by EventID\n| order by count_ desc\n| take 10'
            size: 0
            title: 'Top 10 Security Events (Last 24 Hours)'
            timeContext: {
              durationMs: 86400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'table'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'AzureActivity\n| where TimeGenerated > ago(24h)\n| where ActivityStatus == "Failed"\n| summarize count() by Caller\n| order by count_ desc'
            size: 0
            title: 'Failed Azure Activities by Caller'
            timeContext: {
              durationMs: 86400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'table'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
    tags: {
      purpose: 'testing'
      environment: 'development'
      project: 'azure-mcp'
      category: 'security'
    }
  }
}

// Application Insights Workbook (if Application Insights is available)
resource applicationInsightsWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-app-insights')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Application Insights'
    description: 'Application insights workbook for testing Azure MCP application monitoring'
    category: 'workbook'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Application Insights\n\nThis workbook provides application monitoring capabilities for testing purposes.'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'requests\n| where timestamp > ago(24h)\n| summarize count() by bin(timestamp, 1h)\n| render timechart'
            size: 0
            title: 'Request Count Over Time'
            timeContext: {
              durationMs: 86400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'timechart'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'exceptions\n| where timestamp > ago(24h)\n| summarize count() by type\n| order by count_ desc'
            size: 0
            title: 'Exception Types (Last 24 Hours)'
            timeContext: {
              durationMs: 86400000
            }
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'table'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
    tags: {
      purpose: 'testing'
      environment: 'development'
      project: 'azure-mcp'
      category: 'application'
    }
  }
}

// Simple Test Workbook for CRUD operations
resource simpleTestWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-simple-test')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Simple Test Workbook'
    description: 'Simple workbook for testing basic CRUD operations with Azure MCP'
    category: 'workbook'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Simple Test Workbook\n\nThis is a simple workbook for testing basic operations.\n\n## Purpose\nThis workbook is designed to test:\n- Create operations\n- Read operations\n- Update operations\n- Delete operations\n\n## Test Data\nThis workbook contains minimal test data for validation purposes.'
          }
        }
        {
          type: 9
          content: {
            version: 'ParametersItem/1.0'
            parameters: [
              {
                id: 'timeRange'
                version: 'KqlParameterItem/1.0'
                name: 'TimeRange'
                type: 4
                value: {
                  durationMs: 3600000
                }
                typeSettings: {
                  selectableValues: [
                    {
                      durationMs: 3600000
                    }
                    {
                      durationMs: 14400000
                    }
                    {
                      durationMs: 43200000
                    }
                    {
                      durationMs: 86400000
                    }
                  ]
                }
              }
            ]
            style: 'pills'
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
          }
        }
        {
          type: 3
          content: {
            version: 'KqlItem/1.0'
            query: 'print "Test Query Result", now(), "Azure MCP Workbook Test"'
            size: 0
            title: 'Test Query'
            timeContext: {
              durationMs: 0
            }
            timeContextFromParameter: 'TimeRange'
            queryType: 0
            resourceType: 'microsoft.operationalinsights/workspaces'
            visualization: 'table'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
    tags: {
      purpose: 'testing'
      environment: 'development'
      project: 'azure-mcp'
      category: 'crud-test'
    }
  }
}

// User workbook for testing different kind
// TODO: kind'user' is unsupported.  The only valid value is 'shared'.
// resource userWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
//   name: guid('${baseName}-user-workbook')
//   location: location
//   kind: 'user'
//   properties: {
//     displayName: '${baseName} User Workbook'
//     description: 'User workbook for testing different kind filter'
//     category: 'workbook'
//     sourceId: workspace.id
//     serializedData: string({
//       version: 'Notebook/1.0'
//       items: [
//         {
//           type: 1
//           content: {
//             json: '# User Workbook\n\nThis is a user workbook for testing kind filters.'
//           }
//         }
//       ]
//       styleSettings: {}
//       '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
//     })
//   }
//   tags: {
//     purpose: 'testing'
//     environment: 'development'
//     project: 'azure-mcp'
//     category: 'filter-test'
//   }
// }

// Sentinel workbook for testing different category
resource sentinelWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-sentinel-workbook')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} Sentinel Workbook'
    description: 'Sentinel workbook for testing different category filter'
    category: 'sentinel'
    sourceId: workspace.id
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# Sentinel Workbook\n\nThis is a Sentinel workbook for testing category filters.'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
  }
  tags: {
    purpose: 'testing'
    environment: 'development'
    project: 'azure-mcp'
    category: 'filter-test'
  }
}

// TSG workbook for testing different category
resource tsgWorkbook 'Microsoft.Insights/workbooks@2023-06-01' = {
  name: guid('${baseName}-tsg-workbook')
  location: location
  kind: 'shared'
  properties: {
    displayName: '${baseName} TSG Workbook'
    description: 'TSG workbook for testing different category filter'
    category: 'TSG'
    sourceId: 'azure monitor'
    serializedData: string({
      version: 'Notebook/1.0'
      items: [
        {
          type: 1
          content: {
            json: '# TSG Workbook\n\nThis is a TSG workbook for testing category and source ID filters.'
          }
        }
      ]
      styleSettings: {}
      '$schema': 'https://github.com/Microsoft/Application-Insights-Workbooks/blob/master/schema/workbook.json'
    })
  }
  tags: {
    purpose: 'testing'
    environment: 'development'
    project: 'azure-mcp'
    category: 'filter-test'
  }
}

// Output the workbook IDs for testing purposes
output basicMonitoringWorkbookId string = basicMonitoringWorkbook.id
output performanceWorkbookId string = performanceWorkbook.id
output securityWorkbookId string = securityWorkbook.id
output applicationInsightsWorkbookId string = applicationInsightsWorkbook.id
output simpleTestWorkbookId string = simpleTestWorkbook.id
//output userWorkbookId string = userWorkbook.id
output sentinelWorkbookId string = sentinelWorkbook.id
output tsgWorkbookId string = tsgWorkbook.id

output workbookNames array = [
  basicMonitoringWorkbook.properties.displayName
  performanceWorkbook.properties.displayName
  securityWorkbook.properties.displayName
  applicationInsightsWorkbook.properties.displayName
  simpleTestWorkbook.properties.displayName
  //userWorkbook.properties.displayName
  sentinelWorkbook.properties.displayName
  tsgWorkbook.properties.displayName
]

output workspaceId string = workspace.id
