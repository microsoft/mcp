// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.ActivityLog;

/// <summary>
/// Activity log event level enumeration.
/// https://learn.microsoft.com/en-us/rest/api/monitor/activity-logs/list#eventlevel
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ActivityLogEventLevel>))]
public enum ActivityLogEventLevel
{
    Critical,
    Error,
    Informational,
    Verbose,
    Warning
}
