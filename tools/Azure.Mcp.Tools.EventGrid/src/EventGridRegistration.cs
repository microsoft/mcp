// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.EventGrid.Commands.Events;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Commands.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.EventGrid;

public sealed class EventGridRegistration : IAreaRegistration
{
    public static string AreaName => "eventgrid";

    public static string AreaTitle => "Azure Event Grid";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Event Grid operations - Commands for managing and accessing Event Grid topics, domains, and event subscriptions.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "events",
                Description = "Event Grid event operations - Commands for publishing and managing events sent to Event Grid topics.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d5f216a4-c45e-4c29-a414-d3feaa5929e2",
                        Name = "publish",
                        Description = "Publish custom events to Event Grid topics for event-driven architectures. This tool sends structured event data to Event Grid topics with schema validation and delivery guarantees for downstream subscribers. Returns publish operation status. Requires topic, data, and optional schema.",
                        Title = "Publish",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "topic",
                                Description = "The name of the Event Grid topic.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "data",
                                Description = "The event data as JSON string to publish to the Event Grid topic.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "schema",
                                Description = "The event schema type (CloudEvents, EventGrid, or Custom). Defaults to EventGrid.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventGridPublishCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "subscription",
                Description = "Event Grid subscription operations - Commands for managing event subscriptions with filtering and endpoint configuration.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "716a33e5-755c-4168-87ed-8a4651476c6e",
                        Name = "list",
                        Description = "Show all available Event Grid subscriptions with optional topic filtering. This tool displays active event subscriptions including webhook endpoints, event filters, and delivery retry policies. Use this when you need to show, list, or get Event Grid subscriptions for topics. Requires either topic name OR subscription. If only topic is provided, searches all accessible subscriptions for a topic with that name. Resource group and location filters can be applied, but only when used with a subscription or topic.",
                        Title = "List",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "topic",
                                Description = "The name of the Event Grid topic.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region to filter resources by (e.g., 'eastus', 'westus2').",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TopicListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "topic",
                Description = "Event Grid topic operations - Commands for managing Event Grid topics and their configurations.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "42390294-2856-4980-a057-095c91355650",
                        Name = "list",
                        Description = "List or show all Event Grid topics in a subscription, optionally filtered by resource group, returning endpoints, access keys, provisioning state, and subscription details for event publishing and management. A subscription or topic name is required.",
                        Title = "List",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TopicListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IEventGridService, EventGridService>();
        services.AddSingleton<TopicListCommand>();
        services.AddSingleton<SubscriptionListCommand>();
        services.AddSingleton<EventGridPublishCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(EventGridPublishCommand) => serviceProvider.GetRequiredService<EventGridPublishCommand>(),
            nameof(TopicListCommand) => serviceProvider.GetRequiredService<TopicListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in eventgrid area.")
        };
}
