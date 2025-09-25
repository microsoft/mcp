// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.EventGrid;

public class EventGridSetup : IAreaSetup
{
    public string Name => "eventgrid";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IEventGridService, EventGridService>();
        services.AddSingleton<TopicListCommand>();
        services.AddSingleton<SubscriptionListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Event Grid top-level group
        var eventGrid = new CommandGroup(Name, "Event Grid operations - Commands for managing and accessing Event Grid topics, domains, and event subscriptions.");

        // Topics subgroup
        var topics = new CommandGroup("topic", "Event Grid topic operations - Commands for managing Event Grid topics and their configurations.");
        eventGrid.AddSubGroup(topics);

        // Subscriptions subgroup
        var subscriptions = new CommandGroup("subscription", "Event Grid subscription operations - Commands for managing event subscriptions with filtering and endpoint configuration.");
        eventGrid.AddSubGroup(subscriptions);

        // Register Topic commands
        var listCommand = serviceProvider.GetRequiredService<TopicListCommand>();
        topics.AddCommand(listCommand.Name, listCommand);

        // Register Subscription commands
        subscriptions.AddCommand("list", serviceProvider.GetRequiredService<SubscriptionListCommand>());

        return eventGrid;
    }
}
