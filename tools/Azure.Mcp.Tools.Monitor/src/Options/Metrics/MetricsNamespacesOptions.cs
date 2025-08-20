// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for listing metric namespaces
/// </summary>
public class MetricsNamespacesOptions : BaseMetricsOptions
{
    /// <summary>
    /// The maximum number of metric namespaces to return. Defaults to 10.
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.LimitName)]
    public int Limit { get; set; } = 10;

    /// <summary>
    /// Optional search string to filter metric namespaces by name
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.SearchStringName)]
    public string? SearchString { get; set; }
}
