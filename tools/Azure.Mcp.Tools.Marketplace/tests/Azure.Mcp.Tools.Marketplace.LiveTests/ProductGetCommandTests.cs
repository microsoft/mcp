// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.LiveTests;

public sealed class ProductGetCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private const string ProductKey = "product";
    private const string ProductId = "test_test_pmc2pc1.vmsr_uat_beta";
    private const string Language = "en";

    [Fact]
    public async Task Should_get_marketplace_product()
    {
        Assert.SkipWhen(Settings.IsAzureUSGovernment, "Marketplace product catalog test product is not published in Azure US Government");
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
        Assert.SkipWhen(Settings.IsAzureUSGovernment, "Marketplace product catalog test product is not published in Azure US Government");
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
    public async Task Should_get_marketplace_product_with_multiple_options()
    {
        Assert.SkipWhen(Settings.IsAzureUSGovernment, "Marketplace product catalog test product is not published in Azure US Government");
        var result = await CallToolAsync(
            "marketplace_product_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "product-id", ProductId },
                { "language", Language },
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
