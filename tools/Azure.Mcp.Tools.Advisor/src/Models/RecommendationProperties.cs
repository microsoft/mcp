// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>Properties returned for an Azure Advisor recommendation.</summary>
public sealed record RecommendationProperties(
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("impact")] string? Impact = null,
    [property: JsonPropertyName("impactedField")] string? ImpactedField = null,
    [property: JsonPropertyName("impactedValue")] string? ImpactedValue = null,
    [property: JsonPropertyName("recommendationStatus")] string? RecommendationStatus = null,
    [property: JsonPropertyName("lastRefreshed")] DateTimeOffset? LastRefreshed = null,
    [property: JsonPropertyName("lastUpdated")] DateTimeOffset? LastUpdated = null,
    [property: JsonPropertyName("createdTime")] DateTimeOffset? CreatedTime = null,
    [property: JsonPropertyName("recommendationTypeId")] string? RecommendationTypeId = null,
    [property: JsonPropertyName("shortDescription")] RecommendationShortDescription? ShortDescription = null,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, JsonElement>? Metadata = null,
    [property: JsonPropertyName("extendedProperties")] IReadOnlyDictionary<string, JsonElement>? ExtendedProperties = null,
    [property: JsonPropertyName("resourceMetadata")] RecommendationResourceMetadata? ResourceMetadata = null,
    [property: JsonPropertyName("risk")] string? Risk = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("label")] string? Label = null,
    [property: JsonPropertyName("learnMoreLink")] string? LearnMoreLink = null,
    [property: JsonPropertyName("potentialBenefits")] string? PotentialBenefits = null,
    [property: JsonPropertyName("actions")] JsonElement? Actions = null,
    [property: JsonPropertyName("remediation")] JsonElement? Remediation = null,
    [property: JsonPropertyName("exposedMetadataProperties")] IReadOnlyDictionary<string, JsonElement>? ExposedMetadataProperties = null,
    [property: JsonPropertyName("trackedProperties")] JsonElement? TrackedProperties = null,
    [property: JsonPropertyName("review")] JsonElement? Review = null,
    [property: JsonPropertyName("resourceWorkload")] JsonElement? ResourceWorkload = null,
    [property: JsonPropertyName("sourceSystem")] string? SourceSystem = null,
    [property: JsonPropertyName("notes")] string? Notes = null);
