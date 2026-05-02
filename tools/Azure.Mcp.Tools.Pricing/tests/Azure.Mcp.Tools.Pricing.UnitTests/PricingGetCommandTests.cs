// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Pricing.Commands;
using Azure.Mcp.Tools.Pricing.Models;
using Azure.Mcp.Tools.Pricing.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Pricing.UnitTests;

public sealed class PricingGetCommandTests : CommandUnitTestsBase<PricingGetCommand, IPricingService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", Command.Name);
        Assert.Equal("Get Azure Retail Pricing", Command.Title);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("--sku Standard_D4s_v5")]
    [InlineData("--sku Standard_D4s_v5 --service \"Virtual Machines\"")]
    [InlineData("--region eastus")]
    [InlineData("--service-family Compute")]
    [InlineData("--price-type Consumption")]
    [InlineData("--filter \"meterId eq 'abc-123'\"")]
    [InlineData("--sku Standard_D4s_v5 --region eastus")]
    [InlineData("--sku Standard_D4s_v5 --service \"Virtual Machines\" --currency EUR")]
    public async Task ExecuteAsync_WithValidFilters_ReturnsPrices(string cliArgs)
    {
        // Arrange
        var expectedPrices = new List<PriceItem>
        {
            new()
            {
                ArmSkuName = "Standard_D4s_v5",
                SkuName = "D4s v5",
                ProductName = "Virtual Machines Dsv5 Series",
                ServiceName = "Virtual Machines",
                ServiceFamily = "Compute",
                Region = "eastus",
                Location = "East US",
                RetailPrice = 0.192,
                UnitPrice = 0.192,
                CurrencyCode = "USD",
                UnitOfMeasure = "1 Hour",
                PriceType = "Consumption"
            }
        };

        Service.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedPrices);

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, PricingJsonContext.Default.PricingGetCommandResult);
        Assert.NotNull(result.Prices);
        Assert.Single(result.Prices);
        Assert.Equal("Standard_D4s_v5", result.Prices[0].ArmSkuName);
        Assert.Equal(0.192, result.Prices[0].RetailPrice);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoFilters_ReturnsBadRequest()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one filter is required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResults()
    {
        // Arrange
        Service.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--sku", "NonExistentSku");

        // Assert
        var result = ValidateAndDeserializeResponse(response, PricingJsonContext.Default.PricingGetCommandResult);
        Assert.Empty(result.Prices);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsErrorStatus()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        Service.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--sku", "Standard_D4s_v5");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncludeSavingsPlan_PassesFlagToService()
    {
        // Arrange
        Service.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            true, // includeSavingsPlan
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync("--sku", "Standard_D4s_v5", "--include-savings-plan");

        // Assert
        await Service.Received(1).GetPricesAsync(
            "Standard_D4s_v5",
            null,
            null,
            null,
            null,
            null,
            true,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCurrency_PassesCurrencyToService()
    {
        // Arrange
        Service.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "EUR",
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await ExecuteCommandAsync("--sku", "Standard_D4s_v5", "--currency", "EUR");

        // Assert
        await Service.Received(1).GetPricesAsync(
            "Standard_D4s_v5",
            null,
            null,
            null,
            null,
            "EUR",
            false,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void BindOptions_BindsAllOptionsCorrectly()
    {
        // Arrange
        var cliArgs = "--sku Standard_D4s_v5 --service \"Virtual Machines\" --region eastus " +
                      "--service-family Compute --price-type Consumption --currency EUR " +
                      "--include-savings-plan --filter \"isPrimaryMeterRegion eq true\"";
        var args = CommandDefinition.Parse(cliArgs);

        // Assert - all options should parse without error
        Assert.Empty(args.Errors);
    }
}
