// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.ConsumerGroup;

public class ConsumerGroupUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<ConsumerGroupUpdateCommand> _logger;
    private readonly ConsumerGroupUpdateCommand _command;
    private readonly CommandContext _context;

    public ConsumerGroupUpdateCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<ConsumerGroupUpdateCommand>>();

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
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group --user-metadata test-metadata", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            var consumerGroup = new Models.ConsumerGroup(
                "test-consumer-group",
                "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
                "test-rg",
                "test-namespace",
                "test-eventhub",
                "East US",
                args.Contains("--user-metadata") ? "test-metadata" : null,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow);

            _eventHubsService.CreateOrUpdateConsumerGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(consumerGroup);
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
    public async Task ExecuteAsync_CreatesConsumerGroupWithUserMetadata()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group --user-metadata custom-metadata");

        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            "custom-metadata",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        _eventHubsService.CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            "custom-metadata",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedConsumerGroup);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await _eventHubsService.Received(1).CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            "custom-metadata",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesConsumerGroupWithoutUserMetadata()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group");

        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            null,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        _eventHubsService.CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(expectedConsumerGroup);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await _eventHubsService.Received(1).CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group");

        _eventHubsService.CreateOrUpdateConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Event Hub 'test-eventhub' could not be found"));

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
        var parseResult = _command.GetCommand().Parse("--subscription unauthorized-sub --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group");

        _eventHubsService.CreateOrUpdateConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }
}
