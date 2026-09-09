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
    /// Checks authentication and connectivity for an ADME instance.
    /// </summary>
    Task<HealthCheckResult> CheckHealthAsync(
        string endpoint,
        string dataPartition,
        string? tenant,
        CancellationToken cancellationToken);
}
