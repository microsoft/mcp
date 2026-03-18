// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Options;

public static class AzureBackupOptionDefinitions
{
    // Existing option names
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
    public const string ExpiryName = "expiry";
    public const string TargetResourceIdName = "target-resource-id";
    public const string RestoreLocationName = "restore-location";

    // New option names for expanded tool set
    public const string RedundancyName = "redundancy";
    public const string EnableCrrName = "enable-crr";
    public const string EncryptionTypeName = "encryption-type";
    public const string KeyVaultUriName = "key-vault-uri";
    public const string KeyNameName = "key-name";
    public const string KeyVersionName = "key-version";
    public const string IdentityTypeName = "identity-type";
    public const string UserAssignedIdentityIdName = "user-assigned-identity-id";
    public const string ImmutabilityStateName = "immutability-state";
    public const string SoftDeleteName = "soft-delete";
    public const string SoftDeleteRetentionDaysName = "soft-delete-retention-days";
    public const string TagsName = "tags";
    public const string OutputIacName = "output-iac";
    public const string ForceName = "force";
    public const string WorkloadTypeName = "workload-type";
    public const string ScheduleFrequencyName = "schedule-frequency";
    public const string ScheduleTimeName = "schedule-time";
    public const string ScheduleDaysName = "schedule-days";
    public const string DailyRetentionDaysName = "daily-retention-days";
    public const string WeeklyRetentionWeeksName = "weekly-retention-weeks";
    public const string MonthlyRetentionMonthsName = "monthly-retention-months";
    public const string YearlyRetentionYearsName = "yearly-retention-years";
    public const string LogBackupFrequencyMinutesName = "log-backup-frequency-minutes";
    public const string ModeName = "mode";
    public const string NewPolicyNameName = "new-policy-name";
    public const string VmResourceIdName = "vm-resource-id";
    public const string InstanceNameName = "instance-name";
    public const string BackupTypeName = "backup-type";
    public const string RuleNameName = "rule-name";
    public const string StartDateName = "start-date";
    public const string EndDateName = "end-date";
    public const string TierName = "tier";
    public const string RestoreModeName = "restore-mode";
    public const string RestoreTypeName = "restore-type";
    public const string TargetVmNameName = "target-vm-name";
    public const string TargetVnetIdName = "target-vnet-id";
    public const string TargetSubnetIdName = "target-subnet-id";
    public const string StagingStorageAccountIdName = "staging-storage-account-id";
    public const string TargetDatabaseNameName = "target-database-name";
    public const string TargetInstanceNameName = "target-instance-name";
    public const string PointInTimeName = "point-in-time";
    public const string TargetServerIdName = "target-server-id";
    public const string TargetClusterIdName = "target-cluster-id";
    public const string TargetStorageAccountIdName = "target-storage-account-id";
    public const string TargetFileShareNameName = "target-file-share-name";
    public const string RestoredDiskNameName = "restored-disk-name";
    public const string BackupInstanceNameName = "backup-instance-name";
    public const string ActionName = "action";
    public const string PrincipalIdName = "principal-id";
    public const string RoleNameName = "role-name";
    public const string ScopeName = "scope";
    public const string ResourceGuardIdName = "resource-guard-id";
    public const string VnetIdName = "vnet-id";
    public const string SubnetIdName = "subnet-id";
    public const string LogAnalyticsWorkspaceIdName = "log-analytics-workspace-id";
    public const string ReportTypeName = "report-type";
    public const string TimeRangeDaysName = "time-range-days";
    public const string ResourceTypeFilterName = "resource-type-filter";
    public const string ResourceGroupFilterName = "resource-group-filter";
    public const string TagFilterName = "tag-filter";
    public const string PolicyDefinitionIdName = "policy-definition-id";
    public const string DeployRemediationName = "deploy-remediation";
    public const string SecondaryRegionName = "secondary-region";
    public const string CrossRegionName = "cross-region";
    public const string ResourceIdsName = "resource-ids";
    public const string IncludeArchiveProjectionName = "include-archive-projection";
    public const string RpoThresholdHoursName = "rpo-threshold-hours";
    public const string IncludeSecurityPostureName = "include-security-posture";
    public const string IacFormatName = "iac-format";
    public const string IncludeProtectedItemsName = "include-protected-items";
    public const string IncludeRbacName = "include-rbac";
    public const string SourcePolicyNameName = "source-policy-name";
    public const string TargetPolicyNameName = "target-policy-name";
    public const string SecurityLevelName = "security-level";
    public const string SnapshotResourceGroupName = "snapshot-resource-group";
    public const string AutoRemediateName = "auto-remediate";
    public const string TriggerFirstBackupName = "trigger-first-backup";
    public const string AutoProtectName = "auto-protect";
    public const string InfectionTimestampName = "infection-timestamp";
    public const string SourceVaultNameName = "source-vault-name";
    public const string TargetVaultNameName = "target-vault-name";
    public const string DiagnosticWorkspaceIdName = "diagnostic-workspace-id";
    public const string CheckEligibilityOnlyName = "check-eligibility-only";
    public const string StatusFilterName = "status-filter";
    public const string OperationFilterName = "operation-filter";

