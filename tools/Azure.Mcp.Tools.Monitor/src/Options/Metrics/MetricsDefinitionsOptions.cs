// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for listing metric definitions
/// </summary>
public sealed class MetricsDefinitionsOptions : BaseMetricsOptions
{
    /// <summary>
    /// Optional search string to filter metric definitions by name and description
    /// </summary>
    [Option(Description = "A string to filter the metric definitions by. Helpful for reducing the number of records returned. Performs case-insensitive matching on metric name and description fields.")]
    public string? SearchString { get; set; }

    /// <summary>
    /// The maximum number of metric definitions to return. Defaults to 10.
    /// </summary>
    [Option(Description = "The maximum number of metric definitions to return. Defaults to 10.", DefaultValue = 10)]
    public int Limit { get; set; } = 10;

    [Option(Description = MonitorOptionDescriptions.MetricNamespace)]
    public string? MetricNamespace { get; set; }
}
