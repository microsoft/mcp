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
        // services.AddSingleton<QueuePeekCommand>();  // Intentionally unregistered: message body binary-data serialization is unresolved (see #2763, #2680). Do not register, document, or add e2e prompts for this command until it is enabled.
        services.AddSingleton<TopicDetailsCommand>();
        services.AddSingleton<SubscriptionDetailsCommand>();
        // services.AddSingleton<SubscriptionPeekCommand>();  // Intentionally unregistered: message body binary-data serialization is unresolved (see #2763, #2680). Do not register, document, or add e2e prompts for this command until it is enabled.
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var serviceBus = new CommandGroup(Name, "Service Bus operations - Commands to manage Azure Service Bus queues, topics, and subscriptions for reliable asynchronous messaging and enterprise integration. Supports point-to-point and publish-subscribe patterns, dead-letter handling, and decoupled architectures. Not intended for real-time communication, direct API calls, or database operations. Uses a hierarchical MCP command model with command, parameters, and learn=true.", Title);

        var queue = new CommandGroup("queue", "Queue operations - Commands for using Azure Service Bus queues.");
        // queue.AddCommand<QueuePeekCommand>(serviceProvider);  // Intentionally unregistered: message body binary-data serialization is unresolved (see #2763, #2680). Do not register, document, or add e2e prompts for this command until it is enabled.
        queue.AddCommand<QueueDetailsCommand>(serviceProvider);

        var topic = new CommandGroup("topic", "Topic operations - Commands for using Azure Service Bus topics and subscriptions.");
        topic.AddCommand<TopicDetailsCommand>(serviceProvider);

        var subscription = new CommandGroup("subscription", "Subscription operations - Commands for using subscriptions within a Service Bus topic.");
        // subscription.AddCommand<SubscriptionPeekCommand>(serviceProvider);  // Intentionally unregistered: message body binary-data serialization is unresolved (see #2763, #2680). Do not register, document, or add e2e prompts for this command until it is enabled.
        subscription.AddCommand<SubscriptionDetailsCommand>(serviceProvider);

        serviceBus.AddSubGroup(queue);
        serviceBus.AddSubGroup(topic);

        topic.AddSubGroup(subscription);

        return serviceBus;
    }
}
