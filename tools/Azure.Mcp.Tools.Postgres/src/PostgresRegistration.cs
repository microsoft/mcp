// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Database;
using Azure.Mcp.Tools.Postgres.Commands.Server;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Postgres;

public sealed class PostgresRegistration : IAreaRegistration
{
    public static string AreaName => "postgres";

    public static string AreaTitle => "Azure Database for PostgreSQL";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "PostgreSQL operations - Commands for managing Azure Database for PostgreSQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "8a12c3f4-2e5d-4b3a-9f2c-5e6d7f8a9b0c",
                Name = "list",
                Description = "List PostgreSQL servers, databases, or tables. Returns all servers in the subscription by default (optionally scoped to a --resource-group). Specify --server to list databases on that server, or --server and --database to list tables in a specific database. --user is required when --server is provided.",
                Title = "List PostgreSQL Resources",
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
                        Name = "user",
                        Description = "The user name to access PostgreSQL server.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "server",
                        Description = "The PostgreSQL server to list databases from (optional).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "database",
                        Description = "The PostgreSQL database to list tables from (optional, requires --server).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "auth-type",
                        Description = "The authentication type to access PostgreSQL server. Supported values are 'MicrosoftEntra' or 'PostgreSQL'. By default 'MicrosoftEntra' is used.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "password",
                        Description = "The user password to access PostgreSQL server, Only required if 'auth-type' is set to 'PostgreSQL' authentication, not needed for 'MicrosoftEntra' authentication.",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(PostgresListCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "database",
                Description = "PostgreSQL database operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "81a28bca-014c-4738-9e1a-654d77cb2dd8",
                        Name = "query",
                        Description = "Executes a SQL query on an Azure Database for PostgreSQL server to search for specific terms, retrieve records, or perform SELECT operations.",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "user",
                                Description = "The user name to access PostgreSQL server.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The PostgreSQL server to be accessed.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The PostgreSQL database to be accessed.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "auth-type",
                                Description = "The authentication type to access PostgreSQL server. Supported values are 'MicrosoftEntra' or 'PostgreSQL'. By default 'MicrosoftEntra' is used.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "password",
                                Description = "The user password to access PostgreSQL server, Only required if 'auth-type' is set to 'PostgreSQL' authentication, not needed for 'MicrosoftEntra' authentication.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "query",
                                Description = "Query to be executed against a PostgreSQL database.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseQueryCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "server",
                Description = "PostgreSQL server operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "config",
                        Description = "PostgreSQL server configuration operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "049a0d10-0a6e-4278-a0a3-15ce6b2e5ee1",
                                Name = "get",
                                Description = "Retrieve the configuration of a PostgreSQL server.",
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
                                        Name = "user",
                                        Description = "The user name to access PostgreSQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The PostgreSQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(TableSchemaGetCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "param",
                        Description = "PostgreSQL server parameter operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "af3a581d-ab64-4939-9765-974815d9c7be",
                                Name = "get",
                                Description = "Retrieves a specific parameter of a PostgreSQL server.",
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
                                        Name = "user",
                                        Description = "The user name to access PostgreSQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The PostgreSQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "param",
                                        Description = "The PostgreSQL parameter to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(TableSchemaGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "2134621b-518f-48ac-a66a-82c40fcb58bb",
                                Name = "set",
                                Description = "Configures PostgreSQL server settings including replication, connection limits, and other parameters.",
                                Title = "Set",
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
                                        Name = "user",
                                        Description = "The user name to access PostgreSQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The PostgreSQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "param",
                                        Description = "The PostgreSQL parameter to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "value",
                                        Description = "The value to set for the PostgreSQL parameter.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(ServerParamSetCommand)
                            },
                        ],
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "table",
                Description = "PostgreSQL table operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "schema",
                        Description = "PostgreSQL table schema operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "643a3497-44e1-4727-b3d6-c2e5dba6cab2",
                                Name = "get",
                                Description = "Retrieves the schema of a specified table in a PostgreSQL database.",
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
                                        Name = "user",
                                        Description = "The user name to access PostgreSQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The PostgreSQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "database",
                                        Description = "The PostgreSQL database to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "auth-type",
                                        Description = "The authentication type to access PostgreSQL server. Supported values are 'MicrosoftEntra' or 'PostgreSQL'. By default 'MicrosoftEntra' is used.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "password",
                                        Description = "The user password to access PostgreSQL server, Only required if 'auth-type' is set to 'PostgreSQL' authentication, not needed for 'MicrosoftEntra' authentication.",
                                        TypeName = "string",
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "table",
                                        Description = "The PostgreSQL table to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(TableSchemaGetCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IEntraTokenProvider, EntraTokenProvider>();
        services.AddSingleton<IDbProvider, DbProvider>();
        services.AddSingleton<IPostgresService, PostgresService>();
        services.AddSingleton<PostgresListCommand>();
        services.AddSingleton<DatabaseQueryCommand>();
        services.AddSingleton<TableSchemaGetCommand>();
        services.AddSingleton<ServerConfigGetCommand>();
        services.AddSingleton<ServerParamGetCommand>();
        services.AddSingleton<ServerParamSetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(PostgresListCommand) => serviceProvider.GetRequiredService<PostgresListCommand>(),
            nameof(DatabaseQueryCommand) => serviceProvider.GetRequiredService<DatabaseQueryCommand>(),
            nameof(TableSchemaGetCommand) => serviceProvider.GetRequiredService<TableSchemaGetCommand>(),
            nameof(ServerParamSetCommand) => serviceProvider.GetRequiredService<ServerParamSetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in postgres area.")
        };
}
