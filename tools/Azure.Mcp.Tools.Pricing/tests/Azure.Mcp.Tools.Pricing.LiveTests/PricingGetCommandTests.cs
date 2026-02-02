// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Pricing.LiveTests;

/// <summary>
/// Live tests for the Pricing toolset.
/// These tests call the real Azure Retail Prices API (https://prices.azure.com).
/// The API is public and does not require authentication.
/// </summary>
public sealed class PricingGetCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    private const string PricesKey = "prices";
    public override bool EnableDefaultSanitizerAdditions => false;

    [Fact]
    public async Task Should_get_prices_by_sku()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "Standard_D4s_v5" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        // Check that we have at least one price
        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate first price item has expected properties
        var price = priceArray[0];
        price.AssertProperty("armSkuName");
        price.AssertProperty("serviceName");
        price.AssertProperty("retailPrice");
        price.AssertProperty("currencyCode");
    }

    [Fact]
    public async Task Should_get_prices_by_service()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "service", "Virtual Machines" },
                { "region", "eastus" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate that all returned items are for Virtual Machines
        foreach (var price in priceArray.Take(10))
        {
            var serviceName = price.AssertProperty("serviceName").GetString();
            Assert.Equal("Virtual Machines", serviceName);
        }
    }

    [Fact]
    public async Task Should_get_prices_by_region()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "region", "westeurope" },
                { "service-family", "Compute" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate first price item has region set
        var price = priceArray[0];
        var region = price.AssertProperty("region").GetString();
        Assert.Equal("westeurope", region);
    }

    [Fact]
    public async Task Should_get_prices_by_service_family()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "service-family", "Storage" },
                { "region", "eastus" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate that all returned items are for Storage family
        foreach (var price in priceArray.Take(10))
        {
            var serviceFamily = price.AssertProperty("serviceFamily").GetString();
            Assert.Equal("Storage", serviceFamily);
        }
    }

    [Fact]
    public async Task Should_get_prices_by_price_type()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "Standard_D4s_v5" },
                { "price-type", "Consumption" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate that all returned items are Consumption type
        foreach (var price in priceArray)
        {
            var priceType = price.AssertProperty("priceType").GetString();
            Assert.Equal("Consumption", priceType);
        }
    }

    [Fact]
    public async Task Should_get_prices_with_currency()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "Standard_D4s_v5" },
                { "currency", "EUR" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate that all returned items are in EUR
        foreach (var price in priceArray)
        {
            var currencyCode = price.AssertProperty("currencyCode").GetString();
            Assert.Equal("EUR", currencyCode);
        }
    }

    [Fact]
    public async Task Should_get_prices_with_raw_filter()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "filter", "serviceName eq 'Storage' and armRegionName eq 'eastus'" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate that all returned items match the filter
        foreach (var price in priceArray.Take(10))
        {
            var serviceName = price.AssertProperty("serviceName").GetString();
            var region = price.AssertProperty("region").GetString();
            Assert.Equal("Storage", serviceName);
            Assert.Equal("eastus", region);
        }
    }

    [Fact]
    public async Task Should_get_prices_with_combined_filters()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "Standard_D4s_v5" },
                { "region", "eastus" },
                { "price-type", "Consumption" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Validate all filters are applied
        foreach (var price in priceArray)
        {
            var sku = price.AssertProperty("armSkuName").GetString();
            var region = price.AssertProperty("region").GetString();
            var priceType = price.AssertProperty("priceType").GetString();

            Assert.Equal("Standard_D4s_v5", sku);
            Assert.Equal("eastus", region);
            Assert.Equal("Consumption", priceType);
        }
    }

    [Fact]
    public async Task Should_return_reservation_prices()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "Standard_D4s_v5" },
                { "price-type", "Reservation" },
                { "region", "eastus" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.NotEmpty(priceArray);

        // Reservation prices should have reservation term
        var price = priceArray[0];
        price.AssertProperty("priceType");
        // ReservationTerm may be present for reservation prices
    }

    [Fact]
    public async Task Should_return_empty_for_nonexistent_sku()
    {
        var result = await CallToolAsync(
            "pricing_get",
            new()
            {
                { "sku", "NonExistent_SKU_That_Does_Not_Exist_12345" }
            });

        var prices = result.AssertProperty(PricesKey);
        Assert.Equal(JsonValueKind.Array, prices.ValueKind);

        var priceArray = prices.EnumerateArray().ToArray();
        Assert.Empty(priceArray);
    }
}
