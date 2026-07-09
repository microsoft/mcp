// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

public sealed class NetAppVolumeCreateParameters
{
    public string CreationToken { get; set; } = string.Empty;

    public long UsageThreshold { get; set; }

    public string? SubnetId { get; set; }

    public string? Subnet { get; set; }

    public string? Vnet { get; set; }

    public string? ServiceLevel { get; set; }

    public List<string>? ProtocolTypes { get; set; }

    public string? AcceptGrowCapacityPoolForShortTermCloneSplit { get; set; }

    public string? AllowedClients { get; set; }

    public string? AvsDataStore { get; set; }

    public string? BackupId { get; set; }

    public string? BackupPolicyId { get; set; }

    public string? BackupVaultId { get; set; }

    public string? CoolAccessRetrievalPolicy { get; set; }

    public string? CoolAccessTieringPolicy { get; set; }

    public string? CapacityPoolResourceId { get; set; }

    public string? ChownMode { get; set; }

    public bool? Cifs { get; set; }

    public bool? CoolAccess { get; set; }

    public int? CoolnessPeriod { get; set; }

    public bool? DeleteBaseSnapshot { get; set; }

    public string? DesiredArpState { get; set; }

    public string? EnableSubvolumes { get; set; }

    public string? EncryptionKeySource { get; set; }

    public JsonElement? ExportPolicyRules { get; set; }

    public string? ExternalHostName { get; set; }

    public string? ExternalServerName { get; set; }

    public string? ExternalVolumeName { get; set; }

    public bool? HasRootAccess { get; set; }

    public bool? IsLargeVolume { get; set; }

    public bool? KerberosEnabled { get; set; }

    public bool? Kerberos5R { get; set; }

    public bool? Kerberos5Rw { get; set; }

    public bool? Kerberos5IR { get; set; }

    public bool? Kerberos5IRw { get; set; }

    public bool? Kerberos5PR { get; set; }

    public bool? Kerberos5PRw { get; set; }

    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    public bool? LdapEnabled { get; set; }

    public string? NetworkFeatures { get; set; }

    public JsonElement? PlacementRules { get; set; }

    public bool? PolicyEnforced { get; set; }

    public string? ProximityPlacementGroup { get; set; }

    public bool? RelocationRequested { get; set; }

    public string? RemoteVolumeResourceId { get; set; }

    public string? RemoteVolumeRegion { get; set; }

    public string? ReplicationSchedule { get; set; }

    public int? RuleIndex { get; set; }

    public string? SecurityStyle { get; set; }

    public string? SmbAccessEnumeration { get; set; }

    public bool? SmbContinuouslyAvailable { get; set; }

    public bool? SmbEncryption { get; set; }

    public string? SmbNonBrowsable { get; set; }

    public bool? SnapshotDirectoryVisible { get; set; }

    public string? SnapshotId { get; set; }

    public string? SnapshotPolicyId { get; set; }

    public Dictionary<string, string>? Tags { get; set; }

    public int? ThroughputMibps { get; set; }

    public string? UnixPermissions { get; set; }

    public bool? UnixReadOnly { get; set; }

    public bool? UnixReadWrite { get; set; }

    public string? VolumeSpecName { get; set; }

    public string? VolumeType { get; set; }

    public List<string>? Zones { get; set; }
}
