// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceConversionTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("/subscriptions/abc", null)]
    [InlineData("/subscriptions/abc/resourceGroups/rg1", null)]
    [InlineData(
        "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage",
        "Microsoft.Storage/storageAccounts")]
    [InlineData(
        "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage/blobServices/default",
        "Microsoft.Storage/storageAccounts/blobServices")]
    [InlineData(
        "/subscriptions/abc/providers/Microsoft.Authorization/roleAssignments/guid",
        "Microsoft.Authorization/roleAssignments")]
    public void ParseImpactedResourceType_ExtractsTypePath(string? resourceId, string? expected)
    {
        Assert.Equal(expected, AdvisorService.ParseImpactedResourceType(resourceId));
    }

    [Fact]
    public void ParseImpactedResourceType_ProviderSegmentAtEnd_ReturnsNull()
    {
        // Malformed id where 'providers' appears with no namespace/type after it.
        // Must not throw — Resource Graph occasionally returns oddly shaped ids.
        var result = AdvisorService.ParseImpactedResourceType("/subscriptions/abc/providers");

        Assert.Null(result);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_PopulatesAllFields()
    {
        // A representative advisorresources row. Property names match the camelCase
        // shape Resource Graph returns and the JsonKnownNamingPolicy on AdvisorJsonContext.
        const string json = """
            {
                "id": "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Advisor/recommendations/rec1",
                "type": "Microsoft.Advisor/recommendations",
                "name": "rec1",
                "subscriptionId": "abc",
                "resourceGroup": "rg1",
                "tenantId": "tenant1",
                "properties": {
                    "category": "Security",
                    "impact": "High",
                    "recommendationTypeId": "Type-A",
                    "impactedField": "Microsoft.Compute/virtualMachines",
                    "impactedValue": "vm1",
                    "recommendationStatus": "New",
                    "completionType": "Succeeded",
                    "reason": "Other",
                    "postponedTime": "2027-07-01T00:00:00Z",
                    "suppressionId": "suppression-id",
                    "createdTime": "2026-05-13T03:19:48.0318731Z",
                    "lastUpdated": "2026-05-14T03:19:48.0318731Z",
                    "lastRefreshed": "2026-05-15T03:19:48.0318731Z",
                    "shortDescription": {
                        "problem": "Enable encryption at rest",
                        "solution": "Turn on encryption"
                    },
                    "extendedProperties": {
                        "recommendationSubCategory": "Scalability",
                        "maturityLevel": "Preview",
                        "recommendationOfferingId": "offering1"
                    },
                    "resourceMetadata": {
                        "resourceId": "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage",
                        "action": {
                            "actionId": "0574d759-144a-4fdc-9201-83370c3bd756",
                            "actionType": 0,
                            "extensionName": "Microsoft_Azure_Storage",
                            "bladeName": "StorageAccountBlade",
                            "metadata": { "id": "{resourceId}" }
                        }
                    },
                    "description": "Configure diagnostic settings",
                    "label": "Configure monitoring",
                    "learnMoreLink": "https://learn.microsoft.com/azure/azure-monitor/",
                    "potentialBenefits": "Enhanced monitoring",
                    "actions": [{
                        "actionId": "b713bb56-949d-412e-8163-d4a7a2d66e61",
                        "description": "Open the monitoring guide",
                        "actionType": "Document",
                        "documentLink": "https://learn.microsoft.com/azure/azure-monitor/",
                        "extensionName": "Microsoft_Azure_Monitoring",
                        "bladeName": "MonitoringMenuBlade",
                        "metadata": { "source": "Advisor" },
                        "condition": "true",
                        "actionApplicabilityScope": "Resource",
                        "isRecommendedAction": true,
                        "recommendedActionButtonText": "Open guide",
                        "copilotCompetencyId": "monitoring",
                        "copilotCompetencyDisplayName": "Monitoring",
                        "promptId": "prompt1",
                        "displayPromptMessage": "Review monitoring guidance",
                        "promptMessage": "Review this resource's monitoring configuration",
                        "copilotAdditionalContext": ["resourceId", "recommendationTypeId"]
                    }],
                    "remediation": {
                        "httpMethod": "PATCH",
                        "uri": "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage",
                        "actionId": "b713bb56-949d-412e-8163-d4a7a2d66e61",
                        "implication": "The storage account configuration will change",
                        "documentationLink": "https://learn.microsoft.com/azure/storage/",
                        "requestBody": { "properties": { "supportsHttpsTrafficOnly": true } },
                        "asyncRequestDetails": { "statusUri": "{azureAsyncOperation}" }
                    },
                    "trackedProperties": { "priority": "High" },
                    "review": { "name": "Review test", "id": "review1" },
                    "resourceWorkload": { "name": "Test workload", "id": "workload1" },
                    "sourceSystem": "Review",
                    "notes": "Review the diagnostic settings"
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal(
            "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage",
            result.Properties.ResourceMetadata!.ResourceId);
        Assert.Equal("/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Advisor/recommendations/rec1", result.Id);
        Assert.Equal("rec1", result.Name);
        Assert.Equal("Microsoft.Advisor/recommendations", result.Type);
        Assert.Equal("Security", result.Properties.Category);
        Assert.Equal("High", result.Properties.Impact);
        Assert.Equal("Scalability", result.Properties.ExtendedProperties!["recommendationSubCategory"].GetString());
        Assert.Equal("Microsoft.Compute/virtualMachines", result.Properties.ImpactedField);
        Assert.Equal("vm1", result.Properties.ImpactedValue);
        Assert.Equal("Type-A", result.Properties.RecommendationTypeId);
        Assert.Equal("New", result.Properties.RecommendationStatus);
        Assert.Equal("Succeeded", result.Properties.CompletionType);
        Assert.Equal("Other", result.Properties.RecommendationDismissReason);
        Assert.Equal(
            DateTimeOffset.Parse("2027-07-01T00:00:00Z", CultureInfo.InvariantCulture),
            result.Properties.PostponedUntilDateTime);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-13T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.Properties.CreatedTime);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-14T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.Properties.LastUpdated);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-15T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.Properties.LastRefreshed);
        Assert.Equal(JsonValueKind.String, result.Properties.ExtendedProperties!["maturityLevel"].ValueKind);
        Assert.Equal("Preview", result.Properties.ExtendedProperties["maturityLevel"].GetString());
        Assert.Equal("offering1", result.Properties.ExtendedProperties["recommendationOfferingId"].GetString());
        Assert.Equal("Enable encryption at rest", result.Properties.ShortDescription!.Problem);
        Assert.Equal("Turn on encryption", result.Properties.ShortDescription.Solution);
        Assert.Equal("Configure diagnostic settings", result.Properties.Description);
        Assert.Equal("Configure monitoring", result.Properties.Label);
        Assert.Equal("https://learn.microsoft.com/azure/azure-monitor/", result.Properties.LearnMoreLink);
        Assert.Equal("Enhanced monitoring", result.Properties.PotentialBenefits);
        var action = result.Properties.Actions!.Value.EnumerateArray().Single();
        Assert.Equal("b713bb56-949d-412e-8163-d4a7a2d66e61", action.GetProperty("actionId").GetString());
        Assert.Equal("Document", action.GetProperty("actionType").GetString());
        Assert.Equal("Advisor", action.GetProperty("metadata").GetProperty("source").GetString());
        Assert.True(action.GetProperty("isRecommendedAction").GetBoolean());
        Assert.Equal(2, action.GetProperty("copilotAdditionalContext").GetArrayLength());
        Assert.Equal("PATCH", result.Properties.Remediation!.Value.GetProperty("httpMethod").GetString());
        Assert.Equal(
            "b713bb56-949d-412e-8163-d4a7a2d66e61",
            result.Properties.Remediation.Value.GetProperty("actionId").GetString());
        Assert.True(
            result.Properties.Remediation.Value.GetProperty("requestBody").GetProperty("properties")
                .GetProperty("supportsHttpsTrafficOnly")
                .GetBoolean());
        Assert.Equal(
            "{azureAsyncOperation}",
            result.Properties.Remediation.Value.GetProperty("asyncRequestDetails").GetProperty("statusUri").GetString());
        Assert.Equal("High", result.Properties.TrackedProperties!.Value.GetProperty("priority").GetString());
        Assert.Equal("review1", result.Properties.Review!.Value.GetProperty("id").GetString());
        Assert.Equal("Review test", result.Properties.Review.Value.GetProperty("name").GetString());
        Assert.Equal("workload1", result.Properties.ResourceWorkload!.Value.GetProperty("id").GetString());
        Assert.Equal("Test workload", result.Properties.ResourceWorkload.Value.GetProperty("name").GetString());
        Assert.Equal("Review", result.Properties.SourceSystem);
        Assert.Equal("Review the diagnostic settings", result.Properties.Notes);

        var serialized = JsonSerializer.Serialize(
            result,
            Azure.Mcp.Tools.Advisor.Commands.AdvisorJsonContext.Default.Recommendation);
        Assert.Contains("\"completionType\":\"Succeeded\"", serialized);
        Assert.Contains("\"recommendationDismissReason\":\"Other\"", serialized);
        Assert.Contains("\"postponedUntilDateTime\":\"2027-07-01T00:00:00+00:00\"", serialized);
        Assert.DoesNotContain("suppressionId", serialized);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_MissingShortDescription_LeavesItNull()
    {
        const string json = """
            {
                "id": "/subscriptions/abc/providers/Microsoft.Advisor/recommendations/rec3",
                "properties": { "category": "Cost" }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Null(result.Properties.ShortDescription);
        Assert.Null(result.Properties.RecommendationStatus);
        Assert.Null(result.Properties.CreatedTime);
        Assert.Null(result.Properties.ExtendedProperties);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_MissingOptionalFields_UsesDefaults()
    {
        // No impact, no resourceMetadata — older recommendations sometimes lack these.
        const string json = """
            {
                "id": "/subscriptions/abc/providers/Microsoft.Advisor/recommendations/rec2",
                "properties": {
                    "category": "Cost",
                    "shortDescription": { "problem": "Right-size your VMs" }
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Null(result.Properties.ResourceMetadata?.ResourceId);
        Assert.Equal("Right-size your VMs", result.Properties.ShortDescription!.Problem);
        Assert.Equal("Cost", result.Properties.Category);
        Assert.Null(result.Properties.Impact);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_ImpactIsCaseSensitiveJsonKey()
    {
        // Guard against a typo regression: AdvisorJsonContext uses camelCase, so
        // 'impact' is the only acceptable JSON key. A future rename to 'Impact'
        // or '"impact":' surrounded by typos would silently drop the value.
        const string json = """
            {
                "id": "/subscriptions/abc/providers/Microsoft.Advisor/recommendations/rec3",
                "properties": {
                    "category": "Performance",
                    "impact": "Medium",
                    "shortDescription": { "problem": "x" },
                    "resourceMetadata": { "resourceId": "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/s" }
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("Medium", result.Properties.Impact);
    }

    [Fact]
    public void ConvertUpdateResponseToAdvisorRecommendationModel_MapsUpdateFieldNames()
    {
        const string json = """
            {
                "id": "/subscriptions/abc/providers/Microsoft.Advisor/recommendations/rec4",
                "name": "rec4",
                "type": "Microsoft.Advisor/recommendations",
                "properties": {
                    "category": "Cost",
                    "recommendationStatus": "Postponed",
                    "recommendationDismissReason": "Other",
                    "postponedUntilDateTime": "2027-01-02T03:04:05Z"
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertUpdateResponseToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("rec4", result.Name);
        Assert.Equal("Postponed", result.Properties.RecommendationStatus);
        Assert.Equal("Other", result.Properties.RecommendationDismissReason);
        Assert.Equal(
            DateTimeOffset.Parse("2027-01-02T03:04:05Z", CultureInfo.InvariantCulture),
            result.Properties.PostponedUntilDateTime);
    }
}
