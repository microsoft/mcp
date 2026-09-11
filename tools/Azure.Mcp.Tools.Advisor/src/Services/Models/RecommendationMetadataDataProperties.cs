// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

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
