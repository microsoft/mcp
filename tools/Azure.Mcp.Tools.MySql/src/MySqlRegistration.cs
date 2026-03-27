// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.MySql;

public sealed class MySqlRegistration : IAreaRegistration
{
    public static string AreaName => "mysql";

    public static string AreaTitle => "Azure Database for MySQL";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "MySQL operations - Commands for managing Azure Database for MySQL Flexible Server resources. Includes operations for listing servers and databases, executing SQL queries, managing table schemas, and configuring server parameters.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "77e60b50-5c16-4879-96b1-6a40d9c08a37",
                Name = "list",
                Description = "List MySQL servers, databases, or tables in your subscription. Returns all servers by default. Specify --server to list databases on that server, or --server and --database to list tables in a specific database.",
                Title = "List MySQL Resources",
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
                        Description = "The user name to access MySQL server.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "server",
                        Description = "The MySQL server to list databases from (optional).",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "database",
                        Description = "The MySQL database to list tables from (optional, requires --server).",
                        TypeName = "string",
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(MySqlListCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "database",
                Description = "MySQL database operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "b73afaa5-4c3f-41e8-9ef3-c54e75215a97",
                        Name = "query",
                        Description = "Executes a safe, read-only SQL SELECT query against a database on Azure Database for MySQL Flexible Server. Use this tool to explore or retrieve table data without modifying it. Rejects non-SELECT statements (INSERT/UPDATE/DELETE/REPLACE/MERGE/TRUNCATE/ALTER/CREATE/DROP), multi-statements, comments hiding writes, transaction control (BEGIN/COMMIT/ROLLBACK), INTO OUTFILE, and other destructive keywords. Only a single SELECT is executed to ensure data integrity. Best practices: List needed columns (avoid SELECT *), add WHERE filters, use LIMIT/OFFSET for paging, ORDER BY for deterministic results, and avoid unnecessary sensitive data. Example: SELECT id, name, status FROM customers WHERE status = 'Active' ORDER BY name LIMIT 50;",
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
                                Description = "The user name to access MySQL server.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The MySQL server to be accessed.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The MySQL database to be accessed.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "query",
                                Description = "Query to be executed against a MySQL database.",
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
                Description = "MySQL server operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "config",
                        Description = "MySQL server configuration operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "677cef4f-0eb1-4665-a3a2-89301a75c201",
                                Name = "get",
                                Description = "Retrieves comprehensive configuration details for the specified Azure Database for MySQL Flexible Server instance. This command provides insights into server settings, performance parameters, security configurations, and operational characteristics essential for database administration and optimization. Returns configuration data in JSON format including ServerName, Location, Version, SKU, StorageSizeGB, BackupRetentionDays, and GeoRedundantBackup properties.",
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
                                        Description = "The user name to access MySQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The MySQL server to be accessed.",
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
                        Description = "MySQL server parameter operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "bae423b4-aee8-4f23-a104-e816727d183f",
                                Name = "get",
                                Description = "Retrieves the current value of a single server configuration parameter on an Azure Database for MySQL Flexible Server. Use to inspect a setting (e.g. max_connections, wait_timeout, slow_query_log) before changing it.",
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
                                        Description = "The user name to access MySQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The MySQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "param",
                                        Description = "The MySQL parameter to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(TableSchemaGetCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "8d086e44-8c8a-4649-a282-38f775704595",
                                Name = "set",
                                Description = "Sets/updates a single MySQL server configuration setting/parameter.",
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
                                        Description = "The user name to access MySQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The MySQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "param",
                                        Description = "The MySQL parameter to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "value",
                                        Description = "The value to set for the MySQL parameter.",
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
                Description = "MySQL table operations",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "schema",
                        Description = "MySQL table schema operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "1c8d2584-fa52-4641-85f9-fb67a8f5c7c9",
                                Name = "get",
                                Description = "Retrieves detailed schema information for a specific table within an Azure Database for MySQL Flexible Server database. This command provides comprehensive metadata including column definitions, data types, constraints, indexes, and relationships, essential for understanding table structure and supporting application development.",
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
                                        Description = "The user name to access MySQL server.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The MySQL server to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "database",
                                        Description = "The MySQL database to be accessed.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "table",
                                        Description = "The MySQL table to be accessed.",
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
        services.AddSingleton<IMySqlService, MySqlService>();
        services.AddSingleton<MySqlListCommand>();
        services.AddSingleton<DatabaseQueryCommand>();
        services.AddSingleton<TableSchemaGetCommand>();
        services.AddSingleton<ServerConfigGetCommand>();
        services.AddSingleton<ServerParamGetCommand>();
        services.AddSingleton<ServerParamSetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(MySqlListCommand) => serviceProvider.GetRequiredService<MySqlListCommand>(),
            nameof(DatabaseQueryCommand) => serviceProvider.GetRequiredService<DatabaseQueryCommand>(),
            nameof(TableSchemaGetCommand) => serviceProvider.GetRequiredService<TableSchemaGetCommand>(),
            nameof(ServerParamSetCommand) => serviceProvider.GetRequiredService<ServerParamSetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in mysql area.")
        };
}
