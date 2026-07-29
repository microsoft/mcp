// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public sealed class PolicyCreateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public required string Policy { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.WorkloadType)]
    public required string WorkloadType { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DailyRetentionDays)]
    public string? DailyRetentionDays { get; set; }

    // Common schedule flags (new in policy create overhaul; not yet consumed by builders).
    [Option(Description = "Windows time-zone identifier for the backup schedule (e.g., 'UTC', 'Pacific Standard Time', 'India Standard Time'). If omitted, the schedule runs in UTC.")]
    public string? TimeZone { get; set; }

    [Option(Description = "Backup schedule frequency. RSV vaults accept 'Daily', 'Weekly', or 'Hourly'. DPP (Backup) vaults accept ISO 8601 intervals: 'PT4H', 'PT6H', 'PT8H', 'PT12H', 'P1D', 'P1W', 'P2W', or 'P1M'.")]
    public string? ScheduleFrequency { get; set; }

    [Option(Description = "Comma-separated list of backup times in 24h HH:mm format (e.g., '02:00' or '02:00,14:00'). Interpreted in --time-zone. Defaults to 02:00 UTC if not specified. Only the first time is used as the schedule start time.")]
    public string? ScheduleTimes { get; set; }

    [Option(Description = "Comma-separated days of the week the backup should run (e.g., 'Monday,Wednesday,Friday'). Required for Weekly schedules.")]
    public string? ScheduleDaysOfWeek { get; set; }

    [Option(Description = "Interval in hours between hourly backups. Valid values: 4, 6, 8, 12. Used only when --schedule-frequency is 'Hourly' (RSV).")]
    public int HourlyIntervalHours { get; set; }

    [Option(Description = "Start time of the hourly backup window in 24h HH:mm format (e.g., '08:00'). Used only when --schedule-frequency is 'Hourly' (RSV).")]
    public string? HourlyWindowStartTime { get; set; }

    [Option(Description = "Duration of the hourly backup window in hours (e.g., 12). Used only when --schedule-frequency is 'Hourly' (RSV).")]
    public int HourlyWindowDurationHours { get; set; }

    // Retention flags (new in policy create overhaul; not yet consumed by builders).
    [Option(Description = "Number of weeks to keep weekly recovery points. Required alongside --weekly-retention-days-of-week.")]
    public int WeeklyRetentionWeeks { get; set; }

    [Option(Description = "Comma-separated days of the week tagged for weekly retention (e.g., 'Sunday' or 'Saturday,Sunday'). Required alongside --weekly-retention-weeks.")]
    public string? WeeklyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Number of months to keep monthly recovery points. Combine with either --monthly-retention-days-of-month (absolute) OR --monthly-retention-week-of-month + --monthly-retention-days-of-week (relative).")]
    public int MonthlyRetentionMonths { get; set; }

    [Option(Description = "Which week of the month to tag for monthly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --monthly-retention-days-of-week (relative scheme).")]
    public string? MonthlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for the monthly retention tag (e.g., 'Sunday'). Use with --monthly-retention-week-of-month (relative scheme).")]
    public string? MonthlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the month for monthly retention (1-28 or 'Last'; e.g., '1,15,Last'). Absolute scheme; mutually exclusive with --monthly-retention-week-of-month.")]
    public string? MonthlyRetentionDaysOfMonth { get; set; }

    [Option(Description = "Number of years to keep yearly recovery points. Combine with --yearly-retention-months and either --yearly-retention-days-of-month (absolute) OR --yearly-retention-week-of-month + --yearly-retention-days-of-week (relative).")]
    public int YearlyRetentionYears { get; set; }

    [Option(Description = "Comma-separated months tagged for yearly retention (e.g., 'January' or 'January,July').")]
    public string? YearlyRetentionMonths { get; set; }

    [Option(Description = "Which week of the selected month(s) to tag for yearly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --yearly-retention-days-of-week (relative scheme).")]
    public string? YearlyRetentionWeekOfMonth { get; set; }

    [Option(Description = "Comma-separated days of the week for the yearly retention tag (e.g., 'Sunday'). Use with --yearly-retention-week-of-month (relative scheme).")]
    public string? YearlyRetentionDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the selected month(s) for yearly retention (1-28 or 'Last'; e.g., '1,Last'). Absolute scheme; mutually exclusive with --yearly-retention-week-of-month.")]
    public string? YearlyRetentionDaysOfMonth { get; set; }

    [Option(Description = "Move recovery points to the archive tier after this many days. Pair with --archive-tier-mode.")]
    public string? ArchiveTierAfterDays { get; set; }

    [Option(Description = "Archive tiering mode: 'TierAfter' (always tier after --archive-tier-after-days) or 'CopyOnExpiry' (copy to archive when the recovery point expires). Use --smart-tier for service-recommended tiering.")]
    public string? ArchiveTierMode { get; set; }

    // RSV-VM only flags (new in policy create overhaul; not yet consumed by builders).
    [Option(Description = "RSV VM policy sub-type: 'Standard' or 'Enhanced'. Enhanced is required for hourly schedules and Trusted Launch VMs. RSV VM only.")]
    public string? PolicySubType { get; set; }

    [Option(Description = "Instant recovery point retention in days (1-30 for Standard, 1-7 for Enhanced). RSV VM only.")]
    public string? InstantRpRetentionDays { get; set; }

    [Option(Description = "Resource group that hosts the instant recovery point snapshots. RSV VM only.")]
    public string? InstantRpResourceGroup { get; set; }

    [Option(Description = "Snapshot consistency mode for VM backups: 'ApplicationConsistent' or 'CrashConsistent'. RSV VM only.")]
    public string? SnapshotConsistency { get; set; }

    // RSV-VmWorkload (SQL / SAPHANA / SAPASE) flags.
    [Option(Description = "Full backup schedule frequency for SQL/SAPHANA/SAPASE: 'Daily' or 'Weekly'. RSV VmWorkload only.")]
    public string? FullScheduleFrequency { get; set; }

    [Option(Description = "Comma-separated days of the week for the Full backup (e.g., 'Sunday'). Required when --full-schedule-frequency is 'Weekly'. RSV VmWorkload only.")]
    public string? FullScheduleDaysOfWeek { get; set; }

    [Option(Description = "Comma-separated days of the week for the Differential backup (e.g., 'Monday,Thursday'). RSV VmWorkload only.")]
    public string? DifferentialScheduleDaysOfWeek { get; set; }

    [Option(Description = "Retention period in days for Differential backups. RSV VmWorkload only.")]
    public int DifferentialRetentionDays { get; set; }

    [Option(Description = "Comma-separated days of the week for the Incremental backup. RSV SAPHANA / SAPASE only.")]
    public string? IncrementalScheduleDaysOfWeek { get; set; }

    [Option(Description = "Retention period in days for Incremental backups. RSV SAPHANA / SAPASE only.")]
    public int IncrementalRetentionDays { get; set; }

    [Option(Description = "Transaction log backup frequency in minutes (e.g., 15, 30, 60). RSV VmWorkload only.")]
    public int LogFrequencyMinutes { get; set; }

    [Option(Description = "Retention period in days for transaction log backups. RSV VmWorkload only.")]
    public int LogRetentionDays { get; set; }

    [Option(Description = "Enable backup compression at the policy level. RSV VmWorkload only.")]
    public bool IsCompression { get; set; }

    [Option(Description = "Enable SQL Server on VM native backup compression. RSV SQL only.")]
    public bool IsSqlCompression { get; set; }

    // ===== Stage 2 expansion =====
    [Option(Description = "Enable smart-tiering (ML-based archive recommendation). RSV VM only - equivalent to TieringMode=TierRecommended. Kept separate from --archive-tier-mode because it emits a structurally different tiering shape (Duration=0, DurationType=Invalid).")]
    public bool SmartTier { get; set; }

    [Option(Description = "Enable snapshot/instance backups (HANA System Replication snapshot RPs). RSV SAPHANA only.")]
    public bool EnableSnapshotBackup { get; set; }

    [Option(Description = "Snapshot instant RP retention range in days. RSV SAPHANA snapshot only.")]
    public string? SnapshotInstantRpRetentionDays { get; set; }

    [Option(Description = "Resource group prefix for snapshot instant RPs. RSV SAPHANA snapshot only.")]
    public string? SnapshotInstantRpResourceGroup { get; set; }

    [Option(Description = "Enable vault-tier copy of operational store backups. DPP AzureDisk only.")]
    public bool EnableVaultTierCopy { get; set; }

    [Option(Description = "Days after which an operational backup is copied to the vault tier. DPP AzureDisk only.")]
    public int VaultTierCopyAfterDays { get; set; }

    [Option(Description = "Backup mode for storage workloads: 'Continuous' (default for AzureBlob, ADLS) or 'Vaulted' (discrete recovery points). DPP AzureBlob, AzureDataLakeStorage.")]
    public string? BackupMode { get; set; }

    [Option(Description = "Point-in-time restore retention in days for continuous backups. DPP AzureBlob, AzureDataLakeStorage.")]
    public int PitrRetentionDays { get; set; }

    [Option(Description = "Resource tags applied to the RSV backup policy as 'k1=v1,k2=v2'. RSV only.")]
    public string? PolicyTags { get; set; }
}
