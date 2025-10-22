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
        var serviceBus = new CommandGroup(Name, "Service Bus operations - Commands for managing Azure Service Bus messaging infrastructure including queues, topics, and subscriptions for reliable asynchronous communication and enterprise application integration. Use this tool when you need to manage message queues for point-to-point communication, configure topics and subscriptions for publish-subscribe messaging patterns, monitor message processing, or set up enterprise messaging scenarios for decoupled application architectures. This tool supports reliable messaging, dead letter handling, and enterprise integration patterns. Do not use this tool for real-time communication, direct API calls, database operations, or simple HTTP-based messaging - Service Bus is designed for asynchronous, reliable messaging between distributed applications and services. This tool is a hierarchical MCP command router where sub-commands are routed to MCP servers that require specific fields inside the \"parameters\" object. To invoke a command, set \"command\" and wrap its arguments in \"parameters\". Set \"learn=true\" to discover available sub-commands for different Service Bus entity management and messaging operations. Note that this tool requires appropriate Service Bus permissions and will only access messaging entities accessible to the authenticated user.", Title);

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
