// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.EventHub;

public class EventHubDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<EventHubDeleteCommand> _logger;
    private readonly EventHubDeleteCommand _command;
    private readonly CommandContext _context;

    public EventHubDeleteCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<EventHubDeleteCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger, _eventHubsService);
        _context = new(_serviceProvider);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            _eventHubsService.DeleteEventHubAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(true);
        }

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, (int)response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(200, (int)response.Status);
            Assert.NotNull(response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(
            "--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg");

        _eventHubsService.DeleteEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Namespace 'test-namespace' not found in resource group 'test-rg'"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(
            "--subscription unauthorized-sub --eventhub test-hub --namespace test-namespace --resource-group test-rg");

        _eventHubsService.DeleteEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("Authentication failed"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfullyDeletesEventHub()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(
            "--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg");

        _eventHubsService.DeleteEventHubAsync(
            Arg.Is("test-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(true);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_IdempotentWhenEventHubNotFound()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(
            "--subscription test-subscription --eventhub nonexistent-hub --namespace test-namespace --resource-group test-rg");

        _eventHubsService.DeleteEventHubAsync(
            Arg.Is("nonexistent-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(false);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }
}
