// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Represents a metric definition
/// </summary>
public class MetricDefinition
{
    /// <summary>
    /// The metric name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The category of the metric
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The metric description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The unit of measurement
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; } = string.Empty;

    /// <summary>
    /// The primary aggregation type
    /// </summary>
    [JsonPropertyName("defaultAggregation")]
    public string? PrimaryAggregationType { get; set; }

    /// <summary>
    /// The supported aggregation types
    /// </summary>
    [JsonPropertyName("supportedAggregationTypes")]
    public List<string> SupportedAggregationTypes { get; set; } = new();

    /// <summary>
    /// Indicates if dimensions are required
    /// </summary>
    [JsonPropertyName("isDimensionRequiredWhenQuerying")]
    public bool? IsDimensionRequired { get; set; }

    /// <summary>
    /// The metric namespace
    /// </summary>
    [JsonPropertyName("metricNamespace")]
    public string? MetricNamespace { get; set; }

    /// <summary>
    /// The allowed time intervals for this metric (e.g., "PT1M", "PT5M", "PT1H", "P1D")
    /// </summary>
    [JsonPropertyName("allowedIntervals")]
    public string[] AllowedIntervals { get; set; } = [];

    /// <summary>
    /// The available dimensions for this metric
    /// </summary>
    [JsonPropertyName("dimensions")]
    public List<string> Dimensions { get; set; } = new();
}
