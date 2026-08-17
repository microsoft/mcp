// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Filters applied when listing or summarizing Advisor recommendations.
/// <see cref="SubCategory"/>, <see cref="TrackingIds"/>, <see cref="RetirementDateOperator"/> and
/// <see cref="RetirementDate"/> are not present on recommendation instances in Azure Resource Graph;
/// they are resolved against the recommendation metadata catalog and translated into a
/// recommendation-type-id restriction before the recommendation query runs.
/// </summary>
public sealed record RecommendationFilters(
    string? Category = null,
    string? Impact = null,
    string? ResourceType = null,
    string? Resource = null,
    string? Search = null,
    string? SubCategory = null,
    IReadOnlyList<string>? TrackingIds = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null);
