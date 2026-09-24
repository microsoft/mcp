// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.Snapshot;
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
        services.AddSingleton<INetAppFilesSnapshotService, NetAppFilesSnapshotService>();
        services.AddSingleton<INetAppFilesVolumeService, NetAppFilesVolumeService>();
        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<AccountUpdateCommand>();
        services.AddSingleton<SnapshotCreateCommand>();
        services.AddSingleton<SnapshotGetCommand>();
        services.AddSingleton<SnapshotUpdateCommand>();
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

        var snapshot = new CommandGroup("snapshot", "Azure NetApp Files snapshot operations.");
        root.AddSubGroup(snapshot);
        snapshot.AddCommand<SnapshotCreateCommand>(serviceProvider);
        snapshot.AddCommand<SnapshotGetCommand>(serviceProvider);
        snapshot.AddCommand<SnapshotUpdateCommand>(serviceProvider);

        var volume = new CommandGroup("volume", "Azure NetApp Files volume operations.");
        root.AddSubGroup(volume);
        volume.AddCommand<VolumeCreateCommand>(serviceProvider);
        volume.AddCommand<VolumeGetCommand>(serviceProvider);
        volume.AddCommand<VolumeUpdateCommand>(serviceProvider);

        return root;
    }
}
