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
        string? displayName = "Catalog display name",
        string? category = "Security",
        string? subCategory = "ZoneResiliency",
        string? impact = "High",
        string? potentialBenefits = "Improved resiliency",
        string? learnMoreLink = "https://aka.ms/advisor",
        RecommendationServiceRetirement? serviceRetirement = null) =>
        new(
            RecommendationTypeId: recommendationTypeId,
            DisplayName: displayName,
            Label: null,
            Category: category,
            SubCategory: subCategory,
            Impact: impact,
            PriorityScore: null,
            PotentialBenefits: potentialBenefits,
            DetailedDescription: null,
            LearnMoreLink: learnMoreLink,
            SupportedResourceType: null,
            Scope: null,
            DataSourceQuery: null,
            ResourceSingularName: null,
            ResourcePluralName: null,
            Actions: null,
            Language: "en",
            LastRefreshed: null,
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
    public void JoinWithMetadata_RefreshesStaleCatalogFields()
    {
        var retirement = new RecommendationServiceRetirement(
            RetirementDate: "2026-03-31",
            RetirementFeatureName: "Legacy feature",
            TrackingIds: ["QNY1-HB8"],
            AshUrls: ["https://aka.ms/ash"]);

        var recommendation = new Models.Recommendation(
            ResourceId: "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/mystorage",
            RecommendationText: "Instance problem text",
            Category: "StaleCategory",
            Impact: "Low",
            ImpactedResourceType: "Microsoft.Storage/storageAccounts",
            RecommendationTypeId: "Type-A");

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([
                CreateMetadata(
                    "type-a",
                    subCategory: "ServiceUpgradeAndRetirement",
                    serviceRetirement: retirement)
            ]));

        var result = Assert.Single(joined);
        // Instance-owned text is preserved; catalog-owned fields are refreshed.
        Assert.Equal("Instance problem text", result.RecommendationText);
        Assert.Equal("Security", result.Category);
        Assert.Equal("High", result.Impact);
        Assert.Equal("ServiceUpgradeAndRetirement", result.SubCategory);
        Assert.Equal("Improved resiliency", result.PotentialBenefits);
        Assert.Equal("https://aka.ms/advisor", result.LearnMoreLink);
        Assert.Same(retirement, result.ServiceRetirement);
        Assert.Equal("Microsoft.Storage/storageAccounts", result.ImpactedResourceType);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData("   ")]
    public void JoinWithMetadata_MissingProblemText_FallsBackToDisplayName(string recommendationText)
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation("resId", recommendationText, "Unknown", RecommendationTypeId: "Type-A")],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A")]));

        Assert.Equal("Catalog display name", Assert.Single(joined).RecommendationText);
    }

    [Fact]
    public void JoinWithMetadata_MetadataWithoutCategoryOrImpact_KeepsInstanceValues()
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation("resId", "problem", "Cost", Impact: "Medium", RecommendationTypeId: "Type-A")],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A", category: null, impact: "  ")]));

        var result = Assert.Single(joined);
        Assert.Equal("Cost", result.Category);
        Assert.Equal("Medium", result.Impact);
    }

    [Fact]
    public void JoinWithMetadata_NoMatchingMetadata_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation("resId", "problem", "Cost", RecommendationTypeId: "Type-Z");

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A")]));

        Assert.Same(recommendation, Assert.Single(joined));
    }

    [Fact]
    public void JoinWithMetadata_MissingRecommendationTypeId_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation("resId", "problem", "Cost");

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
