// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ServiceBus;

public class ServiceBusSetup : IAreaSetup
{
    public string Name => "servicebus";

    public string Title => "Azure Service Bus";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceBusService, ServiceBusService>();

        services.AddSingleton<QueueDetailsCommand>();
        services.AddSingleton<TopicDetailsCommand>();
        services.AddSingleton<SubscriptionDetailsCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var serviceBus = new CommandGroup(Name,
            """
            Service Bus operations – Commands to manage Azure Service Bus queues,
            topics, and subscriptions for reliable asynchronous messaging and
            enterprise integration. Supports point-to-point and publish-subscribe
            patterns, dead-letter handling, and decoupled architectures. Not
            intended for real-time communication, direct API calls, or database
            operations. Uses a hierarchical MCP command model with command,
            parameters, and learn=true.
            """, Title);

        var queue = new CommandGroup("queue", "Queue operations - Commands for using Azure Service Bus queues.");
        // queue.AddCommand("peek", new QueuePeekCommand());
        var queueDetails = serviceProvider.GetRequiredService<QueueDetailsCommand>();
        queue.AddCommand(queueDetails.Name, queueDetails);

        var topic = new CommandGroup("topic",
            """
            Topic operations - Commands for using Azure Service Bus topics and
            subscriptions.
            """);
        var topicDetails = serviceProvider.GetRequiredService<TopicDetailsCommand>();
        topic.AddCommand(topicDetails.Name, topicDetails);

        var subscription = new CommandGroup("subscription",
            """
            Subscription operations - Commands for using subscriptions within a
            Service Bus topic.
            """);
        // subscription.AddCommand("peek", new SubscriptionPeekCommand());
        var subscriptionDetails = serviceProvider.GetRequiredService<SubscriptionDetailsCommand>();
        subscription.AddCommand(subscriptionDetails.Name, subscriptionDetails);

        serviceBus.AddSubGroup(queue);
        serviceBus.AddSubGroup(topic);

        topic.AddSubGroup(subscription);

        return serviceBus;
    }
}
