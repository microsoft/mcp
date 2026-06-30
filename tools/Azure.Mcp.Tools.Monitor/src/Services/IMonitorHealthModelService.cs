// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

public interface IMonitorHealthModelService
{
    /// <summary>
    /// Lists Azure Monitor Health Models (Microsoft.CloudHealth/healthmodels) in a subscription,
    /// optionally scoped to a single resource group.
    /// </summary>
    /// <param name="subscription">Subscription ID or name.</param>
    /// <param name="resourceGroup">Optional resource group to scope the listing.</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The health model resources as JSON nodes.</returns>
    Task<List<JsonNode>> ListHealthModels(
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
    /// <param name="tenant">Optional tenant ID for cross-tenant operations.</param>
    /// <param name="retryPolicy">Optional retry policy for the operation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The health model resource as a JSON node.</returns>
    Task<JsonNode> GetHealthModel(
        string subscription,
        string resourceGroup,
        string healthModelName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
