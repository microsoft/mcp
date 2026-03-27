// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Marketplace;

public sealed class MarketplaceRegistration : IAreaRegistration
{
    public static string AreaName => "marketplace";

    public static string AreaTitle => "Azure Marketplace";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Marketplace operations - Commands for managing and accessing Azure Marketplace products and offers.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "product",
                Description = "Marketplace product operations - Commands for retrieving and managing marketplace products.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "729a12ee-9c63-4a31-b1b8-4a81ad093564",
                        Name = "get",
                        Description = "Retrieves detailed information about a specific Azure Marketplace product (offer) for a given subscription, including available plans, pricing, and product metadata.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "product-id",
                                Description = "The ID of the marketplace product to retrieve. This is the unique identifier for the product in the Azure Marketplace.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "include-stop-sold-plans",
                                Description = "Include stop-sold or hidden plans in the response.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "language",
                                Description = "Product language code (e.g., 'en' for English, 'fr' for French).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "market",
                                Description = "Product market code (e.g., 'US' for United States, 'UK' for United Kingdom).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "lookup-offer-in-tenant-level",
                                Description = "Check against tenant private audience when retrieving the product.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "plan-id",
                                Description = "Filter results by a specific plan ID.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "sku-id",
                                Description = "Filter results by a specific SKU ID.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "include-service-instruction-templates",
                                Description = "Include service instruction templates in the response.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "pricing-audience",
                                Description = "Pricing audience for the request header.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ProductGetCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "0485e8f9-61bf-4baf-b914-7fa5530a6f78",
                        Name = "list",
                        Description = "Retrieves and lists all marketplace products (offers) available to a subscription in the Azure Marketplace. Use this tool to search, select, browse, or filter marketplace offers by product name, publisher, pricing, or metadata. Returns information for each product, including display name, publisher details, category, pricing data, and available plans.",
                        Title = "List",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "language",
                                Description = "Product language code (e.g., 'en' for English, 'fr' for French).",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "search",
                                Description = "Search for products using a short general term (up to 25 characters)",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "filter",
                                Description = "OData filter expression to filter results based on ProductSummary properties (e.g., \"displayName eq 'Azure'\").",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "orderby",
                                Description = "OData orderby expression to sort results by ProductSummary fields (e.g., \"displayName asc\" or \"popularity desc\").",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "select",
                                Description = "OData select expression to choose specific ProductSummary fields to return (e.g., \"displayName,publisherDisplayName,uniqueProductId\").",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "next-cursor",
                                Description = "Pagination cursor to retrieve the next page of results. Use the NextPageLink value from a previous response.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "expand",
                                Description = "OData expand expression to include related data in the response (e.g., \"plans\" to include plan details).",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(ProductListCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMarketplaceService, MarketplaceService>();
        services.AddSingleton<ProductGetCommand>();
        services.AddSingleton<ProductListCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(ProductGetCommand) => serviceProvider.GetRequiredService<ProductGetCommand>(),
            nameof(ProductListCommand) => serviceProvider.GetRequiredService<ProductListCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in marketplace area.")
        };
}
