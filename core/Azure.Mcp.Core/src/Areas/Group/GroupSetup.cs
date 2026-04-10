// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas.Group.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Core.Areas.Group;

public sealed class GroupSetup : IAreaSetup
{
    public string Name => "group";

    public string Title => "Azure Resource Groups";

    public CommandCategory Category => CommandCategory.SubscriptionManagement;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GroupListCommand>();
        services.AddSingleton<ResourceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var group = new CommandGroup(Name, "Resource group operations - Commands for listing and managing Azure resource groups and their resources in your subscriptions.", Title);

        // Register Group commands
        var listCommand = serviceProvider.GetRequiredService<GroupListCommand>();
        group.AddCommand(listCommand.Name, listCommand);

        // Register Resource sub-group
        var resource = new CommandGroup("resource", "Resource operations - Commands for listing resources within a resource group.");
        group.AddSubGroup(resource);

        var resourceListCommand = serviceProvider.GetRequiredService<ResourceListCommand>();
        resource.AddCommand(resourceListCommand.Name, resourceListCommand);

        return group;
    }
}
