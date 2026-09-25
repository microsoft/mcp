// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Security;
using System.Text;
using Azure.Mcp.Tools.Pricing.Models;
using Azure.Mcp.Tools.Pricing.Services;
using Azure.Mcp.Tools.Pricing.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Pricing.Tests;

public sealed class PricingServiceEndpointValidationTests
{
    [Theory]
    [InlineData("Public", "prices.azure.com")]
    [InlineData("China", "prices.azure.cn")]
    [InlineData("Government", "prices.azure.us")]
    public async Task GetPricesAsync_UsesConfiguredCloudEndpoint(string cloud, string expectedHost)
    {
        var handler = new RecordingHttpMessageHandler(
            (_, _) => CreatePricingResponse(nextPageLink: null));
        PricingService service = CreateService(GetArmEnvironment(cloud), handler);

        List<PriceItem> results = await service.GetPricesAsync(
            sku: "Standard_D4s_v5",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(results);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal(expectedHost, requestUri.Host);
    }

    [Fact]
    public async Task GetPricesAsync_WithValidNextPageLink_RequestsNextPage()
    {
        var handler = new RecordingHttpMessageHandler(
            (_, requestNumber) => requestNumber == 1
                ? CreatePricingResponse("https://prices.azure.com:443/api/retail/prices?$skip=1000")
                : CreatePricingResponse(nextPageLink: null));
        PricingService service = CreateService(ArmEnvironment.AzurePublicCloud, handler);

        List<PriceItem> results = await service.GetPricesAsync(
            sku: "Standard_D4s_v5",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(results);
        Assert.Equal(2, handler.RequestUris.Count);
        Assert.All(handler.RequestUris, requestUri => Assert.Equal("prices.azure.com", requestUri.Host));
        Assert.Equal(443, handler.RequestUris[1].Port);
    }

    [Fact]
    public async Task GetPricesAsync_WithAttackerNextPageLink_RejectsBeforeSecondRequest()
    {
        var handler = new RecordingHttpMessageHandler(
            (_, _) => CreatePricingResponse("https://evil.example/api/retail/prices?$skip=1000"));
        PricingService service = CreateService(ArmEnvironment.AzurePublicCloud, handler);

        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            service.GetPricesAsync(
                sku: "Standard_D4s_v5",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Contains("not a valid pricing domain", exception.Message);
        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task GetPricesAsync_WithAuthorityLikeFilter_EscapesFilterAndPreservesHost()
    {
        var handler = new RecordingHttpMessageHandler(
            (_, _) => CreatePricingResponse(nextPageLink: null));
        PricingService service = CreateService(ArmEnvironment.AzurePublicCloud, handler);

        List<PriceItem> results = await service.GetPricesAsync(
            filter: "serviceName eq 'Storage'&redirect=https://evil.example/#fragment",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Empty(results);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal("prices.azure.com", requestUri.Host);
        Assert.Contains(
            "%26redirect%3Dhttps%3A%2F%2Fevil.example%2F%23fragment",
            requestUri.Query,
            StringComparison.OrdinalIgnoreCase);
    }

    private static PricingService CreateService(
        ArmEnvironment armEnvironment,
        HttpMessageHandler handler)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(armEnvironment);

        HttpClientPipelineTransport transport = new(
            new HttpClient(handler, disposeHandler: false));

        return new PricingService(cloudConfiguration, transport);
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };

    private static HttpResponseMessage CreatePricingResponse(string? nextPageLink)
    {
        string nextPageProperty = nextPageLink is null
            ? "\"NextPageLink\":null"
            : $"\"NextPageLink\":\"{nextPageLink}\"";
        string content = $$"""
            {
              "BillingCurrency": "USD",
              "CustomerEntityId": "Default",
              "CustomerEntityType": "Retail",
              "Items": [],
              {{nextPageProperty}},
              "Count": 0
            }
            """;

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }
}
