// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
using Azure.Mcp.Tools.NetAppFiles.Commands.VolumeGroup;
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
        services.AddSingleton<INetAppFilesVolumeGroupService, NetAppFilesVolumeGroupService>();
        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<AccountUpdateCommand>();
        services.AddSingleton<VolumeGroupCreateCommand>();
        services.AddSingleton<VolumeGroupGetCommand>();
        services.AddSingleton<VolumeGroupUpdateCommand>();
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

        var volumeGroup = new CommandGroup("volumegroup", "Azure NetApp Files application volume group operations.");
        root.AddSubGroup(volumeGroup);
        volumeGroup.AddCommand<VolumeGroupCreateCommand>(serviceProvider);
        volumeGroup.AddCommand<VolumeGroupGetCommand>(serviceProvider);
        volumeGroup.AddCommand<VolumeGroupUpdateCommand>(serviceProvider);

        return root;
    }
}
