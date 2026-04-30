// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventGrid.Commands;
using Azure.Mcp.Tools.EventGrid.Commands.Subscription;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.UnitTests.Subscription;

public class SubscriptionListCommandTests : CommandUnitTestsBase<SubscriptionListCommand, IEventGridService>
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionListCommandTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();

        Services.AddSingleton(_subscriptionService);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("list", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
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

        Service.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.SubscriptionListCommandResult);

        Assert.NotNull(result.Subscriptions);
        Assert.Equal(expectedSubscriptions.Count, result.Subscriptions.Count);
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

        Service.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Is(resourceGroup), Arg.Is(topicName), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--topic", topicName);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.SubscriptionListCommandResult);

        Assert.NotNull(result.Subscriptions);
        Assert.Single(result.Subscriptions);
        Assert.Equal("filtered-subscription", result.Subscriptions.First().Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoSubscriptions()
    {
        // Arrange
        var subscription = "sub123";

        Service.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.SubscriptionListCommandResult);

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

        Service.GetSubscriptionsAsync(Arg.Is(subscription), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Is(location), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--location", location);

        // Assert
        var result = ValidateAndDeserializeResponse(response, EventGridJsonContext.Default.SubscriptionListCommandResult);

        Assert.NotNull(result.Subscriptions);
        Assert.Single(result.Subscriptions);
        Assert.Equal("location-filtered-subscription", result.Subscriptions.First().Name);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.GetSubscriptionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Theory]
    [InlineData("--subscription sub", true)]
    [InlineData("--subscription sub --topic my-topic", true)]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("--topic my-topic", true)] // Cross-subscription search - valid with topic alone
    [InlineData("", false)]
    [InlineData("--location eastus", false)]
    [InlineData("--resource-group rg", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.GetSubscriptionsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(
                [
                    new("subscription1", "Microsoft.EventGrid/eventSubscriptions", "WebHook", "https://example.com/webhook1", "Succeeded", null, null, 30, 1440, "2023-01-01T00:00:00Z", "2023-01-02T00:00:00Z")
                ]);

            // Set up subscription service for cross-subscription search scenario
            _subscriptionService.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }
}
