// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Options;

public static class NetAppFilesOptionDefinitions
{
    // public const string AccountName = "account";
    // public const string PoolName = "pool";
    // public const string VolumeName = "volume";
    // public const string BackupName = "backup";
    // public const string BackupPolicyName = "backupPolicy";
    // public const string BackupVaultName = "backupVault";
    // public const string SnapshotName = "snapshot";
    // public const string SnapshotPolicyName = "snapshotPolicy";
    // public const string VolumeGroupName = "volumeGroup";
    // public const string LocationName = "location";
    // public const string SubnetIdName = "subnetId";
    // public const string CreationTokenName = "creationToken";
    // public const string UsageThresholdName = "usageThreshold";
    // public const string ServiceLevelName = "serviceLevel";
    // public const string ProtocolTypesName = "protocolTypes";
    // public const string SubnetName = "subnet";
    // public const string VnetName = "vnet";
    // public const string AcceptGrowCapacityPoolForShortTermCloneSplitName = "acceptGrowCapacityPoolForShortTermCloneSplit";
    // public const string AllowedClientsName = "allowedClients";
    // public const string AvsDataStoreName = "avsDataStore";
    // public const string BackupIdName = "backupId";
    // public const string BackupPolicyIdName = "backupPolicyId";
    // public const string BackupVaultIdName = "backupVaultId";
    // public const string CoolAccessRetrievalPolicyName = "coolAccessRetrievalPolicy";
    // public const string CoolAccessTieringPolicyName = "coolAccessTieringPolicy";
    // public const string CapacityPoolResourceIdName = "capacityPoolResourceId";
    // public const string ChownModeName = "chownMode";
    // public const string CifsName = "cifs";
    // public const string CoolAccessVolumeName = "coolAccessVolume";
    // public const string CoolnessPeriodName = "coolnessPeriod";
    // public const string DeleteBaseSnapshotName = "deleteBaseSnapshot";
    // public const string DesiredArpStateName = "desiredArpState";
    // public const string EnableSubvolumesName = "enableSubvolumes";
    // public const string EncryptionKeySourceName = "encryptionKeySource";
    // public const string ExportPolicyRulesName = "exportPolicyRules";
    // public const string ExternalHostNameName = "externalHostName";
    // public const string ExternalServerNameName = "externalServerName";
    // public const string ExternalVolumeNameName = "externalVolumeName";
    // public const string HasRootAccessName = "hasRootAccess";
    // public const string IsLargeVolumeName = "isLargeVolume";
    // public const string KerberosEnabledName = "kerberosEnabled";
    // public const string Kerberos5RName = "kerberos5R";
    // public const string Kerberos5RwName = "kerberos5Rw";
    // public const string Kerberos5IRName = "kerberos5IR";
    // public const string Kerberos5IRwName = "kerberos5IRw";
    // public const string Kerberos5PRName = "kerberos5PR";
    // public const string Kerberos5PRwName = "kerberos5PRw";
    // public const string KeyVaultPrivateEndpointResourceIdName = "keyVaultPrivateEndpointResourceId";
    // public const string LdapEnabledName = "ldapEnabled";
    // public const string NetworkFeaturesName = "networkFeatures";
    // public const string PlacementRulesName = "placementRules";
    // public const string PolicyEnforcedName = "policyEnforced";
    // public const string ProximityPlacementGroupName = "proximityPlacementGroup";
    // public const string RelocationRequestedName = "relocationRequested";
    // public const string RemoteVolumeResourceIdName = "remoteVolumeResourceId";
    // public const string RemoteVolumeRegionName = "remoteVolumeRegion";
    // public const string ReplicationScheduleName = "replicationSchedule";
    // public const string RuleIndexName = "ruleIndex";
    // public const string SecurityStyleName = "securityStyle";
    // public const string SmbAccessEnumerationName = "smbAccessEnumeration";
    // public const string SmbContinuouslyAvailableName = "smbContinuouslyAvailable";
    // public const string SmbEncryptionName = "smbEncryption";
    // public const string SmbNonBrowsableName = "smbNonBrowsable";
    // public const string SnapshotDirectoryVisibleName = "snapshotDirectoryVisible";
    // public const string SnapshotIdName = "snapshotId";
    // public const string SnapshotPolicyIdName = "snapshotPolicyId";
    // public const string ThroughputMibpsName = "throughputMibps";
    // public const string UnixPermissionsName = "unixPermissions";
    // public const string UnixReadOnlyName = "unixReadOnly";
    // public const string UnixReadWriteName = "unixReadWrite";
    // public const string VolumeSpecNameName = "volumeSpecName";
    // public const string VolumeTypeName = "volumeType";
    // public const string ZonesName = "zones";
    // public const string DailyBackupsToKeepName = "dailyBackupsToKeep";
    // public const string WeeklyBackupsToKeepName = "weeklyBackupsToKeep";
    // public const string MonthlyBackupsToKeepName = "monthlyBackupsToKeep";
    // public const string EnabledName = "enabled";
    // public const string VolumeResourceIdName = "volumeResourceId";
    // public const string LabelName = "label";
    // public const string SizeName = "size";
    // public const string QosTypeName = "qosType";
    // public const string CoolAccessName = "coolAccess";
    // public const string EncryptionTypeName = "encryptionType";
    // public const string HourlyScheduleMinuteName = "hourlyScheduleMinute";
    // public const string HourlyScheduleSnapshotsToKeepName = "hourlyScheduleSnapshotsToKeep";
    // public const string DailyScheduleHourName = "dailyScheduleHour";
    // public const string DailyScheduleMinuteName = "dailyScheduleMinute";
    // public const string DailyScheduleSnapshotsToKeepName = "dailyScheduleSnapshotsToKeep";
    // public const string WeeklyScheduleDayName = "weeklyScheduleDay";
    // public const string WeeklyScheduleHourName = "weeklyScheduleHour";
    // public const string WeeklyScheduleMinuteName = "weeklyScheduleMinute";
    // public const string WeeklyScheduleSnapshotsToKeepName = "weeklyScheduleSnapshotsToKeep";
    // public const string MonthlyScheduleDaysOfMonthName = "monthlyScheduleDaysOfMonth";
    // public const string MonthlyScheduleHourName = "monthlyScheduleHour";
    // public const string MonthlyScheduleMinuteName = "monthlyScheduleMinute";
    // public const string MonthlyScheduleSnapshotsToKeepName = "monthlyScheduleSnapshotsToKeep";
    // public const string ApplicationTypeName = "applicationType";
    // public const string ApplicationIdentifierName = "applicationIdentifier";
    // public const string GroupDescriptionName = "groupDescription";
    // public const string TagsName = "tags";
    // public const string IdsName = "ids";
    // public const string KeyNameName = "keyName";
    // public const string KeySourceName = "keySource";
    // public const string KeyVaultResourceIdName = "keyVaultResourceId";
    // public const string KeyVaultUriName = "keyVaultUri";
    // public const string FederatedClientIdName = "federatedClientId";
    // public const string UserAssignedIdentityName = "userAssignedIdentity";
    // public const string IdentityTypeName = "identityType";
    // public const string UserAssignedIdentitiesName = "userAssignedIdentities";
    // public const string ActiveDirectoriesName = "activeDirectories";
    // public const string NfsV4IdDomainName = "nfsV4IdDomain";
    // public const string NoWaitName = "no-wait";
    // public const string AddName = "add";
    // public const string SetName = "set";
    // public const string RemoveName = "remove";
    // public const string ForceStringName = "force-string";
    // public const string SizeInBytesName = "sizeInBytes";
    // public const string CustomThroughputMibpsName = "customThroughputMibps";
    // public const string AcquirePolicyTokenName = "acquirePolicyToken";
    // public const string ChangeReferenceName = "changeReference";
    // public const string ExcludeName = "exclude";
    // public const string PeerIpAddressesName = "peerIpAddresses";
    // public const string SourceVolumeIdName = "sourceVolumeId";
    // public const string ForceBreakReplicationName = "force";
    // public const string BackupNfsv3Name = "backup-nfsv3";
    // public const string DataBackupReplSkdName = "data-backup-repl-skd";
    // public const string DataBackupSizeName = "data-backup-size";
    // public const string DataBackupSrcIdName = "data-backup-src-id";
    // public const string DataBackupThroughputName = "data-backup-throughput";
    // public const string DataReplSkdName = "data-repl-skd";
    // public const string DataSizeName = "data-size";
    // public const string DataSrcIdName = "data-src-id";
    // public const string DataThroughputName = "data-throughput";
    // public const string GpRulesName = "gp-rules";
    // public const string LogBackupSizeName = "log-backup-size";
    // public const string LogBackupSrcIdName = "log-backup-src-id";
    // public const string LogBackupThroughputName = "log-backup-throughput";
    // public const string LogBackupReplSkdName = "log-backup-repl-skd";
    // public const string LogSizeName = "log-size";
    // public const string LogThroughputName = "log-throughput";
    // public const string BinarySizeName = "binary-size";
    // public const string BinaryThroughputName = "binary-throughput";
    // public const string LogMirrorSizeName = "log-mirror-size";
    // public const string LogMirrorThroughputName = "log-mirror-throughput";
    // public const string VolumesName = "volumes";
    // public const string SharedReplSkdName = "shared-repl-skd";
    // public const string SharedSizeName = "shared-size";
    // public const string SharedSrcIdName = "shared-src-id";
    // public const string SharedThroughputName = "shared-throughput";
    // public const string DatabaseSizeName = "database-size";
    // public const string DatabaseThroughputName = "database-throughput";
    // public const string NumberOfVolumesName = "number-of-volumes";
    // public const string MemoryName = "memory";
    // public const string NumberOfHostsName = "number-of-hosts";
    // public const string AddSnapshotCapacityName = "add-snapshot-capacity";
    // public const string PrefixName = "prefix";
    // public const string SmbAccessName = "smb-access";
    // public const string SmbBrowsableName = "smb-browsable";
    // public const string StartHostIdName = "start-host-id";
    // public const string SystemRoleName = "system-role";
    // public const string GroupMetaDataName = "group-meta-data";

