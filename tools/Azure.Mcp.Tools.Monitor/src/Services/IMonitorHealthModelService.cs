// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

public interface IMonitorHealthModelService
{
    /// <summary>
    /// Lists Azure Monitor Health Models in a subscription or resource group. Returns a summary projection.
    /// </summary>
    /// <param name="subscription">Subscription ID or name.</param>
    /// <param name="resourceGroup">Optional resource group to scope the listing.</param>
    /// <param name="tenant">Optional tenant ID.</param>
    /// <param name="retryPolicy">Optional retry policy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>List health models.</returns>
    Task<List<HealthModelSummary>> ListHealthModels(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single Azure Monitor Health Model by name.
    /// </summary>
    /// <param name="subscription">Subscription ID or name.</param>
    /// <param name="resourceGroup">The resource group containing the health model.</param>
    /// <param name="healthModelName">The health model name.</param>
    /// <param name="tenant">Optional tenant ID.</param>
    /// <param name="retryPolicy">Optional retry policy.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The health model resource.</returns>
    Task<HealthModelDetail> GetHealthModel(
        string subscription,
        string resourceGroup,
        string healthModelName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
