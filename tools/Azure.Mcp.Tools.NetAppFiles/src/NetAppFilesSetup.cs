// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.Backup;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Commands.ReplicationStatus;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
using Azure.Mcp.Tools.NetAppFiles.Commands.SnapshotPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.NetAppFiles;

public class NetAppFilesSetup : IAreaSetup
{
    public string Name => "netappfiles";

    public string Title => "Manage Azure NetApp Files";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<INetAppFilesService, NetAppFilesService>();

        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<BackupCreateCommand>();
        services.AddSingleton<BackupPolicyCreateCommand>();
        services.AddSingleton<BackupGetCommand>();
        services.AddSingleton<BackupPolicyGetCommand>();
        services.AddSingleton<BackupVaultGetCommand>();
        services.AddSingleton<BackupVaultCreateCommand>();
        services.AddSingleton<PoolCreateCommand>();
        services.AddSingleton<PoolGetCommand>();
        services.AddSingleton<ReplicationStatusGetCommand>();
        services.AddSingleton<SnapshotCreateCommand>();
        services.AddSingleton<SnapshotGetCommand>();
        services.AddSingleton<SnapshotPolicyCreateCommand>();
        services.AddSingleton<SnapshotPolicyGetCommand>();
        services.AddSingleton<VolumeGetCommand>();
        services.AddSingleton<VolumeCreateCommand>();
        services.AddSingleton<VolumeGroupCreateCommand>();
        services.AddSingleton<VolumeGroupGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var netAppFiles = new CommandGroup(Name,
            """
            NetApp Files operations - Commands for listing and getting Azure NetApp Files accounts, capacity pools, volumes, and backup vaults.
            Use this tool to list and get NetApp Files account details including provisioning state,
            active directory configuration, and encryption settings, capacity pool details including
            service level, size, QoS type, and encryption type, volume details including
            service level, quota, protocol types, and network features, as well as backup vault details
            including provisioning state. Do not use for Azure Storage
            accounts, Azure Blob Storage, or Azure Files.
            """,
            Title);

        var account = new CommandGroup("account", "NetApp Files account operations - Commands for listing and managing NetApp Files accounts in your Azure subscription.");
        netAppFiles.AddSubGroup(account);

        var accountCreate = serviceProvider.GetRequiredService<AccountCreateCommand>();
        account.AddCommand(accountCreate.Name, accountCreate);

        var accountGet = serviceProvider.GetRequiredService<AccountGetCommand>();
        account.AddCommand(accountGet.Name, accountGet);

        var backup = new CommandGroup("backup", "NetApp Files backup operations - Commands for listing and getting NetApp Files backups in your Azure subscription.");
        netAppFiles.AddSubGroup(backup);

        var backupGet = serviceProvider.GetRequiredService<BackupGetCommand>();
        backup.AddCommand(backupGet.Name, backupGet);

        var backupCreate = serviceProvider.GetRequiredService<BackupCreateCommand>();
        backup.AddCommand(backupCreate.Name, backupCreate);

        var backupPolicy = new CommandGroup("backuppolicy", "NetApp Files backup policy operations - Commands for listing and managing NetApp Files backup policies in your Azure subscription.");
        netAppFiles.AddSubGroup(backupPolicy);

        var backupPolicyCreate = serviceProvider.GetRequiredService<BackupPolicyCreateCommand>();
        backupPolicy.AddCommand(backupPolicyCreate.Name, backupPolicyCreate);

        var backupPolicyGet = serviceProvider.GetRequiredService<BackupPolicyGetCommand>();
        backupPolicy.AddCommand(backupPolicyGet.Name, backupPolicyGet);

        var backupVault = new CommandGroup("backupvault", "NetApp Files backup vault operations - Commands for listing and managing NetApp Files backup vaults in your Azure subscription.");
        netAppFiles.AddSubGroup(backupVault);

        var backupVaultGet = serviceProvider.GetRequiredService<BackupVaultGetCommand>();
        backupVault.AddCommand(backupVaultGet.Name, backupVaultGet);

        var backupVaultCreate = serviceProvider.GetRequiredService<BackupVaultCreateCommand>();
        backupVault.AddCommand(backupVaultCreate.Name, backupVaultCreate);

        var pool = new CommandGroup("pool", "NetApp Files capacity pool operations - Commands for listing and managing NetApp Files capacity pools in your Azure subscription.");
        netAppFiles.AddSubGroup(pool);

        var poolCreate = serviceProvider.GetRequiredService<PoolCreateCommand>();
        pool.AddCommand(poolCreate.Name, poolCreate);

        var poolGet = serviceProvider.GetRequiredService<PoolGetCommand>();
        pool.AddCommand(poolGet.Name, poolGet);

        var replicationStatus = new CommandGroup("replicationstatus", "NetApp Files replication status operations - Commands for listing and getting replication status of NetApp Files volumes in your Azure subscription.");
        netAppFiles.AddSubGroup(replicationStatus);

        var replicationStatusGet = serviceProvider.GetRequiredService<ReplicationStatusGetCommand>();
        replicationStatus.AddCommand(replicationStatusGet.Name, replicationStatusGet);

        var snapshot = new CommandGroup("snapshot", "NetApp Files snapshot operations - Commands for listing and managing NetApp Files snapshots in your Azure subscription.");
        netAppFiles.AddSubGroup(snapshot);

        var snapshotCreate = serviceProvider.GetRequiredService<SnapshotCreateCommand>();
        snapshot.AddCommand(snapshotCreate.Name, snapshotCreate);

        var snapshotGet = serviceProvider.GetRequiredService<SnapshotGetCommand>();
        snapshot.AddCommand(snapshotGet.Name, snapshotGet);

        var snapshotPolicy = new CommandGroup("snapshotpolicy", "NetApp Files snapshot policy operations - Commands for listing and managing NetApp Files snapshot policies in your Azure subscription.");
        netAppFiles.AddSubGroup(snapshotPolicy);

        var snapshotPolicyCreate = serviceProvider.GetRequiredService<SnapshotPolicyCreateCommand>();
        snapshotPolicy.AddCommand(snapshotPolicyCreate.Name, snapshotPolicyCreate);

        var snapshotPolicyGet = serviceProvider.GetRequiredService<SnapshotPolicyGetCommand>();
        snapshotPolicy.AddCommand(snapshotPolicyGet.Name, snapshotPolicyGet);

        var volume = new CommandGroup("volume", "NetApp Files volume operations - Commands for listing and managing NetApp Files volumes in your Azure subscription.");
        netAppFiles.AddSubGroup(volume);

        var volumeGet = serviceProvider.GetRequiredService<VolumeGetCommand>();
        volume.AddCommand(volumeGet.Name, volumeGet);

        var volumeCreate = serviceProvider.GetRequiredService<VolumeCreateCommand>();
        volume.AddCommand(volumeCreate.Name, volumeCreate);

        var volumeGroup = new CommandGroup("volumegroup", "NetApp Files volume group operations - Commands for listing and managing NetApp Files volume groups in your Azure subscription.");
        netAppFiles.AddSubGroup(volumeGroup);

        var volumeGroupCreate = serviceProvider.GetRequiredService<VolumeGroupCreateCommand>();
        volumeGroup.AddCommand(volumeGroupCreate.Name, volumeGroupCreate);

        var volumeGroupGet = serviceProvider.GetRequiredService<VolumeGroupGetCommand>();
        volumeGroup.AddCommand(volumeGroupGet.Name, volumeGroupGet);

        return netAppFiles;
    }
}
