// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire model for GET https://management.azure.com/providers/Microsoft.Advisor/metadata?api-version=2020-01-01
// Documented at: https://learn.microsoft.com/rest/api/advisor/recommendation-metadata/list
//
// The endpoint returns the catalog of Advisor metadata dimensions (recommendationType,
// category, impact, etc.). Each entity in 'value[]' represents one dimension whose
// 'properties.supportedValues' is the list of allowed ids and display names for that
// dimension.

internal record RecommendationMetadataApiResponse(
    [property: JsonPropertyName("value")] List<RecommendationMetadataEntity>? Value
);

internal record RecommendationMetadataEntity(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("properties")] RecommendationMetadataProperties? Properties
);

internal record RecommendationMetadataProperties(
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("applicableScenarios")] List<string>? ApplicableScenarios,
    [property: JsonPropertyName("dependsOn")] List<string>? DependsOn,
    [property: JsonPropertyName("supportedValues")] List<RecommendationMetadataSupportedValue>? SupportedValues
);

internal record RecommendationMetadataSupportedValue(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("recommendationCategory")] string? RecommendationCategory,
    [property: JsonPropertyName("recommendationImpact")] string? RecommendationImpact,
    [property: JsonPropertyName("recommendationSubCategory")] string? RecommendationSubCategory,
    [property: JsonPropertyName("supportedResourceType")] string? SupportedResourceType
);
