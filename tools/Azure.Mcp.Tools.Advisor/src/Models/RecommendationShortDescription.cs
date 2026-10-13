// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A short description of an Advisor recommendation type.
/// </summary>
public sealed record RecommendationShortDescription(
    [property: JsonPropertyName("problem")] string? Problem,
    [property: JsonPropertyName("solution")] string? Solution);
