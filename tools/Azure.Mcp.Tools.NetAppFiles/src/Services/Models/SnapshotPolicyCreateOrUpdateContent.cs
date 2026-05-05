// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp snapshot policy via ARM generic resource API.
/// </summary>
internal sealed class SnapshotPolicyCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public SnapshotPolicyCreateProperties? Properties { get; set; }
}

internal sealed class SnapshotPolicyCreateProperties
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("hourlySchedule")]
    public HourlyScheduleCreate? HourlySchedule { get; set; }

    [JsonPropertyName("dailySchedule")]
    public DailyScheduleCreate? DailySchedule { get; set; }

    [JsonPropertyName("weeklySchedule")]
    public WeeklyScheduleCreate? WeeklySchedule { get; set; }

    [JsonPropertyName("monthlySchedule")]
    public MonthlyScheduleCreate? MonthlySchedule { get; set; }
}

internal sealed class HourlyScheduleCreate
{
    [JsonPropertyName("minute")]
    public int? Minute { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class DailyScheduleCreate
{
    [JsonPropertyName("hour")]
    public int? Hour { get; set; }

    [JsonPropertyName("minute")]
    public int? Minute { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class WeeklyScheduleCreate
{
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}

internal sealed class MonthlyScheduleCreate
{
    [JsonPropertyName("daysOfMonth")]
    public string? DaysOfMonth { get; set; }

    [JsonPropertyName("snapshotsToKeep")]
    public int? SnapshotsToKeep { get; set; }
}
