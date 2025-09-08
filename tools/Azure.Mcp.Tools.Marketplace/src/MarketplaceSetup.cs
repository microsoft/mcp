// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Marketplace;

public class MarketplaceSetup : IAreaSetup
{
    public string Name => "marketplace";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Marketplace command group
        var marketplace = new CommandGroup(Name, "Marketplace operations - Commands for managing and accessing Azure Marketplace products and offers.");
        rootGroup.AddSubGroup(marketplace);

        // Create Product subgroup
        var product = new CommandGroup("product", "Marketplace product operations - Commands for retrieving and managing marketplace products.");
        marketplace.AddSubGroup(product);

        // Register Product commands
        product.AddCommand("get", new ProductGetCommand(
            loggerFactory.CreateLogger<ProductGetCommand>()));
        product.AddCommand("list", new ProductListCommand(
            loggerFactory.CreateLogger<ProductListCommand>()));
    }
}
