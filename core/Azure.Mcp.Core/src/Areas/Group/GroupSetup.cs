// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas.Group;

public sealed class GroupSetup : IAreaSetup
{
    public string Name => "group";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GroupListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name, "Resource group operations - Commands for listing and managing Azure resource groups in your subscriptions.");

        // Register Group commands
        var listCommand = serviceProvider.GetRequiredService<GroupListCommand>();
        group.AddCommand(listCommand.Name, listCommand);

        return group;
    }
}
