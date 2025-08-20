// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for listing metric definitions
/// </summary>
public class MetricsDefinitionsOptions : BaseMetricsOptions
{
    /// <summary>
    /// Optional search string to filter metric definitions by name and description
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.SearchStringName)]
    public string? SearchString { get; set; }

    /// <summary>
    /// The maximum number of metric definitions to return. Defaults to 10.
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.LimitName)]
    public int Limit { get; set; } = 10;
}
