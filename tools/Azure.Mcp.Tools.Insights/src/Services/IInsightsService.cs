// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Insights.Services.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Insights.Services;

/// <summary>
/// Queries Azure Resource Graph and aggregates per-resource-type property value frequencies
/// </summary>
public interface IInsightsService
{
    /// <summary>
    /// Aggregates resources in a single subscription, returning the top-3 most-common observed
    /// values for each whitelisted property leaf.
    /// </summary>
    /// <param name="subscription">The subscription ID to aggregate.</param>
    /// <param name="tenant">The tenant ID to use for authentication; if null, the default tenant is used.</param>
    /// <param name="retryPolicy">Optional retry policy for transient failures.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="progress">Progress reporter; receives a message per ARG page fetched.</param>
    /// <param name="noCache">Fetch new ARG data if true, else use cached data.</param>
    /// <returns>A SubscriptionAggregation object containing the aggregated results.</returns>
    Task<SubscriptionAggregation> AggregateSubscriptionAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken,
        IProgress<string>? progress = null,
        bool noCache = false);

    /// <summary>
    /// Aggregates resources across every accessible subscription in the tenant, returning
    /// the top-3 most-common observed values for each whitelisted property leaf.
    /// </summary>
    /// <param name="tenant">The tenant ID to use for authentication; if null, the default tenant is used.</param>
    /// <param name="retryPolicy">Optional retry policy for transient failures.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="progress">Progress reporter; receives a message per ARG page fetched.</param>
    /// <param name="noCache">Fetch new ARG data if true, else use cached data.</param>
    /// <returns>A SubscriptionAggregation object containing the aggregated results.</returns>
    Task<SubscriptionAggregation> AggregateTenantAsync(
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken,
        IProgress<string>? progress = null,
        bool noCache = false);
}
