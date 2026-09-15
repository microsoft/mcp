// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp volume via ARM generic resource API.
/// </summary>
internal sealed class NetAppVolumeCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public NetAppVolumeCreateProperties? Properties { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }

    [JsonPropertyName("zones")]
    public List<string>? Zones { get; set; }
}

internal sealed class NetAppVolumeCreateProperties
{
    [JsonPropertyName("creationToken")]
    public string? CreationToken { get; set; }

    [JsonPropertyName("usageThreshold")]
    public long? UsageThreshold { get; set; }

    [JsonPropertyName("subnetId")]
    public string? SubnetId { get; set; }

    [JsonPropertyName("serviceLevel")]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName("protocolTypes")]
    public List<string>? ProtocolTypes { get; set; }

    [JsonPropertyName("acceptGrowCapacityPoolForShortTermCloneSplit")]
    public string? AcceptGrowCapacityPoolForShortTermCloneSplit { get; set; }

    [JsonPropertyName("allowedClients")]
    public string? AllowedClients { get; set; }

    [JsonPropertyName("avsDataStore")]
    public string? AvsDataStore { get; set; }

    [JsonPropertyName("backupId")]
    public string? BackupId { get; set; }

    [JsonPropertyName("backupPolicyId")]
    public string? BackupPolicyId { get; set; }

    [JsonPropertyName("backupVaultId")]
    public string? BackupVaultId { get; set; }

    [JsonPropertyName("coolAccessRetrievalPolicy")]
    public string? CoolAccessRetrievalPolicy { get; set; }

    [JsonPropertyName("coolAccessTieringPolicy")]
    public string? CoolAccessTieringPolicy { get; set; }

    [JsonPropertyName("capacityPoolResourceId")]
    public string? CapacityPoolResourceId { get; set; }

    [JsonPropertyName("chownMode")]
    public string? ChownMode { get; set; }

    [JsonPropertyName("cifs")]
    public bool? Cifs { get; set; }

    [JsonPropertyName("coolAccess")]
    public bool? CoolAccess { get; set; }

    [JsonPropertyName("coolnessPeriod")]
    public int? CoolnessPeriod { get; set; }

    [JsonPropertyName("deleteBaseSnapshot")]
    public bool? DeleteBaseSnapshot { get; set; }

    [JsonPropertyName("desiredArpState")]
    public string? DesiredArpState { get; set; }

    [JsonPropertyName("enableSubvolumes")]
    public string? EnableSubvolumes { get; set; }

    [JsonPropertyName("encryptionKeySource")]
    public string? EncryptionKeySource { get; set; }

    [JsonPropertyName("exportPolicyRules")]
    public JsonElement? ExportPolicyRules { get; set; }

    [JsonPropertyName("externalHostName")]
    public string? ExternalHostName { get; set; }

    [JsonPropertyName("externalServerName")]
    public string? ExternalServerName { get; set; }

    [JsonPropertyName("externalVolumeName")]
    public string? ExternalVolumeName { get; set; }

    [JsonPropertyName("hasRootAccess")]
    public bool? HasRootAccess { get; set; }

    [JsonPropertyName("isLargeVolume")]
    public bool? IsLargeVolume { get; set; }

    [JsonPropertyName("kerberosEnabled")]
    public bool? KerberosEnabled { get; set; }

    [JsonPropertyName("kerberos5ReadOnly")]
    public bool? Kerberos5ReadOnly { get; set; }

    [JsonPropertyName("kerberos5ReadWrite")]
    public bool? Kerberos5ReadWrite { get; set; }

    [JsonPropertyName("kerberos5iReadOnly")]
    public bool? Kerberos5iReadOnly { get; set; }

    [JsonPropertyName("kerberos5iReadWrite")]
    public bool? Kerberos5iReadWrite { get; set; }

    [JsonPropertyName("kerberos5pReadOnly")]
    public bool? Kerberos5pReadOnly { get; set; }

    [JsonPropertyName("kerberos5pReadWrite")]
    public bool? Kerberos5pReadWrite { get; set; }

    [JsonPropertyName("keyVaultPrivateEndpointResourceId")]
    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    [JsonPropertyName("ldapEnabled")]
    public bool? LdapEnabled { get; set; }

    [JsonPropertyName("networkFeatures")]
    public string? NetworkFeatures { get; set; }

    [JsonPropertyName("placementRules")]
    public JsonElement? PlacementRules { get; set; }

    [JsonPropertyName("policyEnforced")]
    public bool? PolicyEnforced { get; set; }

    [JsonPropertyName("proximityPlacementGroup")]
    public string? ProximityPlacementGroup { get; set; }

    [JsonPropertyName("relocationRequested")]
    public bool? RelocationRequested { get; set; }

    [JsonPropertyName("remoteVolumeResourceId")]
    public string? RemoteVolumeResourceId { get; set; }

    [JsonPropertyName("remoteVolumeRegion")]
    public string? RemoteVolumeRegion { get; set; }

    [JsonPropertyName("replicationSchedule")]
    public string? ReplicationSchedule { get; set; }

    [JsonPropertyName("ruleIndex")]
    public int? RuleIndex { get; set; }

    [JsonPropertyName("securityStyle")]
    public string? SecurityStyle { get; set; }

    [JsonPropertyName("smbAccessBasedEnumeration")]
    public string? SmbAccessBasedEnumeration { get; set; }

    [JsonPropertyName("smbContinuouslyAvailable")]
    public bool? SmbContinuouslyAvailable { get; set; }

    [JsonPropertyName("smbEncryption")]
    public bool? SmbEncryption { get; set; }

    [JsonPropertyName("smbNonBrowsable")]
    public string? SmbNonBrowsable { get; set; }

    [JsonPropertyName("snapshotDirectoryVisible")]
    public bool? SnapshotDirectoryVisible { get; set; }

    [JsonPropertyName("snapshotId")]
    public string? SnapshotId { get; set; }

    [JsonPropertyName("snapshotPolicyId")]
    public string? SnapshotPolicyId { get; set; }

    [JsonPropertyName("throughputMibps")]
    public int? ThroughputMibps { get; set; }

    [JsonPropertyName("unixPermissions")]
    public string? UnixPermissions { get; set; }

    [JsonPropertyName("unixReadOnly")]
    public bool? UnixReadOnly { get; set; }

    [JsonPropertyName("unixReadWrite")]
    public bool? UnixReadWrite { get; set; }

    [JsonPropertyName("volumeSpecName")]
    public string? VolumeSpecName { get; set; }

    [JsonPropertyName("volumeType")]
    public string? VolumeType { get; set; }
}
