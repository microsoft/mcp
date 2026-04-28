// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Models;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.UnitTests.Product;

public class ProductGetCommandTests : CommandUnitTestsBase<ProductGetCommand, IMarketplaceService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Contains("marketplace product", CommandDefinition.Description.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var productId = "test-product";
        var expectedProduct = new ProductDetails
        {
            UniqueProductId = "test-product",
            DisplayName = "Test Product"
        };

        Service.GetProduct(
            Arg.Is(productId),
            Arg.Is(subscriptionId),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedProduct);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--product-id", productId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        // Arrange
        TestEnvironment.ClearAzureSubscriptionId();

        // Act
        var response = await ExecuteCommandAsync("--product-id", "test-product");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "test-sub";
        var productId = "test-product";

        Service.GetProduct(
            Arg.Is(productId),
            Arg.Is(subscriptionId),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--product-id", productId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
