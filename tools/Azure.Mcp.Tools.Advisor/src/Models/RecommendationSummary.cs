// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Grouped Advisor recommendation summary.</summary>
/// <param name="GroupBy">The grouping applied to <paramref name="Groups"/>.</param>
/// <param name="TotalRecommendations">
/// Sum of all group counts before <c>--top</c> is applied. In normal summaries the group counts, and therefore this total,
/// are recommendation records. In prioritized summaries each group count is the number of distinct affected resources for
/// that recommendation type, so this total is the sum of affected resources per type: a resource is counted once for each
/// recommendation type that affects it, and duplicate records for the same type and resource are counted once.
/// </param>
/// <param name="Groups">The returned groups, in display order.</param>
public sealed record RecommendationSummary(
    string GroupBy,
    int TotalRecommendations,
    List<RecommendationGroup> Groups);
