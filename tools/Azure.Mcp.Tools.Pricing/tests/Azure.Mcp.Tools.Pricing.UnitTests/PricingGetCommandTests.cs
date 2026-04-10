// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Pricing.Commands;
using Azure.Mcp.Tools.Pricing.Models;
using Azure.Mcp.Tools.Pricing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Pricing.UnitTests;

public sealed class PricingGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IPricingService _pricingService;
    private readonly ILogger<PricingGetCommand> _logger;

    public PricingGetCommandTests()
    {
        _pricingService = Substitute.For<IPricingService>();
        _logger = Substitute.For<ILogger<PricingGetCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_pricingService);
        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = new PricingGetCommand(_logger);

        // Assert
        Assert.Equal("get", command.Name);
        Assert.Equal("Get Azure Retail Pricing", command.Title);
        Assert.Contains("Azure retail pricing", command.Description);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
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

        _pricingService.GetPricesAsync(
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

        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PricingJsonContext.Default.PricingGetCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Prices);
        Assert.Single(result.Prices);
        Assert.Equal("Standard_D4s_v5", result.Prices[0].ArmSkuName);
        Assert.Equal(0.192, result.Prices[0].RetailPrice);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoFilters_ReturnsBadRequest()
    {
        // Arrange
        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse(""); // No arguments
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("At least one filter is required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoResults()
    {
        // Arrange
        _pricingService.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<PriceItem>());

        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse("--sku NonExistentSku");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, PricingJsonContext.Default.PricingGetCommandResult);
        Assert.NotNull(result);
        Assert.Empty(result.Prices);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException_AndSetsErrorStatus()
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        _pricingService.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<PriceItem>>(new Exception("Test error")));

        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse("--sku Standard_D4s_v5");
        var context = new CommandContext(_serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncludeSavingsPlan_PassesFlagToService()
    {
        // Arrange
        _pricingService.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            true, // includeSavingsPlan
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<PriceItem>());

        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse("--sku Standard_D4s_v5 --include-savings-plan");
        var context = new CommandContext(_serviceProvider);

        // Act
        await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _pricingService.Received(1).GetPricesAsync(
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
        _pricingService.GetPricesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "EUR",
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<PriceItem>());

        var command = new PricingGetCommand(_logger);
        var args = command.GetCommand().Parse("--sku Standard_D4s_v5 --currency EUR");
        var context = new CommandContext(_serviceProvider);

        // Act
        await command.ExecuteAsync(context, args, TestContext.Current.CancellationToken);

        // Assert
        await _pricingService.Received(1).GetPricesAsync(
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
        var command = new PricingGetCommand(_logger);
        var cliArgs = "--sku Standard_D4s_v5 --service \"Virtual Machines\" --region eastus " +
                      "--service-family Compute --price-type Consumption --currency EUR " +
                      "--include-savings-plan --filter \"isPrimaryMeterRegion eq true\"";
        var args = command.GetCommand().Parse(cliArgs);

        // Assert - all options should parse without error
        Assert.Empty(args.Errors);
    }
}
