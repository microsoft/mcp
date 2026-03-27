// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Kusto;

public sealed class KustoRegistration : IAreaRegistration
{
    public static string AreaName => "kusto";

    public static string AreaTitle => "Azure Data Explorer";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Kusto operations - Commands for managing and querying Azure Data Explorer (Kusto) resources. Includes operations for listing clusters and databases, executing KQL queries, retrieving table schemas, and working with Kusto data analytics workloads.",
        Title = AreaTitle,
        Commands =
        [
            new CommandDescriptor
            {
                Id = "d1e22074-53ce-4eef-8596-0ea134a9e317",
                Name = "query",
                Description = "Executes a query against an Azure Data Explorer/Kusto/KQL cluster to search for specific terms, retrieve records, or perform management operations. Required: --cluster-uri (or --cluster and --subscription), --database, and --query.",
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
                        Name = "cluster-uri",
                        Description = "Kusto Cluster URI.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "cluster",
                        Description = "Kusto Cluster name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "database",
                        Description = "Kusto Database name.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "query",
                        Description = "Kusto query to execute. Uses KQL syntax.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(QueryCommand)
            },
            new CommandDescriptor
            {
                Id = "41daed5c-bf44-4cdf-9f3c-1df775465e53",
                Name = "sample",
                Description = "Return a sample of rows from a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table.",
                Title = "Sample",
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
                        Name = "cluster-uri",
                        Description = "Kusto Cluster URI.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "cluster",
                        Description = "Kusto Cluster name.",
                        TypeName = "string",
                    },
                    new OptionDescriptor
                    {
                        Name = "database",
                        Description = "Kusto Database name.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "table",
                        Description = "Kusto Table name.",
                        TypeName = "string",
                        Required = true,
                    },
                    new OptionDescriptor
                    {
                        Name = "limit",
                        Description = "The maximum number of results to return. Must be a positive integer between 1 and 100000. Default is 10.",
                        TypeName = "string",
                        Required = true,
                    },
                ],
                Kind = CommandKind.Subscription,
                HandlerType = nameof(SampleCommand)
            },
        ],
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "cluster",
                Description = "Kusto cluster operations - Commands for listing clusters in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "5fc5a42b-a7f6-4d4a-9517-a8e119752b7a",
                        Name = "get",
                        Description = "Get/retrieve/show details for a specific Azure Data Explorer/Kusto/KQL cluster in a subscription. Not for listing multiple clusters. Required: --cluster and --subscription.",
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
                                Name = "cluster",
                                Description = "Kusto Cluster name.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ClusterGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "2cff1548-40c9-48ea-8548-6bfa91f2ea85",
                        Name = "list",
                        Description = "List/enumerate all Azure Data Explorer/Kusto/KQL clusters in a subscription.",
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
                        HandlerType = nameof(ClusterListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "database",
                Description = "Kusto database operations - Commands for listing databases in a cluster.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "0bd79f0b-c360-4c96-b3e0-02fce97dcc41",
                        Name = "list",
                        Description = "List/enumerate all databases in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri ( or --cluster and --subscription).",
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
                                Name = "cluster-uri",
                                Description = "Kusto Cluster URI.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "cluster",
                                Description = "Kusto Cluster name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ClusterListCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "table",
                Description = "Kusto table operations - Commands for listing tables in a database.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "3cd1e5f1-3353-4029-99f8-1aaa566d05e4",
                        Name = "list",
                        Description = "List/enumerate all tables in a specific Azure Data Explorer/Kusto/KQL database. Required: --cluster-uri (or --cluster and --subscription), --database.",
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
                                Name = "cluster-uri",
                                Description = "Kusto Cluster URI.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "cluster",
                                Description = "Kusto Cluster name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "Kusto Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ClusterListCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "9a972c48-6797-49bb-9784-8063ad1f7e96",
                        Name = "schema",
                        Description = "Get/retrieve/show the schema of a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table.",
                        Title = "Schema",
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
                                Name = "cluster-uri",
                                Description = "Kusto Cluster URI.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "cluster",
                                Description = "Kusto Cluster name.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "database",
                                Description = "Kusto Database name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "table",
                                Description = "Kusto Table name.",
                                TypeName = "string",
                                Required = true,
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TableSchemaCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IKustoService, KustoService>();
        services.AddSingleton<SampleCommand>();
        services.AddSingleton<QueryCommand>();
        services.AddSingleton<ClusterListCommand>();
        services.AddSingleton<ClusterGetCommand>();
        services.AddSingleton<DatabaseListCommand>();
        services.AddSingleton<TableListCommand>();
        services.AddSingleton<TableSchemaCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(QueryCommand) => serviceProvider.GetRequiredService<QueryCommand>(),
            nameof(SampleCommand) => serviceProvider.GetRequiredService<SampleCommand>(),
            nameof(ClusterGetCommand) => serviceProvider.GetRequiredService<ClusterGetCommand>(),
            nameof(ClusterListCommand) => serviceProvider.GetRequiredService<ClusterListCommand>(),
            nameof(TableSchemaCommand) => serviceProvider.GetRequiredService<TableSchemaCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in kusto area.")
        };
}
