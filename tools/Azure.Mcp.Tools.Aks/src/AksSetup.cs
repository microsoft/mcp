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

        services.AddSingleton<ClusterGetCommand>();
        services.AddSingleton<NodepoolGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create AKS command group
        var aks = new CommandGroup(Name, "Azure Kubernetes Service operations - Manage and query Azure Kubernetes Service (AKS) resources across subscriptions. Use when you need subscription-scoped visibility into AKS cluster and node pool metadata—including Azure resource IDs, networking endpoints, identity configuration, and provisioning state—for governance or automation. Requires Azure subscription context. Not for kubectl execution, pod lifecycle changes, or in-cluster application deployments—use Kubernetes-native tooling for those tasks.");

        // Create AKS subgroups
        var cluster = new CommandGroup("cluster", "AKS cluster operations - Commands for listing and managing AKS clusters in your Azure subscription.");
        aks.AddSubGroup(cluster);
        var nodepool = new CommandGroup("nodepool", "AKS node pool operations - Commands for listing and managing AKS node pools for an AKS cluster.");
        aks.AddSubGroup(nodepool);

        // Register AKS commands
        var clusterGet = serviceProvider.GetRequiredService<ClusterGetCommand>();
        cluster.AddCommand(clusterGet.Name, clusterGet);

        var nodepoolGet = serviceProvider.GetRequiredService<NodepoolGetCommand>();
        nodepool.AddCommand(nodepoolGet.Name, nodepoolGet);

        return aks;
    }
}
