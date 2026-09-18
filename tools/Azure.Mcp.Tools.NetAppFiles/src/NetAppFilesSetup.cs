// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Commands.Volume;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.NetAppFiles;

public class NetAppFilesSetup : IAreaSetup
{
    public string Name => "netappfiles";

    public string Title => "Azure NetApp Files";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<INetAppFilesAccountService, NetAppFilesAccountService>();
        services.AddSingleton<INetAppFilesBackupPolicyService, NetAppFilesBackupPolicyService>();
        services.AddSingleton<INetAppFilesBackupVaultService, NetAppFilesBackupVaultService>();
        services.AddSingleton<INetAppFilesVolumeService, NetAppFilesVolumeService>();
        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<AccountUpdateCommand>();
        services.AddSingleton<BackupPolicyCreateCommand>();
        services.AddSingleton<BackupPolicyGetCommand>();
        services.AddSingleton<BackupPolicyUpdateCommand>();
        services.AddSingleton<BackupVaultCreateCommand>();
        services.AddSingleton<BackupVaultGetCommand>();
        services.AddSingleton<BackupVaultUpdateCommand>();
        services.AddSingleton<VolumeCreateCommand>();
        services.AddSingleton<VolumeGetCommand>();
        services.AddSingleton<VolumeUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(
            Name,
            "Azure NetApp Files operations for managing enterprise file storage resources.",
            Title);

        var account = new CommandGroup("account", "Azure NetApp Files account operations.");
        root.AddSubGroup(account);
        account.AddCommand<AccountCreateCommand>(serviceProvider);
        account.AddCommand<AccountGetCommand>(serviceProvider);
        account.AddCommand<AccountUpdateCommand>(serviceProvider);

        var backupPolicy = new CommandGroup("backuppolicy", "Azure NetApp Files backup policy operations.");
        root.AddSubGroup(backupPolicy);
        backupPolicy.AddCommand<BackupPolicyCreateCommand>(serviceProvider);
        backupPolicy.AddCommand<BackupPolicyGetCommand>(serviceProvider);
        backupPolicy.AddCommand<BackupPolicyUpdateCommand>(serviceProvider);

        var backupVault = new CommandGroup("backupvault", "Azure NetApp Files backup vault operations.");
        root.AddSubGroup(backupVault);
        backupVault.AddCommand<BackupVaultCreateCommand>(serviceProvider);
        backupVault.AddCommand<BackupVaultGetCommand>(serviceProvider);
        backupVault.AddCommand<BackupVaultUpdateCommand>(serviceProvider);

        var volume = new CommandGroup("volume", "Azure NetApp Files volume operations.");
        root.AddSubGroup(volume);
        volume.AddCommand<VolumeCreateCommand>(serviceProvider);
        volume.AddCommand<VolumeGetCommand>(serviceProvider);
        volume.AddCommand<VolumeUpdateCommand>(serviceProvider);

        return root;
    }
}
