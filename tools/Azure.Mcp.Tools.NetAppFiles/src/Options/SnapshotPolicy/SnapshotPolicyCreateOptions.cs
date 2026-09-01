// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.SnapshotPolicy;

public class SnapshotPolicyCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.SnapshotPolicy)]
    public string? SnapshotPolicy { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.HourlyScheduleMinute)]
    public int? HourlyScheduleMinute { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.HourlyScheduleSnapshotsToKeep)]
    public int? HourlyScheduleSnapshotsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DailyScheduleHour)]
    public int? DailyScheduleHour { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DailyScheduleMinute)]
    public int? DailyScheduleMinute { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DailyScheduleSnapshotsToKeep)]
    public int? DailyScheduleSnapshotsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.WeeklyScheduleDay)]
    public string? WeeklyScheduleDay { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.WeeklyScheduleHour)]
    public int? WeeklyScheduleHour { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.WeeklyScheduleMinute)]
    public int? WeeklyScheduleMinute { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.WeeklyScheduleSnapshotsToKeep)]
    public int? WeeklyScheduleSnapshotsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.MonthlyScheduleDaysOfMonth)]
    public string? MonthlyScheduleDaysOfMonth { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.MonthlyScheduleHour)]
    public int? MonthlyScheduleHour { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.MonthlyScheduleMinute)]
    public int? MonthlyScheduleMinute { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.MonthlyScheduleSnapshotsToKeep)]
    public int? MonthlyScheduleSnapshotsToKeep { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Enabled)]
    public bool? Enabled { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AcquirePolicyToken)]
    public bool AcquirePolicyToken { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ChangeReference)]
    public string? ChangeReference { get; set; }
}
