// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationFilters(
    string? Category = null,
    string? Impact = null,
    RecommendationStatus? Status = null,
    IReadOnlyList<string>? RecommendationTypeIds = null,
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
    bool? Prioritized = null,
    bool? ShowPrioritizationSignals = null,
    IReadOnlyList<string>? Resources = null)
{
    internal bool EnumerateAll { get; init; }
    internal bool ReturnPage { get; init; }
    public string? ContinuationToken { get; init; }
}
