// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

public interface IOptimizationService
{
    /// <summary>
    /// Returns the top cost-saving recommendations, ranked by impact and currency-normalized annual
    /// savings. When <paramref name="subscription"/> is null or empty, every subscription accessible to the
    /// caller in a single tenant is queried: <paramref name="tenant"/> (id or display name) when given, otherwise
    /// the only accessible tenant; an <see cref="ArgumentException"/> is thrown when several tenants are accessible
    /// and none is specified. When the subscription name matches more than one subscription, the returned
    /// result carries the candidate subscriptions instead.
    /// </summary>
    Task<CostSavingsResult> ListCostSavingsAsync(
        string? subscription,
        int top,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the parsed alternative resize/SKU options carried on an Advisor right-size
    /// recommendation for the specified compute resource.
    /// </summary>
    Task<IReadOnlyList<AlternativeRecommendation>> GetAlternativesAsync(
        string resourceId,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the matching Advisor recommendation and projects current-versus-target-SKU
    /// utilization, returning the structured utilization time-series for inline chart rendering.
    /// </summary>
    Task<RecommendationExplanationResult> GetRecommendationExplanationAsync(
        string resourceId,
        string? targetSku,
        UtilizationView view,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
