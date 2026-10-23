// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Global Advisor recommendation-type catalog entry sourced from the Azure Resource Graph
/// `advisorresources` table (type `microsoft.advisor/metadata`). Describes a single
/// recommendation <em>type</em> (not a subscription-scoped recommendation instance),
/// keyed by <see cref="RecommendationTypeId"/> and filtered to a single locale.
///
/// Property names and null-handling are governed by JsonSourceGenerationOptions on
/// AdvisorJsonContext (camelCase naming + WhenWritingNull default).
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
