// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for querying metrics
/// </summary>
public sealed class MetricsQueryOptions : BaseMetricsOptions
{
    /// <summary>
    /// The names of metrics to query
    /// </summary>
    [Option(Description = "The names of metrics to query (comma-separated).")]
    public required string MetricNames { get; set; }

    /// <summary>
    /// Start time for the query in ISO format
    /// </summary>
    [Option(Description = "The start time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to 24 hours ago.")]
    public string? StartTime { get; set; }

    /// <summary>
    /// End time for the query in ISO format
    /// </summary>
    [Option(Description = "The end time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to now.")]
    public string? EndTime { get; set; }

    /// <summary>
    /// Time interval for the query
    /// </summary>
    [Option(Description = "The time interval for data points (e.g., PT1H for 1 hour, PT5M for 5 minutes).")]
    public string? Interval { get; set; }

    /// <summary>
    /// Aggregation type for the metrics (Average, Maximum, Minimum, Total, Count)
    /// </summary>
    [Option(Description = "The aggregation type to use (Average, Maximum, Minimum, Total, Count).")]
    public string? Aggregation { get; set; }

    /// <summary>
    /// OData filter for the query
    /// </summary>
    [Option(Description = "OData filter to apply to the metrics query.")]
    public string? Filter { get; set; }

    /// <summary>
    /// The maximum number of time buckets to return. Defaults to 50.
    /// </summary>
    [Option(Description = "The maximum number of time buckets to return. Defaults to 50.", DefaultValue = 50)]
    public int? MaxBuckets { get; set; }

    [Option(Description = MonitorOptionDescriptions.MetricNamespace)]
    public required string MetricNamespace { get; set; }
}
