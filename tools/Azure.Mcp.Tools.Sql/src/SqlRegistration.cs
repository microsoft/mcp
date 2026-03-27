// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Sql.Commands.Database;
using Azure.Mcp.Tools.Sql.Commands.ElasticPool;
using Azure.Mcp.Tools.Sql.Commands.EntraAdmin;
using Azure.Mcp.Tools.Sql.Commands.FirewallRule;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Sql;

public sealed class SqlRegistration : IAreaRegistration
{
    public static string AreaName => "sql";

    public static string AreaTitle => "Azure SQL Database";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure SQL operations - Commands for managing Azure SQL databases, servers, and elastic pools. Includes operations for listing databases, configuring server settings, managing firewall rules, Entra ID administrators, and elastic pool resources.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "db",
                Description = "SQL database operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "a4d9af17-fe8b-4df3-93be-23b69f0b5a0c",
                        Name = "create",
                        Description = "Create a new Azure SQL Database on an existing SQL Server. This command creates a database with configurable performance tiers, size limits, and other settings. Equivalent to 'az sql db create'. Returns the newly created database information including configuration details.",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The Azure SQL Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-name",
                                Description = "The SKU name for the database (e.g., Basic, S0, P1, GP_Gen5_2).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-tier",
                                Description = "The SKU tier for the database (e.g., Basic, Standard, Premium, GeneralPurpose).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-capacity",
                                Description = "The SKU capacity (DTU or vCore count) for the database.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "collation",
                                Description = "The collation for the database (e.g., SQL_Latin1_General_CP1_CI_AS).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "max-size-bytes",
                                Description = "The maximum size of the database in bytes.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "elastic-pool-name",
                                Description = "The name of the elastic pool to assign the database to.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone-redundant",
                                Description = "Whether the database should be zone redundant.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "read-scale",
                                Description = "Read scale option for the database (Enabled or Disabled).",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "c4ef0375-0df9-445c-b8ae-2542e9612425",
                        Name = "delete",
                        Description = "Deletes a database from an Azure SQL Server.This idempotent operation removes the specified database from the server, returning Deleted = false if the database doesn't exist or Deleted = true if successfully removed.",
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
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The Azure SQL Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "2c4e6a8b-1d3f-4e5a-b6c7-8d9e0f1a2b3c",
                        Name = "get",
                        Description = "Show, get, or list Azure SQL databases in a SQL Server. Shows details for a specific Azure SQL database by name, or lists all Azure SQL databases in the specified SQL Server. Use to show or retrieve Azure SQL database information. Equivalent to 'az sql db show' (show one Azure SQL database) or 'az sql db list' (list all Azure SQL databases in a server). Returns database information including configuration details and current status.",
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
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The Azure SQL Database name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "3bddfa1a-ab9d-44f0-830a-e56a159e5469",
                        Name = "rename",
                        Description = "Rename an existing Azure SQL Database to a new name within the same SQL server. This command moves the database resource to a new identifier while preserving configuration and data. Equivalent to 'az sql db rename'. Returns the updated database information using the new name.",
                        Title = "Rename",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The Azure SQL Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "new-database-name",
                                Description = "The new name for the Azure SQL Database.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseRenameCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "16f02fbf-6760-440a-bacc-925365b6de49",
                        Name = "update",
                        Description = "Scale and configure Azure SQL Database performance settings. Update an existing database's SKU, compute tier, storage capacity, or redundancy options to meet changing performance requirements. Returns the updated database configuration including applied scaling changes.",
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
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "The Azure SQL Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-name",
                                Description = "The SKU name for the database (e.g., Basic, S0, P1, GP_Gen5_2).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-tier",
                                Description = "The SKU tier for the database (e.g., Basic, Standard, Premium, GeneralPurpose).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-capacity",
                                Description = "The SKU capacity (DTU or vCore count) for the database.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "collation",
                                Description = "The collation for the database (e.g., SQL_Latin1_General_CP1_CI_AS).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "max-size-bytes",
                                Description = "The maximum size of the database in bytes.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "elastic-pool-name",
                                Description = "The name of the elastic pool to assign the database to.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "zone-redundant",
                                Description = "Whether the database should be zone redundant.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "read-scale",
                                Description = "Read scale option for the database (Enabled or Disabled).",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseUpdateCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "elastic-pool",
                Description = "SQL elastic pool operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "f980fda7-4bd6-4c24-b139-a091f088584f",
                        Name = "list",
                        Description = "Lists all SQL elastic pools in an Azure SQL Server with their SKU, capacity, state, and database limits. Use when you need to: view elastic pool inventory, check pool utilization, compare pool configurations, or find available pools for database placement. Requires: subscription ID, resource group name, server name. Returns: JSON array of elastic pools with complete configuration details. Equivalent to 'az sql elastic-pool list'.",
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
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ElasticPoolListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "server",
                Description = "SQL server operations",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "43f5f55d-2f21-47ac-b7f3-53f5d51b5218",
                        Name = "create",
                        Description = "Creates a new Azure SQL server in the specified resource group and location. The server will be created with the specified administrator credentials and optional configuration settings. Returns the created server with its properties including the fully qualified domain name.",
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
                                Name = "resource-group",
                                Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "administrator-login",
                                Description = "The administrator login name for the SQL server.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "administrator-password",
                                Description = "The administrator password for the SQL server.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "location",
                                Description = "The Azure region location where the SQL server will be created.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "version",
                                Description = "The version of SQL Server to create (e.g., '12.0').",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "public-network-access",
                                Description = "Whether public network access is enabled for the SQL server ('Enabled' or 'Disabled'). Defaults to 'Disabled'.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseCreateCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "381bd0ef-5bb4-45ed-ae51-d129dcc044b2",
                        Name = "delete",
                        Description = "Remove the specified SQL server from your Azure subscription, including all associated databases. This operation permanently deletes all server data and cannot be reversed. Use --force to bypass confirmation.",
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
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "force",
                                Description = "Force delete the server without confirmation prompts.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseDeleteCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "7f9a1c3e-5b7d-4a6c-8e0f-2b4d6a8c0e1f",
                        Name = "get",
                        Description = "Show, get, or list Azure SQL servers in a resource group. Shows details for a specific Azure SQL server by name, or lists all Azure SQL servers in the specified resource group. Use to show, display, or retrieve Azure SQL server information. Equivalent to 'az sql server show' (show one Azure SQL server) or 'az sql server list' (list all Azure SQL servers in a resource group). Returns server information including configuration details and current state.",
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
                                Name = "server",
                                Description = "The Azure SQL Server name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(DatabaseGetCommand)
                    },
                ],
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "entra-admin",
                        Description = "SQL server Microsoft Entra ID administrator operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "240aac03-0eb0-4cd3-91f8-475577289186",
                                Name = "list",
                                Description = "Gets a list of Microsoft Entra ID administrators for a SQL server. This command retrieves all Entra ID administrators configured for the specified SQL server, including their display names, object IDs, and tenant information. Returns an array of Entra ID administrator objects with their properties.",
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
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The Azure SQL Server name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(ElasticPoolListCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "firewall-rule",
                        Description = "SQL server firewall rule operations",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "37c43190-c3f5-4cd2-beda-3ecc2e3ec049",
                                Name = "create",
                                Description = "Creates a firewall rule for a SQL server. Firewall rules control which IP addresses are allowed to connect to the SQL server. You can specify either a single IP address (by setting start and end IP to the same value) or a range of IP addresses. Returns the created firewall rule with its properties.",
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
                                        Name = "resource-group",
                                        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The Azure SQL Server name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "firewall-rule-name",
                                        Description = "The name of the firewall rule.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "start-ip-address",
                                        Description = "The start IP address of the firewall rule range.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "end-ip-address",
                                        Description = "The end IP address of the firewall rule range.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(DatabaseCreateCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "f13fc5d2-7547-480b-a704-36120e2e9b92",
                                Name = "delete",
                                Description = "Deletes a firewall rule from a SQL server. This operation removes the specified firewall rule, potentially restricting access for the IP addresses that were previously allowed by this rule. The operation is idempotent - if the rule doesn't exist, no error is returned.",
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
                                        Name = "server",
                                        Description = "The Azure SQL Server name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "firewall-rule-name",
                                        Description = "The name of the firewall rule.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(DatabaseDeleteCommand)
                            },
                            new CommandDescriptor
                            {
                                Id = "1f55cab9-0bbb-499a-a9ac-1492f11c043a",
                                Name = "list",
                                Description = "Gets a list of firewall rules for a SQL server. This command retrieves all firewall rules configured for the specified SQL server, including their IP address ranges and rule names. Returns an array of firewall rule objects with their properties.",
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
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "server",
                                        Description = "The Azure SQL Server name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(ElasticPoolListCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ISqlService, SqlService>();
        services.AddSingleton<DatabaseGetCommand>();
        services.AddSingleton<DatabaseCreateCommand>();
        services.AddSingleton<DatabaseRenameCommand>();
        services.AddSingleton<DatabaseUpdateCommand>();
        services.AddSingleton<DatabaseDeleteCommand>();
        services.AddSingleton<ServerGetCommand>();
        services.AddSingleton<ServerCreateCommand>();
        services.AddSingleton<ServerDeleteCommand>();
        services.AddSingleton<ElasticPoolListCommand>();
        services.AddSingleton<EntraAdminListCommand>();
        services.AddSingleton<FirewallRuleListCommand>();
        services.AddSingleton<FirewallRuleCreateCommand>();
        services.AddSingleton<FirewallRuleDeleteCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(DatabaseCreateCommand) => serviceProvider.GetRequiredService<DatabaseCreateCommand>(),
            nameof(DatabaseDeleteCommand) => serviceProvider.GetRequiredService<DatabaseDeleteCommand>(),
            nameof(DatabaseGetCommand) => serviceProvider.GetRequiredService<DatabaseGetCommand>(),
            nameof(DatabaseRenameCommand) => serviceProvider.GetRequiredService<DatabaseRenameCommand>(),
            nameof(DatabaseUpdateCommand) => serviceProvider.GetRequiredService<DatabaseUpdateCommand>(),
            nameof(ElasticPoolListCommand) => serviceProvider.GetRequiredService<ElasticPoolListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in sql area.")
        };
}
