// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
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
        var eventHubs = new CommandGroup(Name, "Azure Event Hubs operations - Commands for managing Azure Event Hubs namespaces and event hubs. Includes CRUD operations Event Hubs service resources.");

        var eventHubGroup = new CommandGroup("eventhub", "Event Hub operations");
        eventHubs.AddSubGroup(eventHubGroup);

        var eventHubDeleteCommand = serviceProvider.GetRequiredService<EventHubDeleteCommand>();
        eventHubGroup.AddCommand(eventHubDeleteCommand.Name, eventHubDeleteCommand);

        var eventHubGetCommand = serviceProvider.GetRequiredService<EventHubGetCommand>();
        eventHubGroup.AddCommand(eventHubGetCommand.Name, eventHubGetCommand);

        var eventHubUpdateCommand = serviceProvider.GetRequiredService<EventHubUpdateCommand>();
        eventHubGroup.AddCommand(eventHubUpdateCommand.Name, eventHubUpdateCommand);

        var namespaceGroup = new CommandGroup("namespace", "Event Hubs namespace operations");
        eventHubs.AddSubGroup(namespaceGroup);

        var namespaceGetCommand = serviceProvider.GetRequiredService<NamespaceGetCommand>();
        namespaceGroup.AddCommand(namespaceGetCommand.Name, namespaceGetCommand);

        var namespaceUpdateCommand = serviceProvider.GetRequiredService<NamespaceUpdateCommand>();
        namespaceGroup.AddCommand(namespaceUpdateCommand.Name, namespaceUpdateCommand);

        var namespaceDeleteCommand = serviceProvider.GetRequiredService<NamespaceDeleteCommand>();
        namespaceGroup.AddCommand(namespaceDeleteCommand.Name, namespaceDeleteCommand);

        var consumerGroupGroup = new CommandGroup("consumergroup", "Event Hubs consumer group operations");
        eventHubGroup.AddSubGroup(consumerGroupGroup);

        var consumerGroupDeleteCommand = serviceProvider.GetRequiredService<ConsumerGroupDeleteCommand>();
        consumerGroupGroup.AddCommand(consumerGroupDeleteCommand.Name, consumerGroupDeleteCommand);

        var consumerGroupGetCommand = serviceProvider.GetRequiredService<ConsumerGroupGetCommand>();
        consumerGroupGroup.AddCommand(consumerGroupGetCommand.Name, consumerGroupGetCommand);

        var consumerGroupUpdateCommand = serviceProvider.GetRequiredService<ConsumerGroupUpdateCommand>();
        consumerGroupGroup.AddCommand(consumerGroupUpdateCommand.Name, consumerGroupUpdateCommand);

        return eventHubs;
    }
}
