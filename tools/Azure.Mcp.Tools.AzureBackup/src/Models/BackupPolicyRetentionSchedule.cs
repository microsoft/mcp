// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// A single retention schedule (daily, weekly, monthly, or yearly) returned by RSV backup policies.
/// Properties are populated when supported by the retention schedule frequency.
/// </summary>
public sealed record BackupPolicyRetentionSchedule(
    string Frequency,
    string? RetentionScheduleFormatType,
    IReadOnlyList<string>? RetentionTimes,
    int? DurationCount,
    string? DurationType,
    IReadOnlyList<string>? DaysOfWeek,
    IReadOnlyList<string>? WeeksOfMonth,
    IReadOnlyList<string>? MonthsOfYear,
    IReadOnlyList<string>? DaysOfMonth);
