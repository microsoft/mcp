// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Compatible Chaos scenario identified by exact ARM ID and action contract.</summary>
public sealed record ChaosScenarioCandidate(
    string Id,
    string Name,
    string? RecommendationStatus,
    IReadOnlyList<string> ActionIds);
