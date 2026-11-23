// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record PreValidationBucketSummary(
    string Verdict,
    int Count,
    decimal? YearlyPotentialSavings,
    decimal? Cost,
    int MissingSavingsCount,
    int MissingCostCount,
    int DeletedCount = 0);
