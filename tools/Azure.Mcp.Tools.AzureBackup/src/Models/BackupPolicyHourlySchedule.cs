// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Hourly schedule window returned by RSV schedule-based backup policies.
/// </summary>
public sealed record BackupPolicyHourlySchedule(
    int? Interval,
    string? ScheduleWindowStartTime,
    int? ScheduleWindowDurationInHours);