    internal const string Account = "The name of the Azure NetApp Files account (e.g., 'myanfaccount').";

    internal const string Pool = "The name of the capacity pool (e.g., 'mypool').";

    internal const string Volume = "The name of the volume (e.g., 'myvolume').";

    internal const string Backup = "The name of the backup (e.g., 'mybackup').";

    internal const string BackupPolicy = "The name of the backup policy (e.g., 'mybackuppolicy').";

    internal const string BackupVault = "The name of the backup vault (e.g., 'mybackupvault').";

    internal const string Snapshot = "The name of the snapshot (e.g., 'mysnapshot').";

    internal const string SnapshotPolicy = "The name of the snapshot policy (e.g., 'mysnapshotpolicy').";

    internal const string VolumeGroup = "The name of the volume group (e.g., 'myvolumegroup').";

    internal const string Location = "The Azure region where the volume will be created (e.g., 'eastus', 'westus2').";

    internal const string SubnetId = "The Azure Resource Manager resource identifier of the delegated subnet (e.g., '/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}').";

    internal const string CreationToken = "A unique file path for the volume. Used when creating mount targets (e.g., 'myvolume').";

    internal const string UsageThreshold = "Maximum storage quota allowed for a file system in bytes. Minimum 107374182400 bytes (100 GiB).";

