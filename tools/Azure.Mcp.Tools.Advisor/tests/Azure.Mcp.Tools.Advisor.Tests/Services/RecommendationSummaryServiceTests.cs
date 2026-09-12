// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class RecommendationSummaryServiceTests
{
    [Theory]
    [InlineData(AdvisorRecommendationGroupBy.RecommendationType, true)]
    [InlineData(AdvisorRecommendationGroupBy.Category, true)]
    [InlineData(AdvisorRecommendationGroupBy.Impact, true)]
    [InlineData(AdvisorRecommendationGroupBy.ResourceType, false)]
    [InlineData(AdvisorRecommendationGroupBy.Status, false)]
    [InlineData(AdvisorRecommendationGroupBy.SubCategory, true)]
    [InlineData(AdvisorRecommendationGroupBy.RetirementDate, true)]
    public void RequiresMetadata_RoutesEveryGrouping(AdvisorRecommendationGroupBy groupBy, bool expected)
    {
        Assert.Equal(expected, RecommendationSummaryService.RequiresMetadata(groupBy, null));
    }

    [Fact]
    public void RequiresMetadata_MetadataFilterOnInstanceGrouping_UsesJoin()
    {
        Assert.True(RecommendationSummaryService.RequiresMetadata(
            AdvisorRecommendationGroupBy.ResourceType,
            new RecommendationFilters(Impact: AdvisorRecommendationImpact.High)));
    }

    [Theory]
    [InlineData(AdvisorRecommendationGroupBy.RecommendationType, "recommendation-type")]
    [InlineData(AdvisorRecommendationGroupBy.Category, "category")]
    [InlineData(AdvisorRecommendationGroupBy.Impact, "impact")]
    [InlineData(AdvisorRecommendationGroupBy.ResourceType, "resource-type")]
    [InlineData(AdvisorRecommendationGroupBy.Status, "status")]
    [InlineData(AdvisorRecommendationGroupBy.SubCategory, "sub-category")]
    [InlineData(AdvisorRecommendationGroupBy.RetirementDate, "retirement-date")]
    public void BuildSummaryQuery_AllGroupings_HandleUnknownAndReturnCanonicalGroupName(
        AdvisorRecommendationGroupBy groupBy,
        string expectedGroupBy)
    {
        var query = RecommendationSummaryService.BuildSummaryQuery("subscription-id", null, groupBy, null);

        Assert.Contains("'Unknown'", query);
        Assert.Contains("isempty", query);
        Assert.EndsWith("summarize count() by key, label", query);
        Assert.Equal(
            groupBy != AdvisorRecommendationGroupBy.Status,
            query.Contains(RecommendationQueryBuilder.ActiveRecommendationClause, StringComparison.Ordinal));

        using var document = JsonDocument.Parse("[]");
        var summary = RecommendationSummaryService.ParseSummary(groupBy, document.RootElement);

        Assert.Equal(expectedGroupBy, summary.GroupBy);
        Assert.Equal(0, summary.TotalRecommendations);
        Assert.Empty(summary.Groups);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void BuildSummaryQuery_UnsupportedGroupBy_Throws(int groupBy)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecommendationSummaryService.BuildSummaryQuery(
                "subscription-id", null, (AdvisorRecommendationGroupBy)groupBy, null));
    }

    [Fact]
    public void BuildSummaryQuery_Category_UsesMetadataWithInstanceFallback()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            "rg-name",
            AdvisorRecommendationGroupBy.Category,
            null);

        Assert.Contains("subscriptionId =~ 'subscription-id'", query);
        Assert.Contains("resourceGroup =~ 'rg-name'", query);
        Assert.Contains("join kind=leftouter", query);
        Assert.Contains("properties.language) =~ 'en'", query);
        Assert.Contains(
            "category = iff(isnotempty(metadataCategory), metadataCategory, instanceCategory)",
            query);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, query);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.Contains(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.EndsWith("summarize count() by key, label", query);
    }

    [Fact]
    public void BuildSummaryQuery_RecommendationType_UsesStableIdAndMetadataLabel()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.RecommendationType,
            null);

        Assert.Contains("tolower(tostring(properties.recommendationTypeId))", query);
        Assert.Contains("metadataDisplayName", query);
        Assert.Contains("metadataLabel", query);
        Assert.Contains("recommendationTypeId", query);
        Assert.DoesNotContain("key = tostring(properties.shortDescription.problem)", query);
    }

    [Fact]
    public void BuildSummaryQuery_ResourceType_UsesImpactedFieldWithResourceIdFallback()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.ResourceType,
            new RecommendationFilters(ResourceType: "Microsoft.Web/sites"));

        Assert.DoesNotContain("join kind=leftouter", query);
        Assert.Contains("tostring(properties.impactedField) =~ 'Microsoft.Web/sites'", query);
        Assert.Contains("tolower(tostring(properties.impactedField))", query);
        Assert.Contains(
            "extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId))",
            query);
    }

    [Fact]
    public void BuildSummaryQuery_ResourceTypeWithMetadataFilters_UsesMetadataJoin()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.ResourceType,
            new RecommendationFilters(
                Category: AdvisorRecommendationCategory.Security,
                Impact: AdvisorRecommendationImpact.High));

        Assert.Contains("join kind=leftouter", query);
        Assert.Contains("where category =~ 'Security'", query);
        Assert.Contains("where impact =~ 'High'", query);
        Assert.Contains("extend resourceType =", query);
    }

    [Fact]
    public void BuildSummaryQuery_StatusIncludesEveryLifecycleState()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.Status,
            null);

        Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, query);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.DoesNotContain("join kind=leftouter", query);
        Assert.Contains("groupValue = tostring(properties.recommendationStatus)", query);
    }

    [Fact]
    public void BuildSummaryQuery_RetirementDate_InfersSubCategoryAndNormalizesDate()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.RetirementDate,
            null);

        Assert.Contains("subCategory =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains(
            "format_datetime(startofday(todatetime(retirementDateRaw)), 'yyyy-MM-dd')",
            query);
        Assert.Contains("iff(isempty(retirementDate), 'Unknown', retirementDate)", query);
    }

    [Fact]
    public void BuildSummaryQuery_RetirementDateFilter_UsesParsedComparison()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            AdvisorRecommendationGroupBy.Impact,
            new RecommendationFilters(
                RetirementDateOperator: "le",
                RetirementDate: new DateOnly(2026, 12, 31)));

        Assert.Contains("subCategory =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains("todatetime(retirementDate) <= datetime(2026-12-31)", query);
    }

    [Fact]
    public void BuildSummaryQuery_EscapesScopeAndFilters()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "sub'|id",
            "rg'|name",
            AdvisorRecommendationGroupBy.ResourceType,
            new RecommendationFilters(Search: "it's|unsafe"));

        Assert.DoesNotContain("sub'|id", query);
        Assert.DoesNotContain("rg'|name", query);
        Assert.DoesNotContain("it's|unsafe", query);
        Assert.Contains("sub''id", query);
        Assert.Contains("rg''name", query);
        Assert.Contains("it''s", query);
    }

    [Fact]
    public void ParseSummary_SortsByCountAndPreservesUnknownAtTail()
    {
        using var document = JsonDocument.Parse(
            """
            [
              { "key": "Unknown", "label": "Unknown", "count_": 4 },
              { "key": "Cost", "label": "Cost", "count_": 2 },
              { "key": "Security", "label": "Security", "count_": 5 }
            ]
            """);

        var summary = RecommendationSummaryService.ParseSummary(
            AdvisorRecommendationGroupBy.Category,
            document.RootElement);

        Assert.Equal(11, summary.TotalRecommendations);
        Assert.Equal(["Security", "Cost", "Unknown"], summary.Groups.Select(group => group.Key));
    }

    [Fact]
    public void ParseSummary_RetirementDatesSortAscending()
    {
        using var document = JsonDocument.Parse(
            """
            [
              { "key": "2027-03-31", "label": "2027-03-31", "count_": 8 },
              { "key": "Unknown", "label": "Unknown", "count_": 1 },
              { "key": "2025-02-28", "label": "2025-02-28", "count_": 2 }
            ]
            """);

        var summary = RecommendationSummaryService.ParseSummary(
            AdvisorRecommendationGroupBy.RetirementDate,
            document.RootElement);

        Assert.Equal(
            ["2025-02-28", "2027-03-31", "Unknown"],
            summary.Groups.Select(group => group.Key));
    }

    [Fact]
    public void ParseSummary_RollsUpDuplicateKeysAndPrefersFriendlyLabel()
    {
        using var document = JsonDocument.Parse(
            """
            [
              { "key": "type-a", "label": "type-a", "count_": 2 },
              { "key": "TYPE-A", "label": "Friendly name", "count_": 3 }
            ]
            """);

        var summary = RecommendationSummaryService.ParseSummary(
            AdvisorRecommendationGroupBy.RecommendationType,
            document.RootElement);

        var group = Assert.Single(summary.Groups);
        Assert.Equal("type-a", group.Key);
        Assert.Equal("Friendly name", group.Label);
        Assert.Equal(5, group.Count);
    }

    [Fact]
    public void ParseSummary_CountOutsideIntRangeThrows()
    {
        using var document = JsonDocument.Parse(
            """[{ "key": "Security", "label": "Security", "count_": 2147483648 }]""");

        Assert.Throws<OverflowException>(() =>
            RecommendationSummaryService.ParseSummary(AdvisorRecommendationGroupBy.Category, document.RootElement));
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "next-page")]
    public void EnsureCompleteResult_TruncationSignalThrows(bool truncated, string? skipToken)
    {
        Assert.Throws<InvalidOperationException>(() =>
            RecommendationSummaryService.EnsureCompleteResult(truncated, skipToken));
    }

    [Theory]
    [InlineData("le", null)]
    [InlineData(null, "2026-12-31")]
    public void BuildSummaryQuery_IncompleteRetirementFilterThrows(
        string? comparisonOperator,
        string? retirementDate)
    {
        var filters = new RecommendationFilters(
            RetirementDateOperator: comparisonOperator,
            RetirementDate: retirementDate is null ? null : DateOnly.Parse(retirementDate));

        Assert.Throws<ArgumentException>(() =>
            RecommendationSummaryService.BuildSummaryQuery(
                "subscription-id",
                null,
                AdvisorRecommendationGroupBy.Impact,
                filters));
    }
}
