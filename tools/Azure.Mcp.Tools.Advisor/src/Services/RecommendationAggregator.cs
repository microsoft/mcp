// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

internal static class RecommendationAggregator
{
    public const string GroupByRecommendation = "recommendation";
    public const string GroupByCategory = "category";
    public const string GroupByImpact = "impact";
    public const string GroupByResourceType = "resource-type";
    public const string GroupByResource = "resource";

    public static readonly IReadOnlyList<string> AllowedGroupBy =
    [
        GroupByRecommendation,
        GroupByCategory,
        GroupByImpact,
        GroupByResourceType,
        GroupByResource,
    ];

    public static List<RecommendationGroup> Aggregate(
        IEnumerable<Recommendation> recommendations,
        string groupBy,
        int top)
    {
        ArgumentNullException.ThrowIfNull(recommendations);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        if (top <= 0)
        {
            return [];
        }

        Func<Recommendation, string> keySelector = groupBy.ToLowerInvariant() switch
        {
            GroupByRecommendation => r => string.IsNullOrWhiteSpace(r.RecommendationText) ? "Unknown" : r.RecommendationText,
            GroupByCategory => r => string.IsNullOrWhiteSpace(r.Category) ? "Unknown" : r.Category,
            GroupByImpact => r => string.IsNullOrWhiteSpace(r.Impact) ? "Unknown" : r.Impact!,
            GroupByResourceType => r => string.IsNullOrWhiteSpace(r.ImpactedResourceType) ? "Unknown" : r.ImpactedResourceType!,
            GroupByResource => r => string.IsNullOrWhiteSpace(r.ResourceId) ? "Unknown" : r.ResourceId,
            _ => throw new ArgumentException(
                $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
                nameof(groupBy)),
        };

        return [.. recommendations
            .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
            .Select(g => new RecommendationGroup(g.Key, g.Count()))
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Take(top)];
    }
}
