// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record SnapshotPolicyCreateRequest(
    string Account,
    string SnapshotPolicy,
    string Location,
    string ResourceGroup,
    bool Enabled,
    int? HourlyMinute,
    int? HourlySnapshotsToKeep,
    int? DailyHour,
    int? DailyMinute,
    int? DailySnapshotsToKeep,
    string? WeeklyDay,
    int? WeeklyHour,
    int? WeeklyMinute,
    int? WeeklySnapshotsToKeep,
    string? MonthlyDaysOfMonth,
    int? MonthlyHour,
    int? MonthlyMinute,
    int? MonthlySnapshotsToKeep,
    IReadOnlyDictionary<string, string>? Tags);
