// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Runtime;

public class RuntimeShowCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<RuntimeShowCommand> _logger;
    private readonly RuntimeShowCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public RuntimeShowCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<RuntimeShowCommand>>();

        var collection = new ServiceCollection().AddSingleton(_signalRService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("show", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("runtime information", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --signalr-name signalr1", true)]
    [InlineData("--subscription sub1 --signalr-name signalr1", false)] // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing signalr-name
    [InlineData("--resource-group rg1 --signalr-name signalr1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testRuntime = new Models.Runtime
            {
                Name = "test-signalr",
                ResourceGroupName = "test-rg",
                Location = "East US",
                SkuName = "Standard_S1"
            };

            _signalRService.GetRuntimeAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<AuthMethod?>(),
                    Arg.Any<RetryPolicyOptions>())
                .Returns(testRuntime);
        }

        var parseResult = _parser.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRuntimeWhenFound()
    {
        // Arrange
        var expectedRuntime = new Models.Runtime
        {
            Name = "test-signalr",
            ResourceGroupName = "test-rg",
            Location = "East US",
            SkuName = "Standard_S1"
        };

        _signalRService.GetRuntimeAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedRuntime);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);

        var json = JsonSerializer.Serialize(response.Results);
        var result =
            JsonSerializer.Deserialize<RuntimeShowCommand.RuntimeShowCommandResult>(json,
                Commands.SignalRJsonContext.Default.RuntimeShowCommandResult);
        Assert.NotNull(result);

        Assert.Equal("test-signalr", result.Runtime.Name);
        Assert.Equal("test-rg", result.Runtime.ResourceGroupName);
        Assert.Equal("East US", result.Runtime.Location);
        Assert.Equal("Standard_S1", result.Runtime.SkuName);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenRuntimeNotFound()
    {
        // Arrange
        _signalRService.GetRuntimeAsync(
                "test-subscription",
                "test-rg",
                "nonexistent-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns((Models.Runtime?)null);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name",
            "nonexistent-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Null(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _signalRService.GetRuntimeAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Runtime?>(new Exception("Service unavailable")));

        var parseResult = _parser.Parse([
            "--subscription", "invalid-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains("Service unavailable", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesServiceMethodCalled()
    {
        // Arrange
        var expectedRuntime = new Models.Runtime { Name = "test-signalr", Location = "East US" };

        _signalRService.GetRuntimeAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedRuntime);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        await _command.ExecuteAsync(_context, parseResult);

        // Assert
        await _signalRService.Received(1).GetRuntimeAsync(
            "test-subscription",
            "test-rg",
            "test-signalr",
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Theory]
    [InlineData("East US")]
    [InlineData("West US")]
    [InlineData("Central US")]
    [InlineData("North Europe")]
    public async Task ExecuteAsync_HandlesAllLocations(string location)
    {
        // Arrange
        var runtime = new Models.Runtime { Name = "test-signalr", Location = location, SkuName = "Standard_S1" };

        _signalRService.GetRuntimeAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(runtime);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result =
            JsonSerializer.Deserialize<RuntimeShowCommand.RuntimeShowCommandResult>(json,
                Commands.SignalRJsonContext.Default.RuntimeShowCommandResult);
        Assert.NotNull(result);

        Assert.Equal(location, result.Runtime.Location);
    }

    [Theory]
    [InlineData("Standard_S1")]
    [InlineData("Free_F1")]
    [InlineData("Premium_P1")]
    public async Task ExecuteAsync_HandlesAllSkuTypes(string skuName)
    {
        // Arrange
        var runtime = new Models.Runtime { Name = "test-signalr", SkuName = skuName, Location = "East US" };

        _signalRService.GetRuntimeAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(runtime);

        var parseResult = _parser.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr-name", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result =
            JsonSerializer.Deserialize<RuntimeShowCommand.RuntimeShowCommandResult>(json,
                Commands.SignalRJsonContext.Default.RuntimeShowCommandResult);
        Assert.NotNull(result);

        Assert.Equal(skuName, result.Runtime.SkuName);
    }
}
