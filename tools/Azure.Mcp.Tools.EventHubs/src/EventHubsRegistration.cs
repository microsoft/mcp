// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.EventHubs;

public sealed class EventHubsRegistration : IAreaRegistration
{
    public static string AreaName => "eventhubs";

    public static string AreaTitle => "Azure Event Hubs";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Event Hubs operations - Commands for managing Azure Event Hubs namespaces and event hubs. Includes CRUD operations Event Hubs service resources.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "eventhub",
                Description = "Event Hub operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "108ffeab-8d37-4c29-98c9-aa99eb8f61c7",
                        Name = "delete",
                        Description = "Delete an Event Hub from an Azure Event Hubs namespace. This operation permanently removes the specified Event Hub and all its data. This is a destructive operation. The operation is idempotent - if the Event Hub doesn't exist, the command reports success with Deleted = false. If the Event Hub is successfully deleted, Deleted = true is returned. Warning: This operation cannot be undone. All messages and consumer groups in the Event Hub will be permanently deleted.",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "eventhub",
                                Description = "The name of the Event Hub within the namespace.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "ab774777-76ac-4e24-ba19-da67254441a9",
                        Name = "get",
                        Description = "Get Event Hubs from Azure namespace. This command can either: 1. List all Event Hubs in a namespace 2. Get a single Event Hub by name When retrieving a single Event Hub or listing multiple Event Hubs, detailed information including partition count, settings, and metadata is returned for all Event Hubs.",
                        Title = "Get",
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "eventhub",
                                Description = "The name of the Event Hub within the namespace.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "1df73670-9de5-4d4b-bdd8-9d2d9e16f732",
                        Name = "update",
                        Description = "Create or update an Event Hub within an Azure Event Hubs namespace. This command can either: 1. Create a new Event Hub if it doesn't exist 2. Update an existing Event Hub's configuration You can configure: - Partition count (number of partitions for parallel processing) - Message retention time (how long messages are retained in hours) Note: Some properties like partition count cannot be changed after creation. This is a potentially long-running operation that waits for completion.",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "eventhub",
                                Description = "The name of the Event Hub within the namespace.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "partition-count",
                                Description = "The number of partitions for the event hub. Must be between 1 and 32 (or higher based on namespace tier).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "message-retention-in-hours",
                                Description = "The message retention time in hours. Minimum is 1 hour, maximum depends on the namespace tier.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "status",
                                Description = "The status of the event hub (Active, Disabled, etc.). Note: Status may be read-only in some operations.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubUpdateCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "consumergroup",
                        Description = "Event Hubs consumer group operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "08980fd4-c7c2-41cd-a3c2-eda5303bd458",
                                Name = "delete",
                                Description = "Delete a Consumer Group. This tool will delete a pre-existing Consumer Group from the specified Event Hub. This tool will remove existing configurations, and is considered to be destructive. The tool requires specifying the resource group, Namespace name, Event Hub name, and Consumer Group name to identify the Consumer Group to delete.",
                                Title = "Delete",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = true,
                                    Idempotent = true,
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
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "namespace",
                                        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "eventhub",
                                        Description = "The name of the Event Hub within the namespace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "consumer-group",
                                        Description = "The name of the consumer group within the Event Hub.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(EventHubDeleteCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "604fda48-2438-419d-a819-5f9d2f3b21f8",
                                Name = "get",
                                Description = "Get consumer groups from Azure Event Hub. This command can either: 1) List all consumer groups in an Event Hub 2) Get a single consumer group by name The EventHub, Namespace, and ResourceGroup parameters are required (for both get and list) The Consumer Group parameter is only required for getting a specific consumer-group When retrieving a single consumer group and when listing all available consumer groups, return all available metadata on the consumer group.",
                                Title = "Get",
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
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "namespace",
                                        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "eventhub",
                                        Description = "The name of the Event Hub within the namespace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "consumer-group",
                                        Description = "The name of the consumer group within the Event Hub.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(EventHubGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "859871ba-b8dc-439c-a607-11b0d89f5112",
                                Name = "update",
                                Description = "Create or Update a Consumer Group. This tool will either create a Consumer Group resource or update a pre-existing Consumer Group resource within the specified Event Hub, depending on whether or not the specified Consumer Group already exists. This tool may modify existing configurations, and is considered to be destructive. The tool requires specifying the resource group, namespace name, event hub name, and consumer group name. Optionally, you can provide user metadata for the consumer group.",
                                Title = "Update",
                                Annotations = new ToolAnnotations
                                {
                                    Destructive = true,
                                    Idempotent = true,
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
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "namespace",
                                        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "eventhub",
                                        Description = "The name of the Event Hub within the namespace.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "consumer-group",
                                        Description = "The name of the consumer group within the Event Hub.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "user-metadata",
                                        Description = "User metadata for the consumer group.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(EventHubUpdateCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "namespace",
                Description = "Event Hubs namespace operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "187ffc25-1e32-4e39-a7d4-94859852ac50",
                        Name = "delete",
                        Description = "Delete Event Hubs namespace. This tool will delete a pre-existing Namespace from the specified resource group. This tool will remove existing configurations, and is considered to be destructive. WARNING: This operation is irreversible. All Event Hubs, Consumer Groups, and configurations within the namespace will be permanently deleted. The namespace must exist in the specified resource group. If the namespace is not found, an error will be returned.",
                        Title = "Delete",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "71ec6c5b-b6e4-4c64-b31b-2d61dfad3b5c",
                        Name = "get",
                        Description = "Get Event Hubs namespaces from Azure. This command supports three modes of operation: 1. List all Event Hubs namespaces in a subscription 2. List all Event Hubs namespaces in a specific resource group 3. Get a single namespace by name When retrieving a single namespace, detailed information including SKU, settings, and metadata is returned. When listing namespaces, the same detailed information is returned for all namespaces in the specified scope. The --resource-group parameter is optional for listing operations but required when getting a specific namespace.",
                        Title = "Get",
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
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "225eb25d-52c5-4c3a-9eb4-066cf2b9da84",
                        Name = "update",
                        Description = "Create or Update a Namespace. This tool will either create a Namespace resource or update a pre-existing Namespace resource within the specified resource group, depending on whether or not the specified Namespace already exists. This tool may modify existing configurations, and is considered to be destructive. This is a potentially long-running operation. When updating an existing namespace, you only need to provide the properties you want to change. Unspecified properties will retain their existing values. At least one update property must be provided. Common update scenarios: - Scale up/down by changing SKU tier or capacity - Enable/disable auto-inflate and set maximum throughput units - Enable/disable Kafka support - Modify tags for resource management - Enable/disable zone redundancy (Premium SKU only)",
                        Title = "Update",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = true,
                            Idempotent = true,
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "namespace",
                                Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region where the namespace is located (e.g., 'eastus', 'westus2').",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-name",
                                Description = "The SKU name for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-tier",
                                Description = "The SKU tier for the namespace. Valid values: 'Basic', 'Standard', 'Premium'.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-capacity",
                                Description = "The SKU capacity (throughput units) for the namespace. Valid range depends on the SKU.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "is-auto-inflate-enabled",
                                Description = "Enable or disable auto-inflate for the namespace.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "maximum-throughput-units",
                                Description = "The maximum throughput units when auto-inflate is enabled.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "kafka-enabled",
                                Description = "Enable or disable Kafka for the namespace.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone-redundant",
                                Description = "Enable or disable zone redundancy for the namespace.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "tags",
                                Description = "Tags for the namespace in JSON format (e.g., '{\"key1\":\"value1\",\"key2\":\"value2\"}').",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(EventHubUpdateCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(EventHubDeleteCommand) => serviceProvider.GetRequiredService<EventHubDeleteCommand>(),
            nameof(EventHubGetCommand) => serviceProvider.GetRequiredService<EventHubGetCommand>(),
            nameof(EventHubUpdateCommand) => serviceProvider.GetRequiredService<EventHubUpdateCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in eventhubs area.")
        };
}
