// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for querying metrics
/// </summary>
public class MetricsQueryOptions : BaseMetricsOptions
{
    /// <summary>
    /// The names of metrics to query
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.MetricNamesName)]
    public string? MetricNames { get; set; }

    /// <summary>
    /// Start time for the query in ISO format
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.StartTimeName)]
    public string? StartTime { get; set; }

    /// <summary>
    /// End time for the query in ISO format
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.EndTimeName)]
    public string? EndTime { get; set; }

    /// <summary>
    /// Time interval for the query
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.IntervalName)]
    public string? Interval { get; set; }

    /// <summary>
    /// Aggregation type for the metrics (Average, Maximum, Minimum, Total, Count)
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.AggregationName)]
    public string? Aggregation { get; set; }

    /// <summary>
    /// OData filter for the query
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.FilterName)]
    public string? Filter { get; set; }

    /// <summary>
    /// The maximum number of time buckets to return. Defaults to 50.
    /// </summary>
    [JsonPropertyName(MonitorOptionDefinitions.Metrics.MaxBucketsName)]
    public int? MaxBuckets { get; set; }
}
