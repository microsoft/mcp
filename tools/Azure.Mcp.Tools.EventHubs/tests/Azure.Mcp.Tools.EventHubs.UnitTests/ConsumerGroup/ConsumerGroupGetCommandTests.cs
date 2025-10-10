// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands;
using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.ConsumerGroup;

public class ConsumerGroupGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<ConsumerGroupGetCommand> _logger;
    private readonly ConsumerGroupGetCommand _command;
    private readonly CommandContext _context;

    public ConsumerGroupGetCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<ConsumerGroupGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            var consumerGroups = new List<Models.ConsumerGroup>
            {
                new("test-group", "test-id", "test-rg", "test-namespace", "test-eventhub")
            };

            _eventHubsService.GetConsumerGroupsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(consumerGroups);

            _eventHubsService.GetConsumerGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(consumerGroups.First());
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
    public async Task ExecuteAsync_ListsConsumerGroupsSuccessfully()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub");

        var expectedConsumerGroups = new List<Models.ConsumerGroup>
        {
            new("group1", "id1", "test-rg", "test-namespace", "test-eventhub"),
            new("group2", "id2", "test-rg", "test-namespace", "test-eventhub")
        };

        _eventHubsService.GetConsumerGroupsAsync(
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedConsumerGroups);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await _eventHubsService.Received(1).GetConsumerGroupsAsync(
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleConsumerGroupSuccessfully()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group");

        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "test-id",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            "test metadata",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        _eventHubsService.GetConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedConsumerGroup);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await _eventHubsService.Received(1).GetConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyListWhenConsumerGroupNotFound()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group nonexistent-group");

        _eventHubsService.GetConsumerGroupAsync(
            "nonexistent-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns((Models.ConsumerGroup?)null);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub");

        _eventHubsService.GetConsumerGroupsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Event Hub 'test-eventhub' could not be found"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription unauthorized-sub --resource-group test-rg --namespace test-namespace --eventhub test-eventhub");

        _eventHubsService.GetConsumerGroupsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }



    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Act
        var command = _command.GetCommand();

        // Assert
        Assert.Equal("get", command.Name);
        Assert.False(string.IsNullOrEmpty(command.Description));
        Assert.NotEmpty(_command.Title);
    }
}
