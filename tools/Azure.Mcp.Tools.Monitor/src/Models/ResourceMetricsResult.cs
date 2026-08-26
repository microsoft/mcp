// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Represents the compact metric results for a single resource returned from a batch metrics query
/// </summary>
public class ResourceMetricsResult
{
    /// <summary>
    /// The resource ID the metrics were queried for
    /// </summary>
    [JsonPropertyName("resourceId")]
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// The compact metric results for this resource
    /// </summary>
    [JsonPropertyName("metrics")]
    public List<MetricResult> Metrics { get; set; } = [];
}
