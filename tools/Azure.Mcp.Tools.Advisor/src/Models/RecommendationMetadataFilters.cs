// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationMetadataFilters(
    string? ResourceType = null,
    AdvisorImpact? Impact = null,
    AdvisorCategory? Category = null,
    string? SubCategory = null,
    string? TrackingId = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null)
{
    internal const string ServiceRetirementSubCategory = "ServiceUpgradeAndRetirement";
}
