// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
                "properties": {
                    "category": "Security",
                    "impact": "High",
                    "shortDescription": { "problem": "Enable encryption at rest" },
                    "resourceMetadata": {
                        "resourceId": "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage"
                    }
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal(
            "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Storage/storageAccounts/mystorage",
            result.ResourceId);
        Assert.Equal("Enable encryption at rest", result.RecommendationText);
        Assert.Equal("Security", result.Category);
        Assert.Equal("High", result.Impact);
        Assert.Equal("Microsoft.Storage/storageAccounts", result.ImpactedResourceType);
        Assert.Null(result.RecommendationId);
        Assert.Null(result.RecommendationStatus);
        Assert.Null(result.RecommendationDismissReason);
        Assert.Null(result.PostponedUntilDateTime);
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

        Assert.Equal("Unknown", result.ResourceId);
        Assert.Equal("Right-size your VMs", result.RecommendationText);
        Assert.Equal("Cost", result.Category);
        Assert.Null(result.Impact);
        Assert.Null(result.ImpactedResourceType);
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

        Assert.Equal("Medium", result.Impact);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_UpdateResponsePopulatesLifecycleFields()
    {
        const string json = """
            {
                "id": "/subscriptions/abc/providers/Microsoft.Advisor/recommendations/rec4",
                "type": "Microsoft.Advisor/recommendations",
                "name": "rec4",
                "properties": {
                    "category": "HighAvailability",
                    "impact": "High",
                    "control": "ZoneResiliency",
                    "impactedField": "Microsoft.Compute/virtualMachines",
                    "impactedValue": "vm1",
                    "recommendationStatus": "Dismissed",
                    "recommendationDismissReason": "RiskIsAcceptable",
                    "postponedUntilDateTime": "2027-01-02T03:04:05Z",
                    "lastRefreshed": "2026-01-02T03:04:05Z",
                    "lastUpdated": "2026-02-03T04:05:06Z",
                    "createdTime": "2025-03-04T05:06:07Z",
                    "recommendationTypeId": "type-1",
                    "completionType": "ManuallyCompleted",
                    "risk": "Service disruption",
                    "description": "Use availability zones.",
                    "label": "Improve resiliency",
                    "learnMoreLink": "https://learn.microsoft.com/azure/reliability/",
                    "potentialBenefits": "Higher availability",
                    "sourceSystem": "Advisor",
                    "suppressionId": "suppression-1",
                    "shortDescription": {
                        "problem": "The resource is not zone resilient.",
                        "solution": "Deploy across zones."
                    },
                    "resourceMetadata": {
                        "resourceId": "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1"
                    }
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToUpdatedAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("rec4", result.RecommendationId);
        Assert.Equal("Dismissed", result.RecommendationStatus);
        Assert.Equal("RiskIsAcceptable", result.RecommendationDismissReason);
        Assert.Equal(DateTimeOffset.Parse("2027-01-02T03:04:05Z"), result.PostponedUntilDateTime);
    }

    [Fact]
    public void ConvertToAdvisorRecommendationModel_PreservesUnknownLifecycleValues()
    {
        const string json = """
            {
                "name": "rec5",
                "properties": {
                    "category": "Cost",
                    "recommendationStatus": "InProgress",
                    "recommendationDismissReason": "NotRelevant",
                    "shortDescription": {
                        "problem": "Right-size a resource."
                    }
                }
            }
            """;

        using var doc = JsonDocument.Parse(json);
        var result = AdvisorService.ConvertToUpdatedAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("InProgress", result.RecommendationStatus);
        Assert.Equal("NotRelevant", result.RecommendationDismissReason);
    }
}
