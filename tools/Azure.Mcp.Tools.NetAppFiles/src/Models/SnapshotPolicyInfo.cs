// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp snapshot policy with commonly useful metadata.
/// </summary>
public sealed record SnapshotPolicyInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("enabled")] bool? Enabled,
    [property: JsonPropertyName("hourlyScheduleMinute")] int? HourlyScheduleMinute,
    [property: JsonPropertyName("hourlyScheduleSnapshotsToKeep")] int? HourlyScheduleSnapshotsToKeep,
    [property: JsonPropertyName("dailyScheduleHour")] int? DailyScheduleHour,
    [property: JsonPropertyName("dailyScheduleMinute")] int? DailyScheduleMinute,
    [property: JsonPropertyName("dailyScheduleSnapshotsToKeep")] int? DailyScheduleSnapshotsToKeep,
    [property: JsonPropertyName("weeklyScheduleDay")] string? WeeklyScheduleDay,
    [property: JsonPropertyName("weeklyScheduleSnapshotsToKeep")] int? WeeklyScheduleSnapshotsToKeep,
    [property: JsonPropertyName("monthlyScheduleDaysOfMonth")] string? MonthlyScheduleDaysOfMonth,
    [property: JsonPropertyName("monthlyScheduleSnapshotsToKeep")] int? MonthlyScheduleSnapshotsToKeep);
