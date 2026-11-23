// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record PreValidationResourceResult(
    string ResourceId,
    string? ResourceName,
    string ResourceType,
    IReadOnlyList<string> RecommendationIds,
    string Verdict,
    IReadOnlyList<PreValidationCheckResult> Checks,
    IReadOnlyList<string> ManualConfirmations,
    decimal? YearlyPotentialSavings,
    decimal? Cost,
    DateTimeOffset? ResourceDeletedOn = null,
    string? Reason = null,
    bool? CanPostponeDismiss = null);
