// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Provides access to the ADME storage info endpoint.
/// </summary>
public interface IHealthService
{
    /// <summary>
    /// Calls the storage info endpoint for an ADME instance.
    /// </summary>
    Task<AdmeResponse<HealthCheckResult>> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        CancellationToken cancellationToken);
}
