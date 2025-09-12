// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.UnitTests.Subscription;

[Trait("Area", "EventGrid")]
public class SubscriptionListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventGridService _eventGridService;
    private readonly ILogger<SubscriptionListCommand> _logger;
    private readonly SubscriptionListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SubscriptionListCommandTests()
    {
        _eventGridService = Substitute.For<IEventGridService>();
        _logger = Substitute.For<ILogger<SubscriptionListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_eventGridService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var subscription = "sub123";
        var expectedSubscriptions = new List<Models.EventGridSubscriptionInfo>
        {
            new("subscription1", "Microsoft.EventGrid/eventSubscriptions", "WebHook", "https://example.com/webhook1", "Succeeded", null, null, 30, 1440, "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z"),
            new("subscription2", "Microsoft.EventGrid/eventSubscriptions", "StorageQueue", "https://storage.queue.core.windows.net/myqueue", "Succeeded", null, null, 10, 720, "2023-01-03T00:00:00Z", "2023-01-04T00:00:00Z")
        };

        _eventGridService.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(expectedSubscriptions));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Deserialize<SubscriptionListResult>(json, options);

        Assert.NotNull(result);
        Assert.NotNull(result!.Subscriptions);
        Assert.Equal(expectedSubscriptions.Count, result.Subscriptions!.Count);
        Assert.Equal(expectedSubscriptions.Select(s => s.Name), result.Subscriptions.Select(s => s.Name));
    }

    [Fact]
    public async Task ExecuteAsync_WithTopicNameFilter_FiltersCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var resourceGroup = "test-rg";
        var topicName = "test-topic";
        var expectedSubscriptions = new List<Models.EventGridSubscriptionInfo>
        {
            new("filtered-subscription", "Microsoft.EventGrid/eventSubscriptions", "WebHook", "https://example.com/webhook", "Succeeded", null, null, 30, 1440, "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z")
        };

        _eventGridService.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Is(resourceGroup), Arg.Is(topicName), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(expectedSubscriptions));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--topic", topicName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Deserialize<SubscriptionListResult>(json, options);

        Assert.NotNull(result);
        Assert.NotNull(result!.Subscriptions);
        Assert.Single(result.Subscriptions);
        Assert.Equal("filtered-subscription", result.Subscriptions.First().Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoSubscriptions()
    {
        // Arrange
        var subscription = "sub123";

        _eventGridService.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(new List<Models.EventGridSubscriptionInfo>()));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Deserialize<SubscriptionListResult>(json, options);
        Assert.NotNull(result);
        Assert.NotNull(result.Subscriptions);
        Assert.Empty(result.Subscriptions);
    }

    [Fact]
    public async Task ExecuteAsync_WithLocationFilter_FiltersCorrectly()
    {
        // Arrange
        var subscription = "sub123";
        var location = "eastus";
        var expectedSubscriptions = new List<Models.EventGridSubscriptionInfo>
        {
            new("location-filtered-subscription", "Microsoft.EventGrid/eventSubscriptions", "WebHook", "https://example.com/webhook", "Succeeded", null, null, 30, 1440, "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z")
        };

        _eventGridService.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(location), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(expectedSubscriptions));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--location", location]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Deserialize<SubscriptionListResult>(json, options);

        Assert.NotNull(result);
        Assert.NotNull(result!.Subscriptions);
        Assert.Single(result.Subscriptions);
        Assert.Equal("location-filtered-subscription", result.Subscriptions.First().Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";

        _eventGridService.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    private class SubscriptionListResult
    {
        [JsonPropertyName("subscriptions")]
        public List<Models.EventGridSubscriptionInfo>? Subscriptions { get; set; }
    }
}
