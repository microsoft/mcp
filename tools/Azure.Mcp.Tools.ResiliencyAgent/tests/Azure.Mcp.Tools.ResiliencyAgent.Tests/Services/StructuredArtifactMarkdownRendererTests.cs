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

        Assert.Equal("ServiceGroupGoalStatus.md", artifact.Name);
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

        Assert.Equal("GoalResourceExemptionStatus.md", artifact.Name);
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

        Assert.Equal("ServiceGroupDetails.md", artifact.Name);
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

        Assert.Equal("ResourceCriteriaResolution.md", artifact.Name);
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
}
