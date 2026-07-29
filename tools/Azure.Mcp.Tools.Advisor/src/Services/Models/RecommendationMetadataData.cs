// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Advisor.Commands;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

// Wire model for Azure Resource Graph 'advisorresources' rows of type
// 'microsoft.advisor/metadata' (the global Advisor recommendation-metadata catalog).
// Property names follow the JsonSourceGenerationOptions camelCase policy on
// AdvisorJsonContext, which matches the ARG payload.

internal sealed record RecommendationMetadataData(RecommendationMetadataDataProperties? Properties)
{
    /// <summary>
    /// Read a single ARG result row and create a model instance from it.
    /// </summary>
    public static RecommendationMetadataData? FromJson(JsonElement source) =>
        JsonSerializer.Deserialize(source, AdvisorJsonContext.Default.RecommendationMetadataData);
}

internal sealed record RecommendationMetadataDataProperties(
    string? RecommendationTypeId,
    string? DisplayName,
    string? Label,
    string? RecommendationCategory,
    string? RecommendationSubCategory,
    string? RecommendationImpact,
    double? PriorityScore,
    string? PotentialBenefits,
    string? DetailedDescription,
    string? LearnMoreLink,
    string? SupportedResourceType,
    string? RecommendationScope,
    string? RecommendationDataSourceQuery,
    RecommendationMetadataResourceMetadata? ResourceMetadata,
    List<RecommendationMetadataActionData>? Actions,
    string? Language,
    string? LastRefreshed,
    RecommendationMetadataSourceProperties? SourceProperties);

internal sealed record RecommendationMetadataResourceMetadata(
    string? Singular,
    string? Plural);

internal sealed record RecommendationMetadataActionData(
    string? ActionType,
    string? Caption,
    string? DocumentLink,
    string? BladeName);

internal sealed record RecommendationMetadataSourceProperties(
    RecommendationMetadataServiceRetirementData? ServiceRetirement);

internal sealed record RecommendationMetadataServiceRetirementData(
    string? RetirementDate,
    string? RetirementFeatureName,
    RecommendationMetadataServiceHealthData? ServiceHealth);

internal sealed record RecommendationMetadataServiceHealthData(
    List<string>? TrackingIds,
    List<string>? AshUrls);
