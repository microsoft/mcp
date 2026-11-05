// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record PreValidationResult(
    string RecommendationTypeId,
    string Verdict,
    DateTimeOffset EvaluatedAt,
    int MinimumUnusedDays,
    string PricingBasis,
    string Currency,
    bool AreResultsTruncated,
    IReadOnlyList<string> UnmatchedResources,
    IReadOnlyList<PreValidationResourceResult> Safe,
    IReadOnlyList<PreValidationResourceResult> NeedsReview,
    IReadOnlyList<PreValidationResourceResult> Blocked,
    IReadOnlyList<PreValidationBucketSummary> Summary,
    string Notice,
    int? RemainingResourceCount = null)
{
    public int CheckedResourceCount => Safe.Count + NeedsReview.Count + Blocked.Count;
}
