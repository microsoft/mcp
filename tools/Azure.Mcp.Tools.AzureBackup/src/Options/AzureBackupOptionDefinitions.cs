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

    // Policy create — common schedule flags (new in policy create overhaul)
    public const string TimeZoneName = "time-zone";
    public const string ScheduleFrequencyName = "schedule-frequency";
    public const string ScheduleTimesName = "schedule-times";
    public const string ScheduleDaysOfWeekName = "schedule-days-of-week";
    public const string HourlyIntervalHoursName = "hourly-interval-hours";
    public const string HourlyWindowStartTimeName = "hourly-window-start-time";
    public const string HourlyWindowDurationHoursName = "hourly-window-duration-hours";

    // Policy create — retention flags (new in policy create overhaul)
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

    public static readonly Option<string> ScheduleFrequency = new($"--{ScheduleFrequencyName}")
    {
        Description = "Backup schedule frequency. RSV vaults accept 'Daily', 'Weekly', or 'Hourly'. DPP (Backup) vaults accept ISO 8601 intervals: 'PT4H', 'PT6H', 'PT8H', 'PT12H', 'P1D', 'P1W', 'P2W', or 'P1M'.",
        Required = false
    };

    public static readonly Option<string> ScheduleTimes = new($"--{ScheduleTimesName}")
    {
        Description = "Comma-separated list of backup times in 24h HH:mm format (e.g., '02:00' or '02:00,14:00'). Interpreted in --time-zone.",
        Required = false
    };

    public static readonly Option<string> ScheduleDaysOfWeek = new($"--{ScheduleDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week the backup should run (e.g., 'Monday,Wednesday,Friday'). Required for Weekly schedules.",
        Required = false
    };

    public static readonly Option<string> HourlyIntervalHours = new($"--{HourlyIntervalHoursName}")
    {
        Description = "Interval in hours between hourly backups (e.g., 4, 6, 8, 12). Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<string> HourlyWindowStartTime = new($"--{HourlyWindowStartTimeName}")
    {
        Description = "Start time of the hourly backup window in 24h HH:mm format (e.g., '08:00'). Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<string> HourlyWindowDurationHours = new($"--{HourlyWindowDurationHoursName}")
    {
        Description = "Duration of the hourly backup window in hours (e.g., 12). Used only when --schedule-frequency is 'Hourly' (RSV).",
        Required = false
    };

    public static readonly Option<string> WeeklyRetentionWeeks = new($"--{WeeklyRetentionWeeksName}")
    {
        Description = "Number of weeks to keep weekly recovery points. Pair with --weekly-retention-days-of-week.",
        Required = false
    };

    public static readonly Option<string> WeeklyRetentionDaysOfWeek = new($"--{WeeklyRetentionDaysOfWeekName}")
    {
        Description = "Comma-separated days of the week tagged for weekly retention (e.g., 'Sunday' or 'Saturday,Sunday'). Pair with --weekly-retention-weeks.",
        Required = false
    };

    public static readonly Option<string> MonthlyRetentionMonths = new($"--{MonthlyRetentionMonthsName}")
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

    public static readonly Option<string> YearlyRetentionYears = new($"--{YearlyRetentionYearsName}")
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
        Description = "Archive tiering mode: 'TierAfter' (always tier after --archive-tier-after-days) or 'TierRecommended' (tier when service recommends it).",
        Required = false
    };
}
