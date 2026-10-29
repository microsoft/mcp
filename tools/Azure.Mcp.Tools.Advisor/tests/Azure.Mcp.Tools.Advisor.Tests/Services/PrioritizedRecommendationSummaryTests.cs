// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

public class PrioritizedRecommendationSummaryTests
{
    private static readonly RecommendationFilters s_prioritized = new(Prioritized: true);

    private static string BuildSubscriptionQuery(RecommendationFilters? filters = null) =>
        RecommendationSummaryService.BuildSummaryQuery(
            "subscription-id",
            null,
            RecommendationSummaryService.GroupByRecommendationType,
            filters ?? s_prioritized);

    [Fact]
    public void Build_AppliesStagesInDocumentedOrder()
    {
        var query = BuildSubscriptionQuery();

        string[] stageMarkers =
        [
            "subscriptionId =~ 'subscription-id'",
            RecommendationQueryBuilder.ActiveRecommendationClause,
            "bag_has_key(extendedProperties, 'term')",
            "extend resourceKey =",
            "by recommendationTypeId, resourceKey",
            "summarize resourceCount = count()",
            "join kind=inner",
            "| order by ",
            "| project key =",
        ];
        var positions = stageMarkers.Select(marker => query.IndexOf(marker, StringComparison.Ordinal)).ToList();

        Assert.All(positions, position => Assert.True(position >= 0));
        Assert.Equal(positions.OrderBy(position => position), positions);
    }

