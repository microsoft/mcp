// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Text.Json;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Represents an Azure Advisor recommendation and its impacted resource. Type-level fields are
/// populated from the matching recommendation metadata record.
/// </summary>
public sealed record Recommendation(
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("control")] string? Control = null,
    [property: JsonPropertyName("impact")] string? Impact = null,
    [property: JsonPropertyName("impactedField")] string? ImpactedField = null,
    [property: JsonPropertyName("impactedValue")] string? ImpactedValue = null,
    [property: JsonPropertyName("recommendationStatus")] string? RecommendationStatus = null,
    [property: JsonPropertyName("lastRefreshed")] DateTimeOffset? LastRefreshed = null,
    [property: JsonPropertyName("lastUpdated")] DateTimeOffset? LastUpdated = null,
    [property: JsonPropertyName("createdTime")] DateTimeOffset? CreatedTime = null,
    [property: JsonPropertyName("recommendationTypeId")] string? RecommendationTypeId = null,
    [property: JsonPropertyName("shortDescription")] RecommendationShortDescription? ShortDescription = null,
    [property: JsonPropertyName("extendedProperties")] IReadOnlyDictionary<string, JsonElement>? ExtendedProperties = null,
    [property: JsonPropertyName("subCategory")] string? SubCategory = null,
    [property: JsonPropertyName("resourceId")] string ResourceId = "Unknown",
    [property: JsonPropertyName("impactedResourceType")] string? ImpactedResourceType = null,
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("subscriptionId")] string? SubscriptionId = null,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup = null,
    [property: JsonPropertyName("tenantId")] string? TenantId = null);
