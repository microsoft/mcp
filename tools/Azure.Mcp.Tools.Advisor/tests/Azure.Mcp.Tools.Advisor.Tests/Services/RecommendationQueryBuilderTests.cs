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
    public void BuildInstancePredicates_ListExcludesLegacyAndServiceGroupRecommendations()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            null,
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.StartsWith(
            $"{RecommendationQueryBuilder.CurrentRecommendationNameClause} and " +
            $"{RecommendationQueryBuilder.ServiceGroupExclusionClause} and ",
            result);
    }

    [Theory]
    [InlineData(RecommendationStatus.New, "New")]
    [InlineData(RecommendationStatus.Postponed, "Postponed")]
    [InlineData(RecommendationStatus.Dismissed, "Dismissed")]
    [InlineData(RecommendationStatus.Completed, "Completed")]
    public void BuildInstancePredicates_ListUsesEverySupportedStatus(
        RecommendationStatus status,
        string expectedStatus)
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(Status: status),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            $"tostring(properties.recommendationStatus) =~ '{expectedStatus}'",
            result);
    }

    [Theory]
    [InlineData(AdvisorRecommendationCategory.Security, AdvisorRecommendationImpact.High)]
    [InlineData(AdvisorRecommendationCategory.Cost, AdvisorRecommendationImpact.Medium)]
    [InlineData(AdvisorRecommendationCategory.HighAvailability, AdvisorRecommendationImpact.Low)]
    [InlineData(AdvisorRecommendationCategory.Performance, AdvisorRecommendationImpact.High)]
    [InlineData(AdvisorRecommendationCategory.OperationalExcellence, AdvisorRecommendationImpact.Low)]
    public void BuildInstancePredicates_AllInstanceFilters_UsesExpectedSemantics(
        AdvisorRecommendationCategory category,
        AdvisorRecommendationImpact impact)
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                Category: category,
                Impact: impact,
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
        Assert.Contains($"tostring(properties.category) =~ '{category}'", result);
        Assert.Contains($"tostring(properties.impact) =~ '{impact}'", result);
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
            new RecommendationFilters(
                Category: AdvisorRecommendationCategory.Security,
                Impact: AdvisorRecommendationImpact.High),
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
    public void BuildInstancePredicates_MetadataOnlyFilters_AddNoInstanceClauses()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                SubCategory: "ServiceUpgradeAndRetirement",
                TrackingIds: ["QNY1-HB8", "9G0V-_G8"],
                RetirementDateOperator: "ge",
                RetirementDate: new DateOnly(2026, 3, 31)),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false);

        Assert.Equal(
            $"{RecommendationQueryBuilder.CurrentRecommendationEngineClause} and " +
            RecommendationQueryBuilder.ActiveRecommendationClause,
            result);
    }

    [Fact]
    public void BuildInstancePredicates_ResolvedMetadataIds_IntersectsTypeId()
    {
        var typeId = "1d70919c-1a4a-4f79-8300-bb576c291e9d";
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(RecommendationTypeId: typeId),
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: ["Type-A", typeId]);

        Assert.Contains(
            $"tostring(properties.recommendationTypeId) =~ '{typeId}'",
            result);
        Assert.Contains(
            $"tostring(properties.recommendationTypeId) in~ ('Type-A', '{typeId}')",
            result);
    }

    [Fact]
    public void BuildInstancePredicates_EscapesKqlValuesAndRemovesPipes()
    {
        var result = RecommendationQueryBuilder.BuildInstancePredicates(
            new RecommendationFilters(
                RecommendationTypeId: "type'|id",
                ResourceType: "Microsoft.Storage|/storageAccounts",
                Resource: "my'|storage",
                Search: @"it's\unsafe|"),
            includeStatus: true,
            useRequestedStatus: false,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: ["Type|A", "it's"]);

        Assert.DoesNotContain('|', result);
        Assert.Contains(@"'it''s\\unsafe'", result);
        Assert.Contains("'type''id'", result);
        Assert.Contains("'Microsoft.Storage/storageAccounts'", result);
        Assert.Contains("'my''storage'", result);
        Assert.Contains("'TypeA', 'it''s'", result);
    }
}
