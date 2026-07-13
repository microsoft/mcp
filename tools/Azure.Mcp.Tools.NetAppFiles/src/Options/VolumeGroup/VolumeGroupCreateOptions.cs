// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeGroupName)]
    public string? VolumeGroup { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ApplicationTypeName)]
    public string? ApplicationType { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ApplicationIdentifierName)]
    public string? ApplicationIdentifier { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.GroupDescriptionName)]
    public string? GroupDescription { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SubnetName)]
    public string? Subnet { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VnetName)]
    public string? Vnet { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ZonesName)]
    public string[]? Zones { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.EncryptionKeySourceName)]
    public string? EncryptionKeySource { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceIdName)]
    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    [JsonPropertyName("backupNfsv3")]
    public bool BackupNfsv3 { get; set; }

    [JsonPropertyName("dataBackupReplSkd")]
    public string? DataBackupReplSkd { get; set; }

    [JsonPropertyName("dataBackupSize")]
    public int? DataBackupSize { get; set; }

    [JsonPropertyName("dataBackupSrcId")]
    public string? DataBackupSrcId { get; set; }

    [JsonPropertyName("dataBackupThroughput")]
    public int? DataBackupThroughput { get; set; }

    [JsonPropertyName("dataReplSkd")]
    public string? DataReplSkd { get; set; }

    [JsonPropertyName("dataSize")]
    public int? DataSize { get; set; }

    [JsonPropertyName("dataSrcId")]
    public string? DataSrcId { get; set; }

    [JsonPropertyName("dataThroughput")]
    public int? DataThroughput { get; set; }

    [JsonPropertyName("gpRules")]
    public string? GpRules { get; set; }

    [JsonPropertyName("logBackupSize")]
    public int? LogBackupSize { get; set; }

    [JsonPropertyName("logBackupSrcId")]
    public string? LogBackupSrcId { get; set; }

    [JsonPropertyName("logBackupThroughput")]
    public int? LogBackupThroughput { get; set; }

    [JsonPropertyName("logBackupReplSkd")]
    public string? LogBackupReplSkd { get; set; }

    [JsonPropertyName("logSize")]
    public int? LogSize { get; set; }

    [JsonPropertyName("logThroughput")]
    public int? LogThroughput { get; set; }

    [JsonPropertyName("binarySize")]
    public int? BinarySize { get; set; }

    [JsonPropertyName("binaryThroughput")]
    public int? BinaryThroughput { get; set; }

    [JsonPropertyName("logMirrorSize")]
    public int? LogMirrorSize { get; set; }

    [JsonPropertyName("logMirrorThroughput")]
    public int? LogMirrorThroughput { get; set; }

    [JsonPropertyName("volumes")]
    public string? Volumes { get; set; }

    [JsonPropertyName("sharedReplSkd")]
    public string? SharedReplSkd { get; set; }

    [JsonPropertyName("sharedSize")]
    public int? SharedSize { get; set; }

    [JsonPropertyName("sharedSrcId")]
    public string? SharedSrcId { get; set; }

    [JsonPropertyName("sharedThroughput")]
    public int? SharedThroughput { get; set; }

    [JsonPropertyName("databaseSize")]
    public int? DatabaseSize { get; set; }

    [JsonPropertyName("databaseThroughput")]
    public int? DatabaseThroughput { get; set; }

    [JsonPropertyName("numberOfVolumes")]
    public int? NumberOfVolumes { get; set; }

    [JsonPropertyName("memory")]
    public int? Memory { get; set; }

    [JsonPropertyName("numberOfHosts")]
    public int? NumberOfHosts { get; set; }

    [JsonPropertyName("addSnapshotCapacity")]
    public int? AddSnapshotCapacity { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ProximityPlacementGroupName)]
    public string? ProximityPlacementGroup { get; set; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; set; }

    [JsonPropertyName("smbAccess")]
    public string? SmbAccess { get; set; }

    [JsonPropertyName("smbBrowsable")]
    public string? SmbBrowsable { get; set; }

    [JsonPropertyName("startHostId")]
    public int? StartHostId { get; set; }

    [JsonPropertyName("systemRole")]
    public string? SystemRole { get; set; }
}
