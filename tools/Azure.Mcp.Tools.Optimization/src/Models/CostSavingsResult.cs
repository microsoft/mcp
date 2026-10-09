// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// Result of listing cost-saving recommendations. When <see cref="SubscriptionOptions"/> is
/// populated, the subscription name matched more than one subscription and no recommendations were
/// returned; the caller should re-run using the exact subscription id. <see cref="DiskRecommendationSummary"/>
/// is populated only when unattached-disk recommendations exist.
/// </summary>
public sealed record CostSavingsResult(
    List<CostSavingsRecommendation> Recommendations,
    bool AreResultsTruncated,
    IReadOnlyList<SubscriptionOption>? SubscriptionOptions = null,
    DiskRecommendationSummary? DiskRecommendationSummary = null);
