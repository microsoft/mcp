// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class AdvisorServiceMetadataJoinTests
{
    private static RecommendationMetadata CreateMetadata(
        string recommendationTypeId,
        string? displayName = "Metadata display name",
        string? category = "Security",
        string? subCategory = "ZoneResiliency",
        string? impact = "High",
        string? potentialBenefits = "Improved resiliency",
        string? learnMoreLink = "https://aka.ms/advisor",
        RecommendationServiceRetirement? serviceRetirement = null) =>
        new(
            RecommendationTypeId: recommendationTypeId,
            DisplayName: displayName,
            Label: "Reliability",
            Category: category,
            SubCategory: subCategory,
            Impact: impact,
            PriorityScore: 42.5,
            PotentialBenefits: potentialBenefits,
            DetailedDescription: "Detailed description",
            LearnMoreLink: learnMoreLink,
            SupportedResourceType: "microsoft.storage/storageaccounts",
            Scope: "Resource",
            DataSourceQuery: "resources | take 1",
            ResourceSingularName: "storage account",
            ResourcePluralName: "storage accounts",
            Actions: [new RecommendationMetadataAction("Fix", "Enable it", "https://aka.ms/doc", "BladeName")],
            Language: "en",
            LastRefreshed: "2026-08-01",
            ServiceRetirement: serviceRetirement);

    [Fact]
    public void HasMetadataOnlyFilters_NullFilters_ReturnsFalse()
    {
        Assert.False(AdvisorService.HasMetadataOnlyFilters(null));
    }

    [Fact]
    public void HasMetadataOnlyFilters_InstanceOnlyFilters_ReturnsFalse()
    {
        var filters = new RecommendationFilters(
            Category: "Security",
            Impact: "High",
            ResourceType: "Microsoft.Storage/storageAccounts",
            Resource: "mystorage",
            Search: "encryption");

        Assert.False(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Theory]
    [InlineData("ZoneResiliency", null, false)]
    [InlineData(null, "QNY1-HB8", false)]
    [InlineData(null, null, true)]
    [InlineData("  ", "  ", true)]
    public void HasMetadataOnlyFilters_MetadataFilters_ReturnsTrue(
        string? subCategory,
        string? trackingId,
        bool withRetirementDate)
    {
        var filters = new RecommendationFilters(
            SubCategory: subCategory,
            TrackingIds: trackingId is null ? null : [trackingId],
            RetirementDateOperator: withRetirementDate ? "ge" : null,
            RetirementDate: withRetirementDate ? new DateOnly(2026, 3, 31) : null);

        Assert.True(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void HasMetadataOnlyFilters_MultipleTrackingIds_ReturnsTrue()
    {
        var filters = new RecommendationFilters(TrackingIds: ["QNY1-HB8", "9G0V-_G8"]);

        Assert.True(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void HasMetadataOnlyFilters_BlankTrackingIdsOnly_ReturnsFalse()
    {
        var filters = new RecommendationFilters(TrackingIds: ["  ", ""]);

        Assert.False(AdvisorService.HasMetadataOnlyFilters(filters));
    }

    [Fact]
    public void BuildMetadataLookup_IsCaseInsensitiveAndSkipsBlankIds()
    {
        var lookup = AdvisorService.BuildMetadataLookup(
        [
            CreateMetadata("Type-A"),
            CreateMetadata("   "),
        ]);

        Assert.Single(lookup);
        Assert.True(lookup.ContainsKey("type-a"));
    }

    [Fact]
    public void BuildMetadataLookup_DuplicateIds_KeepsLastEntry()
    {
        var lookup = AdvisorService.BuildMetadataLookup(
        [
            CreateMetadata("Type-A", displayName: "first"),
            CreateMetadata("type-a", displayName: "second"),
        ]);

        Assert.Single(lookup);
        Assert.Equal("second", lookup["Type-A"].DisplayName);
    }

    [Fact]
    public void BuildMetadataByTypeIdsQuery_FiltersOnLanguageAndTypeIds()
    {
        var query = AdvisorService.BuildMetadataByTypeIdsQuery(["Type-A", "Type-B"], "en");

        Assert.Contains("| where type =~ 'microsoft.advisor/metadata'", query);
        Assert.Contains("tostring(properties.language) =~ 'en'", query);
        Assert.Contains("tostring(properties.recommendationTypeId) in~ ('Type-A', 'Type-B')", query);
        Assert.EndsWith("| project properties", query);
    }

    [Fact]
    public void BuildMetadataByTypeIdsQuery_EscapesSingleQuotes()
    {
        var query = AdvisorService.BuildMetadataByTypeIdsQuery(["it's"], "en");

        Assert.Contains("in~ ('it''s')", query);
    }

    [Fact]
    public void JoinWithMetadata_OverridesCategoryImpactAndSubCategory()
    {
        var recommendation = new Models.Recommendation(
            ResourceId: "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/mystorage",
            Category: "StaleCategory",
            Impact: "Low",
            SubCategory: "StaleSubCategory",
            ImpactedResourceType: "Microsoft.Storage/storageAccounts",
            RecommendationTypeId: "Type-A",
            RecommendationStatus: "New",
            ShortDescription: new Models.RecommendationShortDescription("Instance problem", "Instance solution"));

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([
                CreateMetadata("type-a", subCategory: "ServiceUpgradeAndRetirement")
            ]));

        var result = Assert.Single(joined);

        // Instance-owned fields survive the join.
        Assert.Equal(
            "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/mystorage",
            result.ResourceId);
        Assert.Equal("Microsoft.Storage/storageAccounts", result.ImpactedResourceType);
        Assert.Equal("Type-A", result.RecommendationTypeId);
        Assert.Equal("New", result.RecommendationStatus);
        Assert.Equal("Metadata display name", result.ShortDescription!.Problem);
        Assert.Null(result.ShortDescription.Solution);

        // Metadata-owned fields are overridden.
        Assert.Equal("Security", result.Category);
        Assert.Equal("High", result.Impact);
        Assert.Equal("ServiceUpgradeAndRetirement", result.SubCategory);
    }

    [Fact]
    public void JoinWithMetadata_BlankMetadataImpactAndSubCategory_AreNotBackfilledFromInstance()
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation(ResourceId: "resId", Category: "Cost", Impact: "Medium", SubCategory: "Scalability", RecommendationTypeId: "Type-A")],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A", impact: null, subCategory: null)]));

        var result = Assert.Single(joined);
        Assert.Null(result.Impact);
        Assert.Null(result.SubCategory);
    }

    [Fact]
    public void JoinWithMetadata_BlankMetadataCategory_ReturnsMetadataValue()
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation(ResourceId: "resId", Category: "Cost", RecommendationTypeId: "Type-A")],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A", category: null)]));

        Assert.Null(Assert.Single(joined).Category);
    }

    [Fact]
    public void JoinWithMetadata_NoMatchingMetadata_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation(ResourceId: "resId", Category: "Cost", RecommendationTypeId: "Type-Z");

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A")]));

        Assert.Same(recommendation, Assert.Single(joined));
    }

    [Fact]
    public void JoinWithMetadata_MissingRecommendationTypeId_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation(ResourceId: "resId", Category: "Cost");

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A")]));

        Assert.Same(recommendation, Assert.Single(joined));
    }

    [Fact]
    public void JoinWithMetadata_EmptyInput_ReturnsEmptyList()
    {
        Assert.Empty(AdvisorService.JoinWithMetadata([], AdvisorService.BuildMetadataLookup([])));
    }
}
