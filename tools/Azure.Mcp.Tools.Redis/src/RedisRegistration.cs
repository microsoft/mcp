// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Redis;

public sealed class RedisRegistration : IAreaRegistration
{
    public static string AreaName => "redis";

    public static string AreaTitle => "Azure Redis";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Redis operations - Commands for managing Azure Redis resources. Includes operations for listing Redis resources, databases, and data access policies, in both the Azure Managed Redis and legacy Azure Cache for Redis services, as well as for creating Azure Managed Redis resources.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "750133dd-d57f-4ed4-9488-c1d406ad4a83",
                Name = "create",
                Description = "Create a new Azure Managed Redis resource in Azure. Use this command to provision a new Redis resource in your subscription.",
                Title = "Create",
                Annotations = new ToolAnnotations
                {
                    Destructive = true,
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
                        Name = "resource",
                        Description = "The name of the Redis resource (e.g., my-redis).",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "resource-group",
                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "sku",
                        Description = "The SKU for the Redis resource. (Default: Balanced_B0)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "location",
                        Description = "The location for the Redis resource (e.g. eastus).",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "access-keys-authentication",
                        Description = "Whether to enable access keys for authentication for the Redis resource. (Default: false)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "public-network-access",
                        Description = "Whether to enable public network access for the Redis resource. (Default: false)",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "modules",
                        Description = "A list of modules to enable on the Azure Managed Redis resource (e.g., RedisBloom, RedisJSON).",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(ResourceCreateCommand)
            },
            new CommandDescriptor
            {
                Id = "eded7479-4187-4742-957f-d7778e03a69d",
                Name = "list",
                Description = "List/show all Redis resources in a subscription. Returns details of all Azure Managed Redis, Azure Cache for Redis, and Azure Redis Enterprise resources. Use this command to explore and view which Redis resources are available in your subscription.",
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
                Options = [],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(ResourceListCommand)
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();
        services.AddSingleton<ResourceListCommand>();
        services.AddSingleton<ResourceCreateCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ResourceCreateCommand) => serviceProvider.GetRequiredService<ResourceCreateCommand>(),
            nameof(ResourceListCommand) => serviceProvider.GetRequiredService<ResourceListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in redis area.")
        };
}
