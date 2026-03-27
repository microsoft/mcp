// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ServiceBus.Commands.Queue;
using Azure.Mcp.Tools.ServiceBus.Commands.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ServiceBus;

public sealed class ServiceBusRegistration : IAreaRegistration
{
    public static string AreaName => "servicebus";

    public static string AreaTitle => "Azure Service Bus";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Service Bus operations – Commands to manage Azure Service Bus queues, topics, and subscriptions for reliable asynchronous messaging and enterprise integration. Supports point-to-point and publish-subscribe patterns, dead-letter handling, and decoupled architectures. Not intended for real-time communication, direct API calls, or database operations. Uses a hierarchical MCP command model with command, parameters, and learn=true.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "queue",
                Description = "Queue operations - Commands for using Azure Service Bus queues.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a02c58ce-e89f-4303-ac4a-c9dfb118e761",
                        Name = "details",
                        Description = "Get details about a Service Bus queue. Returns queue properties and runtime information. Properties returned include lock duration, max message size, queue size, creation date, status, current message counts, etc. Required arguments: - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net) - queue: Queue name to get details and runtime information for.",
                        Title = "Details",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "queue",
                                Description = "The queue name to peek messages from.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(QueueDetailsCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "topic",
                Description = "Topic operations - Commands for using Azure Service Bus topics and subscriptions.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c2487c40-58d0-40f7-98f1-105744865a11",
                        Name = "details",
                        Description = "Retrieves details about a Service Bus topic. Returns runtime information and topic properties including number of subscriptions, max message size, max topic size, number of scheduled messages, etc. Required arguments are namespace: The fully qualified Service Bus namespace host name (usually in the form <namespace>.servicebus.windows.net) and topic: Topic name to get information about. Do not use this to get details on Service Bus subscription- instead use servicebus_topic_subscription_details.",
                        Title = "Details",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "topic",
                                Description = "The name of the topic containing the subscription.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(QueueDetailsCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "subscription",
                        Description = "Subscription operations - Commands for using subscriptions within a Service Bus topic.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "578edf30-01f3-45da-b451-3932dcce7cc5",
                                Name = "details",
                                Description = "Get details about a Service Bus subscription. Returns subscription runtime properties including message counts, delivery settings, and other metadata. Required arguments: - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net) - topic: Topic name containing the subscription - subscription-name: Name of the subscription to get details for",
                                Title = "Details",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = false,
                                    Idempotent = true,
                                    OpenWorld = false,
                                    ReadOnly = true,
                                    LocalRequired = false,
                                    Secret = false,
                                },
                                Options =
                                [
                                    new OptionDescriptor
                                    {
                                        Name = "namespace",
                                        Description = "The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "topic",
                                        Description = "The name of the topic containing the subscription.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "subscription-name",
                                        Description = "The name of subscription to peek messages from.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(QueueDetailsCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceBusService, ServiceBusService>();
        services.AddSingleton<QueueDetailsCommand>();
        services.AddSingleton<TopicDetailsCommand>();
        services.AddSingleton<SubscriptionDetailsCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(QueueDetailsCommand) => serviceProvider.GetRequiredService<QueueDetailsCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in servicebus area.")
        };
}
