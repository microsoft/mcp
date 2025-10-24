// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Microsoft.Extensions.DependencyInjection;

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
        var serviceBus = new CommandGroup(Name, "Service Bus operations - Commands for managing Azure Service Bus messaging infrastructure including queues, topics, and subscriptions for reliable asynchronous communication and enterprise integration. Use this tool to manage message queues for point-to-point communication, configure topics and subscriptions for publish-subscribe patterns, monitor message processing, or set up messaging for decoupled architectures. Supports reliable messaging, dead letter handling, and enterprise integration patterns. Do not use for real-time communication, direct API calls, or database operations - Service Bus is for asynchronous messaging between distributed applications. This is a hierarchical MCP command router where sub-commands are routed to servers requiring specific \"parameters\" fields. To invoke: set \"command\" and wrap arguments in \"parameters\". Set \"learn=true\" to discover sub-commands. Requires appropriate Service Bus permissions.", Title);

        var queue = new CommandGroup("queue", "Queue operations - Commands for using Azure Service Bus queues.");
        // queue.AddCommand("peek", new QueuePeekCommand());
        var queueDetails = serviceProvider.GetRequiredService<QueueDetailsCommand>();
        queue.AddCommand(queueDetails.Name, queueDetails);

        var topic = new CommandGroup("topic", "Topic operations - Commands for using Azure Service Bus topics and subscriptions.");
        var topicDetails = serviceProvider.GetRequiredService<TopicDetailsCommand>();
        topic.AddCommand(topicDetails.Name, topicDetails);

        var subscription = new CommandGroup("subscription", "Subscription operations - Commands for using subscriptions within a Service Bus topic.");
        // subscription.AddCommand("peek", new SubscriptionPeekCommand());
        var subscriptionDetails = serviceProvider.GetRequiredService<SubscriptionDetailsCommand>();
        subscription.AddCommand(subscriptionDetails.Name, subscriptionDetails);

        serviceBus.AddSubGroup(queue);
        serviceBus.AddSubGroup(topic);

        topic.AddSubGroup(subscription);

        return serviceBus;
    }
}
