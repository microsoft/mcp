// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Full-population retail USD Cost aggregates, independent of the displayed summary groups.</summary>
public sealed record RecommendationCostSummary(
    string? PricingBasis,
    int RecommendationCount,
    int BillReducingOpportunityCount,
    int ZeroSavingsCount,
    int UnavailablePricingCount,
    List<RecommendationCostCurrencySummary> CurrencyTotals,
    DateTimeOffset? AsOfUtc = null,
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    List<RecommendationCostStatusSummary>? StatusTotals = null,
    List<RecommendationCostTypeSummary>? TypeTotals = null);
