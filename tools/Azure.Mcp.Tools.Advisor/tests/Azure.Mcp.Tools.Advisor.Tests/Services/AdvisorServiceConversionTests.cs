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
        Assert.Equal("/subscriptions/abc/resourceGroups/rg1/providers/Microsoft.Advisor/recommendations/rec1", result.Id);
        Assert.Equal("rec1", result.Name);
        Assert.Equal("Microsoft.Advisor/recommendations", result.Type);
        Assert.Equal("abc", result.SubscriptionId);
        Assert.Equal("rg1", result.ResourceGroup);
        Assert.Equal("tenant1", result.TenantId);
        Assert.Equal("Security", result.Category);
        Assert.Equal("High", result.Impact);
        Assert.Equal("Scalability", result.SubCategory);
        Assert.Equal("Microsoft.Storage/storageAccounts", result.ImpactedResourceType);
        Assert.Equal("Microsoft.Compute/virtualMachines", result.ImpactedField);
        Assert.Equal("vm1", result.ImpactedValue);
        Assert.Equal("Type-A", result.RecommendationTypeId);
        Assert.Equal("New", result.RecommendationStatus);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-13T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.CreatedTime);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-14T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.LastUpdated);
        Assert.Equal(
            DateTimeOffset.Parse("2026-05-15T03:19:48.0318731Z", CultureInfo.InvariantCulture),
            result.LastRefreshed);
        Assert.Equal(JsonValueKind.String, result.ExtendedProperties!["maturityLevel"].ValueKind);
        Assert.Equal("Preview", result.ExtendedProperties["maturityLevel"].GetString());
        Assert.Equal("offering1", result.ExtendedProperties["recommendationOfferingId"].GetString());
        Assert.Null(result.ShortDescription);
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

        Assert.Null(result.ShortDescription);
        Assert.Null(result.RecommendationStatus);
        Assert.Null(result.CreatedTime);
        Assert.Null(result.SubCategory);
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
        Assert.Null(result.ShortDescription);
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
}
