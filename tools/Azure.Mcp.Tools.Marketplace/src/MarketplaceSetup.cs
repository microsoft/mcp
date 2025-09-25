// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Marketplace;

public class MarketplaceSetup : IAreaSetup
{
    public string Name => "marketplace";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();

        services.AddSingleton<ProductGetCommand>();
        services.AddSingleton<ProductListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Marketplace command group
        var marketplace = new CommandGroup(Name, "Marketplace operations - Commands for managing and accessing Azure Marketplace products and offers.");

        // Create Product subgroup
        var product = new CommandGroup("product", "Marketplace product operations - Commands for retrieving and managing marketplace products.");
        marketplace.AddSubGroup(product);

        // Register Product commands
        var productGet = serviceProvider.GetRequiredService<ProductGetCommand>();
        product.AddCommand(productGet.Name, productGet);
        var productList = serviceProvider.GetRequiredService<ProductListCommand>();
        product.AddCommand(productList.Name, productList);

        return marketplace;
    }
}
