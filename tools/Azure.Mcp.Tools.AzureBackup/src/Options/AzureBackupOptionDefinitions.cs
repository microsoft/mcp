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
    public const string ScheduleFrequencyName = "schedule-frequency";
    public const string ScheduleTimeName = "schedule-time";
    public const string DailyRetentionDaysName = "daily-retention-days";
    public const string WeeklyRetentionWeeksName = "weekly-retention-weeks";
    public const string MonthlyRetentionMonthsName = "monthly-retention-months";
    public const string YearlyRetentionYearsName = "yearly-retention-years";
    public const string VmResourceIdName = "vm-resource-id";
    public const string ResourceTypeFilterName = "resource-type-filter";
    public const string ResourceGroupFilterName = "resource-group-filter";
    public const string TagFilterName = "tag-filter";

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
        Description = "The ARM resource ID of the datasource to protect.",
        Required = true
    };

    public static readonly Option<string> DatasourceType = new($"--{DatasourceTypeName}")
    {
        Description = "The workload type hint: AzureVM, SQLDatabase, SAPHana, SAPASE, AzureFileShare, AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB.",
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
        Description = "Workload type: AzureVM, SQLDatabase, SAPHana, SAPASE, AzureFileShare, AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB.",
        Required = false
    };

    public static readonly Option<string> ScheduleFrequency = new($"--{ScheduleFrequencyName}")
    {
        Description = "Backup schedule frequency: 'Hourly', 'Daily', or 'Weekly'.",
        Required = false
    };

    public static readonly Option<string> ScheduleTime = new($"--{ScheduleTimeName}")
    {
        Description = "Backup time in UTC (e.g., '02:00').",
        Required = false
    };

    public static readonly Option<string> DailyRetentionDays = new($"--{DailyRetentionDaysName}")
    {
        Description = "Daily recovery point retention in days.",
        Required = false
    };

    public static readonly Option<string> WeeklyRetentionWeeks = new($"--{WeeklyRetentionWeeksName}")
    {
        Description = "Weekly recovery point retention in weeks.",
        Required = false
    };

    public static readonly Option<string> MonthlyRetentionMonths = new($"--{MonthlyRetentionMonthsName}")
    {
        Description = "Monthly recovery point retention in months.",
        Required = false
    };

    public static readonly Option<string> YearlyRetentionYears = new($"--{YearlyRetentionYearsName}")
    {
        Description = "Yearly recovery point retention in years.",
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

    public static readonly Option<string> ResourceGroupFilter = new($"--{ResourceGroupFilterName}")
    {
        Description = "Resource group filter.",
        Required = false
    };

    public static readonly Option<string> TagFilter = new($"--{TagFilterName}")
    {
        Description = "Tag-based filter as JSON key-value object.",
        Required = false
    };
}
