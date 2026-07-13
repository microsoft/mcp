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
    public const string SubnetName = "subnet";
    public const string VnetName = "vnet";
    public const string AcceptGrowCapacityPoolForShortTermCloneSplitName = "acceptGrowCapacityPoolForShortTermCloneSplit";
    public const string AllowedClientsName = "allowedClients";
    public const string AvsDataStoreName = "avsDataStore";
    public const string BackupIdName = "backupId";
    public const string BackupPolicyIdName = "backupPolicyId";
    public const string BackupVaultIdName = "backupVaultId";
    public const string CoolAccessRetrievalPolicyName = "coolAccessRetrievalPolicy";
    public const string CoolAccessTieringPolicyName = "coolAccessTieringPolicy";
    public const string CapacityPoolResourceIdName = "capacityPoolResourceId";
    public const string ChownModeName = "chownMode";
    public const string CifsName = "cifs";
    public const string CoolAccessVolumeName = "coolAccessVolume";
    public const string CoolnessPeriodName = "coolnessPeriod";
    public const string DeleteBaseSnapshotName = "deleteBaseSnapshot";
    public const string DesiredArpStateName = "desiredArpState";
    public const string EnableSubvolumesName = "enableSubvolumes";
    public const string EncryptionKeySourceName = "encryptionKeySource";
    public const string ExportPolicyRulesName = "exportPolicyRules";
    public const string ExternalHostNameName = "externalHostName";
    public const string ExternalServerNameName = "externalServerName";
    public const string ExternalVolumeNameName = "externalVolumeName";
    public const string HasRootAccessName = "hasRootAccess";
    public const string IsLargeVolumeName = "isLargeVolume";
    public const string KerberosEnabledName = "kerberosEnabled";
    public const string Kerberos5RName = "kerberos5R";
    public const string Kerberos5RwName = "kerberos5Rw";
    public const string Kerberos5IRName = "kerberos5IR";
    public const string Kerberos5IRwName = "kerberos5IRw";
    public const string Kerberos5PRName = "kerberos5PR";
    public const string Kerberos5PRwName = "kerberos5PRw";
    public const string KeyVaultPrivateEndpointResourceIdName = "keyVaultPrivateEndpointResourceId";
    public const string LdapEnabledName = "ldapEnabled";
    public const string NetworkFeaturesName = "networkFeatures";
    public const string PlacementRulesName = "placementRules";
    public const string PolicyEnforcedName = "policyEnforced";
    public const string ProximityPlacementGroupName = "proximityPlacementGroup";
    public const string RelocationRequestedName = "relocationRequested";
    public const string RemoteVolumeResourceIdName = "remoteVolumeResourceId";
    public const string RemoteVolumeRegionName = "remoteVolumeRegion";
    public const string ReplicationScheduleName = "replicationSchedule";
    public const string RuleIndexName = "ruleIndex";
    public const string SecurityStyleName = "securityStyle";
    public const string SmbAccessEnumerationName = "smbAccessEnumeration";
    public const string SmbContinuouslyAvailableName = "smbContinuouslyAvailable";
    public const string SmbEncryptionName = "smbEncryption";
    public const string SmbNonBrowsableName = "smbNonBrowsable";
    public const string SnapshotDirectoryVisibleName = "snapshotDirectoryVisible";
    public const string SnapshotIdName = "snapshotId";
    public const string SnapshotPolicyIdName = "snapshotPolicyId";
    public const string ThroughputMibpsName = "throughputMibps";
    public const string UnixPermissionsName = "unixPermissions";
    public const string UnixReadOnlyName = "unixReadOnly";
    public const string UnixReadWriteName = "unixReadWrite";
    public const string VolumeSpecNameName = "volumeSpecName";
    public const string VolumeTypeName = "volumeType";
    public const string ZonesName = "zones";
    public const string DailyBackupsToKeepName = "dailyBackupsToKeep";
    public const string WeeklyBackupsToKeepName = "weeklyBackupsToKeep";
    public const string MonthlyBackupsToKeepName = "monthlyBackupsToKeep";
    public const string EnabledName = "enabled";
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
    public const string WeeklyScheduleHourName = "weeklyScheduleHour";
    public const string WeeklyScheduleMinuteName = "weeklyScheduleMinute";
    public const string WeeklyScheduleSnapshotsToKeepName = "weeklyScheduleSnapshotsToKeep";
    public const string MonthlyScheduleDaysOfMonthName = "monthlyScheduleDaysOfMonth";
    public const string MonthlyScheduleHourName = "monthlyScheduleHour";
    public const string MonthlyScheduleMinuteName = "monthlyScheduleMinute";
    public const string MonthlyScheduleSnapshotsToKeepName = "monthlyScheduleSnapshotsToKeep";
    public const string ApplicationTypeName = "applicationType";
    public const string ApplicationIdentifierName = "applicationIdentifier";
    public const string GroupDescriptionName = "groupDescription";
    public const string TagsName = "tags";
    public const string IdsName = "ids";
    public const string KeyNameName = "keyName";
    public const string KeySourceName = "keySource";
    public const string KeyVaultResourceIdName = "keyVaultResourceId";
    public const string KeyVaultUriName = "keyVaultUri";
    public const string FederatedClientIdName = "federatedClientId";
    public const string UserAssignedIdentityName = "userAssignedIdentity";
    public const string IdentityTypeName = "identityType";
    public const string UserAssignedIdentitiesName = "userAssignedIdentities";
    public const string ActiveDirectoriesName = "activeDirectories";
    public const string NfsV4IdDomainName = "nfsV4IdDomain";
    public const string NoWaitName = "no-wait";
    public const string AddName = "add";
    public const string SetName = "set";
    public const string RemoveName = "remove";
    public const string ForceStringName = "force-string";
    public const string SizeInBytesName = "sizeInBytes";
    public const string CustomThroughputMibpsName = "customThroughputMibps";
    public const string AcquirePolicyTokenName = "acquirePolicyToken";
    public const string ChangeReferenceName = "changeReference";
    public const string ExcludeName = "exclude";
    public const string PeerIpAddressesName = "peerIpAddresses";
    public const string SourceVolumeIdName = "sourceVolumeId";
    public const string ForceBreakReplicationName = "force";
    public const string BackupNfsv3Name = "backup-nfsv3";
    public const string DataBackupReplSkdName = "data-backup-repl-skd";
    public const string DataBackupSizeName = "data-backup-size";
    public const string DataBackupSrcIdName = "data-backup-src-id";
    public const string DataBackupThroughputName = "data-backup-throughput";
    public const string DataReplSkdName = "data-repl-skd";
    public const string DataSizeName = "data-size";
    public const string DataSrcIdName = "data-src-id";
    public const string DataThroughputName = "data-throughput";
    public const string GpRulesName = "gp-rules";
    public const string LogBackupSizeName = "log-backup-size";
    public const string LogBackupSrcIdName = "log-backup-src-id";
    public const string LogBackupThroughputName = "log-backup-throughput";
    public const string LogBackupReplSkdName = "log-backup-repl-skd";
    public const string LogSizeName = "log-size";
    public const string LogThroughputName = "log-throughput";
    public const string BinarySizeName = "binary-size";
    public const string BinaryThroughputName = "binary-throughput";
    public const string LogMirrorSizeName = "log-mirror-size";
    public const string LogMirrorThroughputName = "log-mirror-throughput";
    public const string VolumesName = "volumes";
    public const string SharedReplSkdName = "shared-repl-skd";
    public const string SharedSizeName = "shared-size";
    public const string SharedSrcIdName = "shared-src-id";
    public const string SharedThroughputName = "shared-throughput";
    public const string DatabaseSizeName = "database-size";
    public const string DatabaseThroughputName = "database-throughput";
    public const string NumberOfVolumesName = "number-of-volumes";
    public const string MemoryName = "memory";
    public const string NumberOfHostsName = "number-of-hosts";
    public const string AddSnapshotCapacityName = "add-snapshot-capacity";
    public const string PrefixName = "prefix";
    public const string SmbAccessName = "smb-access";
    public const string SmbBrowsableName = "smb-browsable";
    public const string StartHostIdName = "start-host-id";
    public const string SystemRoleName = "system-role";
    public const string GroupMetaDataName = "group-meta-data";

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

    public static readonly Option<string> Subnet = new($"--{SubnetName}")
    {
        Description = "Delegated subnet name used with --vnet when subnetId is not supplied.",
        Required = false
    };

    public static readonly Option<string> Vnet = new($"--{VnetName}")
    {
        Description = "Virtual network name or ARM resource ID used with --subnet when subnetId is not supplied.",
        Required = false
    };

    public static readonly Option<string> AcceptGrowCapacityPoolForShortTermCloneSplit = new($"--{AcceptGrowCapacityPoolForShortTermCloneSplitName}")
    {
        Description = "Accept or decline automatic parent pool grow for short-term clone split. Values: Accepted, Declined.",
        Required = false
    };

    public static readonly Option<string> AllowedClients = new($"--{AllowedClientsName}")
    {
        Description = "Client ingress specification (backward-compatible export policy option).",
        Required = false
    };

    public static readonly Option<string> AvsDataStore = new($"--{AvsDataStoreName}")
    {
        Description = "Specifies whether Azure VMware Solution datastore purpose is enabled. Values: Disabled, Enabled.",
        Required = false
    };

    public static readonly Option<string> BackupId = new($"--{BackupIdName}")
    {
        Description = "Resource identifier of the backup to create the volume from.",
        Required = false
    };

    public static readonly Option<string> BackupPolicyId = new($"--{BackupPolicyIdName}")
    {
        Description = "Backup policy resource ID.",
        Required = false
    };

    public static readonly Option<string> BackupVaultId = new($"--{BackupVaultIdName}")
    {
        Description = "Backup vault resource ID.",
        Required = false
    };

    public static readonly Option<string> CoolAccessRetrievalPolicy = new($"--{CoolAccessRetrievalPolicyName}")
    {
        Description = "Cool access retrieval policy. Values: Default, Never, OnRead.",
        Required = false
    };

    public static readonly Option<string> CoolAccessTieringPolicy = new($"--{CoolAccessTieringPolicyName}")
    {
        Description = "Cool access tiering policy. Values: Auto, SnapshotOnly.",
        Required = false
    };

    public static readonly Option<string> CapacityPoolResourceId = new($"--{CapacityPoolResourceIdName}")
    {
        Description = "Capacity pool resource ID, used for volume group scenarios.",
        Required = false
    };

    public static readonly Option<string> ChownMode = new($"--{ChownModeName}")
    {
        Description = "Who can change file ownership. Values: Restricted, Unrestricted.",
        Required = false
    };

    public static readonly Option<bool?> Cifs = new($"--{CifsName}")
    {
        Description = "Backward-compatible CIFS export policy setting.",
        Required = false
    };

    public static readonly Option<bool?> CoolAccessVolume = new($"--{CoolAccessVolumeName}")
    {
        Description = "Specifies whether cool access tiering is enabled for the volume.",
        Required = false
    };

    public static readonly Option<int?> CoolnessPeriod = new($"--{CoolnessPeriodName}")
    {
        Description = "Number of days after which cold blocks are tiered.",
        Required = false
    };

    public static readonly Option<bool?> DeleteBaseSnapshot = new($"--{DeleteBaseSnapshotName}")
    {
        Description = "If true, delete the base snapshot after clone volume creation.",
        Required = false
    };

    public static readonly Option<string> DesiredArpState = new($"--{DesiredArpStateName}")
    {
        Description = "Desired Advanced Ransomware Protection state. Values: Disabled, Enabled.",
        Required = false
    };

    public static readonly Option<string> EnableSubvolumes = new($"--{EnableSubvolumesName}")
    {
        Description = "Enable or disable subvolume operations. Values: Disabled, Enabled.",
        Required = false
    };

    public static readonly Option<string> EncryptionKeySource = new($"--{EncryptionKeySourceName}")
    {
        Description = "Source of key used to encrypt volume data. Values: Microsoft.NetApp, Microsoft.KeyVault.",
        Required = false
    };

    public static readonly Option<string> ExportPolicyRules = new($"--{ExportPolicyRulesName}")
    {
        Description = "Export policy rules JSON payload.",
        Required = false
    };

    public static readonly Option<string> ExternalHostName = new($"--{ExternalHostNameName}")
    {
        Description = "External ONTAP host name for migration scenarios.",
        Required = false
    };

    public static readonly Option<string> ExternalServerName = new($"--{ExternalServerNameName}")
    {
        Description = "External ONTAP server name for migration scenarios.",
        Required = false
    };

    public static readonly Option<string> ExternalVolumeName = new($"--{ExternalVolumeNameName}")
    {
        Description = "External ONTAP volume name for migration scenarios.",
        Required = false
    };

    public static readonly Option<bool?> HasRootAccess = new($"--{HasRootAccessName}")
    {
        Description = "Backward-compatible export policy has-root-access flag.",
        Required = false
    };

    public static readonly Option<bool?> IsLargeVolume = new($"--{IsLargeVolumeName}")
    {
        Description = "Whether the volume is a large volume.",
        Required = false
    };

    public static readonly Option<bool?> KerberosEnabled = new($"--{KerberosEnabledName}")
    {
        Description = "Whether Kerberos is enabled.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5R = new($"--{Kerberos5RName}")
    {
        Description = "Backward-compatible Kerberos5 read-only flag.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5Rw = new($"--{Kerberos5RwName}")
    {
        Description = "Backward-compatible Kerberos5 read-write flag.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5IR = new($"--{Kerberos5IRName}")
    {
        Description = "Backward-compatible Kerberos5i read-only flag.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5IRw = new($"--{Kerberos5IRwName}")
    {
        Description = "Backward-compatible Kerberos5i read-write flag.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5PR = new($"--{Kerberos5PRName}")
    {
        Description = "Backward-compatible Kerberos5p read-only flag.",
        Required = false
    };

    public static readonly Option<bool?> Kerberos5PRw = new($"--{Kerberos5PRwName}")
    {
        Description = "Backward-compatible Kerberos5p read-write flag.",
        Required = false
    };

    public static readonly Option<string> KeyVaultPrivateEndpointResourceId = new($"--{KeyVaultPrivateEndpointResourceIdName}")
    {
        Description = "Resource ID of Key Vault private endpoint used for CMK volumes.",
        Required = false
    };

    public static readonly Option<bool?> LdapEnabled = new($"--{LdapEnabledName}")
    {
        Description = "Whether LDAP is enabled for NFS volumes.",
        Required = false
    };

    public static readonly Option<string> NetworkFeatures = new($"--{NetworkFeaturesName}")
    {
        Description = "Network features of the volume. Values: Basic, Standard.",
        Required = false
    };

    public static readonly Option<string> PlacementRules = new($"--{PlacementRulesName}")
    {
        Description = "Application-specific placement rules JSON payload.",
        Required = false
    };

    public static readonly Option<bool?> PolicyEnforced = new($"--{PolicyEnforcedName}")
    {
        Description = "Whether backup policy enforcement is enabled.",
        Required = false
    };

    public static readonly Option<string> ProximityPlacementGroup = new($"--{ProximityPlacementGroupName}")
    {
        Description = "Proximity placement group associated with the volume.",
        Required = false
    };

    public static readonly Option<bool?> RelocationRequested = new($"--{RelocationRequestedName}")
    {
        Description = "Whether relocation is requested for the volume.",
        Required = false
    };

    public static readonly Option<string> RemoteVolumeResourceId = new($"--{RemoteVolumeResourceIdName}")
    {
        Description = "Resource ID of the remote volume for replication.",
        Required = false
    };

    public static readonly Option<string> RemoteVolumeRegion = new($"--{RemoteVolumeRegionName}")
    {
        Description = "Remote region for the other end of replication.",
        Required = false
    };

    public static readonly Option<string> ReplicationSchedule = new($"--{ReplicationScheduleName}")
    {
        Description = "Replication schedule. Values: _10minutely, hourly, daily.",
        Required = false
    };

    public static readonly Option<int?> RuleIndex = new($"--{RuleIndexName}")
    {
        Description = "Backward-compatible export policy rule order index.",
        Required = false
    };

    public static readonly Option<string> SecurityStyle = new($"--{SecurityStyleName}")
    {
        Description = "Security style of the volume. Values: unix, ntfs.",
        Required = false
    };

    public static readonly Option<string> SmbAccessEnumeration = new($"--{SmbAccessEnumerationName}")
    {
        Description = "SMB access-based enumeration setting. Values: Disabled, Enabled.",
        Required = false
    };

    public static readonly Option<bool?> SmbContinuouslyAvailable = new($"--{SmbContinuouslyAvailableName}")
    {
        Description = "Whether SMB continuously available shares are enabled.",
        Required = false
    };

    public static readonly Option<bool?> SmbEncryption = new($"--{SmbEncryptionName}")
    {
        Description = "Whether SMB in-flight encryption is enabled.",
        Required = false
    };

    public static readonly Option<string> SmbNonBrowsable = new($"--{SmbNonBrowsableName}")
    {
        Description = "SMB non-browsable setting. Values: Disabled, Enabled.",
        Required = false
    };

    public static readonly Option<bool?> SnapshotDirectoryVisible = new($"--{SnapshotDirectoryVisibleName}")
    {
        Description = "Whether .snapshot directory is visible on the volume.",
        Required = false
    };

    public static readonly Option<string> SnapshotId = new($"--{SnapshotIdName}")
    {
        Description = "Resource identifier of the snapshot to create the volume from.",
        Required = false
    };

    public static readonly Option<string> SnapshotPolicyId = new($"--{SnapshotPolicyIdName}")
    {
        Description = "Snapshot policy resource ID.",
        Required = false
    };

    public static readonly Option<int?> ThroughputMibps = new($"--{ThroughputMibpsName}")
    {
        Description = "Throughput in MiB/s for manual QoS volumes.",
        Required = false
    };

    public static readonly Option<string> UnixPermissions = new($"--{UnixPermissionsName}")
    {
        Description = "UNIX permissions in 4-digit octal format, for example 0755.",
        Required = false
    };

    public static readonly Option<bool?> UnixReadOnly = new($"--{UnixReadOnlyName}")
    {
        Description = "Backward-compatible UNIX read-only export policy flag.",
        Required = false
    };

    public static readonly Option<bool?> UnixReadWrite = new($"--{UnixReadWriteName}")
    {
        Description = "Backward-compatible UNIX read-write export policy flag.",
        Required = false
    };

    public static readonly Option<string> VolumeSpecName = new($"--{VolumeSpecNameName}")
    {
        Description = "Application-specific volume spec name in a volume group.",
        Required = false
    };

    public static readonly Option<string> VolumeType = new($"--{VolumeTypeName}")
    {
        Description = "Volume type, for example DataProtection or ShortTermClone.",
        Required = false
    };

    public static readonly Option<string[]> Zones = new($"--{ZonesName}")
    {
        Description = "Availability zone list.",
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

    public static readonly Option<bool?> Enabled = new($"--{EnabledName}")
    {
        Description = "Whether the backup policy is enabled.",
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

    public static readonly Option<long?> SizeInBytes = new($"--{SizeInBytesName}")
    {
        Description = "Provisioned size of the pool in bytes. Allowed values are in 1 TiB chunks (value must be a multiple of 1099511627776). Use either --size or --sizeInBytes, not both.",
        Required = false
    };

    public static readonly Option<long?> CustomThroughputMibps = new($"--{CustomThroughputMibpsName}")
    {
        Description = "Maximum throughput in MiB/s for manual QoS pools with Flexible service level.",
        Required = false
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

    public static readonly Option<int?> WeeklyScheduleHour = new($"--{WeeklyScheduleHourName}")
    {
        Description = "The hour of the day for the weekly snapshot schedule (0-23).",
        Required = false
    };

    public static readonly Option<int?> WeeklyScheduleMinute = new($"--{WeeklyScheduleMinuteName}")
    {
        Description = "The minute of the hour for the weekly snapshot schedule (0-59).",
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

    public static readonly Option<int?> MonthlyScheduleHour = new($"--{MonthlyScheduleHourName}")
    {
        Description = "The hour of the day for the monthly snapshot schedule (0-23).",
        Required = false
    };

    public static readonly Option<int?> MonthlyScheduleMinute = new($"--{MonthlyScheduleMinuteName}")
    {
        Description = "The minute of the hour for the monthly snapshot schedule (0-59).",
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

    public static readonly Option<string[]> Ids = new($"--{IdsName}")
    {
        Description = "One or more full Azure resource IDs for NetApp accounts.",
        Required = false
    };

    public static readonly Option<string> KeyName = new($"--{KeyNameName}")
    {
        Description = "The name of the Key Vault key used for account encryption.",
        Required = false
    };

    public static readonly Option<string> KeySource = new($"--{KeySourceName}")
    {
        Description = "The encryption key source. Valid values include Microsoft.NetApp and Microsoft.KeyVault.",
        Required = false
    };

    public static readonly Option<string> KeyVaultResourceId = new($"--{KeyVaultResourceIdName}")
    {
        Description = "The Azure resource ID of the Key Vault used for account encryption.",
        Required = false
    };

    public static readonly Option<string> KeyVaultUri = new($"--{KeyVaultUriName}")
    {
        Description = "The URI of the Key Vault used for account encryption.",
        Required = false
    };

    public static readonly Option<string> FederatedClientId = new($"--{FederatedClientIdName}")
    {
        Description = "Client ID of the multi-tenant AAD application used for cross-tenant Key Vault access.",
        Required = false
    };

    public static readonly Option<string> UserAssignedIdentity = new($"--{UserAssignedIdentityName}")
    {
        Description = "The ARM resource ID of the user-assigned identity for Key Vault authentication.",
        Required = false
    };

    public static readonly Option<string> IdentityType = new($"--{IdentityTypeName}")
    {
        Description = "Managed identity type for the account. Valid values include None, SystemAssigned, and UserAssigned.",
        Required = false
    };

    public static readonly Option<string> UserAssignedIdentities = new($"--{UserAssignedIdentitiesName}")
    {
        Description = "User-assigned identities in JSON format.",
        Required = false
    };

    public static readonly Option<string> ActiveDirectories = new($"--{ActiveDirectoriesName}")
    {
        Description = "Active Directory settings in JSON format.",
        Required = false
    };

    public static readonly Option<string> NfsV4IdDomain = new($"--{NfsV4IdDomainName}")
    {
        Description = "Domain for NFSv4 user ID mapping.",
        Required = false
    };

    public static readonly Option<bool> NoWait = new($"--{NoWaitName}")
    {
        Description = "Do not wait for the long-running operation to finish.",
        Required = false
    };

    public static readonly Option<string[]> Add = new($"--{AddName}")
    {
        Description = "Add an object to a list of objects by specifying a path and key-value pairs.",
        Required = false
    };

    public static readonly Option<string[]> Set = new($"--{SetName}")
    {
        Description = "Update an object by specifying a property path and value.",
        Required = false
    };

    public static readonly Option<string[]> Remove = new($"--{RemoveName}")
    {
        Description = "Remove a property or an element from a list.",
        Required = false
    };

    public static readonly Option<bool> ForceString = new($"--{ForceStringName}")
    {
        Description = "Preserve string literals for generic update operations.",
        Required = false
    };

    public static readonly Option<bool> AcquirePolicyToken = new($"--{AcquirePolicyTokenName}")
    {
        Description = "Acquire an Azure Policy token automatically for this resource operation.",
        Required = false
    };

    public static readonly Option<string> ChangeReference = new($"--{ChangeReferenceName}")
    {
        Description = "Related change reference ID for this resource operation.",
        Required = false
    };

    public static readonly Option<string> Exclude = new($"--{ExcludeName}")
    {
        Description = "Exclude replication filter. Valid values: None, Deleted.",
        Required = false
    };

    public static readonly Option<string[]> PeerIpAddresses = new($"--{PeerIpAddressesName}")
    {
        Description = "A list of IC-LIF IP addresses that can be used to connect to the external ONTAP cluster.",
        Required = false
    };

    public static readonly Option<string> SourceVolumeId = new($"--{SourceVolumeIdName}")
    {
        Description = "The Azure resource ID of the source volume for the replication.",
        Required = false
    };

    public static readonly Option<bool> ForceBreakReplication = new($"--{ForceBreakReplicationName}")
    {
        Description = "Force break the replication when it is currently transferring.",
        Required = false
    };

    public static readonly Option<bool> BackupNfsv3 = new($"--{BackupNfsv3Name}")
    {
        Description = "Enable NFSv3 backup behavior for volume group creation scenarios.",
        Required = false
    };

    public static readonly Option<string> DataBackupReplSkd = new($"--{DataBackupReplSkdName}")
    {
        Description = "Replication schedule for data backup volume.",
        Required = false
    };

    public static readonly Option<int?> DataBackupSize = new($"--{DataBackupSizeName}")
    {
        Description = "Capacity in GiB for data backup volumes.",
        Required = false
    };

    public static readonly Option<string> DataBackupSrcId = new($"--{DataBackupSrcIdName}")
    {
        Description = "Resource ID of the data backup source volume.",
        Required = false
    };

    public static readonly Option<int?> DataBackupThroughput = new($"--{DataBackupThroughputName}")
    {
        Description = "Throughput in MiB/s for data backup volumes.",
        Required = false
    };

    public static readonly Option<string> DataReplSkd = new($"--{DataReplSkdName}")
    {
        Description = "Replication schedule for data volume.",
        Required = false
    };

    public static readonly Option<int?> DataSize = new($"--{DataSizeName}")
    {
        Description = "Capacity in GiB for data volumes.",
        Required = false
    };

    public static readonly Option<string> DataSrcId = new($"--{DataSrcIdName}")
    {
        Description = "Resource ID of the data source volume.",
        Required = false
    };

    public static readonly Option<int?> DataThroughput = new($"--{DataThroughputName}")
    {
        Description = "Throughput in MiB/s for data volumes.",
        Required = false
    };

    public static readonly Option<string> GpRules = new($"--{GpRulesName}")
    {
        Description = "Application-specific placement rules for the volume group.",
        Required = false
    };

    public static readonly Option<int?> LogBackupSize = new($"--{LogBackupSizeName}")
    {
        Description = "Capacity in GiB for log backup volumes.",
        Required = false
    };

    public static readonly Option<string> LogBackupSrcId = new($"--{LogBackupSrcIdName}")
    {
        Description = "Resource ID of the log backup source volume.",
        Required = false
    };

    public static readonly Option<int?> LogBackupThroughput = new($"--{LogBackupThroughputName}")
    {
        Description = "Throughput in MiB/s for log backup volumes.",
        Required = false
    };

    public static readonly Option<string> LogBackupReplSkd = new($"--{LogBackupReplSkdName}")
    {
        Description = "Replication schedule for log backup volume.",
        Required = false
    };

    public static readonly Option<int?> LogSize = new($"--{LogSizeName}")
    {
        Description = "Capacity in GiB for log volumes.",
        Required = false
    };

    public static readonly Option<int?> LogThroughput = new($"--{LogThroughputName}")
    {
        Description = "Throughput in MiB/s for log volumes.",
        Required = false
    };

    public static readonly Option<int?> BinarySize = new($"--{BinarySizeName}")
    {
        Description = "Capacity in GiB for binary volume.",
        Required = false
    };

    public static readonly Option<int?> BinaryThroughput = new($"--{BinaryThroughputName}")
    {
        Description = "Throughput in MiB/s for binary volume.",
        Required = false
    };

    public static readonly Option<int?> LogMirrorSize = new($"--{LogMirrorSizeName}")
    {
        Description = "Capacity in GiB for log mirror volume.",
        Required = false
    };

    public static readonly Option<int?> LogMirrorThroughput = new($"--{LogMirrorThroughputName}")
    {
        Description = "Throughput in MiB/s for log mirror volume.",
        Required = false
    };

    public static readonly Option<string> Volumes = new($"--{VolumesName}")
    {
        Description = "List of volumes from group.",
        Required = false
    };

    public static readonly Option<string> SharedReplSkd = new($"--{SharedReplSkdName}")
    {
        Description = "Replication schedule for shared volume.",
        Required = false
    };

    public static readonly Option<int?> SharedSize = new($"--{SharedSizeName}")
    {
        Description = "Capacity in GiB for shared volumes.",
        Required = false
    };

    public static readonly Option<string> SharedSrcId = new($"--{SharedSrcIdName}")
    {
        Description = "Resource ID of the shared source volume.",
        Required = false
    };

    public static readonly Option<int?> SharedThroughput = new($"--{SharedThroughputName}")
    {
        Description = "Throughput in MiB/s for shared volumes.",
        Required = false
    };

    public static readonly Option<int?> DatabaseSize = new($"--{DatabaseSizeName}")
    {
        Description = "Oracle database size in TiB.",
        Required = false
    };

    public static readonly Option<int?> DatabaseThroughput = new($"--{DatabaseThroughputName}")
    {
        Description = "Oracle database throughput in MiB/s.",
        Required = false
    };

    public static readonly Option<int?> NumberOfVolumes = new($"--{NumberOfVolumesName}")
    {
        Description = "Total number of Oracle data volumes.",
        Required = false
    };

    public static readonly Option<int?> Memory = new($"--{MemoryName}")
    {
        Description = "System memory in GiB for SAP HANA sizing.",
        Required = false
    };

    public static readonly Option<int?> NumberOfHosts = new($"--{NumberOfHostsName}")
    {
        Description = "Total number of hosts in SAP HANA deployment.",
        Required = false
    };

    public static readonly Option<int?> AddSnapshotCapacity = new($"--{AddSnapshotCapacityName}")
    {
        Description = "Additional snapshot capacity as percentage of RAM.",
        Required = false
    };

    public static readonly Option<string> Prefix = new($"--{PrefixName}")
    {
        Description = "Prefix text for generated volume names.",
        Required = false
    };

    public static readonly Option<string> SmbAccess = new($"--{SmbAccessName}")
    {
        Description = "SMB access-based enumeration setting.",
        Required = false
    };

    public static readonly Option<string> SmbBrowsable = new($"--{SmbBrowsableName}")
    {
        Description = "SMB browsable setting.",
        Required = false
    };

    public static readonly Option<int?> StartHostId = new($"--{StartHostIdName}")
    {
        Description = "Starting SAP HANA host ID.",
        Required = false
    };

    public static readonly Option<string> SystemRole = new($"--{SystemRoleName}")
    {
        Description = "Role of the storage system (PRIMARY, HA, DR).",
        Required = false
    };

    public static readonly Option<string> GroupMetaData = new($"--{GroupMetaDataName}")
    {
        Description = "Volume group details payload.",
        Required = false
    };
}
