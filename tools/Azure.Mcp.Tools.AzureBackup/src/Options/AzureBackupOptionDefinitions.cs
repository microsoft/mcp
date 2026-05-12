// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Options;

public static class AzureBackupOptionDefinitions
{
    public const string VaultName = "vault";
    public const string VaultTypeName = "vault-type";
    public const string ProtectedItemName = "protected-item";
    public const string ContainerName = "container";
    public const string PolicyName = "policy";
    public const string JobName = "job";
    public const string RecoveryPointName = "recovery-point";
    public const string LocationName = "location";
    public const string DatasourceIdName = "datasource-id";
    public const string DatasourceTypeName = "datasource-type";
    public const string SkuName = "sku";
    public const string StorageTypeName = "storage-type";
    public const string RedundancyName = "redundancy";
    public const string IdentityTypeName = "identity-type";
    public const string ImmutabilityStateName = "immutability-state";
    public const string SoftDeleteName = "soft-delete";
    public const string SoftDeleteRetentionDaysName = "soft-delete-retention-days";
    public const string TagsName = "tags";
    public const string WorkloadTypeName = "workload-type";
    public const string DailyRetentionDaysName = "daily-retention-days";
    public const string VmResourceIdName = "vm-resource-id";
    public const string ResourceTypeFilterName = "resource-type-filter";
    public const string TagFilterName = "tag-filter";
    public const string ResourceGuardIdName = "resource-guard-id";

    // Policy create  -  common schedule flags (new in policy create overhaul)
    public const string ScheduleTimeName = "schedule-time";
    public const string TimeZoneName = "time-zone";
    public const string ScheduleFrequencyName = "schedule-frequency";
    public const string ScheduleTimesName = "schedule-times";
    public const string ScheduleDaysOfWeekName = "schedule-days-of-week";
    public const string HourlyIntervalHoursName = "hourly-interval-hours";
    public const string HourlyWindowStartTimeName = "hourly-window-start-time";
    public const string HourlyWindowDurationHoursName = "hourly-window-duration-hours";

    // Policy create  -  retention flags (new in policy create overhaul)
    public const string WeeklyRetentionWeeksName = "weekly-retention-weeks";
    public const string WeeklyRetentionDaysOfWeekName = "weekly-retention-days-of-week";
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
    // DPP AKS-specific
    public const string AksSnapshotResourceGroupName = "aks-snapshot-resource-group";
    public const string AksIncludedNamespacesName = "aks-included-namespaces";
    public const string AksExcludedNamespacesName = "aks-excluded-namespaces";
    public const string AksLabelSelectorsName = "aks-label-selectors";
    public const string AksIncludeClusterScopeResourcesName = "aks-include-cluster-scope-resources";

    public static readonly Option<string> Vault = new($"--{VaultName}")
    {
        Description = "The name of the backup vault (Recovery Services vault or Backup vault).",
        Required = true
    };

    public static readonly Option<string> VaultType = new($"--{VaultTypeName}")
    {
        Description = "The type of backup vault: 'rsv' (Recovery Services vault) or 'dpp' (Backup vault / Data Protection). Required for vault create; optional elsewhere (auto-detected if omitted).",
        Required = false
    };

    public static readonly Option<string> ProtectedItem = new($"--{ProtectedItemName}")
    {
        Description = "The name of the protected item or backup instance.",
        Required = true
    };

    public static readonly Option<string> Container = new($"--{ContainerName}")
    {
        Description = "The RSV protection container name. Only applicable for Recovery Services vaults.",
        Required = false
    };

    public static readonly Option<string> Policy = new($"--{PolicyName}")
    {
        Description = "The name of the backup policy.",
        Required = true
    };

    public static readonly Option<string> Job = new($"--{JobName}")
    {
        Description = "The backup job ID.",
        Required = true
    };

    public static readonly Option<string> RecoveryPoint = new($"--{RecoveryPointName}")
    {
        Description = "The recovery point ID.",
        Required = true
    };

    public static readonly Option<string> Location = new($"--{LocationName}")
    {
        Description = "The Azure region (e.g., 'eastus', 'westus2').",
        Required = true
    };

