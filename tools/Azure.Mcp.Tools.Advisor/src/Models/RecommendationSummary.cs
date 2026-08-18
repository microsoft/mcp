// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary> Aggregate counts for Advisor recommendations grouped by a selected field. </summary>
public sealed record RecommendationSummary(
    string GroupBy,
    int TotalRecommendations,
    List<RecommendationGroup> Groups);
