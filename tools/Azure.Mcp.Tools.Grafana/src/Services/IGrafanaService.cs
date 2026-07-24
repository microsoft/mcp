// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Grafana.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Grafana.Services;

public interface IGrafanaService
{
    /// <summary>
    /// Lists Azure Managed Grafana workspaces in the specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">Optional resource group name to filter the workspaces</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>List of Grafana workspace details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<ResourceQueryResults<GrafanaWorkspace>> ListWorkspacesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
