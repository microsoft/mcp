// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record Recommendation(
    [property: JsonPropertyName("properties"), JsonPropertyOrder(4)] RecommendationProperties Properties,
    [property: JsonPropertyName("id"), JsonPropertyOrder(1)] string? Id = null,
    [property: JsonPropertyName("name"), JsonPropertyOrder(2)] string? Name = null,
    [property: JsonPropertyName("type"), JsonPropertyOrder(3)] string? Type = null);
