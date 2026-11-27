// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Cost recommendation count for one lifecycle state.</summary>
public sealed record RecommendationCostStatusSummary(string Status, int RecommendationCount);
