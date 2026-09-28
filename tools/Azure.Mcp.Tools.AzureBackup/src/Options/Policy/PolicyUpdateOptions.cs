// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public sealed class PolicyUpdateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public required string Policy { get; set; }

    // ===== Legacy back-compat =====

    [Option(Description = "Backup schedule time in 24h HH:mm format (e.g., '02:00'). Legacy single-time flag; prefer --schedule-times for the new IaasVM parity surface.")]
    public string? ScheduleTime { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DailyRetentionDays)]
    public string? DailyRetentionDays { get; set; }

    // ===== IaasVM policy update parity (RSV Azure VM only) =====
    // Omitted fields preserve existing settings. Other workloads reject VM-only fields.

    [Option(Description = "Windows time-zone identifier for the backup schedule (e.g., 'UTC', 'Pacific Standard Time'). RSV Azure VM only.")]
    public string? TimeZone { get; set; }

    [Option(Description = "Backup schedule frequency: 'Daily', 'Weekly', or 'Hourly'. Hourly requires an existing Enhanced policy. Changing the policy subtype is not supported. RSV Azure VM only.")]
    public string? ScheduleFrequency { get; set; }

    [Option(Description = "One backup time in 24h HH:mm format (e.g., '02:00') for Daily or Weekly schedules. Interpreted in --time-zone. Mutually exclusive with --schedule-time. RSV Azure VM only.")]
    public string? ScheduleTimes { get; set; }

    [Option(Description = "Comma-separated days of the week the backup should run (e.g., 'Monday,Wednesday,Friday'). Required for Weekly schedules. RSV Azure VM only.")]
    public string? ScheduleDaysOfWeek { get; set; }

    [Option(Description = "Number of weeks to keep weekly recovery points. Pair with --weekly-retention-days-of-week. RSV Azure VM only.")]
    public int WeeklyRetentionWeeks { get; set; }

    [Option(Description = "Comma-separated days of the week tagged for weekly retention (e.g., 'Sunday'). Required with --weekly-retention-weeks. RSV Azure VM only.")]
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Number of months to keep monthly recovery points. Combine with EITHER --monthly-retention-days-of-month (absolute) OR --monthly-retention-week-of-month + --monthly-retention-days-of-week (relative). RSV Azure VM only.")]
    public int MonthlyRetentionMonths { get; set; }

    [Option(Description = "Week of the month for monthly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --monthly-retention-days-of-week. RSV Azure VM only.")]
    public string? MonthlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for monthly retention (e.g., 'Sunday'). Use with --monthly-retention-week-of-month. RSV Azure VM only.")]
    public string? MonthlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the month for monthly retention (1-28 or 'Last'; e.g., '1,15,Last'). Mutually exclusive with --monthly-retention-week-of-month. RSV Azure VM only.")]
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    [Option(Description = "Number of years to keep yearly recovery points. Combine with --yearly-retention-months and either --yearly-retention-days-of-month OR --yearly-retention-week-of-month + --yearly-retention-days-of-week. RSV Azure VM only.")]
    public int YearlyRetentionYears { get; set; }

    [Option(Description = "Comma-separated months tagged for yearly retention (e.g., 'January' or 'January,July'). RSV Azure VM only.")]
    public string? YearlyRetentionMonths { get; set; }

    [Option(Description = "Week of the month for yearly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --yearly-retention-days-of-week. RSV Azure VM only.")]
    public string? YearlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for yearly retention (e.g., 'Sunday'). Use with --yearly-retention-week-of-month. RSV Azure VM only.")]
    public string? YearlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the month for yearly retention (1-28 or 'Last'). Mutually exclusive with --yearly-retention-week-of-month. RSV Azure VM only.")]
    public string? YearlyRetentionDaysOfMonth { get; set; }

    [Option(Description = "Interval between hourly backups: 4, 6, 8, or 12 hours. Existing Enhanced VM policies only.")]
    public int? HourlyIntervalHours { get; set; }

    [Option(Description = "Hourly window start as local HH:mm, converted to UTC using supplied --time-zone, otherwise the existing policy time zone, otherwise UTC. Uses 2000-01-01 time-zone rules, not today's DST offset; invalid or ambiguous local times are rejected. Omission preserves the stored timestamp, even when changing time zone. Required with interval and duration when switching to Hourly.")]
    public string? HourlyWindowStartTime { get; set; }

    [Option(Description = "Hourly backup window duration in hours (4-24), at least the interval. Existing Enhanced VM policies only.")]
    public int? HourlyWindowDurationHours { get; set; }

    [Option(Description = "Assert the existing VM policy subtype: Standard or Enhanced. Changing subtype requires a separate migration and is rejected.")]
    public string? PolicySubType { get; set; }

    [Option(Description = "Instant recovery point retention in days (1-30). Azure enforces schedule-specific limits for Standard and Enhanced policies. Omission preserves existing retention. RSV Azure VM only.")]
    public string? InstantRpRetentionDays { get; set; }

    [Option(Description = "Instant recovery point resource group name prefix. Preserves the existing suffix. RSV Azure VM only.")]
    public string? InstantRpResourceGroup { get; set; }

    [Option(Description = "VM snapshot consistency: ApplicationConsistent restores the default application-consistent backup behavior; CrashConsistent requests ARM OnlyCrashConsistent mode.")]
    public string? SnapshotConsistency { get; set; }

    [Option(Description = "Days before moving VM recovery points to archive (at least 45). Use TierAfter mode.")]
    public string? ArchiveTierAfterDays { get; set; }

    [Option(Description = "VM archive mode: TierAfter or TierRecommended. CopyOnExpiry is not supported for RSV VM policies.")]
    public string? ArchiveTierMode { get; set; }

    [Option(Description = "Enable recommended VM archive tiering; false explicitly disables archive tiering. Omission preserves existing tiering. Cannot be combined with archive flags.")]
    public bool? SmartTier { get; set; }

    [Option(Description = "Merge VM policy resource tags as k1=v1,k2=v2, preserving unmentioned tags.")]
    public string? PolicyTags { get; set; }
}
