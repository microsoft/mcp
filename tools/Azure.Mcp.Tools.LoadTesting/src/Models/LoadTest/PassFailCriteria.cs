// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.LoadTesting.Models.LoadTest;
public class PassFailCriteria
{
    /// <summary>
    /// Gets or sets client-side metrics thresholds for pass/fail evaluation (response time, error rate, etc.).
    /// </summary>
    [JsonPropertyName("passFailMetrics")]
    public Dictionary<string, object>? PassFailMetrics { get; set; } = new();

    /// <summary>
    /// Gets or sets server-side metrics thresholds for pass/fail evaluation (CPU, memory, etc.).
    /// </summary>
    [JsonPropertyName("passFailServerMetrics")]
    public Dictionary<string, object>? PassFailServerMetrics { get; set; } = new();
}
