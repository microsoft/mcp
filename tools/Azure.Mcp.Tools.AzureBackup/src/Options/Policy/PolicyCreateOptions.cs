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
    public int HourlyIntervalHours { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.HourlyWindowStartTimeName)]
    public string? HourlyWindowStartTime { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.HourlyWindowDurationHoursName)]
    public int HourlyWindowDurationHours { get; set; }

    // Retention flags (new in policy create overhaul; not yet consumed by builders).
    [JsonPropertyName(AzureBackupOptionDefinitions.WeeklyRetentionWeeksName)]
    public int WeeklyRetentionWeeks { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.WeeklyRetentionDaysOfWeekName)]
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionMonthsName)]
    public int MonthlyRetentionMonths { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionWeekOfMonthName)]
    public string? MonthlyRetentionWeekOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfWeekName)]
    public string? MonthlyRetentionDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.MonthlyRetentionDaysOfMonthName)]
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.YearlyRetentionYearsName)]
    public int YearlyRetentionYears { get; set; }

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

    // RSV-VM only flags (new in policy create overhaul; not yet consumed by builders).
    [JsonPropertyName(AzureBackupOptionDefinitions.PolicySubTypeName)]
    public string? PolicySubType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.InstantRpRetentionDaysName)]
    public string? InstantRpRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.InstantRpResourceGroupName)]
    public string? InstantRpResourceGroup { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SnapshotConsistencyName)]
    public string? SnapshotConsistency { get; set; }

    // RSV-VmWorkload (SQL / SAPHANA / SAPASE) flags.
    [JsonPropertyName(AzureBackupOptionDefinitions.FullScheduleFrequencyName)]
    public string? FullScheduleFrequency { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.FullScheduleDaysOfWeekName)]
    public string? FullScheduleDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DifferentialScheduleDaysOfWeekName)]
    public string? DifferentialScheduleDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DifferentialRetentionDaysName)]
    public int DifferentialRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IncrementalScheduleDaysOfWeekName)]
    public string? IncrementalScheduleDaysOfWeek { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IncrementalRetentionDaysName)]
    public int IncrementalRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.LogFrequencyMinutesName)]
    public int LogFrequencyMinutes { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.LogRetentionDaysName)]
    public int LogRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IsCompressionName)]
    public bool IsCompression { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IsSqlCompressionName)]
    public bool IsSqlCompression { get; set; }

    // ===== Stage 2 expansion =====


    [JsonPropertyName(AzureBackupOptionDefinitions.SmartTierName)]
    public bool SmartTier { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.EnableSnapshotBackupName)]
    public bool EnableSnapshotBackup { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SnapshotInstantRpRetentionDaysName)]
    public string? SnapshotInstantRpRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SnapshotInstantRpResourceGroupName)]
    public string? SnapshotInstantRpResourceGroup { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.EnableVaultTierCopyName)]
    public bool EnableVaultTierCopy { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.VaultTierCopyAfterDaysName)]
    public int VaultTierCopyAfterDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.BackupModeName)]
    public string? BackupMode { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.PitrRetentionDaysName)]
    public int PitrRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.PolicyTagsName)]
    public string? PolicyTags { get; set; }
}
