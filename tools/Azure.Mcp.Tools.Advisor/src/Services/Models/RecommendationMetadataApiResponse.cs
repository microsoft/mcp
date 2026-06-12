// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire model for GET https://management.azure.com/providers/Microsoft.Advisor/metadata?api-version=2025-01-01
// Documented at: https://learn.microsoft.com/rest/api/advisor/recommendation-metadata/list
//
// Mirrors MetadataEntityListResult / MetadataEntity / MetadataSupportedValueDetail in the
// REST API spec exactly. Property names follow JsonSourceGenerationOptions on
// AdvisorJsonContext (camelCase naming policy), which matches the ARM payload.

internal record RecommendationMetadataApiResponse(
    List<RecommendationMetadataEntity>? Value
);

internal record RecommendationMetadataEntity(
    string? Id,
    string? Name,
    string? Type,
    RecommendationMetadataProperties? Properties
);

internal record RecommendationMetadataProperties(
    string? DisplayName,
    List<string>? DependsOn,
    List<string>? ApplicableScenarios,
    List<RecommendationMetadataSupportedValue>? SupportedValues
);

internal record RecommendationMetadataSupportedValue(
    string? Id,
    string? DisplayName
);
