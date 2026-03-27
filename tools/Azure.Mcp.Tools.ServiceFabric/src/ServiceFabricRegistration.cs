// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ServiceFabric;

public sealed class ServiceFabricRegistration : IAreaRegistration
{
    public static string AreaName => "servicefabric";

    public static string AreaTitle => "Manage Azure Service Fabric";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Service Fabric operations - Manage and query Azure Service Fabric managed cluster resources across subscriptions. Use when you need visibility into managed cluster nodes, including node status, node types, IP addresses, and fault/upgrade domains.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "managedcluster",
                Description = "Service Fabric managed cluster operations - Commands for managing Service Fabric managed clusters.",
                SubGroups =
                [
                    new CommandGroupDescriptor
                    {
                        Name = "node",
                        Description = "Node operations - Commands for getting and querying nodes in a Service Fabric managed cluster.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "a3f1b2c4-d5e6-47f8-9a0b-1c2d3e4f5a6b",
                                Name = "get",
                                Description = "Get nodes for a Service Fabric managed cluster. Returns all nodes by default or a single node when a node name is specified. Includes name, node type, status, IP address, fault domain, upgrade domain, health state, and seed node status.",
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
                                        Name = "cluster",
                                        Description = "Service Fabric managed cluster name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "node",
                                        Description = "The node name. When specified, returns a single node instead of all nodes.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(ManagedClusterNodeGetCommand)
                            },
                        ],
                    },
                    new CommandGroupDescriptor
                    {
                        Name = "nodetype",
                        Description = "Node type operations - Commands for managing node types in a Service Fabric managed cluster.",
                        Commands =
                        [
                            new CommandDescriptor
                            {
                                Id = "b4f2c3d5-e6f7-48a9-8b1c-2d3e4f5a6b7c",
                                Name = "restart",
                                Description = "Restart nodes of a specific node type in a Service Fabric managed cluster. Requires the cluster name, node type, and list of node names to restart. Optionally specify the update type (Default or ByUpgradeDomain).",
                                Title = "Restart",
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
                                        Name = "cluster",
                                        Description = "Service Fabric managed cluster name.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "node-type",
                                        Description = "The node type name within the managed cluster.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "nodes",
                                        Description = "The list of node names to restart. Multiple node names can be provided.",
                                        TypeName = "string",
                                        Required = true,
                                    },
                                    new OptionDescriptor
                                    {
                                        Name = "update-type",
                                        Description = "The update type for the restart operation. Valid values: Default, ByUpgradeDomain.",
                                        TypeName = "string",
                                    },
                                ],
                                Kind = CommandKind.Subscription,
                                HandlerType = nameof(ManagedClusterNodeTypeRestartCommand)
                            },
                        ],
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceFabricService, ServiceFabricService>();
        services.AddSingleton<ManagedClusterNodeGetCommand>();
        services.AddSingleton<ManagedClusterNodeTypeRestartCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ManagedClusterNodeGetCommand) => serviceProvider.GetRequiredService<ManagedClusterNodeGetCommand>(),
            nameof(ManagedClusterNodeTypeRestartCommand) => serviceProvider.GetRequiredService<ManagedClusterNodeTypeRestartCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in servicefabric area.")
        };
}
