// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Metadata describing an Azure Advisor recommendation type.
/// </summary>
public sealed record RecommendationMetadata(
    string RecommendationTypeId,
    string? DisplayName,
    string? Label,
    string? Category,
    string? SubCategory,
    string? Impact,
    double? PriorityScore,
    string? PotentialBenefits,
    string? DetailedDescription,
    string? LearnMoreLink,
    string? SupportedResourceType,
    string? Scope,
    string? DataSourceQuery,
    string? ResourceSingularName,
    string? ResourcePluralName,
    IReadOnlyList<RecommendationMetadataAction>? Actions,
    string? Language,
    string? LastRefreshed,
    RecommendationServiceRetirement? ServiceRetirement);