    internal const string ServiceLevel = "The service level of the volume. Valid values: Standard, Premium, Ultra.";

    internal const string ProtocolTypes = "The protocol types for the volume. Valid values: NFSv3, NFSv4.1, CIFS.";

    internal const string Subnet = "Delegated subnet name used with --vnet when subnetId is not supplied.";

    internal const string Vnet = "Virtual network name or ARM resource ID used with --subnet when subnetId is not supplied.";

    internal const string AcceptGrowCapacityPoolForShortTermCloneSplit = "Accept or decline automatic parent pool grow for short-term clone split. Values: Accepted, Declined.";

    internal const string AllowedClients = "Client ingress specification (backward-compatible export policy option).";

    internal const string AvsDataStore = "Specifies whether Azure VMware Solution datastore purpose is enabled. Values: Disabled, Enabled.";

    internal const string BackupId = "Resource identifier of the backup to create the volume from.";

    internal const string BackupPolicyId = "Backup policy resource ID.";

    internal const string BackupVaultId = "Backup vault resource ID.";

    internal const string CoolAccessRetrievalPolicy = "Cool access retrieval policy. Values: Default, Never, OnRead.";

    internal const string CoolAccessTieringPolicy = "Cool access tiering policy. Values: Auto, SnapshotOnly.";

