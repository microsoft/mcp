// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorMetadataServiceTests
{
    [Fact]
    public void BuildMetadataListQuery_UsesMetadataResourceTypeAndLanguage()
    {
        var query = AdvisorService.BuildMetadataListQuery("en", null);

        Assert.Contains("advisorresources", query);
        Assert.Contains("type =~ 'microsoft.advisor/metadata'", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.DoesNotContain("properties.recommendationSubCategory", query);
        Assert.DoesNotContain("array_index_of", query);
        Assert.DoesNotContain("todatetime", query);
        Assert.EndsWith("| project properties", query);
    }

    [Fact]
    public void BuildMetadataListQuery_AddsAllFilters()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "de",
            new RecommendationMetadataFilters(
                ResourceType: "microsoft.compute/virtualmachines",
                Impact: AdvisorImpact.High,
                Category: AdvisorCategory.HighAvailability,
                SubCategory: "ServiceUpgradeAndRetirement",
                TrackingId: "QNY1-HB8",
                RetirementDateOperator: "ge",
                RetirementDate: new DateOnly(2026, 3, 31)));

        Assert.Contains("tostring(properties.supportedResourceType) =~ 'microsoft.compute/virtualmachines'", query);
        Assert.Contains("tostring(properties.recommendationImpact) =~ 'High'", query);
        Assert.Contains("tostring(properties.recommendationCategory) =~ 'HighAvailability'", query);
        Assert.Contains("tostring(properties.recommendationSubCategory) =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains(
            "mv-expand trackingId = properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds",
            query);
        Assert.Contains(
            "where tostring(trackingId) =~ 'QNY1-HB8'",
            query);
        Assert.Contains("isnotempty(tostring(properties.sourceProperties.serviceRetirement.retirementDate))", query);
        Assert.Contains("startofday(todatetime(properties.sourceProperties.serviceRetirement.retirementDate)) >= datetime(2026-03-31)", query);

        var subCategoryIndex = query.IndexOf("recommendationSubCategory", StringComparison.Ordinal);
        var trackingSearchIndex = query.IndexOf("trackingIds", StringComparison.Ordinal);
        var retirementPresenceIndex = query.IndexOf("isnotempty", StringComparison.Ordinal);
        var retirementComparisonIndex = query.IndexOf("startofday", StringComparison.Ordinal);

        Assert.True(subCategoryIndex < trackingSearchIndex);
        Assert.True(subCategoryIndex < retirementPresenceIndex);
        Assert.True(retirementPresenceIndex < retirementComparisonIndex);
    }

    [Fact]
    public void BuildMetadataListQuery_RetirementFiltersImplicitlyApplySubCategoryWithoutCategory()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                TrackingId: "QNY1-HB8",
                RetirementDateOperator: "ge",
                RetirementDate: new DateOnly(2026, 3, 31)));

        var subCategoryIndex = query.IndexOf(
            "tostring(properties.recommendationSubCategory) =~ 'ServiceUpgradeAndRetirement'",
            StringComparison.Ordinal);
        var trackingIndex = query.IndexOf("trackingIds", StringComparison.Ordinal);
        var retirementDateIndex = query.IndexOf("todatetime", StringComparison.Ordinal);

        Assert.True(subCategoryIndex >= 0);
        Assert.DoesNotContain("properties.recommendationCategory", query);
        Assert.True(subCategoryIndex < trackingIndex);
        Assert.True(subCategoryIndex < retirementDateIndex);
    }

    [Fact]
    public void BuildMetadataListQuery_ExplicitRetirementSubCategoryIsNotDuplicated()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                SubCategory: "serviceupgradeandretirement",
                TrackingId: "QNY1-HB8"));

        Assert.Contains("properties.recommendationSubCategory", query);
        Assert.Equal(
            query.IndexOf("properties.recommendationSubCategory", StringComparison.Ordinal),
            query.LastIndexOf("properties.recommendationSubCategory", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildMetadataListQuery_ConflictingRetirementSubCategoryThrows()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            AdvisorService.BuildMetadataListQuery(
                "en",
                new RecommendationMetadataFilters(
                    SubCategory: "ZoneResiliency",
                    TrackingId: "QNY1-HB8")));

        Assert.Contains("ServiceUpgradeAndRetirement", exception.Message);
    }

    [Fact]
    public void BuildMetadataListQuery_RetirementFiltersAllowOperationalExcellenceCategory()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                Category: AdvisorCategory.OperationalExcellence,
                TrackingId: "QNY1-HB8",
                RetirementDateOperator: "ge",
                RetirementDate: new DateOnly(2026, 3, 31)));

        Assert.Contains("tostring(properties.recommendationCategory) =~ 'OperationalExcellence'", query);
        Assert.Contains("tostring(properties.recommendationSubCategory) =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains(
            "mv-expand trackingId = properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds",
            query);
        Assert.Contains(
            "where tostring(trackingId) =~ 'QNY1-HB8'",
            query);
        Assert.Contains(
            "startofday(todatetime(properties.sourceProperties.serviceRetirement.retirementDate)) >= datetime(2026-03-31)",
            query);
    }

    [Fact]
    public void BuildMetadataListQuery_ExplicitRetirementSubCategoryAllowsAnyValidCategory()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                Category: AdvisorCategory.OperationalExcellence,
                SubCategory: "ServiceUpgradeAndRetirement"));

        Assert.Contains("tostring(properties.recommendationCategory) =~ 'OperationalExcellence'", query);
        Assert.Contains("tostring(properties.recommendationSubCategory) =~ 'ServiceUpgradeAndRetirement'", query);
    }

    [Theory]
    [InlineData("ge", null)]
    [InlineData(null, "2026-03-31")]
    public void BuildMetadataListQuery_IncompleteRetirementDateFilterThrows(
        string? comparisonOperator,
        string? date)
    {
        DateOnly? retirementDate = date is null ? null : DateOnly.Parse(date);

        var exception = Assert.Throws<ArgumentException>(() =>
            AdvisorService.BuildMetadataListQuery(
                "en",
                new RecommendationMetadataFilters(
                    RetirementDateOperator: comparisonOperator,
                    RetirementDate: retirementDate)));

        Assert.Contains("must be provided together", exception.Message);
    }

    [Fact]
    public void BuildMetadataListQuery_EscapesKqlValues()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                ResourceType: @"microsoft.test/type\child",
                Category: AdvisorCategory.Cost));
        var trackingQuery = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(TrackingId: "QNY1-'HB8"));

        Assert.Contains(@"microsoft.test/type\\child", query);
        Assert.Contains("tostring(properties.recommendationCategory) =~ 'Cost'", query);
        Assert.Contains("where tostring(trackingId) =~ 'QNY1-''HB8'", trackingQuery);
    }

    [Fact]
    public void BuildMetadataListQuery_TrackingIdUsesCaseInsensitiveExactArrayMatch()
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(TrackingId: "qny1-hb8"));

        Assert.Contains(
            "mv-expand trackingId = properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds",
            query);
        Assert.Contains(
            "where tostring(trackingId) =~ 'qny1-hb8'",
            query);
    }

    [Theory]
    [InlineData("eq", "==")]
    [InlineData("lt", "<")]
    [InlineData("le", "<=")]
    [InlineData("gt", ">")]
    [InlineData("ge", ">=")]
    public void BuildMetadataListQuery_MapsRetirementDateOperator(
        string comparisonOperator,
        string expectedKqlOperator)
    {
        var query = AdvisorService.BuildMetadataListQuery(
            "en",
            new RecommendationMetadataFilters(
                RetirementDateOperator: comparisonOperator,
                RetirementDate: new DateOnly(2025, 8, 15)));

        Assert.Contains(
            $"startofday(todatetime(properties.sourceProperties.serviceRetirement.retirementDate)) {expectedKqlOperator} datetime(2025-08-15)",
            query);
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
