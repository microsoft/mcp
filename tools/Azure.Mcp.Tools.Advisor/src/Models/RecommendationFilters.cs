// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationFilters(
    string? Category = null,
    string? Impact = null,
    RecommendationStatus? Status = null,
    string? RecommendationTypeId = null,
    IReadOnlyList<string>? RecommendationTypeIds = null,
    string? ResourceType = null,
    IReadOnlyList<string>? ResourceTypes = null,
    string? ProductFamily = null,
    string? Resource = null,
    string? Search = null,
    string? SubCategory = null,
    IReadOnlyList<string>? TrackingIds = null,
    string? RetirementDateOperator = null,
    DateOnly? RetirementDate = null,
    decimal? MinimumSavings = null,
    DateTimeOffset? SavingsAsOf = null,
    DateTimeOffset? SavingsFrom = null,
    DateTimeOffset? SavingsTo = null,
    string? ServiceGroup = null,
    IReadOnlyList<string>? ServiceGroups = null,
    bool? Prioritized = null,
    bool? ShowPrioritizationSignals = null);
