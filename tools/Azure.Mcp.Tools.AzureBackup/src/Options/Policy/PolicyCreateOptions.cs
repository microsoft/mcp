// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public class PolicyCreateOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.PolicyName)]
    public string? Policy { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.WorkloadTypeName)]
    public string? WorkloadType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DailyRetentionDaysName)]
    public string? DailyRetentionDays { get; set; }

    // Common schedule flags (new in policy create overhaul; not yet consumed by builders).
    [JsonPropertyName(AzureBackupOptionDefinitions.TimeZoneName)]
    public string? TimeZone { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ScheduleFrequencyName)]
    public string? ScheduleFrequency { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ScheduleTimesName)]
    public string? ScheduleTimes { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ScheduleDaysOfWeekName)]
    public string? ScheduleDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.HourlyIntervalHoursName)]
    public string? HourlyIntervalHours { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.HourlyWindowStartTimeName)]
    public string? HourlyWindowStartTime { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.HourlyWindowDurationHoursName)]
    public string? HourlyWindowDurationHours { get; set; }

    // Retention flags (new in policy create overhaul; not yet consumed by builders).
    [JsonPropertyName(AzureBackupOptionDefinitions.WeeklyRetentionWeeksName)]
    public string? WeeklyRetentionWeeks { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.WeeklyRetentionDaysOfWeekName)]
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionMonthsName)]
    public string? MonthlyRetentionMonths { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonthName)]
    public string? MonthlyRetentionWeekOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfWeekName)]
    public string? MonthlyRetentionDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonthName)]
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionYearsName)]
    public string? YearlyRetentionYears { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionMonthsName)]
    public string? YearlyRetentionMonths { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionWeekOfMonthName)]
    public string? YearlyRetentionWeekOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionDaysOfWeekName)]
    public string? YearlyRetentionDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionDaysOfMonthName)]
    public string? YearlyRetentionDaysOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ArchiveTierAfterDaysName)]
    public string? ArchiveTierAfterDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ArchiveTierModeName)]
    public string? ArchiveTierMode { get; set; }
}
