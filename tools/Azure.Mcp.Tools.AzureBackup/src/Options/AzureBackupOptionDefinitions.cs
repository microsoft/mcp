// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Options;

public static class AzureBackupOptionDefinitions
{
    internal const string Vault = "The name of the backup vault (Recovery Services vault or Backup vault).";
    internal const string VaultType = "The type of backup vault: 'rsv' (Recovery Services vault) or 'dpp' (Backup vault / Data Protection). Auto-detected if omitted for existing vaults.";
    internal const string ProtectedItem = "The name of the protected item or backup instance.";
    internal const string Container = "The RSV protection container name. Only applicable for Recovery Services vaults.";
    internal const string Policy = "The name of the backup policy.";
    internal const string Location = "The Azure region (e.g., 'eastus', 'westus2').";
    internal const string DatasourceId = "The datasource identifier. For VM/FileShare/DPP workloads, use the ARM resource ID (e.g., '/subscriptions/.../virtualMachines/myvm'). For RSV in-guest workloads (SQL/SAPHANA), use the protectable item name from 'protectableitem list' (e.g., 'SAPHanaDatabase;instance;dbname').";
    internal const string ImmutabilityState = "Immutability state: 'Disabled', 'Enabled', or 'Locked' (irreversible).";
    internal const string SoftDelete = "Soft delete state: 'AlwaysOn', 'On', or 'Off'.";
    internal const string SoftDeleteRetentionDays = "Soft delete retention period (14-180 days).";
    internal const string WorkloadType = "Workload type: VM, SQL, SAPHANA, SAPASE, AzureFileShare (RSV types); AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB (DPP types). Also accepts aliases like AzureVM, SQLDatabase, etc.";
    internal const string DailyRetentionDays = "Daily recovery point retention in days. Defaults to datasource-specific value if omitted.";

    public const string LocationName = "location";

    // Policy create  -  common schedule flags (new in policy create overhaul)
    public const string ScheduleFrequencyName = "schedule-frequency";
    public const string ScheduleDaysOfWeekName = "schedule-days-of-week";

    // Policy create  -  retention flags (new in policy create overhaul)
    public const string WeeklyRetentionWeeksName = "weekly-retention-weeks";
    public const string MonthlyRetentionMonthsName = "monthly-retention-months";
    public const string MonthlyRetentionWeekOfMonthName = "monthly-retention-week-of-month";
    public const string MonthlyRetentionDaysOfWeekName = "monthly-retention-days-of-week";
    public const string MonthlyRetentionDaysOfMonthName = "monthly-retention-days-of-month";
    public const string YearlyRetentionYearsName = "yearly-retention-years";
    public const string YearlyRetentionMonthsName = "yearly-retention-months";
    public const string YearlyRetentionWeekOfMonthName = "yearly-retention-week-of-month";
    public const string YearlyRetentionDaysOfWeekName = "yearly-retention-days-of-week";
    public const string YearlyRetentionDaysOfMonthName = "yearly-retention-days-of-month";
    public const string ArchiveTierAfterDaysName = "archive-tier-after-days";
    public const string ArchiveTierModeName = "archive-tier-mode";

    // Policy create  -  RSV-VM only flags
    public const string PolicySubTypeName = "policy-sub-type";
    public const string InstantRpRetentionDaysName = "instant-rp-retention-days";
    public const string InstantRpResourceGroupName = "instant-rp-resource-group";
    public const string SnapshotConsistencyName = "snapshot-consistency";

    // Policy create  -  RSV-VmWorkload (SQL / SAPHANA / SAPASE) flags
    public const string FullScheduleFrequencyName = "full-schedule-frequency";
    public const string FullScheduleDaysOfWeekName = "full-schedule-days-of-week";
    public const string DifferentialScheduleDaysOfWeekName = "differential-schedule-days-of-week";
    public const string DifferentialRetentionDaysName = "differential-retention-days";
    public const string IncrementalScheduleDaysOfWeekName = "incremental-schedule-days-of-week";
    public const string IncrementalRetentionDaysName = "incremental-retention-days";
    public const string LogFrequencyMinutesName = "log-frequency-minutes";
    public const string LogRetentionDaysName = "log-retention-days";
    public const string IsCompressionName = "is-compression";
    public const string IsSqlCompressionName = "is-sql-compression";

    // Policy create  -  Stage 2 expansion flags
    // RSV VM Smart Tier (ML-based archive recommendation)
    public const string SmartTierName = "smart-tier";
    // RSV SAPHANA snapshot/instance backups
    public const string EnableSnapshotBackupName = "enable-snapshot-backup";
    public const string SnapshotInstantRpRetentionDaysName = "snapshot-instant-rp-retention-days";
    public const string SnapshotInstantRpResourceGroupName = "snapshot-instant-rp-resource-group";
    // DPP Disk vault tier copy
    public const string EnableVaultTierCopyName = "enable-vault-tier-copy";
    public const string VaultTierCopyAfterDaysName = "vault-tier-copy-after-days";
    // DPP Blob/ADLS backup mode (Continuous vs Vaulted)
    public const string BackupModeName = "backup-mode";
    // DPP PITR retention for continuous Blob/ADLS
    public const string PitrRetentionDaysName = "pitr-retention-days";
    // RSV policy-level tags
    public const string PolicyTagsName = "policy-tags";
}
