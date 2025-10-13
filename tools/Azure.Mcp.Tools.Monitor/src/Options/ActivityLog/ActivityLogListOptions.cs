// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Monitor.Models.ActivityLog;

namespace Azure.Mcp.Tools.Monitor.Options.ActivityLog;

public class ActivityLogListOptions : BaseMonitorOptions
{
    [JsonPropertyName(ActivityLogOptionDefinitions.ResourceNameName)]
    public string? ResourceName { get; set; }

    [JsonPropertyName(ActivityLogOptionDefinitions.ResourceTypeName)]
    public string? ResourceType { get; set; }

    [JsonPropertyName(ActivityLogOptionDefinitions.HoursName)]
    public double? Hours { get; set; }

    [JsonPropertyName(ActivityLogOptionDefinitions.EventLevelName)]
    public ActivityLogEventLevel? EventLevel { get; set; }

    [JsonPropertyName(ActivityLogOptionDefinitions.TopName)]
    public int? Top { get; set; }
}
