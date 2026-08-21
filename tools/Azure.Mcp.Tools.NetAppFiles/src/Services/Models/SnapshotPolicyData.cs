// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp snapshot policy from Resource Graph.
/// </summary>
internal sealed class SnapshotPolicyData
{
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("properties")]
    public SnapshotPolicyProperties? Properties { get; set; }

    public static SnapshotPolicyData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.SnapshotPolicyData);
    }
}

internal sealed class SnapshotPolicyProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("hourlySchedule")]
    public HourlySchedule? HourlySchedule { get; set; }

    [JsonPropertyName("dailySchedule")]
    public DailySchedule? DailySchedule { get; set; }

    [JsonPropertyName("weeklySchedule")]
    public WeeklySchedule? WeeklySchedule { get; set; }

    [JsonPropertyName("monthlySchedule")]
    public MonthlySchedule? MonthlySchedule { get; set; }
}

internal sealed class HourlySchedule
{
    [JsonPropertyName("minute")]
    public int? Minute { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class DailySchedule
{
    [JsonPropertyName("hour")]
    public int? Hour { get; set; }

    [JsonPropertyName("minute")]
    public int? Minute { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class WeeklySchedule
{
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class MonthlySchedule
{
    [JsonPropertyName("daysOfMonth")]
    public string? DaysOfMonth { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}
