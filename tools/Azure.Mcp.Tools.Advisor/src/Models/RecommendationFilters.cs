// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationFilters(
    string? Category = null,
    string? Impact = null,
    string? RecommendationTypeId = null,
    string? ResourceType = null,
    string? Resource = null,
    string? Search = null,
    string? SubCategory = null,
    IReadOnlyList<string>? TrackingIds = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null);
