// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Base options for all metrics commands
/// </summary>
public class BaseMetricsOptions : BaseMonitorOptions, IMetricsOptions
{
    /// <summary>
    /// The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.ResourceTypeName)]
    public string? ResourceType { get; set; }

    /// <summary>
    /// The resource name (required)
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.ResourceNameName)]
    public string? ResourceName { get; set; }

    /// <summary>
    /// The metric namespace to query
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.MetricNamespaceName)]
    public string? MetricNamespace { get; set; }
}
