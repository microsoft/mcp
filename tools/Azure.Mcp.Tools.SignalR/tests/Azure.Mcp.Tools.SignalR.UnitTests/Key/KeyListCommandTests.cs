// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.CommandLine;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.SignalR.Commands.Key;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.SignalR.UnitTests.Key;

public class KeyListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<KeyListCommand> _logger;
    private readonly KeyListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public KeyListCommandTests()
    {
        _signalRService = Substitute.For<ISignalRService>();
        _logger = Substitute.For<ILogger<KeyListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_signalRService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("access keys", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub1 --resource-group rg1 --signalr signalr1", true)]
    [InlineData("--subscription sub1 --signalr signalr1", false)] // Missing resource-group
    [InlineData("--subscription sub1 --resource-group rg1", false)] // Missing signalr
    [InlineData("--resource-group rg1 --signalr signalr1", false)] // Missing subscription
    [InlineData("", false)] // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var testKeys = new Models.Key
            {
                PrimaryKey = "primary-key-123",
                SecondaryKey = "secondary-key-456",
                PrimaryConnectionString = "connection-string-primary",
                SecondaryConnectionString = "connection-string-secondary"
            };

            _signalRService.ListKeysAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<AuthMethod?>(),
                    Arg.Any<RetryPolicyOptions>())
                .Returns(testKeys);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

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
    public async Task ExecuteAsync_ReturnsKeysWhenFound()
    {
        // Arrange
        var expectedKeys = new Models.Key
        {
            PrimaryKey = "test-primary-key",
            SecondaryKey = "test-secondary-key",
            PrimaryConnectionString =
                "Endpoint=https://test-signalr.service.signalr.net;AccessKey=test-primary-key;Version=1.0;",
            SecondaryConnectionString =
                "Endpoint=https://test-signalr.service.signalr.net;AccessKey=test-secondary-key;Version=1.0;"
        };

        _signalRService.ListKeysAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedKeys);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNullWhenKeysNotFound()
    {
        // Arrange
        _signalRService.ListKeysAsync("test-subscription", "test-rg", "nonexistent-signalr", null, null,
                Arg.Any<RetryPolicyOptions>())
            .Returns((Models.Key?)null);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr",
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
    public async Task ExecuteAsync_HandlesRequestFailedException()
    {
        // Arrange
        var exception = new RequestFailedException(403, "Access denied");
        _signalRService.ListKeysAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Key?>(exception));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _signalRService.ListKeysAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Key?>(new Exception("Service unavailable")));

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
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
        var expectedKeys = new Models.Key { PrimaryKey = "key1", SecondaryKey = "key2" };

        _signalRService.ListKeysAsync(
                "test-subscription",
                "test-rg",
                "test-signalr",
                Arg.Any<string?>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(expectedKeys);

        var parseResult = _commandDefinition.Parse([
            "--subscription", "test-subscription", "--resource-group", "test-rg", "--signalr", "test-signalr"
        ]);

        // Act
        await _command.ExecuteAsync(_context, parseResult);

        // Assert
        await _signalRService.Received(1).ListKeysAsync(
            "test-subscription",
            "test-rg",
            "test-signalr",
            Arg.Any<string?>(),
            Arg.Any<AuthMethod?>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenLocalAuthDisabled()
    {
        // Arrange
        var args = "--subscription sub1 --resource-group rg1 --signalr signalr1".Split(' ',
            StringSplitOptions.RemoveEmptyEntries);
        var parseResult = _commandDefinition.Parse(args);
        _signalRService.ListKeysAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<AuthMethod?>(),
                Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<Models.Key?>(
                new RequestFailedException(403, "Access keys are disabled for this SignalR service.")));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("Access keys are disabled", response.Message);
    }
}