    public static readonly Option<string> DatasourceId = new($"--{DatasourceIdName}")
    {
        Description = "The datasource identifier. For VM/FileShare/DPP workloads, use the ARM resource ID (e.g., '/subscriptions/.../virtualMachines/myvm'). For RSV in-guest workloads (SQL/SAPHANA), use the protectable item name from 'protectableitem list' (e.g., 'SAPHanaDatabase;instance;dbname').",
        Required = true
    };

    public static readonly Option<string> DatasourceType = new($"--{DatasourceTypeName}")
    {
        Description = "The workload type hint: VM, SQL, SAPHANA, SAPASE, AzureFileShare (RSV types); AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB (DPP types). Also accepts aliases like AzureVM, SQLDatabase, etc.",
        Required = false
    };

    public static readonly Option<string> Sku = new($"--{SkuName}")
    {
        Description = "The vault SKU.",
        Required = false
    };

    public static readonly Option<string> StorageType = new($"--{StorageTypeName}")
    {
        Description = "Storage redundancy: 'GeoRedundant', 'LocallyRedundant', or 'ZoneRedundant'.",
        Required = false
    };

    public static readonly Option<string> Redundancy = new($"--{RedundancyName}")
    {
        Description = "Storage redundancy: 'GeoRedundant', 'LocallyRedundant', 'ZoneRedundant', or 'ReadAccessGeoZoneRedundant'.",
        Required = false
    };

    public static readonly Option<string> IdentityType = new($"--{IdentityTypeName}")
    {
        Description = "Managed identity type: 'SystemAssigned', 'UserAssigned', or 'None'.",
        Required = false
    };

    public static readonly Option<string> ImmutabilityState = new($"--{ImmutabilityStateName}")
    {
        Description = "Immutability state: 'Disabled', 'Enabled', or 'Locked' (irreversible).",
        Required = false
    };

    public static readonly Option<string> SoftDelete = new($"--{SoftDeleteName}")
    {
        Description = "Soft delete state: 'AlwaysOn', 'On', or 'Off'.",
        Required = false
    };

    public static readonly Option<string> SoftDeleteRetentionDays = new($"--{SoftDeleteRetentionDaysName}")
    {
        Description = "Soft delete retention period (14-180 days).",
        Required = false
    };

    public static readonly Option<string> Tags = new($"--{TagsName}")
    {
        Description = "Resource tags as JSON key-value object.",
        Required = false
    };

    public static readonly Option<string> WorkloadType = new($"--{WorkloadTypeName}")
    {
        Description = "Workload type: VM, SQL, SAPHANA, SAPASE, AzureFileShare (RSV types); AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB (DPP types). Also accepts aliases like AzureVM, SQLDatabase, etc.",
        Required = false
    };

    public static readonly Option<string> DailyRetentionDays = new($"--{DailyRetentionDaysName}")
    {
        Description = "Daily recovery point retention in days. Defaults to datasource-specific value if omitted.",
        Required = false
    };

    public static readonly Option<string> VmResourceId = new($"--{VmResourceIdName}")
    {
        Description = "ARM ID of the VM hosting SQL or SAP HANA.",
        Required = false
    };

    public static readonly Option<string> ResourceTypeFilter = new($"--{ResourceTypeFilterName}")
    {
        Description = "Resource types to filter (comma-separated).",
        Required = false
    };

    public static readonly Option<string> TagFilter = new($"--{TagFilterName}")
    {
        Description = "Tag-based filter in key=value format (e.g., 'environment=production').",
        Required = false
    };

    public static readonly Option<string> TimeZone = new($"--{TimeZoneName}")
    {
        Description = "Windows time-zone identifier for the backup schedule (e.g., 'UTC', 'Pacific Standard Time', 'India Standard Time'). If omitted, the schedule runs in UTC.",
        Required = false
    };

    public static readonly Option<string> ScheduleTime = new($"--{ScheduleTimeName}")
    {
        Description = "Backup schedule time in 24h HH:mm format (e.g., '02:00'). Used for policy update.",
        Required = false
    };

