// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Marketplace.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.Tests.Models;

public class ProductDeserializationTests
{
    [Fact]
    public void Deserialize_ProductSummaryWithValuesOutsideDocumentedSet_DoesNotThrow()
    {
        // The catalog API models these fields as open enums (x-ms-enum modelAsString), so
        // values outside the documented set must round-trip instead of failing the whole page.
        const string json = """
        {
          "uniqueProductId": "test-product",
          "pricingTypes": ["Free", "RequestPrivateOffer", "SomeFutureValue"],
          "azureBenefit": "SomeFutureBenefit",
          "publisherType": "ThirdParty",
          "publishingStage": "Public",
          "ratingBuckets": ["AboveFour", "SomeFutureBucket"]
        }
        """;

        var product = JsonSerializer.Deserialize(json, MarketplaceJsonContext.Default.ProductSummary);

        Assert.NotNull(product);
        Assert.Equal(["Free", "RequestPrivateOffer", "SomeFutureValue"], product.PricingTypes);
        Assert.Equal("SomeFutureBenefit", product.AzureBenefit);
        Assert.Equal("ThirdParty", product.PublisherType);
        Assert.Equal(["AboveFour", "SomeFutureBucket"], product.RatingBuckets);
    }

    [Fact]
    public void Deserialize_PlanSummaryWithValuesOutsideDocumentedSet_DoesNotThrow()
    {
        const string json = """
        {
          "planId": "test-plan",
          "pricingTypes": ["SomeFutureValue"],
          "vmSecurityTypes": ["SomeFutureSecurityType"],
          "cspState": "SomeFutureState",
          "vmArchitectureType": "SomeFutureArchitecture"
        }
        """;

        var plan = JsonSerializer.Deserialize(json, MarketplaceJsonContext.Default.PlanSummary);

        Assert.NotNull(plan);
        Assert.Equal(["SomeFutureValue"], plan.PricingTypes);
        Assert.Equal(["SomeFutureSecurityType"], plan.VmSecurityTypes);
        Assert.Equal("SomeFutureState", plan.CspState);
        Assert.Equal("SomeFutureArchitecture", plan.VmArchitectureType);
    }

    [Fact]
    public void Deserialize_ProductDetailsWithValuesOutsideDocumentedSet_DoesNotThrow()
    {
        const string json = """
        {
          "uniqueProductId": "test-product",
          "legalTermsType": "SomeFutureTermsType",
          "stopSellInfo": {
            "reason": "SomeFutureReason"
          },
          "artifacts": [
            {
              "name": "test-artifact",
              "type": "SomeFutureArtifactType"
            }
          ]
        }
        """;

        var product = JsonSerializer.Deserialize(json, MarketplaceJsonContext.Default.ProductDetails);

        Assert.NotNull(product);
        Assert.Equal("SomeFutureTermsType", product.LegalTermsType);
        Assert.Equal("SomeFutureReason", product.StopSellInfo?.Reason);
        Assert.Equal("SomeFutureArtifactType", Assert.Single(product.Artifacts!).Type);
    }

    [Fact]
    public void Deserialize_ProductListPageWithOneUnknownValue_ReturnsEveryProduct()
    {
        // A single unmodeled value must not fail the surrounding page (issue #3029).
        const string json = """
        {
          "value": [
            { "uniqueProductId": "product-1", "pricingTypes": ["Free"] },
            { "uniqueProductId": "product-2", "pricingTypes": ["SomeFutureValue"] },
            { "uniqueProductId": "product-3", "pricingTypes": ["Payg"] }
          ]
        }
        """;

        var response = JsonSerializer.Deserialize(json, MarketplaceJsonContext.Default.ProductsListResponse);

        Assert.NotNull(response);
        Assert.Equal(3, response.Value?.Count);
    }
}
