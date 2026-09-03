// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class RecommendationQueryBuilderTests
{
    [Fact]
    public void BuildInstancePredicates_NullFilters_ReturnsEngineAndActiveFilters()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_StatusDisabled_RetainsEngineFilters()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: false,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(RecommendationQueryBuilder.CurrentRecommendationEngineClause, result);
    }

    [Fact]
    public void BuildInstancePredicates_SummaryIgnoresRequestedStatus()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: RecommendationStatus.Completed),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ListUsesRequestedStatus()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: RecommendationStatus.Completed),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            "tostring(properties.recommendationStatus) =~ 'Completed'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_AllInstanceFilters_UsesExpectedSemantics()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                Category: "Security",
                Impact: "High",
                RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d",
                ResourceType: "Microsoft.Web/sites",
                Resource: "webapp",
                Search: "encrypt"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: true);

        Assert.Contains("properties.recommendationStatus", result);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, result);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, result);
        Assert.Contains("tostring(properties.category) =~ 'Security'", result);
        Assert.Contains("tostring(properties.impact) =~ 'High'", result);
        Assert.Contains("tostring(properties.recommendationTypeId) =~ '1d70919c-1a4a-4f79-8300-bb576c291e9d'", result);
        Assert.Contains("tostring(properties.impactedField) =~ 'Microsoft.Web/sites'", result);
        Assert.Contains("tostring(properties.resourceMetadata.resourceId) contains 'webapp'", result);
        Assert.Contains("tostring(properties.shortDescription.problem) contains 'encrypt'", result);
    }

    [Fact]
    public void BuildInstancePredicates_ListResourceType_PreservesResourceIdContains()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(ResourceType: "Microsoft.Web/sites"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Contains(
            "tostring(properties.resourceMetadata.resourceId) contains 'Microsoft.Web/sites'",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ResolvedMetadataIds_SkipsCategoryAndImpact()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Category: "Security", Impact: "High"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: ["Type-A", "Type-B"]);

        Assert.DoesNotContain("properties.category", result);
        Assert.DoesNotContain("properties.impact", result);
        Assert.Contains("tostring(properties.recommendationTypeId) in~ ('Type-A', 'Type-B')", result);
    }

    [Fact]
    public void BuildInstancePredicates_EscapesKqlValuesAndRemovesPipes()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Search: @"it's\unsafe|"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.DoesNotContain('|', result);
        Assert.Contains(@"'it''s\\unsafe'", result);
    }
}