    [Fact]
    public void Build_SubscriptionScope_ExcludesServiceGroupRecommendations()
    {
        var query = BuildSubscriptionQuery();

        Assert.Contains(RecommendationQueryBuilder.CurrentRecommendationNameClause, query);
        Assert.Contains(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
    }

    [Fact]
    public void Build_ServiceGroupScope_UsesExactServiceGroupWithoutSubscription()
    {
        var query = RecommendationSummaryService.BuildServiceGroupSummaryQuery(
            "service-group-test",
            RecommendationSummaryService.GroupByRecommendationType,
            s_prioritized);

        Assert.DoesNotContain("subscriptionId =~", query);
        Assert.DoesNotContain(RecommendationQueryBuilder.ServiceGroupExclusionClause, query);
        Assert.Contains(
            $"tostring(properties.serviceGroupId) =~ '{RecommendationQueryBuilder.ServiceGroupResourceIdPrefix}service-group-test'",
            query);
    }

    [Fact]
    public void Build_KeepsTrackedRecordsAndAppliesNoConfigurationExclusions()
    {
        var query = BuildSubscriptionQuery();

        Assert.DoesNotContain("tracked", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("suppression", query, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("microsoft.advisor/configurations", query, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_KeepsOnlyThreeYearSevenDayReservedInstanceScenario()
    {
        var query = BuildSubscriptionQuery();

        Assert.Equal(26, PrioritizedRecommendationSummaryQuery.ReservedInstanceRecommendationTypeIds.Distinct().Count());
        Assert.All(
            PrioritizedRecommendationSummaryQuery.ReservedInstanceRecommendationTypeIds,
            id => Assert.Contains($"'{id}'", query));
        Assert.Contains(
            "or (tostring(extendedProperties.term) == 'P3Y' and tostring(extendedProperties.lookbackPeriod) == '7')",
            query);
        Assert.Contains("or isnull(extendedProperties)", query);
        Assert.Contains(
            "(not(bag_has_key(extendedProperties, 'term')) and not(bag_has_key(extendedProperties, 'lookbackPeriod')))",
            query);
    }

    [Fact]
    public void Build_ConvertsDailySavingsToAnnualWithLegacyFallbackAndNeverZero()
    {
        var query = BuildSubscriptionQuery();

        Assert.Contains(
            "annualSavings = iff(isfinite(dailySavings), dailySavings * 12 * 30.4, iff(isfinite(legacyAnnualSavings), legacyAnnualSavings, real(null)))",
            query);
        Assert.Contains("annualSavings = iff(annualSavingsCount > 0, annualSavings, real(null))", query);
        Assert.Contains("criticalityScore = iff(criticalityScoreCount > 0, criticalityScore, real(null))", query);
        Assert.Contains("summedCriticality = iff(criticalityScoreCount > 0, summedCriticality, real(null))", query);
        Assert.DoesNotContain("coalesce(", query);
    }

    [Fact]
    public void Build_RoundsReturnedSavingsOnlyAfterOrdering()
    {
        var query = BuildSubscriptionQuery();

        var order = query.IndexOf("| order by ", StringComparison.Ordinal);
        var rounding = query.IndexOf("round(annualSavings, 2)", StringComparison.Ordinal);
        Assert.True(order > 0 && rounding > order);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(query, @"round\("));
    }

    [Fact]
    public void Build_CollapsesPerResourceWithMaxValues()
    {
        var query = BuildSubscriptionQuery();

        Assert.Contains(
            "summarize criticalityScore = max(criticalityScore), criticalityScoreCount = countif(isnotnull(criticalityScore)), " +
            "annualSavings = max(annualSavings), annualSavingsCount = countif(isnotnull(annualSavings)) " +
            "by recommendationTypeId, resourceKey",
            query);
        Assert.Contains("resourceKey = iff(isempty(resourceKey), tolower(name), resourceKey)", query);
    }

    [Fact]
    public void Build_OrdersByDocumentedKeysInOneOrderBy()
    {
        var query = BuildSubscriptionQuery();

        Assert.Contains(
            "| order by metadataPriorityScore desc nulls last, annualSavings desc nulls last, summedCriticality desc nulls last, " +
            "retirementSortKey asc nulls last, resourceCountSortKey asc, labelSortKey asc, recommendationTypeId asc",
            query);
        Assert.Contains("resourceCountSortKey = iff(category =~ 'Cost', resourceCount, -resourceCount)", query);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(query, @"\| order by "));
    }

    [Fact]
    public void Build_TakesTypePropertiesOnlyFromMetadata()
    {
        var query = BuildSubscriptionQuery();

        Assert.Contains(
            "extend category = metadataCategory, impact = metadataImpact, subCategory = metadataSubCategory, " +
            "retirementDate = format_datetime(startofday(todatetime(metadataRetirementDate)), 'yyyy-MM-dd')",
            query);
        Assert.Contains("metadataPriorityScore=todouble(properties.priorityScore)", query);
        Assert.Contains("metadataSignalValues=properties.signalValues", query);
        Assert.DoesNotContain("tostring(properties.category)", query);
        Assert.DoesNotContain("properties.extendedProperties.retirementDate", query);
    }

    [Fact]
    public void Build_ProjectsNoScoresOrSortKeys()
    {
        var query = BuildSubscriptionQuery();
        var projection = query[query.LastIndexOf("| project ", StringComparison.Ordinal)..];

        Assert.Equal(
            "| project key = recommendationTypeId, label, count_ = resourceCount, " +
            "category, impact, subCategory, retirementDate, annualSavings = round(annualSavings, 2), signalValues = metadataSignalValues",
            projection);
    }

    [Fact]
    public void Build_AppliesMetadataFiltersAfterJoinAndInstanceFiltersBefore()
    {
        var query = BuildSubscriptionQuery(new RecommendationFilters(
            Category: "Cost",
            Impact: "High",
            Search: "it's|x",
            RecommendationTypeId: "1d70919c-1a4a-4f79-8300-bb576c291e9d",
            Prioritized: true));

        var join = query.IndexOf("join kind=inner", StringComparison.Ordinal);
        Assert.True(query.IndexOf("where category =~ 'Cost'", StringComparison.Ordinal) > join);
        Assert.True(query.IndexOf("where impact =~ 'High'", StringComparison.Ordinal) > join);
        Assert.True(query.IndexOf("contains 'it''sx'", StringComparison.Ordinal) is > 0 and var search && search < join);
        Assert.True(query.IndexOf("=~ '1d70919c-1a4a-4f79-8300-bb576c291e9d'", StringComparison.Ordinal) < join);
        Assert.DoesNotContain("it's|x", query);
    }

    [Fact]
    public void Build_RetirementDateFilter_InfersRetirementSubCategory()
    {
        var query = BuildSubscriptionQuery(new RecommendationFilters(
            RetirementDateOperator: "le",
            RetirementDate: new DateOnly(2026, 12, 31),
            Prioritized: true));

        Assert.Contains("subCategory =~ 'ServiceUpgradeAndRetirement'", query);
        Assert.Contains("todatetime(retirementDate) <= datetime(2026-12-31)", query);
    }

    [Theory]
    [InlineData("category")]
    [InlineData("status")]
    [InlineData("retirement-date")]
    public void Build_NonTypeGrouping_Throws(string groupBy)
    {
        Assert.Throws<ArgumentException>(() =>
            RecommendationSummaryService.BuildSummaryQuery("subscription-id", null, groupBy, s_prioritized));
    }

    [Fact]
    public void ParsePrioritizedSummary_KeepsArgOrderTotalsAndFirstDuplicate()
    {
        using var document = JsonDocument.Parse(
            """
            [
              { "key": "type-b", "label": "B", "count_": 1, "category": "HighAvailability" },
              { "key": "type-a", "label": "A", "count_": 5, "category": "Security" },
              { "key": "TYPE-B", "label": "B duplicate", "count_": 9 },
              { "key": "type-c", "label": "C", "count_": 2 }
            ]
            """);

        var summary = RecommendationSummaryService.ParsePrioritizedSummary(document.RootElement);

        Assert.Equal("recommendation-type", summary.GroupBy);
        Assert.Equal(["type-b", "type-a", "type-c"], summary.Groups.Select(group => group.Key));
        Assert.Equal(8, summary.TotalRecommendations);
        Assert.Equal("B", summary.Groups[0].Label);
    }

    [Fact]
    public void ParsePrioritizedSummary_BuildsSignalsAndOmitsAbsentValues()
    {
        using var document = JsonDocument.Parse(
            """
            [
              {
                "key": "type-a", "label": "A", "count_": 3,
                "category": "Cost", "impact": "Medium", "subCategory": "", "retirementDate": "",
                "annualSavings": 2189.5, "signalValues": null
              },
              {
                "key": "type-b", "label": "B", "count_": 2,
                "category": "HighAvailability", "impact": "High", "subCategory": "Unknown",
                "retirementDate": "2025-02-28", "annualSavings": null,
                "signalValues": { "recommendationImpact": "high", "recommendationSubcategory": "retirementPastDue", "criticalService": "qcs" }
              }
            ]
            """);

        var summary = RecommendationSummaryService.ParsePrioritizedSummary(document.RootElement);
        var cost = summary.Groups[0].PrioritizationSignals!;
        var retirement = summary.Groups[1].PrioritizationSignals!;

        Assert.Equal("Cost", cost.Category);
        Assert.Null(cost.SubCategory);
        Assert.Null(cost.RetirementDate);
        Assert.Equal(2189.5, cost.AnnualSavings);
        Assert.Equal("USD", cost.AnnualSavingsCurrency);
        Assert.Null(cost.RetirementWindow);

        Assert.Equal("High", retirement.Impact);
        Assert.Null(retirement.SubCategory);
        Assert.Equal("2025-02-28", retirement.RetirementDate);
        Assert.Null(retirement.AnnualSavings);
        Assert.Null(retirement.AnnualSavingsCurrency);
        Assert.Equal("retirementPastDue", retirement.RetirementWindow);
        Assert.Equal("qcs", retirement.CriticalService);
    }

    [Fact]
    public void ParsePrioritizedSummary_ZeroSavingsIsKept()
    {
        using var document = JsonDocument.Parse(
            """[{ "key": "type-a", "label": "A", "count_": 1, "annualSavings": 0 }]""");

        var summary = RecommendationSummaryService.ParsePrioritizedSummary(document.RootElement);

        Assert.Equal(0, summary.Groups[0].PrioritizationSignals!.AnnualSavings);
        Assert.Equal("USD", summary.Groups[0].PrioritizationSignals!.AnnualSavingsCurrency);
    }

    [Fact]
    public void ParsePrioritizedSummary_InvalidCountThrows()
    {
        using var document = JsonDocument.Parse("""[{ "key": "type-a", "label": "A", "count_": -1 }]""");

        Assert.Throws<JsonException>(() =>
            RecommendationSummaryService.ParsePrioritizedSummary(document.RootElement));
    }

    [Theory]
    [InlineData("recommendationSubcategory", "retirementPastDue", "retirementWindow")]
    [InlineData("recommendationSubcategory", "retirementInDays25", "retirementWindow")]
    [InlineData("recommendationSubcategory", "retirementWithin30Days", "retirementWindow")]
    [InlineData("recommendationSubcategory", "RetirementWithin180Days", "retirementWindow")]
    [InlineData("recommendationSubcategory", "retirementInDays7", "retirementWindow")]
    [InlineData("resourceTypeCriticality", "Networking", "resourceTypeCriticality")]
    [InlineData("resourceTypeCriticality", "OperationalData", "resourceTypeCriticality")]
    [InlineData("criticalService", "qcs", "criticalService")]
    [InlineData("criticalService", "QCS", "criticalService")]
    [InlineData("effort", "Low", "effort")]
    [InlineData("effort", "High", "effort")]
    [InlineData("actionability", "OneClickOrFullyExecutable", "actionability")]
    [InlineData("costImplication", "noCostImpact", "costImplication")]
    [InlineData("costImplication", "lowCost", "costImplication")]
    [InlineData("costImplication", "otherCost", "costImplication")]
    [InlineData("ResourceTypeCriticality", "Compute", "resourceTypeCriticality")]
    public void Filter_EmitsMeaningfulValuesVerbatim(string key, string value, string property)
    {
        var signals = Filter(new Dictionary<string, string> { [key] = value });

        Assert.Equal(value, ReadProperty(signals, property));
    }

    [Theory]
    [InlineData("recommendationImpact", "high")]
    [InlineData("recommendationSubcategory", "other")]
    [InlineData("recommendationSubcategory", "zonalResiliency")]
    [InlineData("recommendationSubcategory", "retirementUnknown")]
    [InlineData("recommendationSubcategory", "retirementInDays")]
    [InlineData("recommendationSubcategory", "retirementInDays7x")]
    [InlineData("recommendationSubcategory", "retirementWithin365Days")]
    [InlineData("recommendationSubcategory", "retirementPastDueSoon")]
    [InlineData("recommendationSubcategory", "dataProtectionAndRecovery")]
    [InlineData("resourceTypeCriticality", "Other")]
    [InlineData("criticalService", "nonQcs")]
    [InlineData("effort", "Unknown")]
    [InlineData("actionability", "Guidance")]
    [InlineData("costImplication", "costUnavailable")]
    [InlineData("costImplication", "notAvailable")]
    [InlineData("effort", "none")]
    [InlineData("effort", " ")]
    public void Filter_OmitsValuesWithoutCustomerMeaning(string key, string value)
    {
        var signals = Filter(new Dictionary<string, string> { [key] = value });

        Assert.Equal(new RecommendationPrioritizationSignals(), signals);
    }

    [Fact]
    public void Filter_IgnoresUnlistedSignals()
    {
        using var document = JsonDocument.Parse(
            """{ "newSignal": "valueA", "category": "Injected", "impact": "Injected", "numeric": 3 }""");

        Assert.Equal(new RecommendationPrioritizationSignals(), PrioritizationSignalFilter.Filter(document.RootElement));
    }

    [Fact]
    public void Filter_FirstMeaningfulValueWinsForCaseVariantKeys()
    {
        using var document = JsonDocument.Parse("""{ "effort": "Unknown", "Effort": "Low", "EFFORT": "High" }""");

        Assert.Equal("Low", PrioritizationSignalFilter.Filter(document.RootElement).Effort);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public void Filter_NonObjectReturnsNoSignals(string json)
    {
        using var document = JsonDocument.Parse(json);

        Assert.Equal(new RecommendationPrioritizationSignals(), PrioritizationSignalFilter.Filter(document.RootElement));
    }

    private static RecommendationPrioritizationSignals Filter(Dictionary<string, string> values)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(values));
        return PrioritizationSignalFilter.Filter(document.RootElement);
    }

    private static string? ReadProperty(RecommendationPrioritizationSignals signals, string property) => property switch
    {
        "retirementWindow" => signals.RetirementWindow,
        "resourceTypeCriticality" => signals.ResourceTypeCriticality,
        "criticalService" => signals.CriticalService,
        "effort" => signals.Effort,
        "actionability" => signals.Actionability,
        "costImplication" => signals.CostImplication,
        _ => throw new ArgumentOutOfRangeException(nameof(property)),
    };
}
