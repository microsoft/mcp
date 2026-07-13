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

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyScheduleHourName)]
    public int? WeeklyScheduleHour { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyScheduleMinuteName)]
    public int? WeeklyScheduleMinute { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeepName)]
    public int? WeeklyScheduleSnapshotsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonthName)]
    public string? MonthlyScheduleDaysOfMonth { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleHourName)]
    public int? MonthlyScheduleHour { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleMinuteName)]
    public int? MonthlyScheduleMinute { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeepName)]
    public int? MonthlyScheduleSnapshotsToKeep { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.EnabledName)]
    public bool? Enabled { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcquirePolicyTokenName)]
    public bool AcquirePolicyToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChangeReferenceName)]
    public string? ChangeReference { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AddName)]
    public string[]? Add { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SetName)]
    public string[]? Set { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoveName)]
    public string[]? Remove { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceStringName)]
    public bool ForceString { get; set; }
}
