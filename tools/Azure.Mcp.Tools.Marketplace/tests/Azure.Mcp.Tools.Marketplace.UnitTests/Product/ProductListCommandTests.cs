// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
public class ProductListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMarketplaceService _marketplaceService;
    private readonly ILogger<ProductListCommand> _logger;
    private readonly ProductListCommand _command;
    private readonly CommandContext _context;
    public ProductListCommandTests()
    {
        _marketplaceService = Substitute.For<IMarketplaceService>();
        _logger = Substitute.For<ILogger<ProductListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_marketplaceService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("marketplace products", command.Description.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product-1",
                DisplayName = "Test Product 1"
            },
            new()
            {
                UniqueProductId = "test-product-2",
                DisplayName = "Test Product 2"
            }
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        var args = _command.GetCommand().Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var search = "azure";
        var language = "en";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product-1",
                DisplayName = "Azure Test Product"
            }
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Is(language),
            Arg.Is(search),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        var args = _command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--search", search,
            "--language", language
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        // Arrange
        var args = _command.GetCommand().Parse(["--search", "test"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResults_ReturnsSuccessWithNullResults()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var emptyProducts = new List<ProductSummary>();

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ProductListResponseWithNextCursor { Items = emptyProducts });

        var args = _command.GetCommand().Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "test-sub";

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _command.GetCommand().Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleODataOptions_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var filter = "displayName eq 'Azure'";
        var orderBy = "displayName asc";
        var select = "displayName,publisherDisplayName";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product",
                DisplayName = "Azure Product"
            }
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(filter),
            Arg.Is(orderBy),
            Arg.Is(select),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        var args = _command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--filter", filter,
            "--orderby", orderBy,
            "--select", select
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithResultsContainingNextCursor_ReturnsNextCursorInResponse()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var expectedNextCursor = "next-page-token-456";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product-1",
                DisplayName = "Test Product 1"
            },
            new()
            {
                UniqueProductId = "test-product-2",
                DisplayName = "Test Product 2"
            }
        };

        var productsListResult = new ProductListResponseWithNextCursor
        {
            Items = expectedProducts,
            NextCursor = expectedNextCursor
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(productsListResult);

        var args = _command.GetCommand().Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the response contains the NextCursor by checking the serialized result
        var resultJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains(expectedNextCursor, resultJson);
        Assert.Contains("test-product-1", resultJson);
        Assert.Contains("test-product-2", resultJson);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutNextCursorInResults_DoesNotIncludeNextCursorInResponse()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product",
                DisplayName = "Test Product"
            }
        };

        var productsListResult = new ProductListResponseWithNextCursor
        {
            Items = expectedProducts,
            NextCursor = null // No next cursor
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(productsListResult);

        var args = _command.GetCommand().Parse(["--subscription", subscriptionId]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        // Verify the response contains null NextCursor
        var resultJson = JsonSerializer.Serialize(response.Results);
        Assert.Contains("test-product", resultJson);
        // The NextCursor should be null and serialized as such
        Assert.DoesNotContain("\"nextCursor\"", resultJson);
    }

    [Fact]
    public async Task ExecuteAsync_WithExpandOption_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var expand = "plans";
        var expectedProducts = new List<ProductSummary>
        {
            new()
            {
                UniqueProductId = "test-product",
                DisplayName = "Test Product with Plans"
            }
        };

        _marketplaceService.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(expand),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        var args = _command.GetCommand().Parse([
            "--subscription", subscriptionId,
            "--expand", expand
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
    }
}
