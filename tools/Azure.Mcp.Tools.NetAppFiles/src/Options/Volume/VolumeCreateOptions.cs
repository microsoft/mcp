// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public class VolumeCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public string? Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volume)]
    public string? Volume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SubnetId)]
    public string? SubnetId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CreationToken)]
    public string? CreationToken { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UsageThreshold)]
    public long? UsageThreshold { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ServiceLevel)]
    public string? ServiceLevel { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ProtocolTypes)]
    public string[]? ProtocolTypes { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Subnet)]
    public string? Subnet { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Vnet)]
    public string? Vnet { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AcceptGrowCapacityPoolForShortTermCloneSplit)]
    public string? AcceptGrowCapacityPoolForShortTermCloneSplit { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AllowedClients)]
    public string? AllowedClients { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AvsDataStore)]
    public string? AvsDataStore { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BackupId)]
    public string? BackupId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BackupPolicyId)]
    public string? BackupPolicyId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BackupVaultId)]
    public string? BackupVaultId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CoolAccessRetrievalPolicy)]
    public string? CoolAccessRetrievalPolicy { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CoolAccessTieringPolicy)]
    public string? CoolAccessTieringPolicy { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CapacityPoolResourceId)]
    public string? CapacityPoolResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ChownMode)]
    public string? ChownMode { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Cifs)]
    public bool? Cifs { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CoolAccessVolume)]
    public bool? CoolAccessVolume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.CoolnessPeriod)]
    public int? CoolnessPeriod { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DeleteBaseSnapshot)]
    public bool? DeleteBaseSnapshot { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DesiredArpState)]
    public string? DesiredArpState { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.EnableSubvolumes)]
    public string? EnableSubvolumes { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.EncryptionKeySource)]
    public string? EncryptionKeySource { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ExportPolicyRules)]
    public string? ExportPolicyRules { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ExternalHostName)]
    public string? ExternalHostName { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ExternalServerName)]
    public string? ExternalServerName { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ExternalVolumeName)]
    public string? ExternalVolumeName { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.HasRootAccess)]
    public bool? HasRootAccess { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.IsLargeVolume)]
    public bool? IsLargeVolume { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KerberosEnabled)]
    public bool? KerberosEnabled { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5R)]
    public bool? Kerberos5R { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5Rw)]
    public bool? Kerberos5Rw { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5IR)]
    public bool? Kerberos5IR { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5IRw)]
    public bool? Kerberos5IRw { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5PR)]
    public bool? Kerberos5PR { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Kerberos5PRw)]
    public bool? Kerberos5PRw { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId)]
    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LdapEnabled)]
    public bool? LdapEnabled { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NetworkFeatures)]
    public string? NetworkFeatures { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.PlacementRules)]
    public string? PlacementRules { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.PolicyEnforced)]
    public bool? PolicyEnforced { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ProximityPlacementGroup)]
    public string? ProximityPlacementGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.RelocationRequested)]
    public bool? RelocationRequested { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.RemoteVolumeResourceId)]
    public string? RemoteVolumeResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.RemoteVolumeRegion)]
    public string? RemoteVolumeRegion { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ReplicationSchedule)]
    public string? ReplicationSchedule { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.RuleIndex)]
    public int? RuleIndex { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SecurityStyle)]
    public string? SecurityStyle { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbAccessEnumeration)]
    public string? SmbAccessEnumeration { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbContinuouslyAvailable)]
    public bool? SmbContinuouslyAvailable { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbEncryption)]
    public bool? SmbEncryption { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbNonBrowsable)]
    public string? SmbNonBrowsable { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SnapshotDirectoryVisible)]
    public bool? SnapshotDirectoryVisible { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SnapshotId)]
    public string? SnapshotId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SnapshotPolicyId)]
    public string? SnapshotPolicyId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ThroughputMibps)]
    public int? ThroughputMibps { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UnixPermissions)]
    public string? UnixPermissions { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UnixReadOnly)]
    public bool? UnixReadOnly { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UnixReadWrite)]
    public bool? UnixReadWrite { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.VolumeSpecName)]
    public string? VolumeSpecName { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.VolumeType)]
    public string? VolumeType { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Zones)]
    public string[]? Zones { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AcquirePolicyToken)]
    public bool AcquirePolicyToken { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ChangeReference)]
    public string? ChangeReference { get; set; }
}
