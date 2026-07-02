// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Options;

public static class NetAppFilesOptionDefinitions
{
    public const string AccountName = "account";
    public const string PoolName = "pool";
    public const string VolumeName = "volume";
    public const string BackupName = "backup";
    public const string BackupPolicyName = "backupPolicy";
    public const string BackupVaultName = "backupVault";
    public const string SnapshotName = "snapshot";
    public const string SnapshotPolicyName = "snapshotPolicy";
    public const string VolumeGroupName = "volumeGroup";
    public const string LocationName = "location";
    public const string SubnetIdName = "subnetId";
    public const string CreationTokenName = "creationToken";
    public const string UsageThresholdName = "usageThreshold";
    public const string ServiceLevelName = "serviceLevel";
    public const string ProtocolTypesName = "protocolTypes";
    public const string DailyBackupsToKeepName = "dailyBackupsToKeep";
    public const string WeeklyBackupsToKeepName = "weeklyBackupsToKeep";
    public const string MonthlyBackupsToKeepName = "monthlyBackupsToKeep";
    public const string VolumeResourceIdName = "volumeResourceId";
    public const string LabelName = "label";
    public const string SizeName = "size";
    public const string QosTypeName = "qosType";
    public const string CoolAccessName = "coolAccess";
    public const string EncryptionTypeName = "encryptionType";
    public const string HourlyScheduleMinuteName = "hourlyScheduleMinute";
    public const string HourlyScheduleSnapshotsToKeepName = "hourlyScheduleSnapshotsToKeep";
    public const string DailyScheduleHourName = "dailyScheduleHour";
    public const string DailyScheduleMinuteName = "dailyScheduleMinute";
    public const string DailyScheduleSnapshotsToKeepName = "dailyScheduleSnapshotsToKeep";
    public const string WeeklyScheduleDayName = "weeklyScheduleDay";
    public const string WeeklyScheduleSnapshotsToKeepName = "weeklyScheduleSnapshotsToKeep";
    public const string MonthlyScheduleDaysOfMonthName = "monthlyScheduleDaysOfMonth";
    public const string MonthlyScheduleSnapshotsToKeepName = "monthlyScheduleSnapshotsToKeep";
    public const string ApplicationTypeName = "applicationType";
    public const string ApplicationIdentifierName = "applicationIdentifier";
    public const string GroupDescriptionName = "groupDescription";
    public const string TagsName = "tags";

    public static readonly Option<string> Account = new($"--{AccountName}")
    {
        Description = "The name of the Azure NetApp Files account (e.g., 'myanfaccount').",
        Required = true
    };

    public static readonly Option<string> Pool = new($"--{PoolName}")
    {
        Description = "The name of the capacity pool (e.g., 'mypool').",
        Required = true
    };

    public static readonly Option<string> Volume = new($"--{VolumeName}")
    {
        Description = "The name of the volume (e.g., 'myvolume').",
        Required = true
    };

    public static readonly Option<string> Backup = new($"--{BackupName}")
    {
        Description = "The name of the backup (e.g., 'mybackup').",
        Required = true
    };

    public static readonly Option<string> BackupPolicy = new($"--{BackupPolicyName}")
    {
        Description = "The name of the backup policy (e.g., 'mybackuppolicy').",
        Required = true
    };

    public static readonly Option<string> BackupVault = new($"--{BackupVaultName}")
    {
        Description = "The name of the backup vault (e.g., 'mybackupvault').",
        Required = true
    };

    public static readonly Option<string> Snapshot = new($"--{SnapshotName}")
    {
        Description = "The name of the snapshot (e.g., 'mysnapshot').",
        Required = true
    };

    public static readonly Option<string> SnapshotPolicy = new($"--{SnapshotPolicyName}")
    {
        Description = "The name of the snapshot policy (e.g., 'mysnapshotpolicy').",
        Required = true
    };

    public static readonly Option<string> VolumeGroup = new($"--{VolumeGroupName}")
    {
        Description = "The name of the volume group (e.g., 'myvolumegroup').",
        Required = true
    };

    public static readonly Option<string> Location = new($"--{LocationName}")
    {
        Description = "The Azure region where the volume will be created (e.g., 'eastus', 'westus2').",
        Required = true
    };

    public static readonly Option<string> SubnetId = new($"--{SubnetIdName}")
    {
        Description = "The Azure Resource Manager resource identifier of the delegated subnet (e.g., '/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}').",
        Required = true
    };

    public static readonly Option<string> CreationToken = new($"--{CreationTokenName}")
    {
        Description = "A unique file path for the volume. Used when creating mount targets (e.g., 'myvolume').",
        Required = true
    };

    public static readonly Option<long> UsageThreshold = new($"--{UsageThresholdName}")
    {
        Description = "Maximum storage quota allowed for a file system in bytes. Minimum 107374182400 bytes (100 GiB).",
        Required = true
    };

    public static readonly Option<string> ServiceLevel = new($"--{ServiceLevelName}")
    {
        Description = "The service level of the volume. Valid values: Standard, Premium, Ultra.",
        Required = false
    };

