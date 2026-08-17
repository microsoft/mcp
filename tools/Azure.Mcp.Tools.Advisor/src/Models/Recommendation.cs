// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A subscription-scoped Advisor recommendation instance. Fields after
/// <see cref="ImpactedResourceType"/> are refreshed from the Advisor recommendation metadata
/// catalog so that catalog-owned values are never served from a stale recommendation snapshot.
/// </summary>
public sealed record Recommendation(
    [property: JsonPropertyName("resourceId")] string ResourceId,
    [property: JsonPropertyName("recommendationText")] string RecommendationText,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("impact")] string? Impact = null,
    [property: JsonPropertyName("impactedResourceType")] string? ImpactedResourceType = null,
    [property: JsonPropertyName("recommendationTypeId")] string? RecommendationTypeId = null,
    [property: JsonPropertyName("subCategory")] string? SubCategory = null,
    [property: JsonPropertyName("potentialBenefits")] string? PotentialBenefits = null,
    [property: JsonPropertyName("learnMoreLink")] string? LearnMoreLink = null,
    [property: JsonPropertyName("serviceRetirement")] RecommendationServiceRetirement? ServiceRetirement = null);
