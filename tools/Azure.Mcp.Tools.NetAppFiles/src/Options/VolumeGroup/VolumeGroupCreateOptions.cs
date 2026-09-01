// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupCreateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.VolumeGroup)]
    public string? VolumeGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ApplicationType)]
    public string? ApplicationType { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ApplicationIdentifier)]
    public string? ApplicationIdentifier { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.GroupDescription)]
    public string? GroupDescription { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Pool)]
    public string? Pool { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Subnet)]
    public string? Subnet { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Vnet)]
    public string? Vnet { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Zones)]
    public string[]? Zones { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.EncryptionKeySource)]
    public string? EncryptionKeySource { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeyVaultPrivateEndpointResourceId)]
    public string? KeyVaultPrivateEndpointResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BackupNfsv3)]
    public bool BackupNfsv3 { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataBackupReplSkd)]
    public string? DataBackupReplSkd { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataBackupSize)]
    public int? DataBackupSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataBackupSrcId)]
    public string? DataBackupSrcId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataBackupThroughput)]
    public int? DataBackupThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataReplSkd)]
    public string? DataReplSkd { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataSize)]
    public int? DataSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataSrcId)]
    public string? DataSrcId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DataThroughput)]
    public int? DataThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.GpRules)]
    public string? GpRules { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogBackupSize)]
    public int? LogBackupSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogBackupSrcId)]
    public string? LogBackupSrcId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogBackupThroughput)]
    public int? LogBackupThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogBackupReplSkd)]
    public string? LogBackupReplSkd { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogSize)]
    public int? LogSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogThroughput)]
    public int? LogThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BinarySize)]
    public int? BinarySize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.BinaryThroughput)]
    public int? BinaryThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogMirrorSize)]
    public int? LogMirrorSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.LogMirrorThroughput)]
    public int? LogMirrorThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Volumes)]
    public string? Volumes { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SharedReplSkd)]
    public string? SharedReplSkd { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SharedSize)]
    public int? SharedSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SharedSrcId)]
    public string? SharedSrcId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SharedThroughput)]
    public int? SharedThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DatabaseSize)]
    public int? DatabaseSize { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.DatabaseThroughput)]
    public int? DatabaseThroughput { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NumberOfVolumes)]
    public int? NumberOfVolumes { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Memory)]
    public int? Memory { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NumberOfHosts)]
    public int? NumberOfHosts { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.AddSnapshotCapacity)]
    public int? AddSnapshotCapacity { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ProximityPlacementGroup)]
    public string? ProximityPlacementGroup { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Prefix)]
    public string? Prefix { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbAccess)]
    public string? SmbAccess { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SmbBrowsable)]
    public string? SmbBrowsable { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.StartHostId)]
    public int? StartHostId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.SystemRole)]
    public string? SystemRole { get; set; }
}
