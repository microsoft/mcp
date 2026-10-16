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
    [InlineData("recommendation-type")]
    [InlineData("category")]
    [InlineData("impact")]
    [InlineData("resource-type")]
    [InlineData("status")]
    [InlineData("sub-category")]
    [InlineData("retirement-date")]
    public void BuildSummaryQuery_EveryGrouping_InnerJoinsEnglishMetadata(string groupBy)
    {
        var query = RecommendationSummaryService.BuildSummaryQuery("subscription-id", null, groupBy, null);

        Assert.Contains("join kind=inner", query);
        Assert.DoesNotContain("leftouter", query);
        Assert.Contains("properties.language) =~ 'en'", query);
        Assert.EndsWith("summarize count() by key, label", query);
    }
    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    public void EnsureFullTenantCoverage_SubscriptionLimitHit_Throws(string headerValue)
    {
        Assert.Throws<InvalidOperationException>(() =>
            AdvisorResourceGraphQueryExecutor.EnsureFullTenantCoverage(headerValue));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    [InlineData("")]
    public void EnsureFullTenantCoverage_FullCoverage_DoesNotThrow(string? headerValue)
    {
        AdvisorResourceGraphQueryExecutor.EnsureFullTenantCoverage(headerValue);
    }

    [Fact]
    public void BuildSummaryQuery_Category_UsesMetadataOnly()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            "rg-name",
            "category",
            null);

        Assert.Contains("subscriptionId =~ 'subscription-id'", query);
        Assert.Contains("resourceGroup =~ 'rg-name'", query);
        Assert.Contains("join kind=inner", query);
        Assert.Contains("properties.language) =~ 'en'", query);
        Assert.Contains("category = metadataCategory", query);
        Assert.DoesNotContain("instanceCategory", query);
        Assert.DoesNotContain("instanceImpact", query);
        Assert.DoesNotContain("instanceSubCategory", query);
        Assert.DoesNotContain("instanceRetirementDate", query);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, query);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.Contains(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.EndsWith("summarize count() by key, label", query);
    }

    [Fact]
    public void BuildServiceGroupSummaryQuery_Category_UsesExactScopeWithoutSubscription()
    {
        var query = RecommendationSummaryService.BuildServiceGroupSummaryQuery(
            "service-group-test",
            "category",
            null);

        Assert.DoesNotContain("subscriptionId =~", query);
        Assert.DoesNotContain("resourceGroup =~", query);
        Assert.Contains(
            "tostring(properties.serviceGroupId) =~ " +
            $"'{RecommendationQueryBuilder.ServiceGroupResourceIdPrefix}service-group-test'",
            query);
        Assert.DoesNotContain(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.Contains("join kind=inner", query);
        Assert.Contains(RecommendationQueryBuilder.ActiveRecommendationClause, query);
    }

    [Fact]
    public void BuildServiceGroupSummaryQuery_StatusIncludesEveryLifecycleState()
    {
        var query = RecommendationSummaryService.BuildServiceGroupSummaryQuery(
            "service-group-test",
            "status",
            null);

        Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.Contains(
            $"{RecommendationQueryBuilder.ServiceGroupResourceIdPrefix}service-group-test",
            query);
        Assert.Contains("join kind=inner", query);
        Assert.Contains("key = iff(isempty(recommendationStatus), 'Unknown', recommendationStatus)", query);
    }

    [Fact]
    public void BuildSummaryQuery_RecommendationType_UsesStableIdAndMetadataLabel()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "recommendation-type",
            null);

        Assert.Contains("tolower(tostring(properties.recommendationTypeId))", query);
        Assert.Contains("metadataDisplayName", query);
        Assert.Contains("metadataLabel", query);
        Assert.Contains("iff(isnotempty(metadataLabel), metadataLabel, recommendationTypeId)", query);
        Assert.Contains("recommendationTypeId", query);
        Assert.DoesNotContain("key = tostring(properties.shortDescription.problem)", query);
    }

    [Fact]
    public void BuildSummaryQuery_ResourceType_UsesImpactedFieldWithResourceIdFallback()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "resource-type",
            new RecommendationFilters(ResourceType: "Microsoft.Web/sites"));

        Assert.Contains("join kind=inner", query);
        Assert.Contains("tostring(properties.impactedField) =~ 'Microsoft.Web/sites'", query);
        Assert.Contains("impactedResourceType = tolower(impactedField)", query);
        Assert.Contains("extract(@'/providers/([^/]+/[^/]+)', 1, resourceId)", query);
    }

    [Fact]
    public void BuildSummaryQuery_ResourceTypeWithImpactFilter_UsesMetadataJoin()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "resource-type",
            new RecommendationFilters(Impact: "High"));

        Assert.Contains("join kind=inner", query);
        Assert.Contains("where impact =~ 'High'", query);
        Assert.Contains("extend resourceType =", query);
    }

    [Fact]
    public void BuildSummaryQuery_StatusIncludesEveryLifecycleState()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "status",
            null);

        Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, query);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.Contains("join kind=inner", query);
        Assert.Contains("key = iff(isempty(recommendationStatus), 'Unknown', recommendationStatus)", query);
    }

    [Fact]
    public void BuildSummaryQuery_RetirementDate_InfersSubCategoryAndNormalizesDate()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "retirement-date",
            null);

        Assert.Contains("subCategory =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains(
            "format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')",
            query);
        Assert.Contains("iff(isempty(retirementDate), 'Unknown', retirementDate)", query);
    }

    [Fact]
    public void BuildSummaryQuery_RetirementDateFilter_UsesParsedComparison()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            "impact",
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
            "resource-type",
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
            "category",
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
            "retirement-date",
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
            "recommendation-type",
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
            RecommendationSummaryService.ParseSummary("category", document.RootElement));
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
                "impact",
                filters));
    }
}
