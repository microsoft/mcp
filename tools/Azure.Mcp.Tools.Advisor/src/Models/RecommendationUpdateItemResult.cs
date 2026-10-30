// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationUpdateItemResult(
    string RecommendationId,
    RecommendationUpdateOutcome Outcome,
    Recommendation? Recommendation = null,
    int? StatusCode = null,
    string? Error = null);
