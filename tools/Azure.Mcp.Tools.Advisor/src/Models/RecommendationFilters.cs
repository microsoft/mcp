// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record RecommendationFilters(
    string? Category = null,
    string? Impact = null,
    string? ResourceType = null,
    string? Resource = null,
    string? Search = null);
