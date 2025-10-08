// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.EventHubs;

public class EventHubsSetup : IAreaSetup
{
    public string Name => "eventhubs";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventHubsService, EventHubsService>();
        services.AddSingleton<NamespaceGetCommand>();
        services.AddSingleton<NamespaceUpdateCommand>();
        services.AddSingleton<NamespaceDeleteCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var eventHubs = new CommandGroup(Name, "Azure Event Hubs operations - Commands for managing Azure Event Hubs namespaces and event hubs. Includes operations for getting, creating, updating, and deleting namespaces in resource groups.");

        var namespaceGroup = new CommandGroup("namespace", "Event Hubs namespace operations");
        eventHubs.AddSubGroup(namespaceGroup);

        var namespaceGetCommand = serviceProvider.GetRequiredService<NamespaceGetCommand>();
        namespaceGroup.AddCommand(namespaceGetCommand.Name, namespaceGetCommand);

        var namespaceUpdateCommand = serviceProvider.GetRequiredService<NamespaceUpdateCommand>();
        namespaceGroup.AddCommand(namespaceUpdateCommand.Name, namespaceUpdateCommand);

        var namespaceDeleteCommand = serviceProvider.GetRequiredService<NamespaceDeleteCommand>();
        namespaceGroup.AddCommand(namespaceDeleteCommand.Name, namespaceDeleteCommand);

        return eventHubs;
    }
}
