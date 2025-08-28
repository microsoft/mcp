// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi;

public class FabricPublicApiSetup : IAreaSetup
{
    public string Name => "fabric";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IFabricPublicApiService, FabricPublicApiService>();
        services.AddHttpClient<FabricPublicApiService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        var fabric = new CommandGroup(Name,
            """
            Microsoft Fabric operations - Commands for managing and accessing Microsoft Fabric workspaces, 
            items, and resources through the public APIs. Use this tool when you need to list workspaces, 
            get workspace details, list items within workspaces, or get detailed information about specific 
            Fabric items like lakehouses, reports, notebooks, and other Fabric artifacts. This tool focuses 
            on read-only operations to discover and explore Fabric resources. This tool is a hierarchical 
            MCP command router where sub-commands are routed to MCP servers that require specific fields 
            inside the "parameters" object. To invoke a command, set "command" and wrap its arguments in 
            "parameters". Set "learn=true" to discover available sub-commands for different Microsoft Fabric 
            operations including workspace and item management. Note that this tool requires appropriate 
            Fabric permissions and will only access resources accessible to the authenticated user.
            """);
        rootGroup.AddSubGroup(fabric);

        // Create Fabric subgroups
        var publicApis = new CommandGroup("publicapis", "Fabric public APIS - Commands for fetching resources and learning about Fabric public APIs");
        fabric.AddSubGroup(publicApis);

        var apiSpecs = new CommandGroup("apispecs", "Fabric API Specs - Commands for listing and retrieving Microsoft Fabric API specifications.");
        publicApis.AddSubGroup(apiSpecs);

        // apiSpecs.AddCommand("list", new ListPublicWorkloadsCommand(loggerFactory.CreateLogger<ListPublicWorkloadsCommand>()));
        apiSpecs.AddCommand("details", new GetWorkloadApisCommand(loggerFactory.CreateLogger<GetWorkloadApisCommand>()));

    }
}
