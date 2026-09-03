// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

internal static class AdvisorRecommendationGroupByExtensions
{
    internal static string ToValue(this AdvisorRecommendationGroupBy groupBy) => groupBy switch
    {
        AdvisorRecommendationGroupBy.RecommendationType => "recommendation-type",
        AdvisorRecommendationGroupBy.Category => "category",
        AdvisorRecommendationGroupBy.Impact => "impact",
        AdvisorRecommendationGroupBy.ResourceType => "resource-type",
        _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
    };
}
