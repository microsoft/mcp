// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Models;
using Azure.Mcp.Tools.Marketplace.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.UnitTests.Product;

[Trait("Area", "Marketplace")]
public class ProductGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMarketplaceService _marketplaceService;
    private readonly ILogger<ProductGetCommand> _logger;
    private readonly ProductGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ProductGetCommandTests()
    {
        _marketplaceService = Substitute.For<IMarketplaceService>();
        _logger = Substitute.For<ILogger<ProductGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_marketplaceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("marketplace product", command.Description.ToLower());
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

        _marketplaceService.GetProduct(
            Arg.Is(productId),
            Arg.Is(subscriptionId),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedProduct);

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--product-id", productId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        // Arrange
        var args = _commandDefinition.Parse(["--product-id", "test-product"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

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

        _marketplaceService.GetProduct(
            Arg.Is(productId),
            Arg.Is(subscriptionId),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--product-id", productId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
