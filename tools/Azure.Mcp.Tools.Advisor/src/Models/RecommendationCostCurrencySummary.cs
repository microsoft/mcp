// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Retail USD savings and value-availability counts; absent measures remain null.</summary>
public sealed record RecommendationCostCurrencySummary(
    string Currency,
    decimal? PotentialSavings,
    decimal? AccumulatedWaste,
    decimal? RealizedSavings,
    int RecommendationCount,
    int PotentialValueCount,
    int AccumulatedWasteCount,
    int RealizedSavingsCount);
