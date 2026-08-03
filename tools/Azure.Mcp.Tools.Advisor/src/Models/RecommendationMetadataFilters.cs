// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationMetadataFilters(
    string? ResourceType = null,
    string? Impact = null,
    string? Category = null,
    string? SubCategory = null,
    string? TrackingId = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null)
{
    internal const string HighAvailabilityCategory = "HighAvailability";
    internal const string ServiceRetirementSubCategory = "ServiceUpgradeAndRetirement";
}
