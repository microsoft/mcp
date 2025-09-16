// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Aks;

public class AksSetup : IAreaSetup
{
    public string Name => "aks";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAksService, AksService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create AKS command group
        var aks = new CommandGroup(Name, "Azure Kubernetes Service operations - Commands for managing Azure Kubernetes Service (AKS) cluster resources. Includes operations for listing clusters, retrieving cluster configurations, and managing Kubernetes environments in Azure.");
        rootGroup.AddSubGroup(aks);

        // Create AKS subgroups
        var cluster = new CommandGroup("cluster", "AKS cluster operations - Commands for listing and managing AKS clusters in your Azure subscription.");
        aks.AddSubGroup(cluster);
        var nodepool = new CommandGroup("nodepool", "AKS node pool operations - Commands for listing and managing AKS node pools for an AKS cluster.");
        aks.AddSubGroup(nodepool);

        // Register AKS commands
        cluster.AddCommand("list", new ClusterListCommand(loggerFactory.CreateLogger<ClusterListCommand>()));
        cluster.AddCommand("get", new ClusterGetCommand(loggerFactory.CreateLogger<ClusterGetCommand>()));
        nodepool.AddCommand("list", new NodepoolListCommand(loggerFactory.CreateLogger<NodepoolListCommand>()));
        nodepool.AddCommand("get", new NodepoolGetCommand(loggerFactory.CreateLogger<NodepoolGetCommand>()));
    }
}
