// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.ResourceGraph.Models;
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
        RecommendationServiceRetirement? serviceRetirement = null,
        string? detailedDescription = "Detailed description") =>
        new(
            RecommendationTypeId: recommendationTypeId,
            DisplayName: displayName,
            Label: "Reliability",
            Category: category,
            SubCategory: subCategory,
            Impact: impact,
            PriorityScore: 42.5,
            PotentialBenefits: potentialBenefits,
            DetailedDescription: detailedDescription,
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

    [Fact]
    public void HasMetadataFilters_InstanceOnlyFilters_ReturnsFalse()
    {
        Assert.False(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(Resource: "mystorage", Search: "encryption")));
    }

    [Theory]
    [InlineData("Security", true)]
    [InlineData(" security ", true)]
    [InlineData("Cost", false)]
    [InlineData(null, false)]
    public void IsDirectSecurityQuery_IdentifiesSecurityQueries(
        string? category,
        bool expected)
    {
        Assert.Equal(expected, AdvisorService.IsDirectSecurityQuery(
            new RecommendationFilters(Category: category)));
    }

    [Fact]
    public void SecurityQueryWithMetadataOnlyFilters_BypassesMetadata()
    {
        var filters = new RecommendationFilters(
            Category: "Security",
            SubCategory: "ZoneResiliency");

        Assert.True(AdvisorService.IsDirectSecurityQuery(filters));
        Assert.False(AdvisorService.HasMetadataFilters(filters));
    }
    [Theory]
    [InlineData("Security", null, null, false)]
    [InlineData(null, "High", null, true)]
    [InlineData(null, null, "Microsoft.Storage/storageAccounts", true)]
    public void HasMetadataFilters_MetadataBackedFilters_HandlesSecurityException(
        string? category,
        string? impact,
        string? resourceType,
        bool expected)
    {
        Assert.Equal(expected, AdvisorService.HasMetadataFilters(
            new RecommendationFilters(
                Category: category,
                Impact: impact,
                ResourceType: resourceType)));
    }

    [Fact]
    public void SecurityCategoryDoesNotRequireMetadataLookup()
    {
        Assert.False(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(Category: "Security", Impact: "High")));
    }
    [Fact]
    public void HasMetadataFilters_CombinedFilters_ReturnsTrue()
    {
        Assert.True(AdvisorService.HasMetadataFilters(
            new RecommendationFilters(
                SubCategory: "ServiceUpgradeAndRetirement",
                Resource: "mystorage",
                Search: "encryption")));
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
    public void EnsureMetadataResultsComplete_TruncatedResultsThrow()
    {
        var results = new ResourceQueryResults<RecommendationMetadata>([], true);

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            AdvisorService.EnsureMetadataResultsComplete(results);
        });

        Assert.Contains("truncated", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EnsureMetadataResultsComplete_CompleteResultsDoNotThrow()
    {
        AdvisorService.EnsureMetadataResultsComplete(
            new ResourceQueryResults<RecommendationMetadata>([], false));
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
            new Models.RecommendationProperties(
                Category: "StaleCategory",
                Impact: "Low",
                RecommendationTypeId: "Type-A",
                RecommendationStatus: "New",
                ShortDescription: new Models.RecommendationShortDescription("Instance problem", "Instance solution"),
                ResourceMetadata: new Models.RecommendationResourceMetadata(
                    "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/mystorage")),
            Id: "recommendation-id");

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([
                CreateMetadata("type-a", subCategory: "ServiceUpgradeAndRetirement")
            ]));

        var result = Assert.Single(joined);

        // Instance-owned fields survive the join.
        Assert.Equal(
            "/subscriptions/abc/providers/Microsoft.Storage/storageAccounts/mystorage",
            result.Properties.ResourceMetadata!.ResourceId);
        Assert.Equal("Type-A", result.Properties.RecommendationTypeId);
        Assert.Equal("New", result.Properties.RecommendationStatus);
        Assert.Equal("Metadata display name", result.Properties.ShortDescription!.Problem);
        Assert.Equal("Metadata display name", result.Properties.ShortDescription.Solution);

        // Metadata-owned fields are overridden.
        Assert.Equal("Security", result.Properties.Category);
        Assert.Equal("High", result.Properties.Impact);
        Assert.Equal("ServiceUpgradeAndRetirement", result.Properties.ExtendedProperties!["recommendationSubCategory"].GetString());
    }

    [Fact]
    public void JoinWithMetadata_BlankMetadataImpactAndSubCategory_AreNotBackfilledFromInstance()
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation(
                new Models.RecommendationProperties(
                    Category: "Cost",
                    Impact: "Medium",
                    RecommendationTypeId: "Type-A"))],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A", impact: null, subCategory: null)]));

        var result = Assert.Single(joined);
        Assert.Null(result.Properties.Impact);
        Assert.Null(result.Properties.ExtendedProperties);
    }

    [Fact]
    public void JoinWithMetadata_MissingDetailedDescription_UsesDisplayNameForBothFields()
    {
        var recommendation = new Models.Recommendation(
            new Models.RecommendationProperties(
                Category: "Cost",
                RecommendationTypeId: "Type-A",
                ShortDescription: new Models.RecommendationShortDescription("Instance problem", "Instance solution")));

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([
                CreateMetadata("Type-A", detailedDescription: null)
            ]));

        var shortDescription = Assert.Single(joined).Properties.ShortDescription;
        Assert.NotNull(shortDescription);
        Assert.Equal("Metadata display name", shortDescription.Problem);
        Assert.Equal("Metadata display name", shortDescription.Solution);
    }

    [Fact]
    public void JoinWithMetadata_MissingMetadataDescription_UsesInstanceFields()
    {
        var recommendation = new Models.Recommendation(
            new Models.RecommendationProperties(
                Category: "Cost",
                RecommendationTypeId: "Type-A",
                ShortDescription: new Models.RecommendationShortDescription("Instance problem", "Instance solution")));

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([
                CreateMetadata("Type-A", displayName: null, detailedDescription: null)
            ]));

        var shortDescription = Assert.Single(joined).Properties.ShortDescription;
        Assert.NotNull(shortDescription);
        Assert.Equal("Instance problem", shortDescription.Problem);
        Assert.Equal("Instance solution", shortDescription.Solution);
    }

    [Fact]
    public void JoinWithMetadata_BlankMetadataCategory_ReturnsMetadataValue()
    {
        var joined = AdvisorService.JoinWithMetadata(
            [new Models.Recommendation(
                new Models.RecommendationProperties(Category: "Cost", RecommendationTypeId: "Type-A"))],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A", category: null)]));

        Assert.Null(Assert.Single(joined).Properties.Category);
    }

    [Fact]
    public void JoinWithMetadata_NoMatchingMetadata_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation(
            new Models.RecommendationProperties(Category: "Cost", RecommendationTypeId: "Type-Z"));

        var joined = AdvisorService.JoinWithMetadata(
            [recommendation],
            AdvisorService.BuildMetadataLookup([CreateMetadata("Type-A")]));

        Assert.Same(recommendation, Assert.Single(joined));
    }

    [Fact]
    public void JoinWithMetadata_MissingRecommendationTypeId_ReturnsRecommendationUnchanged()
    {
        var recommendation = new Models.Recommendation(
            new Models.RecommendationProperties(Category: "Cost"));

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
