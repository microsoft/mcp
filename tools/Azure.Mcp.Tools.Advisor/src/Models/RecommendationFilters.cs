// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Filter values for Advisor recommendation list and summary operations.
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
