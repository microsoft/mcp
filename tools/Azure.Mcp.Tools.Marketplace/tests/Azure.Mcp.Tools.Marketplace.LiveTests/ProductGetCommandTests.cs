// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.LiveTests;

[Trait("Area", "Marketplace")]
public class ProductGetCommandTests : CommandTestsBase,
    IClassFixture<LiveTestFixture>
{
    private const string ProductKey = "product";
    private const string ProductId = "test_test_pmc2pc1.vmsr_uat_beta";
    private const string Language = "en";
    private const string Market = "US";
    private readonly MarketplaceService _marketplaceService;
    private readonly string _subscriptionId;

    public ProductGetCommandTests(LiveTestFixture liveTestFixture, ITestOutputHelper output) : base(liveTestFixture, output)
    {
        var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()));
        var cacheService = new CacheService(memoryCache);
        var tenantService = new TenantService(cacheService);
        _marketplaceService = new MarketplaceService(tenantService);
        _subscriptionId = Settings.SubscriptionId;
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_get_marketplace_product()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "product-id", ProductId }
            });

        var product = result.AssertProperty(ProductKey);
        Assert.Equal(JsonValueKind.Object, product.ValueKind);

        var id = product.AssertProperty("uniqueProductId");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.Contains(ProductId, id.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_get_marketplace_product_with_language_option()
    {
        var result = await CallToolAsync(
            "azmcp_marketplace_product_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "product-id", ProductId },
                { "language", Language }
            });

        var product = result.AssertProperty(ProductKey);
        Assert.Equal(JsonValueKind.Object, product.ValueKind);

        var id = product.AssertProperty("uniqueProductId");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.Contains(ProductId, id.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_get_marketplace_product_with_market_option()
    {


        var result = await CallToolAsync(
            "azmcp_marketplace_product_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "product-id", ProductId },
                { "market", Market }
            });

        var product = result.AssertProperty(ProductKey);
        Assert.Equal(JsonValueKind.Object, product.ValueKind);

        var id = product.AssertProperty("uniqueProductId");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.Contains(ProductId, id.GetString());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task Should_get_marketplace_product_with_multiple_options()
    {

        var result = await CallToolAsync(
            "azmcp_marketplace_product_get",
            new()
            {
                { "subscription", _subscriptionId },
                { "product-id", ProductId },
                { "language", Language },
                { "market", Market },
                { "include-hidden-plans", true },
                { "include-service-instruction-templates", true }
            });

        var product = result.AssertProperty(ProductKey);
        Assert.Equal(JsonValueKind.Object, product.ValueKind);

        var id = product.AssertProperty("uniqueProductId");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.Contains(ProductId, id.GetString());
    }
}
