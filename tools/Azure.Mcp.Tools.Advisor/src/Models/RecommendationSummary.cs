// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationSummary(
    string GroupBy,
    int Top,
    int TotalRecommendations,
    bool AreResultsTruncated,
    List<RecommendationGroup> Groups);
