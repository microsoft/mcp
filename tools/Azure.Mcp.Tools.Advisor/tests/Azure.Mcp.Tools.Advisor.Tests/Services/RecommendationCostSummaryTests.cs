// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public sealed class RecommendationCostSummaryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ParseCostSummary_SortsByTheSelectedSavingsMeasureWithMissingValuesLast(bool completed)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new[]
        {
            Row("type-low", potential: completed ? null : 10, realized: completed ? 10 : null),
            Row("type-missing", potential: null),
            Row("type-zero", potential: completed ? null : 0, realized: completed ? 0 : null),
            Row("type-negative", potential: completed ? null : -1, realized: completed ? -1 : null),
            Row("type-high", potential: completed ? null : 100, realized: completed ? 100 : null),
        }));

        var summary = RecommendationSummaryService.ParseCostSummary(document.RootElement, completed);

        Assert.Equal(["type-high", "type-low", "type-zero", "type-negative", "type-missing"],
            summary.TypeTotals!.Select(static item => item.RecommendationTypeId));
        Assert.Equal(5, summary.RecommendationCount);
    }

    [Fact]
    public void BuildSummaryQuery_IncludesCostInTheExistingAggregation()
    {
        var filters = new RecommendationFilters(
            Category: "Cost",
            Impact: "High",
            ResourceTypes: ["Microsoft.Compute/virtualMachines", "Microsoft.Web/serverfarms"],
            ProductFamily: "Compute",
            SubCategory: "IdleResources",
            Status: RecommendationStatus.Completed,
            SavingsAsOf: new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero),
            SavingsFrom: new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            SavingsTo: new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero));
        var query = RecommendationSummaryService.BuildSummaryQuery(
            RecommendationQueryScope.ForSubscriptions(["sub-a", "sub-b"], "rg-a"), "category", filters);

        Assert.Contains("subscriptionId in~ ('sub-a', 'sub-b')", query);
        Assert.Contains("resourceGroup =~ 'rg-a'", query);
        Assert.Contains(RecommendationSummaryService.JoinRecommendationTypeMetadata, query);
        Assert.Contains(RecommendationQueryBuilder.BuildSavingsValues(filters), query);
        Assert.Contains(RecommendationQueryBuilder.BuildHistoricalCompletedFilter(filters), query);
        Assert.Contains("tostring(properties.recommendationStatus) =~ 'Completed'", query);
        Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
        Assert.Contains("category =~ 'Cost'", query);
        Assert.Contains("impact =~ 'High'", query);
        Assert.Contains("subCategory =~ 'IdleResources'", query);
        Assert.Contains("metadataServiceFamily", query);
        Assert.Contains("Microsoft.Compute/virtualMachines", query);
        Assert.Contains("Microsoft.Web/serverfarms", query);
        Assert.Contains("potential = iff(isOpen, annualSavings, real(null))", query);
        Assert.DoesNotContain("arg_max(", query);
        Assert.Contains("by key, label, recommendationStatus, recommendationTypeId, recommendationName", query);
        Assert.Contains("count_ = count()", query);
        Assert.Contains("recommendationCount = count_", query);
        Assert.DoesNotContain("by currency", query);
        Assert.DoesNotContain("providerCurrency", query);
        Assert.DoesNotContain("billingCurrency", query);
        Assert.DoesNotContain("savings.contracted", query);
        Assert.Contains("billReducingOpportunityCount = countif(isfinite(potential) and potential > 0)", query);
        Assert.Contains("zeroSavingsCount = countif(isfinite(potential) and potential <= 0)", query);
        Assert.Contains("unavailablePricingCount = countif(not(isfinite(potential) or isfinite(cost) or isfinite(realizedSavings)))", query);
        Assert.DoesNotContain("| take ", query);
        Assert.DoesNotContain("| limit ", query);
    }

    [Theory]
    [InlineData("category", null, true)]
    [InlineData("status", null, false)]
    [InlineData("status", RecommendationStatus.Completed, false)]
    [InlineData("impact", RecommendationStatus.Dismissed, false)]
    [InlineData("recommendation-type", RecommendationStatus.Postponed, false)]
    public void BuildSummaryQuery_CostUsesTheRequestedGroupingStatusDefaults(
        string groupBy, RecommendationStatus? status, bool defaultsToNew)
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            RecommendationQueryScope.ForServiceGroup("group-a"),
            groupBy,
            new RecommendationFilters(Category: "Cost", SubCategory: "IdleResources", Status: status));

        Assert.Equal(defaultsToNew, query.Contains(RecommendationQueryBuilder.ActiveRecommendationClause, StringComparison.Ordinal));
        if (status is not null)
        {
            Assert.Contains($"tostring(properties.recommendationStatus) =~ '{status}'", query);
        }
        Assert.Contains("/providers/Microsoft.Management/serviceGroups/group-a", query);
        Assert.DoesNotContain("tostring(properties.serviceGroupId) in~", query);
        Assert.DoesNotContain("subscriptionId =~", query);
    }

    [Theory]
    [InlineData("category", false)]
    [InlineData("status", false)]
    [InlineData("recommendation-type", true)]
    public void BuildSummaryQuery_HistoricalDefaultsToCompletedWithoutOverridingExplicitStates(string groupBy, bool prioritized)
    {
        RecommendationStatus?[] statuses =
            [null, RecommendationStatus.New, RecommendationStatus.Postponed, RecommendationStatus.Dismissed, RecommendationStatus.Completed];
        foreach (var status in statuses)
        {
            for (var dates = 0; dates < 8; dates++)
            {
                var filters = new RecommendationFilters(
                    Category: "Cost",
                    SubCategory: "IdleResources",
                    Status: status,
                    Prioritized: prioritized,
                    SavingsFrom: (dates & 1) != 0 ? new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero) : null,
                    SavingsTo: (dates & 2) != 0 ? new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero) : null,
                    SavingsAsOf: (dates & 4) != 0 ? new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero) : null);
                var query = RecommendationSummaryService.BuildSummaryQuery(
                    RecommendationQueryScope.ForSubscription("sub-a", null), groupBy, filters);
                var effectiveStatus = status ?? (dates != 0
                    ? RecommendationStatus.Completed
                    : groupBy != "status" ? RecommendationStatus.New : (RecommendationStatus?)null);
                var hasStatusFilter = effectiveStatus is not null;

                Assert.Equal(hasStatusFilter,
                    query.Contains("tostring(properties.recommendationStatus) =~ '", StringComparison.Ordinal));
                if (hasStatusFilter)
                {
                    Assert.Contains($"tostring(properties.recommendationStatus) =~ '{effectiveStatus}'", query);
                }
                if (status is not null and not RecommendationStatus.New)
                {
                    Assert.DoesNotContain(RecommendationQueryBuilder.ActiveRecommendationClause, query);
                }
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("RightSize")]
    [InlineData("ReservedInstances")]
    public void BuildSummaryQuery_OtherCostWorkflowsKeepTheExistingCountAggregation(string? subCategory)
    {
        var query = RecommendationSummaryService.BuildSummaryQuery(
            RecommendationQueryScope.ForSubscription("sub-a", null),
            RecommendationSummaryService.GroupByCategory,
            new RecommendationFilters(Category: "Cost", SubCategory: subCategory));

        Assert.EndsWith("| summarize count() by key, label", query);
        Assert.DoesNotContain("potentialSavings", query);
        Assert.DoesNotContain("properties.savings", query);
    }

    [Fact]
    public void ParseCostSummary_MergesUsdRetailStatesAndTypesWithoutTreatingUnknownValuesAsZero()
    {
        var summary = Parse(
            Row("TYPE-A", potential: 120.5m, cost: 40.25m),
            Row("type-a", potential: 0),
            Row("type-a", status: "Completed", potential: null, realized: 30),
            Row("type-a", potential: 50, cost: 0),
            Row("type-unknown", potential: null),
            Row("type-b", potential: 10, cost: 1));

        Assert.Equal("Retail", summary.PricingBasis);
        Assert.Equal(6, summary.RecommendationCount);
        Assert.Equal(3, summary.BillReducingOpportunityCount);
        Assert.Equal(1, summary.ZeroSavingsCount);
        Assert.Equal(1, summary.UnavailablePricingCount);
        var usd = Assert.Single(summary.CurrencyTotals);
        Assert.Equal("USD", usd.Currency);
        Assert.Equal(180.5m, usd.PotentialSavings);
        Assert.Equal(41.25m, usd.AccumulatedWaste);
        Assert.Equal(30m, usd.RealizedSavings);
        Assert.Equal(6, usd.RecommendationCount);
        Assert.Equal(4, usd.PotentialValueCount);
        Assert.Equal(3, usd.AccumulatedWasteCount);
        Assert.Equal(1, usd.RealizedSavingsCount);
        var unavailable = Assert.Single(summary.TypeTotals!, static item => item.RecommendationTypeId == "type-unknown");
        Assert.Null(unavailable.PotentialSavings);
        Assert.Null(unavailable.AccumulatedWaste);
        Assert.Null(unavailable.RealizedSavings);
        Assert.Equal(0, unavailable.PotentialValueCount);
        Assert.Equal(5, Assert.Single(summary.StatusTotals!, static item => item.Status == "New").RecommendationCount);
        Assert.Equal(1, Assert.Single(summary.StatusTotals!, static item => item.Status == "Completed").RecommendationCount);
        var type = Assert.Single(summary.TypeTotals!, static item => item.RecommendationTypeId == "type-a");
        Assert.Equal(170.5m, type.PotentialSavings);
        Assert.Equal(40.25m, type.AccumulatedWaste);
        Assert.Equal(30m, type.RealizedSavings);
        Assert.Equal(4, type.RecommendationCount);
        Assert.All(summary.TypeTotals!, static item => Assert.Equal("USD", item.Currency));
        Assert.All(summary.TypeTotals!, static item => Assert.Equal("Retail", item.PricingBasis));
        Assert.Equal(["type-a", "type-b", "type-unknown"], summary.TypeTotals!.Select(static item => item.RecommendationTypeId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(10)]
    public void ParseCostSummary_RetailCurrencyRemainsUsdWhenPricingIsUnavailable(int? potential)
    {
        var summary = Parse(Row(potential: potential));

        Assert.Equal("USD", Assert.Single(summary.CurrencyTotals).Currency);
        Assert.Equal("Retail", summary.PricingBasis);
        Assert.Equal("Retail", Assert.Single(summary.TypeTotals!).PricingBasis);
        Assert.Equal(potential is null ? 1 : 0, summary.UnavailablePricingCount);
    }

    [Fact]
    public void ParseCostSummary_EmptyRowsReturnAnEmptyReport()
    {
        var summary = Parse();

        Assert.Equal("Retail", summary.PricingBasis);
        Assert.Equal(0, summary.RecommendationCount);
        Assert.Equal(0, summary.BillReducingOpportunityCount);
        Assert.Equal(0, summary.ZeroSavingsCount);
        Assert.Equal(0, summary.UnavailablePricingCount);
        Assert.Empty(summary.CurrencyTotals);
        Assert.Empty(summary.TypeTotals!);
        Assert.Empty(summary.StatusTotals!);
    }

    [Fact]
    public void ParseCostSummary_PrioritizedRowsKeepFullPrecisionAndAllLifecycleTotals()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(new[]
        {
            new
            {
                key = "type-a",
                label = "Idle resources",
                costSummaryRows = new[]
                {
                    Row(potential: 364.812345m, cost: 20),
                    Row(status: "Postponed", potential: 0, cost: 0),
                    Row(status: "Dismissed", potential: null),
                    Row(status: "Completed", potential: null, realized: 70),
                },
            },
        }));

        var summary = RecommendationSummaryService.ParseCostSummary(document.RootElement);

        Assert.Equal(4, summary.RecommendationCount);
        var totals = Assert.Single(summary.CurrencyTotals);
        Assert.Equal(364.812345m, totals.PotentialSavings);
        Assert.Equal(20m, totals.AccumulatedWaste);
        Assert.Equal(70m, totals.RealizedSavings);
        Assert.Equal(2, totals.PotentialValueCount);
        Assert.Equal(2, totals.AccumulatedWasteCount);
        Assert.Equal(1, totals.RealizedSavingsCount);
        Assert.Equal(1, summary.BillReducingOpportunityCount);
        Assert.Equal(1, summary.ZeroSavingsCount);
        Assert.Equal(1, summary.UnavailablePricingCount);
        Assert.Equal(4, summary.StatusTotals!.Count);
        var type = Assert.Single(summary.TypeTotals!);
        Assert.Equal("type-a", type.RecommendationTypeId);
        Assert.Equal("Idle resources", type.RecommendationName);
        Assert.Equal(4, type.RecommendationCount);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("[null]")]
    [InlineData("""[{"potentialSavings":"invalid"}]""")]
    [InlineData("""[{"recommendationCount":-1}]""")]
    [InlineData("""[{"recommendationCount":1}]""")]
    [InlineData("""[{"key":"type-a","label":"A","costSummaryRows":null}]""")]
    [InlineData("""[{"key":"type-a","label":"A","costSummaryRows":[null]}]""")]
    public void ParseCostSummary_InvalidPayloadFailsExplicitly(string payload)
    {
        using var document = JsonDocument.Parse(payload);

        Assert.Throws<JsonException>(() => RecommendationSummaryService.ParseCostSummary(document.RootElement));
    }

    [Fact]
    public void ParseCostSummary_CompletedKeepsUnpricedResourcesAndPublishedCostSeparateFromRealizedSavings()
    {
        var summary = Parse(
            Row(status: "Completed", potential: null, cost: 91.42m, realized: 91.42m),
            Row(status: "Completed", potential: null, cost: 486.42m, realized: 8.74m),
            Row(status: "Completed", potential: null, cost: 448.55m, realized: 8.74m),
            Row(status: "Completed", potential: null));

        Assert.Equal(4, summary.RecommendationCount);
        Assert.Equal(1, summary.UnavailablePricingCount);
        var totals = Assert.Single(summary.CurrencyTotals);
        Assert.Null(totals.PotentialSavings);
        Assert.Equal(108.90m, totals.RealizedSavings);
        Assert.Equal(1026.39m, totals.AccumulatedWaste);
        Assert.Equal(3, totals.RealizedSavingsCount);
        Assert.Equal(3, totals.AccumulatedWasteCount);
        Assert.Equal(4, Assert.Single(summary.StatusTotals!).RecommendationCount);
        Assert.Equal(4, Assert.Single(summary.TypeTotals!).RecommendationCount);
    }

    [Fact]
    public void ParseCostSummary_CountOverflowFailsExplicitly()
    {
        Assert.Throws<OverflowException>(() => Parse(
            Row(potential: null, count: int.MaxValue), Row(potential: null)));
    }

    [Fact]
    public void ParseCostSummary_AmountOverflowFailsExplicitly()
    {
        Assert.Throws<OverflowException>(() => Parse(Row(potential: decimal.MaxValue), Row(potential: 1)));
    }

    [Fact]
    public void CostReport_SerializesWithSourceGeneratedContext()
    {
        var summary = new RecommendationSummary("category", 1, [new("Cost", "Cost", 1)],
            Cost: Parse(Row()));
        var json = JsonSerializer.Serialize(summary, AdvisorJsonContext.Default.RecommendationSummary);
        using var document = JsonDocument.Parse(json);
        var cost = document.RootElement.GetProperty("cost");

        Assert.Equal("Retail", cost.GetProperty("pricingBasis").GetString());
        Assert.Equal("Retail", cost.GetProperty("typeTotals")[0].GetProperty("pricingBasis").GetString());
        Assert.Equal(100m, cost.GetProperty("currencyTotals")[0].GetProperty("potentialSavings").GetDecimal());
        Assert.Equal("type-a", cost.GetProperty("typeTotals")[0].GetProperty("recommendationTypeId").GetString());
        Assert.Equal("New", cost.GetProperty("statusTotals")[0].GetProperty("status").GetString());
        Assert.False(cost.TryGetProperty("asOfUtc", out _));
    }

    private static RecommendationCostSummary Parse(params object[] rows) =>
        RecommendationSummaryService.ParseCostSummary(JsonSerializer.SerializeToElement(rows));

    private static object Row(
        string type = "type-a",
        string status = "New",
        decimal? potential = 100,
        decimal? cost = null,
        decimal? realized = null,
        int count = 1) => new
        {
            recommendationTypeId = type,
            recommendationName = type,
            recommendationStatus = status,
            potentialSavings = potential,
            accumulatedWaste = cost,
            realizedSavings = realized,
            recommendationCount = count,
            potentialCount = potential is null ? 0 : count,
            costCount = cost is null ? 0 : count,
            realizedCount = realized is null ? 0 : count,
            billReducingOpportunityCount = potential > 0 ? count : 0,
            zeroSavingsCount = potential <= 0 ? count : 0,
            unavailablePricingCount = potential is null && cost is null && realized is null ? count : 0,
        };
}
