// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public class VolumeCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeName)]
    public string? Volume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SubnetIdName)]
    public string? SubnetId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CreationTokenName)]
    public string? CreationToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UsageThresholdName)]
    public long? UsageThreshold { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ServiceLevelName)]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ProtocolTypesName)]
    public string[]? ProtocolTypes { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SubnetName)]
    public string? Subnet { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VnetName)]
    public string? Vnet { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcceptGrowCapacityPoolForShortTermCloneSplitName)]
    public string? AcceptGrowCapacityPoolForShortTermCloneSplit { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AllowedClientsName)]
    public string? AllowedClients { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AvsDataStoreName)]
    public string? AvsDataStore { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupIdName)]
    public string? BackupId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupPolicyIdName)]
    public string? BackupPolicyId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.BackupVaultIdName)]
    public string? BackupVaultId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CoolAccessRetrievalPolicyName)]
    public string? CoolAccessRetrievalPolicy { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CoolAccessTieringPolicyName)]
    public string? CoolAccessTieringPolicy { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CapacityPoolResourceIdName)]
    public string? CapacityPoolResourceId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChownModeName)]
    public string? ChownMode { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CifsName)]
    public bool? Cifs { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CoolAccessVolumeName)]
    public bool? CoolAccessVolume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CoolnessPeriodName)]
    public int? CoolnessPeriod { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DeleteBaseSnapshotName)]
    public bool? DeleteBaseSnapshot { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.DesiredArpStateName)]
    public string? DesiredArpState { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.EnableSubvolumesName)]
    public string? EnableSubvolumes { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.EncryptionKeySourceName)]
    public string? EncryptionKeySource { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ExportPolicyRulesName)]
    public string? ExportPolicyRules { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ExternalHostNameName)]
    public string? ExternalHostName { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ExternalServerNameName)]
    public string? ExternalServerName { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ExternalVolumeNameName)]
    public string? ExternalVolumeName { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.HasRootAccessName)]
    public bool? HasRootAccess { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IsLargeVolumeName)]
    public bool? IsLargeVolume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KerberosEnabledName)]
    public bool? KerberosEnabled { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5RName)]
    public bool? Kerberos5R { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5RwName)]
    public bool? Kerberos5Rw { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5IRName)]
    public bool? Kerberos5IR { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5IRwName)]
    public bool? Kerberos5IRw { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5PRName)]
    public bool? Kerberos5PR { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.Kerberos5PRwName)]
    public bool? Kerberos5PRw { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceIdName)]
    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LdapEnabledName)]
    public bool? LdapEnabled { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NetworkFeaturesName)]
    public string? NetworkFeatures { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.PlacementRulesName)]
    public string? PlacementRules { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.PolicyEnforcedName)]
    public bool? PolicyEnforced { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ProximityPlacementGroupName)]
    public string? ProximityPlacementGroup { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RelocationRequestedName)]
    public bool? RelocationRequested { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoteVolumeResourceIdName)]
    public string? RemoteVolumeResourceId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoteVolumeRegionName)]
    public string? RemoteVolumeRegion { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ReplicationScheduleName)]
    public string? ReplicationSchedule { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RuleIndexName)]
    public int? RuleIndex { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SecurityStyleName)]
    public string? SecurityStyle { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SmbAccessEnumerationName)]
    public string? SmbAccessEnumeration { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SmbContinuouslyAvailableName)]
    public bool? SmbContinuouslyAvailable { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SmbEncryptionName)]
    public bool? SmbEncryption { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SmbNonBrowsableName)]
    public string? SmbNonBrowsable { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotDirectoryVisibleName)]
    public bool? SnapshotDirectoryVisible { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotIdName)]
    public string? SnapshotId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SnapshotPolicyIdName)]
    public string? SnapshotPolicyId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ThroughputMibpsName)]
    public int? ThroughputMibps { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UnixPermissionsName)]
    public string? UnixPermissions { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UnixReadOnlyName)]
    public bool? UnixReadOnly { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UnixReadWriteName)]
    public bool? UnixReadWrite { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeSpecNameName)]
    public string? VolumeSpecName { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeTypeName)]
    public string? VolumeType { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ZonesName)]
    public string[]? Zones { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AcquirePolicyTokenName)]
    public bool AcquirePolicyToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ChangeReferenceName)]
    public string? ChangeReference { get; set; }
}
