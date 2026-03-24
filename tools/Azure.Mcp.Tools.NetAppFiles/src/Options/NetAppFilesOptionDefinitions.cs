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
}
