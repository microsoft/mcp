// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Aks.Commands.Cluster;
using Azure.Mcp.Tools.Aks.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

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

        // Register AKS commands
        cluster.AddCommand("list", new ClusterListCommand(loggerFactory.CreateLogger<ClusterListCommand>()));
        cluster.AddCommand("get", new ClusterGetCommand(loggerFactory.CreateLogger<ClusterGetCommand>()));
    }
}
