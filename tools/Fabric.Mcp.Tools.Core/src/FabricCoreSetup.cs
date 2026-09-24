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
        services.AddSingleton<ItemCreateCommand>();
        services.AddSingleton<CatalogSearchCommand>();
        services.AddSingleton<ItemGetCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricCore = new CommandGroup(Name,
            """
            Microsoft Fabric Core Operations - Search, create, and manage Fabric items.
            Use this tool when you need to:
            - Search the OneLake catalog to discover Fabric items across workspaces
            - Get metadata for a known Fabric item by workspace ID and item ID
            - Create new Fabric items (Lakehouse, Notebook, etc.)
            - Manage core Fabric workspace items
            This tool provides core operations for working with Fabric resources.
            """);

        fabricCore.AddCommand<ItemCreateCommand>(serviceProvider);
        fabricCore.AddCommand<CatalogSearchCommand>(serviceProvider);
        fabricCore.AddCommand<ItemGetCommand>(serviceProvider);

        return fabricCore;
    }
}
