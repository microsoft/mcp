// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.LiveTests;

public sealed class ProductGetCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private const string ProductKey = "product";
    private const string ProductId = "test_test_pmc2pc1.vmsr_uat_beta";
    private const string Language = "en";
    private const string Market = "US";

    [Fact]
    public async Task Should_get_marketplace_product()
    {
        var result = await CallToolAsync(
            "marketplace_product_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "product-id", ProductId }
            });

        var product = result.AssertProperty(ProductKey);
        Assert.Equal(JsonValueKind.Object, product.ValueKind);

        var id = product.AssertProperty("uniqueProductId");
        Assert.Equal(JsonValueKind.String, id.ValueKind);
        Assert.Contains(ProductId, id.GetString());
    }

    [Fact]
    public async Task Should_get_marketplace_product_with_language_option()
    {
        var result = await CallToolAsync(
            "marketplace_product_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
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
    public async Task Should_get_marketplace_product_with_market_option()
    {
        var result = await CallToolAsync(
            "marketplace_product_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
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
    public async Task Should_get_marketplace_product_with_multiple_options()
    {
        var result = await CallToolAsync(
            "marketplace_product_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
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
