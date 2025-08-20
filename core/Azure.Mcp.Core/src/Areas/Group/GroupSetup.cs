// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Commands;
using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Areas.Group;

public sealed class GroupSetup : IAreaSetup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // No additional services needed for Group area
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Group command group
        var group = new CommandGroup("group", "Resource group operations - Commands for listing and managing Azure resource groups in your subscriptions.");
        rootGroup.AddSubGroup(group);

        // Register Group commands
        group.AddCommand("list", new GroupListCommand(loggerFactory.CreateLogger<GroupListCommand>()));
    }
}
