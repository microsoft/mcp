// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cspell:ignore contosolrsstg Fproviders Fservice

using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class StructuredArtifactMarkdownRendererTests
{
    [Fact]
    public void ReadArtifact_RendersServiceGroupGoalStatusDataGrid()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Service Group Goal Status",
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "name", "header": "Service Group", "type": "deepLink", "sortable": true },
                    { "id": "goalAssignedStatus", "header": "Goal Status", "type": "iconLabel", "sortable": true }
                  ],
                  "data": [
                    {
                      "name": {
                        "label": "ContosoAppFleetSG",
                        "deepLink": "#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/goalsAndRecommendations/serviceGroupId/ContosoAppFleetSG"
                      },
                      "goalAssignedStatus": { "label": "Assigned" }
                    },
                    {
                      "name": {
                        "label": "NeedsGoal",
                        "deepLink": "#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/goalsAndRecommendations/serviceGroupId/NeedsGoal"
                      },
                      "goalAssignedStatus": { "label": "Not assigned", "icon": "warning" }
                    }
                  ],
                  "sortable": true
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Service Group Goal Status.md", artifact.Name);
        Assert.Equal("goals", artifact.Family);
        Assert.Equal("text/markdown", artifact.MimeType);
        Assert.Contains("# Service Group goal status", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("**Service groups:** 2", artifact.Content, StringComparison.Ordinal);
        Assert.Contains(
            "https://portal.azure.com/#view/Microsoft_Azure_Resources/ServiceGroup.MenuView",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.Contains("Warning: Not assigned", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_UsesSemanticFallbackNamesWhenStructuredArtifactsHaveNoName()
    {
        using JsonDocument goalStatus = JsonDocument.Parse(
            """
            {
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "name", "header": "Service Group", "type": "string" },
                    { "id": "goalAssignedStatus", "header": "Goal Status", "type": "string" }
                  ],
                  "data": []
                }
              }]
            }
            """);
        using JsonDocument participation = JsonDocument.Parse(
            """
            {
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "name", "header": "Resource", "type": "string" },
                    { "id": "exemptionStatus", "header": "Participation", "type": "string" }
                  ],
                  "data": []
                }
              }]
            }
            """);

        AgentArtifact goalArtifact = Assert.Single(
            ResiliencyAgentService.ReadArtifact(goalStatus.RootElement));
        AgentArtifact participationArtifact = Assert.Single(
            ResiliencyAgentService.ReadArtifact(participation.RootElement));

        Assert.Equal("ServiceGroupGoalStatus.md", goalArtifact.Name);
        Assert.Equal("GoalResourceExemptionStatus.md", participationArtifact.Name);
        Assert.NotEqual(goalArtifact.Name, participationArtifact.Name);
    }

    [Theory]
    [InlineData("Members of app.prod", "Members of app.prod.md")]
    [InlineData("Report.md", "Report.md")]
    [InlineData("Report.MD", "Report.MD")]
    public void ReadArtifact_AddsMarkdownExtensionUnlessNameAlreadyEndsInMarkdown(
        string artifactName,
        string expectedName)
    {
        using JsonDocument document = JsonDocument.Parse(
            $$"""
            {
              "name": "{{artifactName}}",
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "name", "header": "Name", "type": "string" }
                  ],
                  "data": []
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal(expectedName, artifact.Name);
        Assert.Equal("text/markdown", artifact.MimeType);
    }

    [Fact]
    public void ReadArtifact_UsesBackendNameAndEscapesLinkLabelOnce()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Goal Status West",
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "name", "header": "Service Group", "type": "deepLink" },
                    { "id": "goalAssignedStatus", "header": "Goal Status", "type": "string" }
                  ],
                  "data": [{
                    "name": {
                      "label": "Group[Primary]",
                      "deepLink": "#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/goalsAndRecommendations/serviceGroupId/GroupPrimary"
                    },
                    "goalAssignedStatus": "Assigned"
                  }]
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Goal Status West.md", artifact.Name);
        Assert.Contains("[Group\\[Primary\\]](", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("[Group\\\\[Primary\\\\]](", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersGoalResourceExemptionDataGrid()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Goal Resource Exemption Status",
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "resourceName", "header": "Resource", "type": "deepLink" },
                    { "id": "subscriptionId", "header": "Subscription", "type": "deepLink" },
                    { "id": "resourceGroup", "header": "Resource group", "type": "deepLink" },
                    { "id": "exemptionStatus", "header": "Exemption status", "type": "string" },
                    { "id": "exclusionReason", "header": "Selected exclusion reason", "type": "string" }
                  ],
                  "data": [{
                    "resourceName": {
                      "label": "contosolrsstg01",
                      "deepLink": "#@/resource/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/contosolrsstg01/overview"
                    },
                    "subscriptionId": { "label": "sub", "deepLink": "#@/resource/subscriptions/sub/overview" },
                    "resourceGroup": { "label": "rg", "deepLink": "#@/resource/subscriptions/sub/resourceGroups/rg/overview" },
                    "exemptionStatus": "Excluded",
                    "exclusionReason": "I am ensuring zone resiliency via a custom solution."
                  }],
                  "sortable": true
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Goal Resource Exemption Status.md", artifact.Name);
        Assert.Equal("goals", artifact.Family);
        Assert.Contains("# Goal resource participation", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("**Resources:** 1", artifact.Content, StringComparison.Ordinal);
        Assert.Contains(
            "https://portal.azure.com/#@/resource/subscriptions/sub/",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.Contains("I am ensuring zone resiliency via a custom solution.", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersServiceGroupDetailsUsingPortalCompatibleLayout()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Members of ContosoAppFleetSG",
              "metadata": { "type": "resiliencyagent" },
              "parts": [{
                "kind": "data",
                "data": {
                  "resiliencyAgentArtifactType": "ServiceGroupDetails",
                  "data": {
                    "serviceGroup": {
                      "name": "ContosoAppFleetSG",
                      "resourceId": "/providers/Microsoft.Management/serviceGroups/ContosoAppFleetSG",
                      "parent": "Tenant Root",
                      "parentResourceId": "/providers/Microsoft.Management/serviceGroups/72f988bf-86f1-41af-91ab-2d7cd011db47"
                    },
                    "goalsStatus": { "assigned": true },
                    "memberSummary": {
                      "Microsoft.Storage/storageAccounts": 1,
                      "Microsoft.Compute/virtualMachines": 1
                    },
                    "resourceCount": 2,
                    "resources": [
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stg1",
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1"
                    ]
                  }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Members of ContosoAppFleetSG.md", artifact.Name);
        Assert.Equal("service-groups", artifact.Family);
        Assert.Contains("# Service Group details", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("ContosoAppFleetSG", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| Resiliency goal | Assigned |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("**Total members:** 2", artifact.Content, StringComparison.Ordinal);
        Assert.Contains(
            "https://portal.azure.com/#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/overview/" +
            "serviceGroupId/ContosoAppFleetSG",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.Contains(
            "https://portal.azure.com/#view/Microsoft_Azure_Resources/ServiceGroupsBrowse.ReactView/" +
            "id/%2Fproviders%2FMicrosoft.Management%2FserviceGroups%2F72f988bf-86f1-41af-91ab-2d7cd011db47",
            artifact.Content,
            StringComparison.Ordinal);
        Assert.Contains("https://portal.azure.com/#@/resource/subscriptions/sub/", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersResourceCriteriaResolution()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Resource criteria resolution",
              "metadata": { "type": "resiliencyagent" },
              "parts": [{
                "kind": "data",
                "data": {
                  "resiliencyAgentArtifactType": "ResourceCriteriaResolution",
                  "data": {
                    "summary": {
                      "Microsoft.Compute/virtualMachines": "2"
                    },
                    "resources": [
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1",
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm2"
                    ]
                  }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Resource criteria resolution.md", artifact.Name);
        Assert.Equal("resources", artifact.Family);
        Assert.Contains("# Resource criteria resolution", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("**Total resources:** 2", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| [vm1](https://portal.azure.com/#@/resource/", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_GroupsNestedAndExtensionResourcesByFullType()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Resource criteria resolution",
              "metadata": { "type": "resiliencyagent" },
              "parts": [{
                "kind": "data",
                "data": {
                  "resiliencyAgentArtifactType": "ResourceCriteriaResolution",
                  "data": {
                    "summary": {},
                    "resources": [
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Sql/servers/sql1/databases/db1",
                      "/subscriptions/sub/resourceGroups/rg/providers/Contoso.Provider/parents/providers/children/child1",
                      "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1/providers/Microsoft.Insights/diagnosticSettings/default"
                    ]
                  }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Contains("## SQL Databases", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("## Children", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("## Diagnostic Settings", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("## Servers", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("## Parents", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| [db1](https://portal.azure.com/#@/resource/", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| [child1](https://portal.azure.com/#@/resource/", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| [default](https://portal.azure.com/#@/resource/", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersBackupDataGridWithBackendTitleAndFamily()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Jobs",
              "metadata": { "type": "datagrid" },
              "parts": [{
                "kind": "data",
                "data": {
                  "columns": [
                    { "id": "item", "header": "Backup item", "type": "deepLink" },
                    { "id": "status", "header": "Status", "type": "string" }
                  ],
                  "data": [{
                    "item": {
                      "label": "vm-prod",
                      "deepLink": "#@/resource/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm-prod"
                    },
                    "status": "Failed"
                  }]
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Jobs.md", artifact.Name);
        Assert.Equal("data-resiliency", artifact.Family);
        Assert.Contains("# Jobs", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("**Rows:** 1", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("vm-prod", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Azure resiliency data", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersBackupProtectionCoverageWithoutZonalTerminology()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Backup protection coverage",
              "metadata": { "type": "resiliencyagent" },
              "parts": [{
                "kind": "data",
                "data": {
                  "resiliencyAgentArtifactType": "GetZonalPosture",
                  "summary": {
                    "totalResources": 2,
                    "resilientResources": 1,
                    "nonResilientResources": 1
                  },
                  "resources": {
                    "Microsoft.Compute/virtualMachines": [{
                      "resourceId": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1",
                      "status": "Protected"
                    }],
                    "Microsoft.Storage/storageAccounts": [{
                      "resourceId": "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/st1",
                      "status": "Unprotected"
                    }]
                  }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Backup protection coverage.md", artifact.Name);
        Assert.Equal("data-resiliency", artifact.Family);
        Assert.Contains("# Backup Protection Coverage", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| Existing Resources | Protected | Unprotected |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| 2 | 1 | 1 |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("deleted resources are not counted", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Zonal Posture", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("Resilient Resources", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersBackupPieChartAsDataAndMermaid()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Backup protection coverage chart",
              "metadata": { "type": "chart" },
              "parts": [{
                "kind": "data",
                "data": {
                  "title": "Protected and unprotected resources",
                  "chartType": "pie",
                  "series": [{
                    "name": "Protected and unprotected resources",
                    "data": [
                      { "name": "Protected", "y": 8 },
                      { "name": "Unprotected", "y": 3 }
                    ]
                  }]
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Backup protection coverage chart.md", artifact.Name);
        Assert.Equal("data-resiliency", artifact.Family);
        Assert.Contains("| Protected | 8 |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| Unprotected | 3 |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("```mermaid", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("pie showData", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_RendersBackupColumnChartAsDataAndMermaid()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Backup jobs by status",
              "metadata": { "type": "chart" },
              "parts": [{
                "kind": "data",
                "data": {
                  "title": "Backup jobs by status",
                  "chartType": "column",
                  "series": [{
                    "name": "Jobs",
                    "data": [12, 2, 1]
                  }],
                  "xAxis": {
                    "type": "category",
                    "title": { "text": "Status" },
                    "categories": ["Completed", "Failed", "Running"]
                  },
                  "yAxis": {
                    "title": { "text": "Count" }
                  }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Equal("Backup jobs by status.md", artifact.Name);
        Assert.Equal("data-resiliency", artifact.Family);
        Assert.Contains("| Completed | 12 |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("| Failed | 2 |", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("xychart-beta", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("bar [12, 2, 1]", artifact.Content, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadArtifact_UnsupportedLocalChartTypeUsesNoLossFallback()
    {
        using JsonDocument document = JsonDocument.Parse(
            """
            {
              "name": "Backup trend",
              "metadata": { "type": "chart" },
              "parts": [{
                "kind": "data",
                "data": {
                  "title": "Backup trend",
                  "chartType": "line",
                  "series": [{ "name": "Jobs", "data": [1, 2] }],
                  "xAxis": { "categories": ["Day 1", "Day 2"] }
                }
              }]
            }
            """);

        AgentArtifact artifact = Assert.Single(ResiliencyAgentService.ReadArtifact(document.RootElement));

        Assert.Contains("chartType", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("line", artifact.Content, StringComparison.Ordinal);
        Assert.Contains("Day 1", artifact.Content, StringComparison.Ordinal);
        Assert.DoesNotContain("xychart-beta", artifact.Content, StringComparison.Ordinal);
    }
}
