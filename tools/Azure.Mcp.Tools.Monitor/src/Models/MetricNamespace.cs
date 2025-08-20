// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models;

/// <summary>
/// Represents a metric namespace
/// </summary>
public class MetricNamespace
{
    /// <summary>
    /// The namespace name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The namespace type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The classification type of the namespace
    /// </summary>
    [JsonPropertyName("classificationType")]
    public string ClassificationType { get; set; } = string.Empty;
}
