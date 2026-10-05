// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core;

public class FabricCoreSetup : IAreaSetup
{
    public string Name => "core";
    public string Title => "Microsoft Fabric Core";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<IFabricCoreService, FabricCoreService>();
        services.AddSingleton<CapacityGetCommand>();
        services.AddSingleton<CapacityListCommand>();
        services.AddSingleton<ItemCreateCommand>();
        services.AddSingleton<ItemListCommand>();
        services.AddSingleton<CatalogSearchCommand>();
        services.AddSingleton<WorkspaceGetCommand>();
        services.AddSingleton<WorkspaceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricCore = new CommandGroup(Name,
            "Microsoft Fabric Core Operations - Inspect capacities, discover workspaces, and search, create, and manage Fabric items.\n" +
            "Use this tool when you need to:\n" +
            "- Get metadata for a known Fabric capacity\n" +
            "- List accessible Fabric capacities and their metadata\n" +
            "- Search the OneLake catalog to discover Fabric items across workspaces\n" +
            "- Get metadata for an existing Fabric workspace by ID\n" +
            "- List accessible workspaces and their management metadata, optionally filtered by the caller's workspace roles\n" +
            "- List item metadata within a known workspace or folder, optionally filtered by type\n" +
            "- Create new Fabric items (Lakehouse, Notebook, etc.)\n" +
            "- Manage core Fabric workspace items\n" +
            "This tool provides core operations for working with Fabric resources.");

        fabricCore.AddCommand<CapacityGetCommand>(serviceProvider);
        fabricCore.AddCommand<CapacityListCommand>(serviceProvider);
        fabricCore.AddCommand<ItemCreateCommand>(serviceProvider);
        fabricCore.AddCommand<ItemListCommand>(serviceProvider);
        fabricCore.AddCommand<CatalogSearchCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceGetCommand>(serviceProvider);
        fabricCore.AddCommand<WorkspaceListCommand>(serviceProvider);

        return fabricCore;
    }
}
