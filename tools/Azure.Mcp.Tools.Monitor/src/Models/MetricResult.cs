// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Represents a compact metric result optimized for minimal JSON payload
/// </summary>
public class MetricResult
{
    /// <summary>
    /// The metric name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The unit of measurement
    /// </summary>
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// The compact time series data for this metric
    /// </summary>
    [JsonPropertyName("timeSeries")]
    public List<MetricTimeSeries> TimeSeries { get; set; } = new();
}
