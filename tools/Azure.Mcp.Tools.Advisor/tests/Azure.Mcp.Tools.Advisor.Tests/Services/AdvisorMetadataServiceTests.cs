// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorMetadataServiceTests
{
    [Fact]
    public void BuildMetadataListQuery_UsesMetadataResourceTypeAndLanguage()
    {
        var query = AdvisorService.BuildMetadataListQuery("en", null, null, null);

        Assert.Contains("advisorresources", query);
        Assert.Contains("type =~ 'microsoft.advisor/metadata'", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.EndsWith("| project properties", query);
    }

    [Fact]
    public void BuildMetadataListQuery_AddsAllFilters()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "de",
            "microsoft.compute/virtualmachines",
            "High",
            "Cost");

        Assert.Contains("tostring(properties.supportedResourceType) =~ 'microsoft.compute/virtualmachines'", query);
        Assert.Contains("tostring(properties.recommendationImpact) =~ 'High'", query);
        Assert.Contains("tostring(properties.recommendationCategory) =~ 'Cost'", query);
    }

    [Fact]
    public void BuildMetadataListQuery_EscapesKqlValues()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            @"microsoft.test/type\child",
            null,
            "Cost' or true");

        Assert.Contains(@"microsoft.test/type\\child", query);
        Assert.Contains("Cost'' or true", query);
        Assert.DoesNotContain("Cost' or true'", query);
    }

    [Fact]
    public void ConvertToRecommendationMetadataModel_MapsRichMetadata()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "metadata-id",
                "displayName": "Service retirement recommendation",
                "label": "Service retirement",
                "recommendationCategory": "HighAvailability",
                "recommendationSubCategory": "ServiceUpgradeAndRetirement",
                "recommendationImpact": "Medium",
                "priorityScore": 0.631,
                "potentialBenefits": "Avoid disruption",
                "detailedDescription": "Upgrade the affected resources.",
                "learnMoreLink": "https://learn.microsoft.com/azure/advisor/",
                "supportedResourceType": "microsoft.compute/virtualmachinescalesets",
                "recommendationScope": "Public",
                "recommendationDataSourceQuery": "resources | where type =~ 'microsoft.compute/virtualmachinescalesets'",
                "resourceMetadata": {
                  "singular": "Virtual machine scale set",
                  "plural": "Virtual machine scale sets"
                },
                "actions": [
                  {
                    "actionType": "Document",
                    "caption": "Review upgrade guidance",
                    "documentLink": "https://learn.microsoft.com/azure/advisor/",
                    "bladeName": null
                  }
                ],
                "language": "en",
                "lastRefreshed": "2026-07-27T08:56:16Z",
                "sourceProperties": {
                  "serviceRetirement": {
                    "retirementDate": "2027-03-31",
                    "retirementFeatureName": "Legacy feature",
                    "serviceHealth": {
                      "trackingIds": ["tracking-id"],
                      "ashUrls": ["https://app.azure.com/h/tracking-id/"]
                    }
                  }
                }
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.Equal("metadata-id", metadata.RecommendationTypeId);
        Assert.Equal("HighAvailability", metadata.Category);
        Assert.Equal(0.631, metadata.PriorityScore);
        Assert.Equal("Virtual machine scale set", metadata.ResourceSingularName);
        Assert.Equal("Document", Assert.Single(metadata.Actions!).ActionType);
        Assert.Equal("2027-03-31", metadata.ServiceRetirement!.RetirementDate);
        Assert.Equal("tracking-id", Assert.Single(metadata.ServiceRetirement.TrackingIds!));
    }

    [Fact]
    public void ConvertToRecommendationMetadataModel_MinimalPayloadLeavesOptionalDataNull()
    {
        var element = Parse("""
            {
              "properties": {
                "recommendationTypeId": "metadata-id",
                "language": "en"
              }
            }
            """);

        var metadata = AdvisorService.ConvertToRecommendationMetadataModel(element);

        Assert.Equal("metadata-id", metadata.RecommendationTypeId);
        Assert.Null(metadata.Actions);
        Assert.Null(metadata.ServiceRetirement);
    }

    [Fact]
    public void ConvertToRecommendationMetadataModel_MissingPropertiesThrows()
    {
        var element = Parse("""{ "type": "microsoft.advisor/metadata" }""");

        var exception = Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToRecommendationMetadataModel(element));

        Assert.Contains("properties", exception.Message);
    }

    [Fact]
    public void ConvertToRecommendationMetadataModel_MissingRecommendationTypeIdThrows()
    {
        var element = Parse("""{ "properties": { "language": "en" } }""");

        var exception = Assert.Throws<JsonException>(
            () => AdvisorService.ConvertToRecommendationMetadataModel(element));

        Assert.Contains("recommendationTypeId", exception.Message);
    }

    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
