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
    public void BuildSummaryQuery_MultipleSubscriptionsUsesInPredicate()
    {
        var scope = RecommendationQueryScope.ForSubscriptions(
            ["sub-123", "sub-456", "SUB-123"],
            null);

        var query = RecommendationSummaryService.BuildSummaryQuery(
            scope,
            "category",
            null);

        Assert.Contains("subscriptionId in~ ('sub-123', 'sub-456')", query);
        Assert.DoesNotContain("subscriptionId =~", query);
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
            "isnotempty(properties.serviceGroupId) and tostring(properties.serviceGroupId) =~ " +
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

    [Fact]
    public void BuildCostSummaryQuery_UsesSelectedPricingBasisAndKeepsCurrenciesSeparate()
    {
        var query = RecommendationSummaryService.BuildCostSummaryQuery(
            RecommendationQueryScope.ForSubscription("subscription-id", null),
            new RecommendationFilters(Category: "Cost"));

        Assert.Contains("properties.savings.retail.dailyPotentialSavings", query);
        Assert.Contains("properties.savings.retail.estimatedAccumulatedSavings", query);
        Assert.Contains("properties.savings.retail.realizedAccumulatedSavings", query);
        Assert.Contains("properties.savings.resourceDeletedOn", query);
        Assert.Contains("_isOpen = tostring(properties.recommendationStatus) !~ 'Completed'", query);
        Assert.Contains("potential = iif(_isOpen and isfinite(_daily), _daily * 364.8, double(null))", query);
        Assert.Contains("recommendationStatus = tostring(properties.recommendationStatus)", query);
        Assert.Contains("_daily * 364.8", query);
        Assert.Contains("properties.savings.providerCurrency", query);
        Assert.Contains("by currency", query);
        Assert.Contains("billReducingOpportunityCount", query);
        Assert.DoesNotContain("subscriptionId in~", query);
    }

    [Fact]
    public void BuildMetadataSummaryQuery_ProductFamilyFiltersUsingProjectedResourceTypeFallback()
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            RecommendationQueryScope.ForSubscription("sub-123", null),
            RecommendationSummaryService.GroupByRecommendationType,
            new RecommendationFilters(ProductFamily: "Compute"));

        Assert.Contains("metadataServiceFamily=tostring(properties.serviceFamily)", query);
        Assert.Contains("tolower(tostring(metadataServiceFamily)) =~ 'compute'", query);
        Assert.Contains("isempty(tostring(metadataServiceFamily))", query);
        Assert.Contains("tolower(tostring(impactedField)) startswith 'microsoft.web/serverfarms/'", query);
        Assert.Contains(
            "tolower(tostring(resourceId)) contains '/providers/microsoft.web/serverfarms/'",
            query);
    }

    [Fact]
    public void BuildCostSummaryQuery_HistoricalWindowUsesAsOfAndKeepsOpenStates()
    {
        var query = RecommendationSummaryService.BuildCostSummaryQuery(
            RecommendationQueryScope.ForSubscription("subscription-id", null),
            new RecommendationFilters(
                Category: "Cost",
                SavingsAsOf: new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero),
                SavingsFrom: new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
                SavingsTo: new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero)));

        Assert.Contains("datetime(2026-09-30", query);
        Assert.Contains("T23:59:59.000Z", query);
        Assert.Contains("_isOpen", query);
        Assert.Contains("_deletedOn >= datetime(2026-09-01", query);
        Assert.Contains("T00:00:00.000Z", query);
        Assert.Contains("_deletedOn <= datetime(2026-09-30", query);
        Assert.Contains("by currency, recommendationStatus", query);
        Assert.Contains(
            "summarize arg_max(_effectiveOn, *) by recommendationTypeId, " +
            "tostring(properties.resourceMetadata.resourceId), recommendationStatus",
            query);
        Assert.DoesNotContain("recommendationStatus) =~ 'New'", query);
    }

    [Fact]
    public void ParseCostSummary_SeparatesUnavailablePricingAndCurrencyTotals()
    {
        using var document = JsonDocument.Parse(
            """
            [
              {
                "currency": "USD",
                "potentialSavings": 120.5,
                "accumulatedWaste": 40.25,
                "realizedSavings": 30,
                "recommendationCount": 2,
                "potentialCount": 2,
                "costCount": 2,
                "realizedCount": 1,
                "billReducingOpportunityCount": 1,
                "zeroSavingsCount": 1,
                "unavailablePricingCount": 0
              },
              {
                "currency": "Unavailable",
                "potentialSavings": null,
                "accumulatedWaste": null,
                "realizedSavings": null,
                "recommendationCount": 3,
                "potentialCount": 0,
                "costCount": 0,
                "realizedCount": 0,
                "billReducingOpportunityCount": 0,
                "zeroSavingsCount": 0,
                "unavailablePricingCount": 3
              },
              {
                "currency": "USD",
                "recommendationStatus": "Completed",
                "potentialSavings": 10,
                "accumulatedWaste": null,
                "realizedSavings": 5,
                "recommendationCount": 1,
                "potentialCount": 1,
                "costCount": 0,
                "realizedCount": 1,
                "billReducingOpportunityCount": 1,
                "zeroSavingsCount": 0,
                "unavailablePricingCount": 0
              }
            ]
            """);

        var summary = RecommendationSummaryService.ParseCostSummary(document.RootElement);

        Assert.Equal(6, summary.RecommendationCount);
        Assert.Equal(2, summary.BillReducingOpportunityCount);
        Assert.Equal(1, summary.ZeroSavingsCount);
        Assert.Equal(3, summary.UnavailablePricingCount);
        var currency = Assert.Single(summary.CurrencyTotals, item => item.Currency == "USD");
        Assert.Equal("USD", currency.Currency);
        Assert.Equal(130.5m, currency.PotentialSavings);
        Assert.Equal(40.25m, currency.AccumulatedWaste);
        Assert.Equal(35m, currency.RealizedSavings);
        Assert.Equal(3, currency.PotentialValueCount);
        Assert.Equal(2, currency.AccumulatedWasteCount);
        Assert.Equal(2, currency.RealizedSavingsCount);
        var unavailable = Assert.Single(summary.CurrencyTotals, item => item.Currency == "Unavailable");
        Assert.Null(unavailable.PotentialSavings);
        Assert.Null(unavailable.AccumulatedWaste);
        Assert.Null(unavailable.RealizedSavings);
        Assert.Equal(5, Assert.Single(summary.StatusTotals!, item => item.Status == "Unknown").RecommendationCount);
        Assert.Equal(1, Assert.Single(summary.StatusTotals!, item => item.Status == "Completed").RecommendationCount);
        Assert.Equal(3, currency.RecommendationCount);
    }

    [Fact]
    public void ParseCostSummary_OrdersTypeTotalsByPotentialSavingsDescending()
    {
        using var document = JsonDocument.Parse(
            """
            [
              {
                "currency": "USD",
                "recommendationTypeId": "type-low",
                "recommendationName": "Low savings",
                "potentialSavings": 10,
                "recommendationCount": 1,
                "potentialCount": 1,
                "costCount": 0,
                "realizedCount": 0,
                "billReducingOpportunityCount": 1,
                "zeroSavingsCount": 0,
                "unavailablePricingCount": 0
              },
              {
                "currency": "USD",
                "recommendationTypeId": "type-high",
                "recommendationName": "High savings",
                "potentialSavings": 100,
                "recommendationCount": 1,
                "potentialCount": 1,
                "costCount": 0,
                "realizedCount": 0,
                "billReducingOpportunityCount": 1,
                "zeroSavingsCount": 0,
                "unavailablePricingCount": 0
              },
              {
                "currency": "Unavailable",
                "recommendationTypeId": "type-unknown",
                "recommendationName": "Unknown savings",
                "potentialSavings": null,
                "recommendationCount": 1,
                "potentialCount": 0,
                "costCount": 0,
                "realizedCount": 0,
                "billReducingOpportunityCount": 0,
                "zeroSavingsCount": 0,
                "unavailablePricingCount": 1
              }
            ]
            """);

        var summary = RecommendationSummaryService.ParseCostSummary(document.RootElement);

        Assert.Equal(
            ["type-high", "type-low", "type-unknown"],
            summary.TypeTotals!.Select(static item => item.RecommendationTypeId));
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
