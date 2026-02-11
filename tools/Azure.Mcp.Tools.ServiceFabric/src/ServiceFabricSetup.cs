// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ServiceFabric.Commands.ManagedCluster;
using Azure.Mcp.Tools.ServiceFabric.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ServiceFabric;

public class ServiceFabricSetup : IAreaSetup
{
    public string Name => "servicefabric";
    public string Title => "Manage Azure Service Fabric";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceFabricService, ServiceFabricService>();

        services.AddSingleton<ManagedClusterNodeListCommand>();
        services.AddSingleton<ManagedClusterNodeTypeRestartCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var serviceFabric = new CommandGroup(
            Name,
            "Azure Service Fabric operations - Manage and query Azure Service Fabric managed cluster resources across subscriptions. Use when you need visibility into managed cluster nodes, including node status, node types, IP addresses, and fault/upgrade domains.",
            Title);

        var managedCluster = new CommandGroup(
            "managedcluster",
            "Service Fabric managed cluster operations - Commands for managing Service Fabric managed clusters.");
        serviceFabric.AddSubGroup(managedCluster);

        var node = new CommandGroup(
            "node",
            "Node operations - Commands for listing and querying nodes in a Service Fabric managed cluster.");
        managedCluster.AddSubGroup(node);

        var nodeList = serviceProvider.GetRequiredService<ManagedClusterNodeListCommand>();
        node.AddCommand(nodeList.Name, nodeList);

        var nodetype = new CommandGroup(
            "nodetype",
            "Node type operations - Commands for managing node types in a Service Fabric managed cluster.");
        managedCluster.AddSubGroup(nodetype);

        var nodeTypeRestart = serviceProvider.GetRequiredService<ManagedClusterNodeTypeRestartCommand>();
        nodetype.AddCommand(nodeTypeRestart.Name, nodeTypeRestart);

        return serviceFabric;
    }
}
