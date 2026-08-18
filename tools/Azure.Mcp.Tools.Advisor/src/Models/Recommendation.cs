// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

public sealed record Recommendation(
    [property: JsonPropertyName("resourceId")] string ResourceId,
    [property: JsonPropertyName("recommendationText")] string RecommendationText,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("impact")] string? Impact = null,
    [property: JsonPropertyName("impactedResourceType")] string? ImpactedResourceType = null,
    [property: JsonPropertyName("recommendationId")] string? RecommendationId = null,
    [property: JsonPropertyName("solution")] string? Solution = null,
    [property: JsonPropertyName("subCategory")] string? SubCategory = null,
    [property: JsonPropertyName("impactedResource")] string? ImpactedResource = null,
    [property: JsonPropertyName("recommendationStatus")] string? RecommendationStatus = null,
    [property: JsonPropertyName("recommendationDismissReason")] string? RecommendationDismissReason = null,
    [property: JsonPropertyName("postponedUntilDateTime")] DateTimeOffset? PostponedUntilDateTime = null,
    [property: JsonPropertyName("lastUpdated")] DateTimeOffset? LastUpdated = null,
    [property: JsonPropertyName("recommendationTypeId")] string? RecommendationTypeId = null,
    [property: JsonPropertyName("completionType")] string? CompletionType = null);
