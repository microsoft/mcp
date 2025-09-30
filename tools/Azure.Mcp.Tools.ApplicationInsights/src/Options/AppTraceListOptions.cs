// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ApplicationInsights.Options;

/// <summary>
/// Options for listing Application Insights trace metadata.
/// </summary>
public class AppTraceListOptions : SubscriptionOptions
{
    /// <summary>
    /// The name of the Application Insights resource.
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.ResourceNameName)]
    public string? ResourceName { get; set; }

    /// <summary>
    /// The resource ID of the Application Insights resource.
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.ResourceIdName)]
    public string? ResourceId { get; set; }

    /// <summary>
    /// Optional start time (UTC) for the trace window.
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.StartTimeName)]
    public DateTime? StartTimeUtc { get; set; }

    /// <summary>
    /// Optional end time (UTC) for the trace window.
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.EndTimeName)]
    public DateTime? EndTimeUtc { get; set; }

    /// <summary>
    /// The table to list traces on
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.TableName)]
    public string? Table { get; set; }

    /// <summary>
    /// Filters for the traces
    /// </summary>
    [JsonPropertyName(ApplicationInsightsOptionDefinitions.FiltersName)]
    public string[] Filters { get; set; } = [];
}
