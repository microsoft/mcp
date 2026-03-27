// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Cosmos;

public sealed class CosmosRegistration : IAreaRegistration
{
    public static string AreaName => "cosmos";

    public static string AreaTitle => "Azure Cosmos DB";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Cosmos DB operations - Commands for managing and querying Azure Cosmos DB resources. Includes operations for accounts, databases, containers, and document queries.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                Name = "list",
                Description = "List Cosmos DB accounts, databases, or containers. Returns all accounts in the subscription by default. Specify --account to list databases in that account, or --account and --database to list containers in a specific database.",
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
                        Name = "account",
                        Description = "The name of the Cosmos DB account (optional). When not specified, lists all accounts in the subscription. Specify this to list databases, or combine with --database to list containers.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "database",
                        Description = "The name of the database (optional). Requires --account to be specified. When provided, lists containers within this database.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(CosmosListCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "database",
                Description = "Cosmos DB database operations - Commands for managing databases within your Cosmos DB accounts.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "container",
                        Description = "Cosmos DB container operations - Commands for managing containers within your Cosmos DB databases.",
                        SubGroups =
                        [
                            new CommandGroupDescriptor
                            {
                                Name = "item",
                                Description = "Cosmos DB item operations - Commands for querying, creating, updating, and deleting documents within your Cosmos DB containers.",
                                Commands =
                                [
                                    new CommandDescriptor
                                    {
                                        Id = "5c19a92a-4e0c-44dc-b1e7-5560a0d277b5",
                                        Name = "query",
                                        Description = "List items from a Cosmos DB container by specifying the account name, database name, and container name, optionally providing a custom SQL query to filter results.",
                                        Title = "Query",
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
                                                Name = "account",
                                                Description = "The name of the Cosmos DB account to query (e.g., my-cosmos-account).",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "database",
                                                Description = "The name of the database to query (e.g., my-database).",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "container",
                                                Description = "The name of the container to query (e.g., my-container).",
                                                TypeName = "string",
                                                Required = true,
                                            },
                                            new OptionDescriptor
                                            {
                                                Name = "query",
                                                Description = "SQL query to execute against the container. Uses Cosmos DB SQL syntax.",
                                                TypeName = "string",
                                            },
                                        ],
                                        Kind = CommandKind.Subscription,
                                        HandlerType = nameof(ItemQueryCommand)
                                    },
                                ],
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICosmosService, CosmosService>();
        services.AddSingleton<CosmosListCommand>();
        services.AddSingleton<ItemQueryCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(CosmosListCommand) => serviceProvider.GetRequiredService<CosmosListCommand>(),
            nameof(ItemQueryCommand) => serviceProvider.GetRequiredService<ItemQueryCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in cosmos area.")
        };
}