    public static readonly Option<string> ScheduleFrequency = new($"--{ScheduleFrequencyName}")
    {
        Description = "Backup schedule frequency. RSV vaults accept 'Daily', 'Weekly', or 'Hourly'. DPP (Backup) vaults accept ISO 8601 intervals: 'PT4H', 'PT6H', 'PT8H', 'PT12H', 'P1D', 'P1W', 'P2W', or 'P1M'.",
        Required = false
    };

    public static readonly Option<string> ScheduleTimes = new($"--{ScheduleTimesName}")
    {
        Description = "Comma-separated list of backup times in 24h HH:mm format (e.g., '02:00' or '02:00,14:00'). Interpreted in --time-zone. Defaults to 02:00 UTC if not specified. Only the first time is used as the schedule start time.",
        Required = false
    };

    public static readonly Option<string> ScheduleDaysOfWeek = new($"--{ScheduleDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week the backup should run (e.g., 'Monday,Wednesday,Friday'). Required for Weekly schedules.",
        Required = false
    };

    public static readonly Option<int> HourlyIntervalHours = new($"--{HourlyIntervalHoursName}")
    {
        Description = "Interval in hours between hourly backups. Valid values: 4, 6, 8, 12. Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<string> HourlyWindowStartTime = new($"--{HourlyWindowStartTimeName}")
    {
        Description = "Start time of the hourly backup window in 24h HH:mm format (e.g., '08:00'). Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<int> HourlyWindowDurationHours = new($"--{HourlyWindowDurationHoursName}")
    {
        Description = "Duration of the hourly backup window in hours (e.g., 12). Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<int> WeeklyRetentionWeeks = new($"--{WeeklyRetentionWeeksName}")
    {
        Description = "Number of weeks to keep weekly recovery points. Required alongside --weekly-retention-days-of-week.",
        Required = false
    };

    public static readonly Option<string> WeeklyRetentionDaysOfWeek = new($"--{WeeklyRetentionDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week tagged for weekly retention (e.g., 'Sunday' or 'Saturday,Sunday'). Required alongside --weekly-retention-weeks.",
        Required = false
    };

    public static readonly Option<int> MonthlyRetentionMonths = new($"--{MonthlyRetentionMonthsName}")
    {
        Description = "Number of months to keep monthly recovery points. Combine with either --monthly-retention-days-of-month (absolute) OR --monthly-retention-week-of-month + --monthly-retention-days-of-week (relative).",
        Required = false
    };

    public static readonly Option<string> MonthlyRetentionWeekOfMonth = new($"--{MonthlyRetentionWeekOfMonthName}")
    {
        Description = "Which week of the month to tag for monthly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --monthly-retention-days-of-week (relative scheme).",
        Required = false
    };

    public static readonly Option<string> MonthlyRetentionDaysOfWeek = new($"--{MonthlyRetentionDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week for the monthly retention tag (e.g., 'Sunday'). Use with --monthly-retention-week-of-month (relative scheme).",
        Required = false
    };

    public static readonly Option<string> MonthlyRetentionDaysOfMonth = new($"--{MonthlyRetentionDaysOfMonthName}")
    {
        Description = "Comma-separated days of the month for monthly retention (1-28 or 'Last'; e.g., '1,15,Last'). Absolute scheme; mutually exclusive with --monthly-retention-week-of-month.",
        Required = false
    };

    public static readonly Option<int> YearlyRetentionYears = new($"--{YearlyRetentionYearsName}")
    {
        Description = "Number of years to keep yearly recovery points. Combine with --yearly-retention-months and either --yearly-retention-days-of-month (absolute) OR --yearly-retention-week-of-month + --yearly-retention-days-of-week (relative).",
        Required = false
    };

    public static readonly Option<string> YearlyRetentionMonths = new($"--{YearlyRetentionMonthsName}")
    {
        Description = "Comma-separated months tagged for yearly retention (e.g., 'January' or 'January,July').",
        Required = false
    };

    public static readonly Option<string> YearlyRetentionWeekOfMonth = new($"--{YearlyRetentionWeekOfMonthName}")
    {
        Description = "Which week of the selected month(s) to tag for yearly retention: 'First', 'Second', 'Third', 'Fourth', or 'Last'. Use with --yearly-retention-days-of-week (relative scheme).",
        Required = false
    };

    public static readonly Option<string> YearlyRetentionDaysOfWeek = new($"--{YearlyRetentionDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week for the yearly retention tag (e.g., 'Sunday'). Use with --yearly-retention-week-of-month (relative scheme).",
        Required = false
    };

    public static readonly Option<string> YearlyRetentionDaysOfMonth = new($"--{YearlyRetentionDaysOfMonthName}")
    {
        Description = "Comma-separated days of the selected month(s) for yearly retention (1-28 or 'Last'; e.g., '1,Last'). Absolute scheme; mutually exclusive with --yearly-retention-week-of-month.",
        Required = false
    };

    public static readonly Option<string> ArchiveTierAfterDays = new($"--{ArchiveTierAfterDaysName}")
    {
        Description = "Move recovery points to the archive tier after this many days. Pair with --archive-tier-mode.",
        Required = false
    };

    public static readonly Option<string> ArchiveTierMode = new($"--{ArchiveTierModeName}")
    {
        Description = "Archive tiering mode: 'TierAfter' (always tier after --archive-tier-after-days) or 'CopyOnExpiry' (copy to archive when the recovery point expires). Use --smart-tier for service-recommended tiering.",
        Required = false
    };

    public static readonly Option<string> PolicySubType = new($"--{PolicySubTypeName}")
    {
        Description = "RSV VM policy sub-type: 'Standard' or 'Enhanced'. Enhanced is required for hourly schedules and Trusted Launch VMs. RSV VM only.",
        Required = false
    };

    public static readonly Option<string> InstantRpRetentionDays = new($"--{InstantRpRetentionDaysName}")
    {
        Description = "Instant recovery point retention in days (1-30 for Standard, 1-7 for Enhanced). RSV VM only.",
        Required = false
    };

    public static readonly Option<string> InstantRpResourceGroup = new($"--{InstantRpResourceGroupName}")
    {
        Description = "Resource group that hosts the instant recovery point snapshots. RSV VM only.",
        Required = false
    };

    public static readonly Option<string> SnapshotConsistency = new($"--{SnapshotConsistencyName}")
    {
        Description = "Snapshot consistency mode for VM backups: 'ApplicationConsistent' or 'CrashConsistent'. RSV VM only.",
        Required = false
    };

    public static readonly Option<string> FullScheduleFrequency = new($"--{FullScheduleFrequencyName}")
    {
        Description = "Full backup schedule frequency for SQL/SAPHANA/SAPASE: 'Daily' or 'Weekly'. RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<string> FullScheduleDaysOfWeek = new($"--{FullScheduleDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week for the Full backup (e.g., 'Sunday'). Required when --full-schedule-frequency is 'Weekly'. RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<string> DifferentialScheduleDaysOfWeek = new($"--{DifferentialScheduleDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week for the Differential backup (e.g., 'Monday,Thursday'). RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<int> DifferentialRetentionDays = new($"--{DifferentialRetentionDaysName}")
    {
        Description = "Retention period in days for Differential backups. RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<string> IncrementalScheduleDaysOfWeek = new($"--{IncrementalScheduleDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week for the Incremental backup. RSV SAPHANA / SAPASE only.",
        Required = false
    };

    public static readonly Option<int> IncrementalRetentionDays = new($"--{IncrementalRetentionDaysName}")
    {
        Description = "Retention period in days for Incremental backups. RSV SAPHANA / SAPASE only.",
        Required = false
    };

    public static readonly Option<int> LogFrequencyMinutes = new($"--{LogFrequencyMinutesName}")
    {
        Description = "Transaction log backup frequency in minutes (e.g., 15, 30, 60). RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<int> LogRetentionDays = new($"--{LogRetentionDaysName}")
    {
        Description = "Retention period in days for transaction log backups. RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<bool> IsCompression = new($"--{IsCompressionName}")
    {
        Description = "Enable backup compression at the policy level. RSV VmWorkload only.",
        Required = false
    };

    public static readonly Option<bool> IsSqlCompression = new($"--{IsSqlCompressionName}")
    {
        Description = "Enable SQL Server on VM native backup compression. RSV SQL only.",
        Required = false
    };

    // ===== Stage 2 expansion =====


    public static readonly Option<bool> SmartTier = new($"--{SmartTierName}")
    {
        Description = "Enable smart-tiering (ML-based archive recommendation). RSV VM only - equivalent to TieringMode=TierRecommended. Kept separate from --archive-tier-mode because it emits a structurally different tiering shape (Duration=0, DurationType=Invalid).",
        Required = false
    };

    public static readonly Option<bool> EnableSnapshotBackup = new($"--{EnableSnapshotBackupName}")
    {
        Description = "Enable snapshot/instance backups (HANA System Replication snapshot RPs). RSV SAPHANA only.",
        Required = false
    };

    public static readonly Option<string> SnapshotInstantRpRetentionDays = new($"--{SnapshotInstantRpRetentionDaysName}")
    {
        Description = "Snapshot instant RP retention range in days. RSV SAPHANA snapshot only.",
        Required = false
    };

    public static readonly Option<string> SnapshotInstantRpResourceGroup = new($"--{SnapshotInstantRpResourceGroupName}")
    {
        Description = "Resource group prefix for snapshot instant RPs. RSV SAPHANA snapshot only.",
        Required = false
    };

    public static readonly Option<bool> EnableVaultTierCopy = new($"--{EnableVaultTierCopyName}")
    {
        Description = "Enable vault-tier copy of operational store backups. DPP AzureDisk only.",
        Required = false
    };

    public static readonly Option<int> VaultTierCopyAfterDays = new($"--{VaultTierCopyAfterDaysName}")
    {
        Description = "Days after which an operational backup is copied to the vault tier. DPP AzureDisk only.",
        Required = false
    };

    public static readonly Option<string> BackupMode = new($"--{BackupModeName}")
    {
        Description = "Backup mode for storage workloads: 'Continuous' (default for AzureBlob, ADLS) or 'Vaulted' (discrete recovery points). DPP AzureBlob, AzureDataLakeStorage.",
        Required = false
    };

    public static readonly Option<int> PitrRetentionDays = new($"--{PitrRetentionDaysName}")
    {
        Description = "Point-in-time restore retention in days for continuous backups. DPP AzureBlob, AzureDataLakeStorage.",
        Required = false
    };

    public static readonly Option<string> PolicyTags = new($"--{PolicyTagsName}")
    {
        Description = "Resource tags applied to the RSV backup policy as 'k1=v1,k2=v2'. RSV only.",
        Required = false
    };

    public static readonly Option<string> AksSnapshotResourceGroup = new($"--{AksSnapshotResourceGroupName}")
    {
        Description = "Resource group used to store AKS volume snapshots created by Backup. DPP AKS only.",
        Required = false
    };

    public static readonly Option<string> AksIncludedNamespaces = new($"--{AksIncludedNamespacesName}")
    {
        Description = "Comma-separated list of namespaces to include in the AKS backup policy default scope. DPP AKS only.",
        Required = false
    };

    public static readonly Option<string> AksExcludedNamespaces = new($"--{AksExcludedNamespacesName}")
    {
        Description = "Comma-separated list of namespaces to exclude from the AKS backup policy default scope. DPP AKS only.",
        Required = false
    };

    public static readonly Option<string> AksLabelSelectors = new($"--{AksLabelSelectorsName}")
    {
        Description = "Comma-separated label selectors (e.g. 'app=frontend,tier=web') applied to the AKS backup policy default scope. DPP AKS only.",
        Required = false
    };

    public static readonly Option<bool> AksIncludeClusterScopeResources = new($"--{AksIncludeClusterScopeResourcesName}")
    {
        Description = "Include cluster-scoped resources in the AKS backup policy. DPP AKS only.",
        Required = false
    };

    public static readonly Option<string> ResourceGuardId = new($"--{ResourceGuardIdName}")
    {
        Description = "ARM resource ID of the Resource Guard to link for Multi-User Authorization (e.g., '/subscriptions/.../resourceGroups/.../providers/Microsoft.DataProtection/resourceGuards/myGuard').",
        Required = false
    };
}
