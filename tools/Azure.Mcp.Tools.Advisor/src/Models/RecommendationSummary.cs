// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationSummary(
    string GroupBy,
    int TotalRecommendations,
    List<RecommendationGroup> Groups);
