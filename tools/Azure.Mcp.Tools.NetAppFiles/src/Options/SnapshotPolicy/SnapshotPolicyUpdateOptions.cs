// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;

public class SnapshotPolicyUpdateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotPolicyName)]
    public string? SnapshotPolicy { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.HourlyScheduleMinuteName)]
    public int? HourlyScheduleMinute { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeepName)]
    public int? HourlyScheduleSnapshotsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DailyScheduleHourName)]
    public int? DailyScheduleHour { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DailyScheduleMinuteName)]
    public int? DailyScheduleMinute { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeepName)]
    public int? DailyScheduleSnapshotsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyScheduleDayName)]
    public string? WeeklyScheduleDay { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeepName)]
    public int? WeeklyScheduleSnapshotsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonthName)]
    public string? MonthlyScheduleDaysOfMonth { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeepName)]
    public int? MonthlyScheduleSnapshotsToKeep { get; set; }
}
