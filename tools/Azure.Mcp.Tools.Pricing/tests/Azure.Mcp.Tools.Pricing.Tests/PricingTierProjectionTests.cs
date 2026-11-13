// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.Pricing.Commands;
using Azure.Mcp.Tools.Pricing.Models;
using Azure.Mcp.Tools.Pricing.Services;
using Azure.Mcp.Tools.Pricing.Tests.TestSupport;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Pricing.Tests;

/// <summary>
/// Projects reported tier shapes through the maintained client, mapper, and pricing get serializer.
/// The numbers are fixture data from the public bug report, not a current tariff or a purchase recommendation.
/// </summary>
public sealed class PricingTierProjectionTests
{
    private const string ZoneMeterId = "0b608a26-f611-4232-8192-ce81b6b57194";
    private const string RecordMeterId = "11111111-1111-1111-1111-111111111111";
    private const string IngressMeterId = "22222222-2222-2222-2222-222222222222";

    [Fact]
    public async Task PricingGet_PreservesTierThresholdsAndRelatedRowFields()
    {
        var handler = new RecordingHttpMessageHandler((_, _) => CreateFixtureResponse());
        PricingService service = CreateService(handler);
        var command = new PricingGetCommand(NullLogger<PricingGetCommand>.Instance, service);
        var context = new CommandContext();

        CommandResponse response = await ((IBaseCommand)command).ExecuteAsync(
            context,
            command.GetCommand().Parse("--filter \"meterName eq 'Private Zone'\""),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal("prices.azure.com", requestUri.Host);
        Assert.Contains("meterName", requestUri.Query, StringComparison.Ordinal);

        string json = JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement prices = document.RootElement.GetProperty("results").GetProperty("prices");
        JsonElement[] rows = prices.EnumerateArray().ToArray();
        Assert.Equal(7, rows.Length);

        AssertRow(rows[0], tier: 0, retail: 0.5, unit: 0.5, currency: "USD", unitOfMeasure: "1", meterId: ZoneMeterId, meterName: "Private Zone", skuName: "Private", serviceName: "Azure DNS");
        AssertRow(rows[1], tier: 25, retail: 0.1, unit: 0.1, currency: "USD", unitOfMeasure: "1", meterId: ZoneMeterId, meterName: "Private Zone", skuName: "Private", serviceName: "Azure DNS");
        AssertRow(rows[2], tier: 0, retail: 0, unit: 0, currency: "USD", unitOfMeasure: "1", meterId: RecordMeterId, meterName: "Private Record Set", skuName: "Private", serviceName: "Azure DNS");
        AssertRow(rows[3], tier: 25000, retail: 0.0015, unit: 0.0015, currency: "USD", unitOfMeasure: "1", meterId: RecordMeterId, meterName: "Private Record Set", skuName: "Private", serviceName: "Azure DNS");
        AssertRow(rows[4], tier: 0, retail: 0.01, unit: 0.01, currency: "USD", unitOfMeasure: "1 GB", meterId: IngressMeterId, meterName: "Standard Data Processed - Ingress", skuName: "Standard", serviceName: "Virtual Network");
        AssertRow(rows[5], tier: 1000000, retail: 0.006, unit: 0.006, currency: "USD", unitOfMeasure: "1 GB", meterId: IngressMeterId, meterName: "Standard Data Processed - Ingress", skuName: "Standard", serviceName: "Virtual Network");
        AssertRow(rows[6], tier: 5000000, retail: 0.004, unit: 0.004, currency: "USD", unitOfMeasure: "1 GB", meterId: IngressMeterId, meterName: "Standard Data Processed - Ingress", skuName: "Standard", serviceName: "Virtual Network");

        PricingGetCommand.PricingGetCommandResult? typed = JsonSerializer.Deserialize(
            JsonSerializer.Serialize(response.Results),
            PricingJsonContext.Default.PricingGetCommandResult);
        Assert.NotNull(typed);
        Assert.Equal(7, typed.Prices.Count);
        Assert.Equal(ZoneMeterId, typed.Prices[0].MeterId);
        Assert.Equal(ZoneMeterId, typed.Prices[1].MeterId);
        Assert.Equal(0, typed.Prices[0].TierMinimumUnits);
        Assert.Equal(25, typed.Prices[1].TierMinimumUnits);
        Assert.Equal(0, typed.Prices[2].TierMinimumUnits);
        Assert.Equal(25000, typed.Prices[3].TierMinimumUnits);
        Assert.Equal(0, typed.Prices[4].TierMinimumUnits);
        Assert.Equal(1000000, typed.Prices[5].TierMinimumUnits);
        Assert.Equal(5000000, typed.Prices[6].TierMinimumUnits);
        Assert.Equal(0.5, typed.Prices[0].RetailPrice);
        Assert.Equal(0.1, typed.Prices[1].RetailPrice);
        Assert.Equal(0.1, typed.Prices[1].UnitPrice);
        Assert.Equal("USD", typed.Prices[1].CurrencyCode);
        Assert.Equal("1", typed.Prices[1].UnitOfMeasure);
        Assert.Equal("Consumption", typed.Prices[1].PriceType);
        Assert.Equal("Networking", typed.Prices[1].ServiceFamily);
        Assert.Equal("Zone 1", typed.Prices[1].Region);
        IReadOnlyList<SavingsPlanPrice> savingsPlans = typed.Prices[1].SavingsPlan ?? throw new InvalidOperationException("savings plan was not mapped");
        SavingsPlanPrice savingsPlan = Assert.Single(savingsPlans);
        Assert.Equal("1 Year", savingsPlan.Term);
        Assert.Equal(0.08, savingsPlan.UnitPrice);
        Assert.Equal(0.08, savingsPlan.RetailPrice);
        Assert.Null(typed.Prices[0].SavingsPlan);
        Assert.Equal(0, typed.Prices[2].RetailPrice);
        Assert.Equal(0, typed.Prices[2].UnitPrice);
        Assert.Equal(0.0015, typed.Prices[3].RetailPrice);
        Assert.Equal(DateTimeOffset.Parse("2026-01-15T00:00:00Z"), typed.Prices[4].EffectiveStartDate);
        Assert.True(typed.Prices[4].IsPrimaryMeterRegion);
        Assert.Equal("1 GB", typed.Prices[6].UnitOfMeasure);
        Assert.Equal(IngressMeterId, typed.Prices[4].MeterId);
        Assert.Equal(IngressMeterId, typed.Prices[5].MeterId);
        Assert.Equal(IngressMeterId, typed.Prices[6].MeterId);
    }

    private static void AssertRow(
        JsonElement price,
        double tier,
        double retail,
        double unit,
        string currency,
        string unitOfMeasure,
        string meterId,
        string meterName,
        string skuName,
        string serviceName)
    {
        Assert.True(
            price.TryGetProperty("tierMinimumUnits", out JsonElement tierElement),
            $"pricing get dropped tierMinimumUnits for {meterName} retailPrice {retail}.");
        Assert.Equal(tier, tierElement.GetDouble());
        Assert.Equal(retail, price.GetProperty("retailPrice").GetDouble());
        Assert.Equal(unit, price.GetProperty("unitPrice").GetDouble());
        Assert.Equal(currency, price.GetProperty("currencyCode").GetString());
        Assert.Equal(unitOfMeasure, price.GetProperty("unitOfMeasure").GetString());
        Assert.Equal(meterId, price.GetProperty("meterId").GetString());
        Assert.Equal(meterName, price.GetProperty("meterName").GetString());
        Assert.Equal(skuName, price.GetProperty("skuName").GetString());
        Assert.Equal(serviceName, price.GetProperty("serviceName").GetString());
        Assert.Equal("Consumption", price.GetProperty("priceType").GetString());
        Assert.Equal("Networking", price.GetProperty("serviceFamily").GetString());
    }

    private static PricingService CreateService(HttpMessageHandler handler)
    {
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        HttpClientPipelineTransport transport = new(new HttpClient(handler, disposeHandler: false));
        return new PricingService(
            cloudConfiguration,
            transport,
            new EndpointValidator(
                new SsrfProtectionPolicy(null),
                NullLogger<EndpointValidator>.Instance,
                new CommandContextAccessor()));
    }

    private static HttpResponseMessage CreateFixtureResponse()
    {
        const string content = """
            {
              "BillingCurrency": "USD",
              "CustomerEntityId": "Default",
              "CustomerEntityType": "Retail",
              "Items": [
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 0.0,
                  "retailPrice": 0.5,
                  "unitPrice": 0.5,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "0b608a26-f611-4232-8192-ce81b6b57194",
                  "meterName": "Private Zone",
                  "productId": "fixture-dns-product",
                  "skuId": "fixture-dns-sku",
                  "productName": "Azure DNS",
                  "skuName": "Private",
                  "serviceName": "Azure DNS",
                  "serviceId": "fixture-dns-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Private"
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 25.0,
                  "retailPrice": 0.1,
                  "unitPrice": 0.1,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "0b608a26-f611-4232-8192-ce81b6b57194",
                  "meterName": "Private Zone",
                  "productId": "fixture-dns-product",
                  "skuId": "fixture-dns-sku",
                  "productName": "Azure DNS",
                  "skuName": "Private",
                  "serviceName": "Azure DNS",
                  "serviceId": "fixture-dns-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Private",
                  "savingsPlan": [
                    { "unitPrice": 0.08, "retailPrice": 0.08, "term": "1 Year" }
                  ]
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 0.0,
                  "retailPrice": 0.0,
                  "unitPrice": 0.0,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "11111111-1111-1111-1111-111111111111",
                  "meterName": "Private Record Set",
                  "productId": "fixture-record-product",
                  "skuId": "fixture-record-sku",
                  "productName": "Azure DNS",
                  "skuName": "Private",
                  "serviceName": "Azure DNS",
                  "serviceId": "fixture-record-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Private"
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 25000.0,
                  "retailPrice": 0.0015,
                  "unitPrice": 0.0015,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "11111111-1111-1111-1111-111111111111",
                  "meterName": "Private Record Set",
                  "productId": "fixture-record-product",
                  "skuId": "fixture-record-sku",
                  "productName": "Azure DNS",
                  "skuName": "Private",
                  "serviceName": "Azure DNS",
                  "serviceId": "fixture-record-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Private"
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 0.0,
                  "retailPrice": 0.01,
                  "unitPrice": 0.01,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "22222222-2222-2222-2222-222222222222",
                  "meterName": "Standard Data Processed - Ingress",
                  "productId": "fixture-ingress-product",
                  "skuId": "fixture-ingress-sku",
                  "productName": "Virtual Network Private Link",
                  "skuName": "Standard",
                  "serviceName": "Virtual Network",
                  "serviceId": "fixture-ingress-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1 GB",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Standard"
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 1000000.0,
                  "retailPrice": 0.006,
                  "unitPrice": 0.006,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "22222222-2222-2222-2222-222222222222",
                  "meterName": "Standard Data Processed - Ingress",
                  "productId": "fixture-ingress-product",
                  "skuId": "fixture-ingress-sku",
                  "productName": "Virtual Network Private Link",
                  "skuName": "Standard",
                  "serviceName": "Virtual Network",
                  "serviceId": "fixture-ingress-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1 GB",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Standard"
                },
                {
                  "currencyCode": "USD",
                  "tierMinimumUnits": 5000000.0,
                  "retailPrice": 0.004,
                  "unitPrice": 0.004,
                  "armRegionName": "Zone 1",
                  "location": "Zone 1",
                  "effectiveStartDate": "2026-01-15T00:00:00Z",
                  "meterId": "22222222-2222-2222-2222-222222222222",
                  "meterName": "Standard Data Processed - Ingress",
                  "productId": "fixture-ingress-product",
                  "skuId": "fixture-ingress-sku",
                  "productName": "Virtual Network Private Link",
                  "skuName": "Standard",
                  "serviceName": "Virtual Network",
                  "serviceId": "fixture-ingress-service",
                  "serviceFamily": "Networking",
                  "unitOfMeasure": "1 GB",
                  "type": "Consumption",
                  "isPrimaryMeterRegion": true,
                  "armSkuName": "Standard"
                }
              ],
              "NextPageLink": null,
              "Count": 7
            }
            """;

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }
}
