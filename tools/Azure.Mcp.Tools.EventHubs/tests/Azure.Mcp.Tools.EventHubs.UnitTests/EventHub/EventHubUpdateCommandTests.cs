// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.EventHub;

public class EventHubUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventHubsService _eventHubsService;
    private readonly ILogger<EventHubUpdateCommand> _logger;
    private readonly EventHubUpdateCommand _command;
    private readonly CommandContext _context;

    public EventHubUpdateCommandTests()
    {
        _eventHubsService = Substitute.For<IEventHubsService>();
        _logger = Substitute.For<ILogger<EventHubUpdateCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventHubsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger, _eventHubsService);
        _context = new(_serviceProvider);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg --partition-count 4", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg --message-retention-in-hours 168", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(args);
        if (shouldSucceed)
        {
            var eventHub = new Models.EventHub(
                "test-hub",
                "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-hub",
                "test-rg",
                null,
                4,
                7,
                "Active",
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow,
                new List<string> { "0", "1", "2", "3" });

            _eventHubsService.CreateOrUpdateEventHubAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>())
                .Returns(eventHub);
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

        _eventHubsService.CreateOrUpdateEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
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

        _eventHubsService.CreateOrUpdateEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
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
    public async Task ExecuteAsync_SuccessfullyCreatesEventHub()
    {
        // Arrange
        var parseResult = _command.GetCommand().Parse(
            "--subscription test-subscription --eventhub new-hub --namespace test-namespace --resource-group test-rg --partition-count 8 --message-retention-in-hours 336");

        var eventHub = new Models.EventHub(
            "new-hub",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/new-hub",
            "test-rg",
            null,
            8,
            14, // 336 hours = 14 days
            "Active",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            new List<string> { "0", "1", "2", "3", "4", "5", "6", "7" });

        _eventHubsService.CreateOrUpdateEventHubAsync(
            Arg.Is("new-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Is(8),
            Arg.Is(336L),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(eventHub);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }
}
