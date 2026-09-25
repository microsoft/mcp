// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Schedule policy details returned by RSV backup policies.
/// Properties are populated when supported by the schedule policy type.
/// </summary>
public sealed record BackupPolicySchedule(
    string? SchedulePolicyType,
    string? ScheduleRunFrequency,
    IReadOnlyList<string>? ScheduleRunDays,
    IReadOnlyList<string>? ScheduleRunTimes,
    int? ScheduleWeeklyFrequency,
    BackupPolicyHourlySchedule? HourlySchedule);
