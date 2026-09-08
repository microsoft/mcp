// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Models;

/// <summary>
/// A single Azure Advisor cost-saving recommendation projected from the curated Azure Resource
/// Graph query, with savings normalized for ranking.
/// </summary>
public sealed record CostSavingsRecommendation(
    string? Id,
    string? Name,
    string? TenantId,
    string? ResourceGroup,
    string? SubscriptionId,
    string? RecommendationTypeId,
    string? SavingsCurrency,
    double? AnnualSavingsAmount,
    double? SavingsAmount,
    double? MonthlyCarbonSavings,
    string? RecommendationMessage,
    string? RecommendationMessageDetailed,
    string? RecommendationTypeSubCategory,
    string? Solution,
    string? ImpactedField,
    string? ImpactedValue,
    string? Impact,
    string? ResourceId);
