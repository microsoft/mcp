// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Aks;

public sealed class AksRegistration : IAreaRegistration
{
    public static string AreaName => "aks";

    public static string AreaTitle => "Manage Azure Kubernetes Service";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Kubernetes Service operations - Manage and query Azure Kubernetes Service (AKS) resources across subscriptions. Use when you need subscription-scoped visibility into AKS cluster and node pool metadata—including Azure resource IDs, networking endpoints, identity configuration, and provisioning state—for governance or automation. Requires Azure subscription context. Not for kubectl execution, pod lifecycle changes, or in-cluster application deployments—use Kubernetes-native tooling for those tasks.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "cluster",
                Description = "AKS cluster operations - Commands for listing and managing AKS clusters in your Azure subscription.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "34e0d3d3-cbc5-4df8-8244-1439b97f3de5",
                        Name = "get",
                        Description = "List/enumerate all AKS (Azure Kubernetes Service) clusters in a subscription. Get/retrieve/show the details of a specific cluster if a name is provided.",
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
                                Name = "cluster",
                                Description = "AKS Cluster name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ClusterGetCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "nodepool",
                Description = "AKS node pool operations - Commands for listing and managing AKS node pools for an AKS cluster.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "9abb0904-2ffc-4aab-b4ea-fc454b6351b1",
                        Name = "get",
                        Description = "List/enumerate all AKS (Azure Kubernetes Service) node pools in a cluster. Get/retrieve/show the details of a specific node pool if a name is provided.",
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
                                Description = "AKS Cluster name.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "nodepool",
                                Description = "AKS node pool (agent pool) name.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ClusterGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IAksService, AksService>();
        services.AddSingleton<ClusterGetCommand>();
        services.AddSingleton<NodepoolGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ClusterGetCommand) => serviceProvider.GetRequiredService<ClusterGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in aks area.")
        };
}
