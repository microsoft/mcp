// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.EventHubs;

public class EventHubsSetup : IAreaSetup
{
    public string Name => "eventhubs";

    public string Title => "Azure Event Hubs";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventHubsService, EventHubsService>();
        services.AddSingleton<EventHubDeleteCommand>();
        services.AddSingleton<EventHubGetCommand>();
        services.AddSingleton<EventHubUpdateCommand>();
        services.AddSingleton<NamespaceGetCommand>();
        services.AddSingleton<NamespaceUpdateCommand>();
        services.AddSingleton<NamespaceDeleteCommand>();
        services.AddSingleton<ConsumerGroupDeleteCommand>();
        services.AddSingleton<ConsumerGroupGetCommand>();
        services.AddSingleton<ConsumerGroupUpdateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var eventHubs = new CommandGroup(Name, "Azure Event Hubs operations - Commands for managing Azure Event Hubs namespaces and event hubs. Includes CRUD operations Event Hubs service resources.", Title);

        var eventHubGroup = new CommandGroup("eventhub", "Event Hub operations");
        eventHubs.AddSubGroup(eventHubGroup);

        eventHubGroup.AddCommand<EventHubDeleteCommand>(serviceProvider);
        eventHubGroup.AddCommand<EventHubGetCommand>(serviceProvider);
        eventHubGroup.AddCommand<EventHubUpdateCommand>(serviceProvider);

        var namespaceGroup = new CommandGroup("namespace", "Event Hubs namespace operations");
        eventHubs.AddSubGroup(namespaceGroup);

        namespaceGroup.AddCommand<NamespaceGetCommand>(serviceProvider);
        namespaceGroup.AddCommand<NamespaceUpdateCommand>(serviceProvider);
        namespaceGroup.AddCommand<NamespaceDeleteCommand>(serviceProvider);

        var consumerGroupGroup = new CommandGroup("consumergroup", "Event Hubs consumer group operations");
        eventHubGroup.AddSubGroup(consumerGroupGroup);

        consumerGroupGroup.AddCommand<ConsumerGroupDeleteCommand>(serviceProvider);
        consumerGroupGroup.AddCommand<ConsumerGroupGetCommand>(serviceProvider);
        consumerGroupGroup.AddCommand<ConsumerGroupUpdateCommand>(serviceProvider);

        return eventHubs;
    }
}
