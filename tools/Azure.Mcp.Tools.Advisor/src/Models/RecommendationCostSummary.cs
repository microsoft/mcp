// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Cost-specific aggregates returned when a summary is filtered to Cost recommendations.</summary>
public sealed record RecommendationCostSummary(
    string PricingBasis,
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

/// <summary>Cost aggregate counts for one Advisor lifecycle status.</summary>
public sealed record RecommendationCostStatusSummary(
    string Status,
    int RecommendationCount);

/// <summary>Cost aggregates for one recommendation type and currency.</summary>
public sealed record RecommendationCostTypeSummary(
    string RecommendationTypeId,
    string RecommendationName,
    string Currency,
    string PricingBasis,
    decimal? PotentialSavings,
    decimal? AccumulatedWaste,
    decimal? RealizedSavings,
    int RecommendationCount,
    int PotentialValueCount,
    int AccumulatedWasteCount,
    int RealizedSavingsCount);

/// <summary>
/// Cost aggregates for one currency; currencies are never combined. PotentialSavings is the
/// annualized Advisor estimate. AccumulatedWaste is the estimated cost-to-date measure. Each value
/// is the sum of per-recommendation values, with null when no recommendation in the currency group
/// has that value.
/// </summary>
public sealed record RecommendationCostCurrencySummary(
    string Currency,
    decimal? PotentialSavings,
    decimal? AccumulatedWaste,
    decimal? RealizedSavings,
    int RecommendationCount,
    int PotentialValueCount,
    int AccumulatedWasteCount,
    int RealizedSavingsCount);