    internal const string CapacityPoolResourceId = "Capacity pool resource ID, used for volume group scenarios.";

    internal const string ChownMode = "Who can change file ownership. Values: Restricted, Unrestricted.";

    internal const string Cifs = "Backward-compatible CIFS export policy setting.";

    internal const string CoolAccessVolume = "Specifies whether cool access tiering is enabled for the volume.";

    internal const string CoolnessPeriod = "Number of days after which cold blocks are tiered.";

    internal const string DeleteBaseSnapshot = "If true, delete the base snapshot after clone volume creation.";

    internal const string DesiredArpState = "Desired Advanced Ransomware Protection state. Values: Disabled, Enabled.";

    internal const string EnableSubvolumes = "Enable or disable subvolume operations. Values: Disabled, Enabled.";

    internal const string EncryptionKeySource = "Source of key used to encrypt volume data. Values: Microsoft.NetApp, Microsoft.KeyVault.";

    internal const string ExportPolicyRules = "Export policy rules JSON payload.";

    internal const string ExternalHostName = "External ONTAP host name for migration scenarios.";

    internal const string ExternalServerName = "External ONTAP server name for migration scenarios.";

    internal const string ExternalVolumeName = "External ONTAP volume name for migration scenarios.";

    internal const string HasRootAccess = "Backward-compatible export policy has-root-access flag.";

    internal const string IsLargeVolume = "Whether the volume is a large volume.";

    internal const string KerberosEnabled = "Whether Kerberos is enabled.";

    internal const string Kerberos5R = "Backward-compatible Kerberos5 read-only flag.";

    internal const string Kerberos5Rw = "Backward-compatible Kerberos5 read-write flag.";

    internal const string Kerberos5IR = "Backward-compatible Kerberos5i read-only flag.";

    internal const string Kerberos5IRw = "Backward-compatible Kerberos5i read-write flag.";

    internal const string Kerberos5PR = "Backward-compatible Kerberos5p read-only flag.";

    internal const string Kerberos5PRw = "Backward-compatible Kerberos5p read-write flag.";

    internal const string KeyVaultPrivateEndpointResourceId = "Resource ID of Key Vault private endpoint used for CMK volumes.";

    internal const string LdapEnabled = "Whether LDAP is enabled for NFS volumes.";

    internal const string NetworkFeatures = "Network features of the volume. Values: Basic, Standard.";

    internal const string PlacementRules = "Application-specific placement rules JSON payload.";

    internal const string PolicyEnforced = "Whether backup policy enforcement is enabled.";

    internal const string ProximityPlacementGroup = "Proximity placement group associated with the volume.";

    internal const string RelocationRequested = "Whether relocation is requested for the volume.";

    internal const string RemoteVolumeResourceId = "Resource ID of the remote volume for replication.";

    internal const string RemoteVolumeRegion = "Remote region for the other end of replication.";

    internal const string ReplicationSchedule = "Replication schedule. Values: _10minutely, hourly, daily.";

    internal const string RuleIndex = "Backward-compatible export policy rule order index.";

    internal const string SecurityStyle = "Security style of the volume. Values: unix, ntfs.";

    internal const string SmbAccessEnumeration = "SMB access-based enumeration setting. Values: Disabled, Enabled.";

    internal const string SmbContinuouslyAvailable = "Whether SMB continuously available shares are enabled.";

    internal const string SmbEncryption = "Whether SMB in-flight encryption is enabled.";

    internal const string SmbNonBrowsable = "SMB non-browsable setting. Values: Disabled, Enabled.";

    internal const string SnapshotDirectoryVisible = "Whether .snapshot directory is visible on the volume.";

    internal const string SnapshotId = "Resource identifier of the snapshot to create the volume from.";

    internal const string SnapshotPolicyId = "Snapshot policy resource ID.";

    internal const string ThroughputMibps = "Throughput in MiB/s for manual QoS volumes.";

    internal const string UnixPermissions = "UNIX permissions in 4-digit octal format, for example 0755.";

