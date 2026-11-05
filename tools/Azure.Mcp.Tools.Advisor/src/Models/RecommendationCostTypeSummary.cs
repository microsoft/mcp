// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Retail USD savings and value-availability counts for one recommendation type.</summary>
public sealed record RecommendationCostTypeSummary(
    string RecommendationTypeId,
    string RecommendationName,
    string Currency,
    string? PricingBasis,
    decimal? PotentialSavings,
    decimal? AccumulatedWaste,
    decimal? RealizedSavings,
    int RecommendationCount,
    int PotentialValueCount,
    int AccumulatedWasteCount,
    int RealizedSavingsCount);
