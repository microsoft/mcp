// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Tools.EventGrid.Commands.Events;
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
        services.AddSingleton<EventGridPublishCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Event Grid top-level group
        var eventGrid = new CommandGroup(Name, "Event Grid operations - Commands for managing and accessing Event Grid topics, domains, and event subscriptions.");

        // Events subgroup
        var events = new CommandGroup("events", "Event Grid event operations - Commands for publishing and managing events sent to Event Grid topics.");
        eventGrid.AddSubGroup(events);

        // Topics subgroup
        var topics = new CommandGroup("topic", "Event Grid topic operations - Commands for managing Event Grid topics and their configurations.");
        eventGrid.AddSubGroup(topics);

        // Subscriptions subgroup
        var subscriptions = new CommandGroup("subscription", "Event Grid subscription operations - Commands for managing event subscriptions with filtering and endpoint configuration.");
        eventGrid.AddSubGroup(subscriptions);

        // Register Events commands
        var eventsPublish = serviceProvider.GetRequiredService<EventGridPublishCommand>();
        events.AddCommand(eventsPublish.Name, eventsPublish);

        // Register Topic commands
        var topicList = serviceProvider.GetRequiredService<TopicListCommand>();
        topics.AddCommand(topicList.Name, topicList);

        // Register Subscription commands
        var subscriptionList = serviceProvider.GetRequiredService<SubscriptionListCommand>();
        subscriptions.AddCommand(subscriptionList.Name, subscriptionList);

        return eventGrid;
    }
}
