// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationMetadataFilters(
    string? ResourceType = null,
    AdvisorRecommendationImpact? Impact = null,
    AdvisorRecommendationCategory? Category = null,
    string? SubCategory = null,
    IReadOnlyList<string>? TrackingIds = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null)
{
    internal const string ServiceRetirementSubCategory = "ServiceUpgradeAndRetirement";
}