    internal const string UnixReadOnly = "Backward-compatible UNIX read-only export policy flag.";

    internal const string UnixReadWrite = "Backward-compatible UNIX read-write export policy flag.";

    internal const string VolumeSpecName = "Application-specific volume spec name in a volume group.";

    internal const string VolumeType = "Volume type, for example DataProtection or ShortTermClone.";

    internal const string Zones = "Availability zone list.";

    internal const string DailyBackupsToKeep = "The number of daily backups to keep (e.g., 2).";

    internal const string WeeklyBackupsToKeep = "The number of weekly backups to keep (e.g., 1).";

    internal const string MonthlyBackupsToKeep = "The number of monthly backups to keep (e.g., 1).";

    internal const string Enabled = "Whether the backup policy is enabled.";

    internal const string VolumeResourceId = "The Azure resource ID of the volume to back up (e.g., '/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}/volumes/{volume}').";

    internal const string Label = "A label for the backup (e.g., 'daily-backup').";

    internal const string Size = "Provisioned size of the pool in bytes. Must be a multiple of 4398046511104 (4 TiB). Minimum 4398046511104 bytes (4 TiB).";

    internal const string SizeInBytes = "Provisioned size of the pool in bytes. Allowed values are in 1 TiB chunks (value must be a multiple of 1099511627776). Use either --size or --size-in-bytes, not both.";

    internal const string CustomThroughputMibps = "Maximum throughput in MiB/s for manual QoS pools with Flexible service level.";

    internal const string QosType = "The QoS type of the pool. Valid values: Auto, Manual.";

    internal const string CoolAccess = "Whether cool access (tiering) is enabled for volumes in the pool.";

    internal const string EncryptionType = "The encryption type of the pool. Valid values: Single, Double.";

    internal const string HourlyScheduleMinute = "The minute of the hour for the hourly snapshot schedule (0-59).";

    internal const string HourlyScheduleSnapshotsToKeep = "The number of hourly snapshots to keep (e.g., 5).";

    internal const string DailyScheduleHour = "The hour of the day for the daily snapshot schedule (0-23).";

    internal const string DailyScheduleMinute = "The minute of the hour for the daily snapshot schedule (0-59).";

    internal const string DailyScheduleSnapshotsToKeep = "The number of daily snapshots to keep (e.g., 5).";

    internal const string WeeklyScheduleDay = "The day of the week for the weekly snapshot schedule (e.g., 'Monday').";

    internal const string WeeklyScheduleHour = "The hour of the day for the weekly snapshot schedule (0-23).";

    internal const string WeeklyScheduleMinute = "The minute of the hour for the weekly snapshot schedule (0-59).";

    internal const string WeeklyScheduleSnapshotsToKeep = "The number of weekly snapshots to keep (e.g., 4).";

    internal const string MonthlyScheduleDaysOfMonth = "The days of the month for the monthly snapshot schedule (e.g., '1,15').";

    internal const string MonthlyScheduleHour = "The hour of the day for the monthly snapshot schedule (0-23).";

    internal const string MonthlyScheduleMinute = "The minute of the hour for the monthly snapshot schedule (0-59).";

    internal const string MonthlyScheduleSnapshotsToKeep = "The number of monthly snapshots to keep (e.g., 2).";

    internal const string ApplicationType = "The application type of the volume group (e.g., 'SAP-HANA').";

    internal const string ApplicationIdentifier = "The application specific identifier (e.g., 'SH1' for SAP HANA SID).";

    internal const string GroupDescription = "A description for the volume group (e.g., 'Volume group for SAP HANA').";

    internal const string Tags = "Tags for the account in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').";

    internal const string Ids = "One or more full Azure resource IDs for NetApp accounts.";

    internal const string KeyName = "The name of the Key Vault key used for account encryption.";

    internal const string KeySource = "The encryption key source. Valid values include Microsoft.NetApp and Microsoft.KeyVault.";

    internal const string KeyVaultResourceId = "The Azure resource ID of the Key Vault used for account encryption.";

    internal const string KeyVaultUri = "The URI of the Key Vault used for account encryption.";

    internal const string FederatedClientId = "Client ID of the multi-tenant AAD application used for cross-tenant Key Vault access.";

