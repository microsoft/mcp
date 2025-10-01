// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.LiveTests;

[Trait("Area", "Marketplace")]
public class ProductListCommandTests : CommandTestsBase
{
    private const string ProductsKey = "products";
    private const string Language = "en";
    private readonly MarketplaceService _marketplaceService;

    public ProductListCommandTests(ITestOutputHelper output) : base(output)
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        _marketplaceService = new MarketplaceService(tenantService);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Check that we have at least one product
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
        var product = productArray[0];
        product.AssertProperty("uniqueProductId");
        product.AssertProperty("displayName");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_language_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "language", Language }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Check that we have at least one product
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_french_language_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "language", "fr" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Check that we have at least one product
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_language_and_search_options()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "language", Language },
                { "search", "test" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        var productArray = products.EnumerateArray().ToArray();
        var product = productArray[0];
        product.AssertProperty("uniqueProductId");
        product.AssertProperty("displayName");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_search_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "search", "test" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        var productArray = products.EnumerateArray().ToArray();
        var product = productArray[0];
        product.AssertProperty("uniqueProductId");
        product.AssertProperty("displayName");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_multiple_options()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "language", Language },
                { "search", "microsoft" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Results may be filtered, but structure should be valid
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_filter_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "filter", "publisherDisplayName eq 'Microsoft'" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Results may be filtered, but structure should be valid
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_orderby_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "orderby", "displayName asc" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Check that we have at least one product
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_select_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "select", "displayName,uniqueProductId,publisherDisplayName" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Check that we have at least one product
        var productArray = products.EnumerateArray().ToArray();
        // Verify selected properties are present
        Assert.NotEmpty(productArray);
        var product = productArray[0];
        product.AssertProperty("uniqueProductId");
        product.AssertProperty("displayName");
        product.AssertProperty("publisherDisplayName");
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_list_marketplace_products_with_multiple_odata_options()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "filter", "publisherDisplayName eq 'Microsoft'" },
                { "orderby", "displayName desc" },
                { "select", "displayName,uniqueProductId" }
            });

        var products = result.AssertProperty(ProductsKey);
        Assert.Equal(JsonValueKind.Array, products.ValueKind);

        // Results may be filtered, but structure should be valid
        var productArray = products.EnumerateArray().ToArray();
        Assert.NotEmpty(productArray);
        var product = productArray[0];
        product.AssertProperty("uniqueProductId");
        product.AssertProperty("displayName");
    }
}
