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
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var fabricCore = new CommandGroup(Name,
            """
            Microsoft Fabric Core Operations - Create and manage Fabric items.
            Use this tool when you need to:
            - Create new Fabric items (Lakehouses, Notebooks, etc.)
            - Manage core Fabric workspace items
            This tool provides core operations for working with Fabric resources.
            """);

        var createItem = serviceProvider.GetRequiredService<ItemCreateCommand>();
        fabricCore.AddCommand(createItem.Name, createItem);

        return fabricCore;
    }
}
