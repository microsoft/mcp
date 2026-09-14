// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Marketplace.Models;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.Tests.Models;

public class ProductDeserializationTests
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

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

        var product = JsonSerializer.Deserialize<ProductSummary>(json, s_options);

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

        var plan = JsonSerializer.Deserialize<PlanSummary>(json, s_options);

        Assert.NotNull(plan);
        Assert.Equal(["SomeFutureValue"], plan.PricingTypes);
        Assert.Equal(["SomeFutureSecurityType"], plan.VmSecurityTypes);
        Assert.Equal("SomeFutureState", plan.CspState);
        Assert.Equal("SomeFutureArchitecture", plan.VmArchitectureType);
    }
}