    // Existing option objects
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
        Description = "The workload type hint (e.g., 'AzureVM', 'AzureDisk').",
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

    public static readonly Option<string> Expiry = new($"--{ExpiryName}")
    {
        Description = "Recovery point expiry time in ISO 8601 format.",
        Required = false
    };

    public static readonly Option<string> TargetResourceId = new($"--{TargetResourceIdName}")
    {
        Description = "ARM resource ID of the target resource for restore.",
        Required = false
    };

    public static readonly Option<string> RestoreLocation = new($"--{RestoreLocationName}")
    {
        Description = "Azure region to restore to.",
        Required = false
    };

    // New option objects for expanded tool set
    public static readonly Option<string> Redundancy = new($"--{RedundancyName}")
    {
        Description = "Storage redundancy: LRS, GRS, ZRS, or RAGRS.",
        Required = false
    };

    public static readonly Option<string> EnableCrr = new($"--{EnableCrrName}")
    {
        Description = "Enable Cross-Region Restore (RSV + GRS only). Set to 'true' to enable.",
        Required = false
    };

    public static readonly Option<string> EncryptionType = new($"--{EncryptionTypeName}")
    {
        Description = "Encryption type: 'platform' or 'cmk'.",
        Required = false
    };

    public static readonly Option<string> KeyVaultUri = new($"--{KeyVaultUriName}")
    {
        Description = "Key Vault URI for CMK encryption.",
        Required = false
    };

    public static readonly Option<string> KeyName = new($"--{KeyNameName}")
    {
        Description = "Encryption key name in Key Vault.",
        Required = false
    };

    public static readonly Option<string> KeyVersion = new($"--{KeyVersionName}")
    {
        Description = "Specific key version (omit for latest).",
        Required = false
    };

    public static readonly Option<string> IdentityType = new($"--{IdentityTypeName}")
    {
        Description = "Managed identity type: 'SystemAssigned', 'UserAssigned', or 'None'.",
        Required = false
    };

