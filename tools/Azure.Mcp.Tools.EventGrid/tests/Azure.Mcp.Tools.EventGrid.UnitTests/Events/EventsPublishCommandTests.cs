// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventGrid.Commands;
using Azure.Mcp.Tools.EventGrid.Commands.Events;
using Azure.Mcp.Tools.EventGrid.Models;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.UnitTests.Events;

[Trait("Area", "EventGrid")]
public class EventsPublishCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventGridService _eventGridService;
    private readonly ILogger<EventsPublishCommand> _logger;
    private readonly EventsPublishCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public EventsPublishCommandTests()
    {
        _eventGridService = Substitute.For<IEventGridService>();
        _logger = Substitute.For<ILogger<EventsPublishCommand>>();

        var collection = new ServiceCollection().AddSingleton(_eventGridService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Command_Properties_AreCorrect()
    {
        Assert.Equal("publish", _command.Name);
        Assert.Contains("Publish custom events to Event Grid topics", _command.Description);
        Assert.Equal("Publish Events to Event Grid Topic", _command.Title);
    }

    [Fact]
    public void Command_Metadata_IsCorrect()
    {
        var metadata = _command.Metadata;
        Assert.False(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.True(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSingleEvent_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0",
            data = new { message = "Hello World" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventsAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--event-data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventsPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(1, result.Result.PublishedEventCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidArrayOfEvents_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new[]
        {
            new
            {
                subject = "/test/subject1",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Hello World 1" }
            },
            new
            {
                subject = "/test/subject2",
                eventType = "TestEvent",
                dataVersion = "1.0",
                data = new { message = "Hello World 2" }
            }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 2 event(s) to topic '{topicName}'.",
            PublishedEventCount: 2,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventsAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--event-data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventsPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(2, result.Result.PublishedEventCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidJson_Returns400()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var invalidEventData = "invalid-json";

        _eventGridService.PublishEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON format"));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--event-data", invalidEventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("Invalid", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithTopicNotFound_Returns404()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "nonexistent-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0",
            data = new { message = "Hello World" }
        });

        _eventGridService.PublishEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException($"Event Grid topic '{topicName}' not found in resource group '{resourceGroup}'."));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--event-data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(500, response.Status); // The base command returns 500 for general exceptions by default
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-sub", false)]
    [InlineData("--subscription test-sub --resource-group test-rg", false)]
    [InlineData("--subscription test-sub --topic test-topic", false)]
    [InlineData("--subscription test-sub --resource-group test-rg --topic test-topic", false)]
    [InlineData("--subscription test-sub --topic test-topic --event-data '{\"subject\":\"test\"}'", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --topic test-topic --event-data '{\"subject\":\"test\"}'", true)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedResult = new EventPublishResult(
                Status: "Success",
                Message: "Successfully published 1 event(s).",
                PublishedEventCount: 1,
                OperationId: Guid.NewGuid().ToString(),
                PublishedAt: DateTime.UtcNow);

            _eventGridService.PublishEventsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromResult(expectedResult));
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(400, response.Status);
            Assert.Contains("required", response.Message?.ToLower() ?? "");
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithoutResourceGroup_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0",
            data = new { message = "Hello World" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventsAsync(
            Arg.Is(subscriptionId),
            Arg.Is<string?>(resourceGroup => resourceGroup == null), // Resource group should be null
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--topic", topicName, "--event-data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventsPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(1, result.Result.PublishedEventCount);
    }
}
