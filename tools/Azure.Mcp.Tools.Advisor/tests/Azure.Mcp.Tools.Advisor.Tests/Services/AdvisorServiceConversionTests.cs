// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceConversionTests
{
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
                    "impactedField": "Microsoft.Compute/virtualMachines",
                    "impactedValue": "vm1",
                    "recommendationStatus": "New",
                    "recommendationDismissReason": "Other",
                    "postponedUntilDateTime": "2027-07-01T00:00:00Z",
                    "recommendationTypeId": "Type-A",
                    "shortDescription": {
                        "problem": "Enable encryption at rest",
                        "solution": "Turn on encryption"
                    },
                    "extendedProperties": {
                        "maturityLevel": "Preview"
                    },
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
            result.Properties.ResourceMetadata?.ResourceId);
        Assert.Equal(
            "/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Advisor/recommendations/rec1",
            result.Id);
        Assert.Equal("rec1", result.Name);
        Assert.Equal("Microsoft.Advisor/recommendations", result.Type);
        Assert.Equal("Enable encryption at rest", result.Properties.ShortDescription?.Problem);
        Assert.Equal("Turn on encryption", result.Properties.ShortDescription?.Solution);
        Assert.Equal("Security", result.Properties.Category);
        Assert.Equal("High", result.Properties.Impact);
        Assert.Equal("Microsoft.Compute/virtualMachines", result.Properties.ImpactedField);
        Assert.Equal("vm1", result.Properties.ImpactedValue);
        Assert.Equal("Type-A", result.Properties.RecommendationTypeId);
        Assert.Equal("New", result.Properties.RecommendationStatus);
        Assert.Equal("Other", result.Properties.RecommendationDismissReason);
        Assert.Equal(
            DateTimeOffset.Parse("2027-07-01T00:00:00Z"),
            result.Properties.PostponedUntilDateTime);
        Assert.Equal("Preview", result.Properties.ExtendedProperties?["maturityLevel"].GetString());

        var serialized = JsonSerializer.Serialize(
            result,
            Azure.Mcp.Tools.Advisor.Commands.AdvisorJsonContext.Default.Recommendation);
        Assert.StartsWith("{\"id\":", serialized);
        Assert.Contains("\"name\":\"rec1\"", serialized);
        Assert.Contains("\"type\":\"Microsoft.Advisor/recommendations\"", serialized);
        Assert.Contains("\"properties\":", serialized);
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

        Assert.Null(result.Properties.ResourceMetadata);
        Assert.Equal("Right-size your VMs", result.Properties.ShortDescription?.Problem);
        Assert.Equal("Cost", result.Properties.Category);
        Assert.Null(result.Properties.Impact);
        Assert.Null(result.Properties.RecommendationStatus);
        Assert.Null(result.Properties.CompletionType);
        Assert.Null(result.Properties.RecommendationDismissReason);
        Assert.Null(result.Properties.PostponedUntilDateTime);
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
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("rec4", result.Name);
        Assert.Equal("Dismissed", result.Properties.RecommendationStatus);
        Assert.Equal("ManuallyCompleted", result.Properties.CompletionType);
        Assert.Equal("RiskIsAcceptable", result.Properties.RecommendationDismissReason);
        Assert.Equal(
            DateTimeOffset.Parse("2027-01-02T03:04:05Z"),
            result.Properties.PostponedUntilDateTime);
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
        var result = AdvisorService.ConvertToAdvisorRecommendationModel(doc.RootElement);

        Assert.Equal("InProgress", result.Properties.RecommendationStatus);
        Assert.Equal("NotRelevant", result.Properties.RecommendationDismissReason);
    }
}
