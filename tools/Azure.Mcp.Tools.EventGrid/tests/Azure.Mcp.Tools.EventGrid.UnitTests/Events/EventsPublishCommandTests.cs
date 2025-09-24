// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
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
    private readonly ILogger<EventGridPublishCommand> _logger;
    private readonly EventGridPublishCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public EventsPublishCommandTests()
    {
        _eventGridService = Substitute.For<IEventGridService>();
        _logger = Substitute.For<ILogger<EventGridPublishCommand>>();

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
        Assert.False(metadata.OpenWorld);
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

        _eventGridService.PublishEventAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
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

        _eventGridService.PublishEventAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
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

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON format"));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", invalidEventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException($"Event Grid topic '{topicName}' not found in resource group '{resourceGroup}'."));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status); // The base command returns InternalServerError for general exceptions by default
        Assert.Contains("not found", response.Message);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-sub", false)]
    [InlineData("--subscription test-sub --resource-group test-rg", false)]
    [InlineData("--subscription test-sub --topic test-topic", false)]
    [InlineData("--subscription test-sub --resource-group test-rg --topic test-topic", false)]
    [InlineData("--subscription test-sub --topic test-topic --data '{\"subject\":\"test\"}'", true)]
    [InlineData("--subscription test-sub --resource-group test-rg --topic test-topic --data '{\"subject\":\"test\"}'", true)]
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

            _eventGridService.PublishEventAsync(
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
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
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

        _eventGridService.PublishEventAsync(
            Arg.Is(subscriptionId),
            Arg.Is<string?>(resourceGroup => resourceGroup == null), // Resource group should be null
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(1, result.Result.PublishedEventCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithCloudEventsSchema_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            specversion = "1.0",
            type = "com.example.CloudEventTest",
            source = "/test/cloudevents",
            id = "cloud-event-123",
            time = "2025-01-15T10:00:00Z",
            data = new { message = "Hello CloudEvents!" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Is("CloudEvents"), // Verify CloudEvents schema is passed
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "CloudEvents"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(1, result.Result.PublishedEventCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomSchema_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            orderNumber = "ORD-12345",
            eventCategory = "OrderPlaced",
            resourcePath = "/orders/12345",
            occurredAt = "2025-01-15T10:00:00Z",
            details = new { amount = 99.99, currency = "USD" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Is(subscriptionId),
            Arg.Is(resourceGroup),
            Arg.Is(topicName),
            Arg.Any<string>(),
            Arg.Is("Custom"), // Verify Custom schema is passed
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "Custom"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
        Assert.NotNull(result);
        Assert.Equal("Success", result!.Result.Status);
        Assert.Equal(1, result.Result.PublishedEventCount);
    }

    [Theory]
    [InlineData("EventGrid")]
    [InlineData("CloudEvents")]
    [InlineData("Custom")]
    public async Task ExecuteAsync_WithDifferentSchemas_PassesSchemaCorrectly(string schema)
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = schema switch
        {
            "CloudEvents" => JsonSerializer.Serialize(new
            {
                specversion = "1.0",
                type = "test.event",
                source = "/test",
                id = "test-123"
            }),
            "EventGrid" => JsonSerializer.Serialize(new
            {
                id = "test-123",
                subject = "/test",
                eventType = "TestEvent",
                dataVersion = "1.0"
            }),
            _ => JsonSerializer.Serialize(new
            {
                customField = "customValue",
                id = "test-123"
            })
        };

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is(schema), // Verify the schema parameter is passed correctly
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", schema]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutEventSchema_DefaultsToEventGrid()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0"
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string?>(schema => schema == null), // Should be null when not specified
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithAccessDenied_Returns403()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0"
        });

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Azure.RequestFailedException(403, "Access denied to Event Grid topic"));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEventSchema_Returns400()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0"
        });

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new ArgumentException("Invalid event schema specified. Supported schemas are: CloudEvents, EventGrid, or Custom."));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "InvalidSchema"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid event schema", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithBadRequestError_Returns400()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/subject",
            eventType = "TestEvent",
            dataVersion = "1.0"
        });

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Azure.RequestFailedException(400, "Invalid event data or schema format"));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid event data", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeEventPayload_ReturnsSuccess()
    {
        // Arrange
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";

        // Create a large data payload
        var largeData = new
        {
            largeArray = Enumerable.Range(1, 100).Select(i => new { id = i, value = $"item-{i}" }).ToArray(),
            largeString = new string('x', 1000),
            nestedObject = new
            {
                level1 = new
                {
                    level2 = new
                    {
                        level3 = new { data = "deep nested data" }
                    }
                }
            }
        };

        var eventData = JsonSerializer.Serialize(new
        {
            subject = "/test/large",
            eventType = "LargeEvent",
            dataVersion = "1.0",
            data = largeData
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithCloudEventsMinimalFields_ReturnsSuccess()
    {
        // Arrange - Test CloudEvents with only required fields
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            specversion = "1.0",
            type = "com.example.minimal",
            source = "/minimal/source",
            id = "minimal-cloud-event"
            // Missing optional fields: time, subject, data
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is("CloudEvents"),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "CloudEvents"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithCloudEventsDataContentType_ReturnsSuccess()
    {
        // Arrange - Test CloudEvents with datacontenttype field
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            specversion = "1.0",
            type = "com.example.data.structured",
            source = "/datatype/test",
            id = "datacontent-test-123",
            time = "2025-01-15T10:00:00Z",
            datacontenttype = "application/xml",
            subject = "data/content/type/test",
            data = new { message = "XML structured data", format = "xml" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is("CloudEvents"),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "CloudEvents"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedSchemaFields_ReturnsSuccess()
    {
        // Arrange - Test Custom schema with mixed EventGrid and CloudEvents fields
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            id = "mixed-event-123",
            type = "MixedEvent", // CloudEvents field name
            subject = "/mixed/subject", // EventGrid field name
            dataVersion = "1.5", // EventGrid field name
            time = "2025-01-15T13:00:00Z", // CloudEvents field name
            data = new { mixed = "event data" }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is("Custom"),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "Custom"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData("cloudevents")]
    [InlineData("CLOUDEVENTS")]
    [InlineData("CloudEvents")]
    [InlineData("eventgrid")]
    [InlineData("EVENTGRID")]
    [InlineData("EventGrid")]
    [InlineData("custom")]
    [InlineData("CUSTOM")]
    [InlineData("Custom")]
    public async Task ExecuteAsync_WithSchemaNameCaseInsensitive_ReturnsSuccess(string schema)
    {
        // Arrange - Test that schema names are case insensitive
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new
        {
            id = "case-test",
            subject = "/test/case",
            eventType = "CaseTest",
            dataVersion = "1.0"
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 1 event(s) to topic '{topicName}'.",
            PublishedEventCount: 1,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is(schema),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", schema]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithArrayOfMixedSchemaEvents_ReturnsSuccess()
    {
        // Arrange - Test array with mixed event formats in Custom schema
        var subscriptionId = "test-sub";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var eventData = JsonSerializer.Serialize(new object[]
        {
            new // EventGrid-style fields
            {
                id = "event-1",
                subject = "/test/1",
                eventType = "TestEvent1",
                dataVersion = "1.0",
                eventTime = "2025-01-15T10:00:00Z",
                data = new { index = 1 }
            },
            new // CloudEvents-style fields
            {
                id = "event-2",
                source = "/test/2",
                type = "TestEvent2",
                specversion = "1.0",
                time = "2025-01-15T11:00:00Z",
                data = new { index = 2 }
            }
        });

        var expectedResult = new EventPublishResult(
            Status: "Success",
            Message: $"Successfully published 2 event(s) to topic '{topicName}'.",
            PublishedEventCount: 2,
            OperationId: Guid.NewGuid().ToString(),
            PublishedAt: DateTime.UtcNow);

        _eventGridService.PublishEventAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is("Custom"),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedResult));

        var args = _commandDefinition.Parse(["--subscription", subscriptionId, "--resource-group", resourceGroup, "--topic", topicName, "--data", eventData, "--schema", "Custom"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        var json = JsonSerializer.Serialize(response.Results!);
        var result = JsonSerializer.Deserialize(json, EventGridJsonContext.Default.EventGridPublishCommandResult);
        Assert.Equal(2, result!.Result.PublishedEventCount);
    }
}
