// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Commands.Account;
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
        services.AddSingleton<INetAppFilesVolumeService, NetAppFilesVolumeService>();
        services.AddSingleton<AccountCreateCommand>();
        services.AddSingleton<AccountGetCommand>();
        services.AddSingleton<AccountUpdateCommand>();
        services.AddSingleton<VolumeCreateCommand>();
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

        var volume = new CommandGroup("volume", "Azure NetApp Files volume operations.");
        root.AddSubGroup(volume);
        volume.AddCommand<VolumeCreateCommand>(serviceProvider);

        return root;
    }
}