    public static readonly Option<string> UserAssignedIdentityId = new($"--{UserAssignedIdentityIdName}")
    {
        Description = "ARM ID of user-assigned managed identity.",
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

    public static readonly Option<string> OutputIac = new($"--{OutputIacName}")
    {
        Description = "Output IaC template: 'none', 'terraform', or 'bicep'.",
        Required = false
    };

    public static readonly Option<string> Force = new($"--{ForceName}")
    {
        Description = "Force operation. Set to 'true' to force.",
        Required = false
    };

    public static readonly Option<string> WorkloadType = new($"--{WorkloadTypeName}")
    {
        Description = "Workload type: AzureVM, SQLDatabase, SAPHana, AzureFileShare, AzureDisk, AzureBlob, PostgreSQLFlexible, MySQLFlexible, AKS.",
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

    public static readonly Option<string> ScheduleDays = new($"--{ScheduleDaysName}")
    {
        Description = "Days of week for weekly schedules (comma-separated).",
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

    public static readonly Option<string> LogBackupFrequencyMinutes = new($"--{LogBackupFrequencyMinutesName}")
    {
        Description = "Log backup interval in minutes (SQL/HANA: 15-480).",
        Required = false
    };

    public static readonly Option<string> Mode = new($"--{ModeName}")
    {
        Description = "Operation mode: 'RetainData' or 'DeleteData' (for stop protection).",
        Required = false
    };

    public static readonly Option<string> NewPolicyName = new($"--{NewPolicyNameName}")
    {
        Description = "New policy name to switch to.",
        Required = false
    };

    public static readonly Option<string> VmResourceId = new($"--{VmResourceIdName}")
    {
        Description = "ARM ID of the VM hosting SQL or SAP HANA.",
        Required = false
    };

    public static readonly Option<string> InstanceName = new($"--{InstanceNameName}")
    {
        Description = "SQL instance name or SAP HANA SID.",
        Required = false
    };

    public static readonly Option<string> BackupType = new($"--{BackupTypeName}")
    {
        Description = "Backup type: 'Full', 'Differential', 'Log', 'CopyOnlyFull', 'SnapshotFull', 'SnapshotCopyOnlyFull', or 'Incremental'.",
        Required = false
    };

    public static readonly Option<string> RuleName = new($"--{RuleNameName}")
    {
        Description = "Backup rule name from the policy (BV only).",
        Required = false
    };

    public static readonly Option<string> StartDate = new($"--{StartDateName}")
    {
        Description = "Filter start datetime (ISO8601).",
        Required = false
    };

    public static readonly Option<string> EndDate = new($"--{EndDateName}")
    {
        Description = "Filter end datetime (ISO8601).",
        Required = false
    };

    public static readonly Option<string> Tier = new($"--{TierName}")
    {
        Description = "Recovery point tier: 'Snapshot', 'VaultStandard', or 'VaultArchive'.",
        Required = false
    };

    public static readonly Option<string> RestoreMode = new($"--{RestoreModeName}")
    {
        Description = "Restore mode: 'OriginalLocation', 'AlternateLocation', or 'RestoreDisks'.",
        Required = false
    };

    public static readonly Option<string> RestoreType = new($"--{RestoreTypeName}")
    {
        Description = "Restore type: 'FullShareRestore', 'ItemLevelRestore', 'PointInTime', or 'RecoveryPoint'.",
        Required = false
    };

    public static readonly Option<string> TargetVmName = new($"--{TargetVmNameName}")
    {
        Description = "Target VM name for alternate-location restore.",
        Required = false
    };

    public static readonly Option<string> TargetVnetId = new($"--{TargetVnetIdName}")
    {
        Description = "Target VNet ARM ID.",
        Required = false
    };

    public static readonly Option<string> TargetSubnetId = new($"--{TargetSubnetIdName}")
    {
        Description = "Target subnet ARM ID.",
        Required = false
    };

    public static readonly Option<string> StagingStorageAccountId = new($"--{StagingStorageAccountIdName}")
    {
        Description = "Staging storage account ARM ID for restore.",
        Required = false
    };

    public static readonly Option<string> TargetDatabaseName = new($"--{TargetDatabaseNameName}")
    {
        Description = "Target database name for restore.",
        Required = false
    };

    public static readonly Option<string> TargetInstanceName = new($"--{TargetInstanceNameName}")
    {
        Description = "Target SQL/HANA instance name.",
        Required = false
    };

    public static readonly Option<string> PointInTime = new($"--{PointInTimeName}")
    {
        Description = "Point-in-time datetime for log-based restore (ISO8601).",
        Required = false
    };

    public static readonly Option<string> TargetServerId = new($"--{TargetServerIdName}")
    {
        Description = "Target server ARM ID for database restore.",
        Required = false
    };

    public static readonly Option<string> TargetClusterId = new($"--{TargetClusterIdName}")
    {
        Description = "Target AKS cluster ARM ID.",
        Required = false
    };

    public static readonly Option<string> TargetStorageAccountId = new($"--{TargetStorageAccountIdName}")
    {
        Description = "Target storage account ARM ID.",
        Required = false
    };

    public static readonly Option<string> TargetFileShareName = new($"--{TargetFileShareNameName}")
    {
        Description = "Target file share name.",
        Required = false
    };

    public static readonly Option<string> RestoredDiskName = new($"--{RestoredDiskNameName}")
    {
        Description = "Name for the restored disk.",
        Required = false
    };

    public static readonly Option<string> BackupInstanceName = new($"--{BackupInstanceNameName}")
    {
        Description = "Backup instance name (BV only).",
        Required = false
    };

    public static readonly Option<string> Action = new($"--{ActionName}")
    {
        Description = "Action to perform: 'mount' or 'revoke'.",
        Required = false
    };

    public static readonly Option<string> PrincipalId = new($"--{PrincipalIdName}")
    {
        Description = "User, service principal, or managed identity object ID.",
        Required = false
    };

    public static readonly Option<string> RoleName = new($"--{RoleNameName}")
    {
        Description = "Built-in backup role name or GUID.",
        Required = false
    };

    public static readonly Option<string> Scope = new($"--{ScopeName}")
    {
        Description = "ARM scope for role assignment.",
        Required = false
    };

    public static readonly Option<string> ResourceGuardId = new($"--{ResourceGuardIdName}")
    {
        Description = "ARM ID of the Resource Guard for MUA.",
        Required = false
    };

    public static readonly Option<string> VnetId = new($"--{VnetIdName}")
    {
        Description = "Target VNet ARM ID for private endpoint.",
        Required = false
    };

    public static readonly Option<string> SubnetId = new($"--{SubnetIdName}")
    {
        Description = "Target subnet ARM ID for private endpoint.",
        Required = false
    };

    public static readonly Option<string> LogAnalyticsWorkspaceId = new($"--{LogAnalyticsWorkspaceIdName}")
    {
        Description = "Log Analytics workspace ARM ID.",
        Required = false
    };

    public static readonly Option<string> ReportType = new($"--{ReportTypeName}")
    {
        Description = "Report type: 'BackupItems', 'JobSummary', 'StorageConsumption', 'PolicyCompliance', or 'RPOAnalysis'.",
        Required = false
    };

    public static readonly Option<string> TimeRangeDays = new($"--{TimeRangeDaysName}")
    {
        Description = "Lookback period in days.",
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

    public static readonly Option<string> PolicyDefinitionId = new($"--{PolicyDefinitionIdName}")
    {
        Description = "Azure Policy definition ARM ID.",
        Required = false
    };

    public static readonly Option<string> DeployRemediation = new($"--{DeployRemediationName}")
    {
        Description = "Deploy remediation task. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> SecondaryRegion = new($"--{SecondaryRegionName}")
    {
        Description = "Target paired secondary region.",
        Required = false
    };

    public static readonly Option<string> CrossRegion = new($"--{CrossRegionName}")
    {
        Description = "Cross-region restore. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> ResourceIds = new($"--{ResourceIdsName}")
    {
        Description = "Comma-separated list of ARM resource IDs.",
        Required = false
    };

    public static readonly Option<string> IncludeArchiveProjection = new($"--{IncludeArchiveProjectionName}")
    {
        Description = "Include archive tier cost projection. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> RpoThresholdHours = new($"--{RpoThresholdHoursName}")
    {
        Description = "RPO threshold in hours to flag breaching items.",
        Required = false
    };

    public static readonly Option<string> IncludeSecurityPosture = new($"--{IncludeSecurityPostureName}")
    {
        Description = "Include vault security posture check. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> IacFormat = new($"--{IacFormatName}")
    {
        Description = "IaC format: 'terraform' or 'bicep'.",
        Required = false
    };

    public static readonly Option<string> IncludeProtectedItems = new($"--{IncludeProtectedItemsName}")
    {
        Description = "Include protected items in IaC output. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> IncludeRbac = new($"--{IncludeRbacName}")
    {
        Description = "Include RBAC assignments in IaC output. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> SourcePolicyName = new($"--{SourcePolicyNameName}")
    {
        Description = "Source policy name for bulk policy update.",
        Required = false
    };

    public static readonly Option<string> TargetPolicyName = new($"--{TargetPolicyNameName}")
    {
        Description = "Target policy name for bulk policy update.",
        Required = false
    };

    public static readonly Option<string> SecurityLevel = new($"--{SecurityLevelName}")
    {
        Description = "Security preset: 'Standard', 'Enhanced', or 'Maximum'.",
        Required = false
    };

    public static readonly Option<string> SnapshotResourceGroup = new($"--{SnapshotResourceGroupName}")
    {
        Description = "Resource group for disk/AKS snapshots.",
        Required = false
    };

    public static readonly Option<string> AutoRemediate = new($"--{AutoRemediateName}")
    {
        Description = "Auto-remediate without confirmation. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> TriggerFirstBackup = new($"--{TriggerFirstBackupName}")
    {
        Description = "Trigger first backup after setup. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> AutoProtect = new($"--{AutoProtectName}")
    {
        Description = "Auto-protect new databases. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> InfectionTimestamp = new($"--{InfectionTimestampName}")
    {
        Description = "Datetime when infection was detected (ISO8601).",
        Required = false
    };

    public static readonly Option<string> SourceVaultName = new($"--{SourceVaultNameName}")
    {
        Description = "Source vault name for migration.",
        Required = false
    };

    public static readonly Option<string> TargetVaultName = new($"--{TargetVaultNameName}")
    {
        Description = "Target vault name for migration.",
        Required = false
    };

    public static readonly Option<string> DiagnosticWorkspaceId = new($"--{DiagnosticWorkspaceIdName}")
    {
        Description = "Log Analytics workspace ARM ID for diagnostics.",
        Required = false
    };

    public static readonly Option<string> CheckEligibilityOnly = new($"--{CheckEligibilityOnlyName}")
    {
        Description = "Only check eligibility without acting. Set to 'true'.",
        Required = false
    };

    public static readonly Option<string> StatusFilter = new($"--{StatusFilterName}")
    {
        Description = "Job status filter: 'InProgress', 'Completed', 'Failed', 'Cancelled'.",
        Required = false
    };

    public static readonly Option<string> OperationFilter = new($"--{OperationFilterName}")
    {
        Description = "Job operation filter: 'Backup', 'Restore', 'ConfigureBackup', 'DeleteBackupData'.",
        Required = false
    };
}
