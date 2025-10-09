// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.ActivityLog;

/// <summary>
/// Activity log list API response model.
/// https://learn.microsoft.com/en-us/rest/api/monitor/activity-logs/list#response
/// </summary>
public sealed class ActivityLogListResponse
{
    [JsonPropertyName("nextLink")]
    public string? NextLink { get; set; }

    [JsonPropertyName("value")]
    public ActivityLogEventData[] Value { get; set; } = [];
}
