// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Interface for metrics options
/// </summary>
public interface IMetricsOptions
{
    /// <summary>
    /// The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')
    /// </summary>
    string? ResourceType { get; set; }

    /// <summary>
    /// The resource name (required)
    /// </summary>
    string? ResourceName { get; set; }

    /// <summary>
    /// The metric namespace to query
    /// </summary>
    string? MetricNamespace { get; set; }
}
