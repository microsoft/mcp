// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Commands.Nodepool;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Aks;

public class AksSetup : IAreaSetup
{
    public string Name => "aks";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAksService, AksService>();

        services.AddSingleton<ClusterListCommand>();
        services.AddSingleton<ClusterGetCommand>();
        services.AddSingleton<NodepoolListCommand>();
        services.AddSingleton<NodepoolGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create AKS command group
        var aks = new CommandGroup(Name, "Azure Kubernetes Service operations - Commands for managing Azure Kubernetes Service (AKS) cluster resources. Includes operations for listing clusters, retrieving cluster configurations, and managing Kubernetes environments in Azure.");

        // Create AKS subgroups
        var cluster = new CommandGroup("cluster", "AKS cluster operations - Commands for listing and managing AKS clusters in your Azure subscription.");
        aks.AddSubGroup(cluster);
        var nodepool = new CommandGroup("nodepool", "AKS node pool operations - Commands for listing and managing AKS node pools for an AKS cluster.");
        aks.AddSubGroup(nodepool);

        // Register AKS commands
        var clusterList = serviceProvider.GetRequiredService<ClusterListCommand>();
        cluster.AddCommand(clusterList.Name, clusterList);

        var clusterGet = serviceProvider.GetRequiredService<ClusterGetCommand>();
        cluster.AddCommand(clusterGet.Name, clusterGet);

        var nodepoolList = serviceProvider.GetRequiredService<NodepoolListCommand>();
        nodepool.AddCommand(nodepoolList.Name, nodepoolList);

        var nodepoolGet = serviceProvider.GetRequiredService<NodepoolGetCommand>();
        nodepool.AddCommand(nodepoolGet.Name, nodepoolGet);

        return aks;
    }
}