    public static readonly Option<string[]> ProtocolTypes = new($"--{ProtocolTypesName}")
    {
        Description = "The protocol types for the volume. Valid values: NFSv3, NFSv4.1, CIFS.",
        Required = false
    };

    public static readonly Option<int?> DailyBackupsToKeep = new($"--{DailyBackupsToKeepName}")
    {
        Description = "The number of daily backups to keep (e.g., 2).",
        Required = false
    };

    public static readonly Option<int?> WeeklyBackupsToKeep = new($"--{WeeklyBackupsToKeepName}")
    {
        Description = "The number of weekly backups to keep (e.g., 1).",
        Required = false
    };

    public static readonly Option<int?> MonthlyBackupsToKeep = new($"--{MonthlyBackupsToKeepName}")
    {
        Description = "The number of monthly backups to keep (e.g., 1).",
        Required = false
    };

    public static readonly Option<string> VolumeResourceId = new($"--{VolumeResourceIdName}")
    {
        Description = "The Azure resource ID of the volume to back up (e.g., '/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}').",
        Required = true
    };

    public static readonly Option<string> Label = new($"--{LabelName}")
    {
        Description = "A label for the backup (e.g., 'daily-backup').",
        Required = false
    };

    public static readonly Option<long> Size = new($"--{SizeName}")
    {
        Description = "Provisioned size of the pool in bytes. Must be a multiple of 4398046511104 (4 TiB). Minimum 4398046511104 bytes (4 TiB).",
        Required = true
    };

    public static readonly Option<string> QosType = new($"--{QosTypeName}")
    {
        Description = "The QoS type of the pool. Valid values: Auto, Manual.",
        Required = false
    };

    public static readonly Option<bool?> CoolAccess = new($"--{CoolAccessName}")
    {
        Description = "Whether cool access (tiering) is enabled for volumes in the pool.",
        Required = false
    };

    public static readonly Option<string> EncryptionType = new($"--{EncryptionTypeName}")
    {
        Description = "The encryption type of the pool. Valid values: Single, Double.",
        Required = false
    };

    public static readonly Option<int?> HourlyScheduleMinute = new($"--{HourlyScheduleMinuteName}")
    {
        Description = "The minute of the hour for the hourly snapshot schedule (0-59).",
        Required = false
    };

    public static readonly Option<int?> HourlyScheduleSnapshotsToKeep = new($"--{HourlyScheduleSnapshotsToKeepName}")
    {
        Description = "The number of hourly snapshots to keep (e.g., 5).",
        Required = false
    };

    public static readonly Option<int?> DailyScheduleHour = new($"--{DailyScheduleHourName}")
    {
        Description = "The hour of the day for the daily snapshot schedule (0-23).",
        Required = false
    };

    public static readonly Option<int?> DailyScheduleMinute = new($"--{DailyScheduleMinuteName}")
    {
        Description = "The minute of the hour for the daily snapshot schedule (0-59).",
        Required = false
    };

    public static readonly Option<int?> DailyScheduleSnapshotsToKeep = new($"--{DailyScheduleSnapshotsToKeepName}")
    {
        Description = "The number of daily snapshots to keep (e.g., 5).",
        Required = false
    };

    public static readonly Option<string> WeeklyScheduleDay = new($"--{WeeklyScheduleDayName}")
    {
        Description = "The day of the week for the weekly snapshot schedule (e.g., 'Monday').",
        Required = false
    };

    public static readonly Option<int?> WeeklyScheduleSnapshotsToKeep = new($"--{WeeklyScheduleSnapshotsToKeepName}")
    {
        Description = "The number of weekly snapshots to keep (e.g., 4).",
        Required = false
    };

    public static readonly Option<string> MonthlyScheduleDaysOfMonth = new($"--{MonthlyScheduleDaysOfMonthName}")
    {
        Description = "The days of the month for the monthly snapshot schedule (e.g., '1,15').",
        Required = false
    };

    public static readonly Option<int?> MonthlyScheduleSnapshotsToKeep = new($"--{MonthlyScheduleSnapshotsToKeepName}")
    {
        Description = "The number of monthly snapshots to keep (e.g., 2).",
        Required = false
    };

    public static readonly Option<string> ApplicationType = new($"--{ApplicationTypeName}")
    {
        Description = "The application type of the volume group (e.g., 'SAP-HANA').",
        Required = true
    };

    public static readonly Option<string> ApplicationIdentifier = new($"--{ApplicationIdentifierName}")
    {
        Description = "The application specific identifier (e.g., 'SH1' for SAP HANA SID).",
        Required = true
    };

    public static readonly Option<string> GroupDescription = new($"--{GroupDescriptionName}")
    {
        Description = "A description for the volume group (e.g., 'Volume group for SAP HANA').",
        Required = false
    };

    public static readonly Option<string> Tags = new($"--{TagsName}")
    {
        Description = "Tags for the account in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').",
        Required = false
    };
}
