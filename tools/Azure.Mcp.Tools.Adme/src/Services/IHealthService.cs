// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models;

namespace Azure.Mcp.Tools.Adme.Services;

/// <summary>
/// Provides ADME authentication and connectivity checks.
/// </summary>
public interface IHealthService
{
    /// <summary>
    /// Runs the requested health checks against an ADME instance.
    /// </summary>
    Task<HealthCheckResult> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        bool includeAuth,
        bool includeConnectivity,
        CancellationToken cancellationToken);
}
