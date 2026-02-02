// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

public record Recommendation(
    [property: JsonPropertyName("resourceId")] string ResourceId,
    [property: JsonPropertyName("recommendationText")] string RecommendationText,
    [property: JsonPropertyName("category")] string Category
);
