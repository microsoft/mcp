// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Marketplace.Commands;
using Azure.Mcp.Tools.Marketplace.Commands.Product;
using Azure.Mcp.Tools.Marketplace.Models;
using Azure.Mcp.Tools.Marketplace.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Marketplace.Tests.Product;

public class ProductListCommandTests : SubscriptionCommandUnitTestsBase<ProductListCommand, IMarketplaceService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.Contains("marketplace products", CommandDefinition.Description.ToLower());
    }

    [Fact]
    public void Constructor_ExposesOptionalCursorAndLegacyParameter()
    {
        var cursor = Assert.Single(CommandDefinition.Options, option => option.Name == "--cursor");
        var legacyCursor = Assert.Single(CommandDefinition.Options, option => option.Name == "--next-cursor");

        Assert.Equal(typeof(string), cursor.ValueType);
        Assert.False(cursor.Required);
        Assert.False(legacyCursor.Required);
    }

    [Theory]
    [InlineData("--cursor", false)]
    [InlineData("--cursor", true)]
    [InlineData("--next-cursor", false)]
    [InlineData("--next-cursor", true)]
    public async Task ExecuteAsync_ContinuesPagesWithCanonicalOrLegacyCursor(string cursorOption, bool emptyFirstPage)
    {
        const string cursor = " opaque+/=%2F== ";
        Service.ListProducts(
                "test-sub", Arg.Any<string?>(), "azure", Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(
                new ProductListResponseWithNextCursor
                {
                    Items = emptyFirstPage ? [] : [new ProductSummary { UniqueProductId = "first-product" }],
                    NextCursor = cursor,
                },
                new ProductListResponseWithNextCursor
                {
                    Items = [new ProductSummary { UniqueProductId = "final-product" }],
                });

        var firstResponse = await ExecuteCommandAsync("--subscription", "test-sub", "--search", "azure");
        var first = ValidateAndDeserializeResponse(firstResponse, MarketplaceJsonContext.Default.ProductListCommandResult);
        Assert.Equal(cursor, first.NextCursor);
        Assert.Equal(emptyFirstPage ? 0 : 1, first.Products.Count);
        var firstPayload = JsonSerializer.SerializeToElement(first, MarketplaceJsonContext.Default.ProductListCommandResult);
        Assert.Equal(cursor, firstPayload.GetProperty("nextCursor").GetString());

        var finalResponse = await ExecuteCommandAsync(
            "--subscription", "test-sub", "--search", "azure", cursorOption, first.NextCursor!);
        var final = ValidateAndDeserializeResponse(finalResponse, MarketplaceJsonContext.Default.ProductListCommandResult);
        Assert.Equal("final-product", Assert.Single(final.Products).UniqueProductId);
        Assert.Null(final.NextCursor);
        var finalPayload = JsonSerializer.SerializeToElement(final, MarketplaceJsonContext.Default.ProductListCommandResult);
        Assert.False(finalPayload.TryGetProperty("nextCursor", out _));

        await Service.Received(1).ListProducts(
            "test-sub", null, "azure", null, null, null, null, null, null,
            Arg.Any<CancellationToken>());
        await Service.Received(1).ListProducts(
            "test-sub", null, "azure", null, null, null, cursor, null, null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("first-cursor", "second-cursor")]
    [InlineData("same-cursor", "same-cursor")]
    public async Task ExecuteAsync_WithBothCursorParameters_ReturnsValidationError(string cursor, string legacyCursor)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--cursor", cursor,
            "--next-cursor", legacyCursor);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specify either --cursor or --next-cursor, not both.", response.Message);
        Assert.Empty(Service.ReceivedCalls());
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Is(language),
            Arg.Is(search),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--search", search,
            "--language", language);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingSubscription_ReturnsValidationError()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("--search", "test");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyResults_ReturnsSuccessWithNullResults()
    {
        // Arrange
        var subscriptionId = "test-sub";

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProductListResponseWithNextCursor { Items = [] });

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscriptionId = "test-sub";

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(filter),
            Arg.Is(orderBy),
            Arg.Is(select),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--filter", filter,
            "--orderby", orderBy,
            "--select", select);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(productsListResult);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MarketplaceJsonContext.Default.ProductListCommandResult);

        Assert.Equal(expectedNextCursor, result.NextCursor);
        Assert.Contains(result.Products, p => p.UniqueProductId == "test-product-1");
        Assert.Contains(result.Products, p => p.UniqueProductId == "test-product-2");
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(productsListResult);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId);

        // Assert
        var result = ValidateAndDeserializeResponse(response, MarketplaceJsonContext.Default.ProductListCommandResult);

        Assert.Null(result.NextCursor);
        Assert.Contains(result.Products, p => p.UniqueProductId == "test-product");
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

        Service.ListProducts(
            Arg.Is(subscriptionId),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is(expand),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ProductListResponseWithNextCursor { Items = expectedProducts });

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscriptionId, "--expand", expand);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }
}
