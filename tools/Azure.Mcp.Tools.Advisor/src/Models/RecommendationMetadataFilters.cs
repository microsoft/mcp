// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationMetadataFilters(
    string? ResourceType = null,
    string? Impact = null,
    string? Category = null,
    string? SubCategory = null,
    IReadOnlyList<string>? TrackingIds = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null)
{
    internal const string ServiceRetirementSubCategory = "ServiceUpgradeAndRetirement";
}
