// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Grafana.Models;

namespace Azure.Mcp.Tools.Grafana.Services;

public interface IGrafanaService
{
    /// <summary>
    /// Lists Azure Managed Grafana workspaces in the specified subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="tenant">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of Grafana workspace details</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<IEnumerable<GrafanaWorkspace>> ListWorkspacesAsync(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
