// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.ActivityLog;

/// <summary>
/// Localizable string model for activity log properties.
/// https://learn.microsoft.com/en-us/rest/api/monitor/activity-logs/list#localizablestring
/// </summary>
public sealed class ActivityLogLocalizableString
{
    [JsonPropertyName("localizedValue")]
    public required string LocalizedValue { get; set; }

    [JsonPropertyName("value")]
    public required string Value { get; set; }
}
