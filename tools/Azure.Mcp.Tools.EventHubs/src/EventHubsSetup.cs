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

        eventHubGroup.AddCommand(serviceProvider.GetRequiredService<EventHubDeleteCommand>());
        eventHubGroup.AddCommand(serviceProvider.GetRequiredService<EventHubGetCommand>());
        eventHubGroup.AddCommand(serviceProvider.GetRequiredService<EventHubUpdateCommand>());

        var namespaceGroup = new CommandGroup("namespace", "Event Hubs namespace operations");
        eventHubs.AddSubGroup(namespaceGroup);

        namespaceGroup.AddCommand(serviceProvider.GetRequiredService<NamespaceGetCommand>());
        namespaceGroup.AddCommand(serviceProvider.GetRequiredService<NamespaceUpdateCommand>());
        namespaceGroup.AddCommand(serviceProvider.GetRequiredService<NamespaceDeleteCommand>());

        var consumerGroupGroup = new CommandGroup("consumergroup", "Event Hubs consumer group operations");
        eventHubGroup.AddSubGroup(consumerGroupGroup);

        consumerGroupGroup.AddCommand(serviceProvider.GetRequiredService<ConsumerGroupDeleteCommand>());
        consumerGroupGroup.AddCommand(serviceProvider.GetRequiredService<ConsumerGroupGetCommand>());
        consumerGroupGroup.AddCommand(serviceProvider.GetRequiredService<ConsumerGroupUpdateCommand>());

        return eventHubs;
    }
}
