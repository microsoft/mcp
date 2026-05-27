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
    Task<SubscriptionAggregation> AggregateSubscriptionAsync(
        string subscription,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);

    /// <summary>
    /// Aggregates resources across every accessible subscription in the tenant, returning
    /// the top-3 most-common observed values for each whitelisted property leaf.
    /// </summary>
    Task<SubscriptionAggregation> AggregateTenantAsync(
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken);
}