    internal const string UserAssignedIdentity = "The ARM resource ID of the user-assigned identity for Key Vault authentication.";

    internal const string IdentityType = "Managed identity type for the account. Valid values include None, SystemAssigned, and UserAssigned.";

    internal const string UserAssignedIdentities = "User-assigned identities in JSON format.";

    internal const string ActiveDirectories = "Active Directory settings in JSON format.";

    internal const string NfsV4IdDomain = "Domain for NFSv4 user ID mapping.";

    internal const string NoWait = "Do not wait for the long-running operation to finish.";

    internal const string Add = "Add an object to a list of objects by specifying a path and key-value pairs.";

    internal const string Set = "Update an object by specifying a property path and value.";

    internal const string Remove = "Remove a property or an element from a list.";

    internal const string ForceString = "Preserve string literals for generic update operations.";

    internal const string AcquirePolicyToken = "Acquire an Azure Policy token automatically for this resource operation.";

    internal const string ChangeReference = "Related change reference ID for this resource operation.";

    internal const string Exclude = "Exclude replication filter. Valid values: None, Deleted.";

    internal const string PeerIpAddresses = "A list of IC-LIF IP addresses that can be used to connect to the external ONTAP cluster.";

    internal const string SourceVolumeId = "The Azure resource ID of the source volume for the replication.";

    internal const string ForceBreakReplication = "Force break the replication when it is currently transferring.";

    internal const string BackupNfsv3 = "Enable NFSv3 backup behavior for volume group creation scenarios.";

    internal const string DataBackupReplSkd = "Replication schedule for data backup volume.";

    internal const string DataBackupSize = "Capacity in GiB for data backup volumes.";

    internal const string DataBackupSrcId = "Resource ID of the data backup source volume.";

    internal const string DataBackupThroughput = "Throughput in MiB/s for data backup volumes.";

    internal const string DataReplSkd = "Replication schedule for data volume.";

    internal const string DataSize = "Capacity in GiB for data volumes.";

    internal const string DataSrcId = "Resource ID of the data source volume.";

    internal const string DataThroughput = "Throughput in MiB/s for data volumes.";

    internal const string GpRules = "Application-specific placement rules for the volume group.";

    internal const string LogBackupSize = "Capacity in GiB for log backup volumes.";

    internal const string LogBackupSrcId = "Resource ID of the log backup source volume.";

    internal const string LogBackupThroughput = "Throughput in MiB/s for log backup volumes.";

    internal const string LogBackupReplSkd = "Replication schedule for log backup volume.";

    internal const string LogSize = "Capacity in GiB for log volumes.";

    internal const string LogThroughput = "Throughput in MiB/s for log volumes.";

    internal const string BinarySize = "Capacity in GiB for binary volume.";

    internal const string BinaryThroughput = "Throughput in MiB/s for binary volume.";

    internal const string LogMirrorSize = "Capacity in GiB for log mirror volume.";

    internal const string LogMirrorThroughput = "Throughput in MiB/s for log mirror volume.";

    internal const string Volumes = "List of volumes from group.";

    internal const string SharedReplSkd = "Replication schedule for shared volume.";

    internal const string SharedSize = "Capacity in GiB for shared volumes.";

    internal const string SharedSrcId = "Resource ID of the shared source volume.";

    internal const string SharedThroughput = "Throughput in MiB/s for shared volumes.";

    internal const string DatabaseSize = "Oracle database size in TiB.";

    internal const string DatabaseThroughput = "Oracle database throughput in MiB/s.";

    internal const string NumberOfVolumes = "Total number of Oracle data volumes.";

    internal const string Memory = "System memory in GiB for SAP HANA sizing.";

    internal const string NumberOfHosts = "Total number of hosts in SAP HANA deployment.";

    internal const string AddSnapshotCapacity = "Additional snapshot capacity as percentage of RAM.";

    internal const string Prefix = "Prefix text for generated volume names.";

    internal const string SmbAccess = "SMB access-based enumeration setting.";

    internal const string SmbBrowsable = "SMB browsable setting.";

    internal const string StartHostId = "Starting SAP HANA host ID.";

    internal const string SystemRole = "Role of the storage system (PRIMARY, HA, DR).";

    internal const string GroupMetaData = "Volume group details payload.";
}
