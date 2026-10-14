// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>The weighted contribution of an Advisor prioritization signal.</summary>
public sealed record RecommendationSignalScore(
    [property: JsonPropertyName("weightedScore")] double WeightedScore);
