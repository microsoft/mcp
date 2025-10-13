// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Subscription.Commands;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

public class SubscriptionListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly McpServer _mcpServer;
    private readonly ILogger<SubscriptionListCommand> _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly SubscriptionListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public SubscriptionListCommandTests()
    {
        _mcpServer = Substitute.For<McpServer>();
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _logger = Substitute.For<ILogger<SubscriptionListCommand>>();
        var collection = new ServiceCollection()
            .AddSingleton(_mcpServer)
            .AddSingleton(_subscriptionService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSubscriptions);

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(2, subscriptionsArray.GetArrayLength());

        var first = subscriptionsArray[0];
        var second = subscriptionsArray[1];

        Assert.Equal("sub1", first.GetProperty("subscriptionId").GetString());
        Assert.Equal("Subscription 1", first.GetProperty("displayName").GetString());
        Assert.Equal("sub2", second.GetProperty("subscriptionId").GetString());
        Assert.Equal("Subscription 2", second.GetProperty("displayName").GetString());

        await _subscriptionService.Received(1).GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenantId_PassesTenantToService()
    {
        // Arrange
        var tenantId = "test-tenant-id";
        var args = _commandDefinition.Parse($"--tenant {tenantId}");

        _subscriptionService
            .GetSubscriptions(Arg.Is<string>(x => x == tenantId), Arg.Any<RetryPolicyOptions>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptySubscriptionList_ReturnsNotNullResults()
    {
        // Arrange
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var expectedError = "Test error message";
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<SubscriptionData>>(new Exception(expectedError)));

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToCommand()
    {
        // Arrange
        var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();
        var args = _commandDefinition.Parse($"--auth-method {authMethod}");

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_WithCharacterLimit_ParsesAndBindsCorrectly()
    {
        // Arrange
        var characterLimit = 5000;
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Test Subscription 1")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(subscriptions);

        var args = _commandDefinition.Parse($"--character-limit {characterLimit}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        Assert.Contains("1 subscriptions returned", result.Message);
        Assert.Contains("characters", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ResponseWithinCharacterLimit_ReturnsAllSubscriptions()
    {
        // Arrange
        var characterLimit = 10000; // Large limit
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(subscriptions);

        var args = _commandDefinition.Parse($"--character-limit {characterLimit}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(2, subscriptionsArray.GetArrayLength());
        Assert.Contains("All 2 subscriptions returned", result.Message);
        Assert.Contains("characters", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ResponseExceedsCharacterLimit_TruncatesSubscriptions()
    {
        // Arrange
        var characterLimit = 100; // Very small limit to force truncation
        var subscriptions = SubscriptionTestHelpers.CreateTestSubscriptions(10); // Create 10 subscriptions

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(subscriptions);

        var args = _commandDefinition.Parse($"--character-limit {characterLimit}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        // Should have fewer than 10 subscriptions due to truncation
        Assert.True(subscriptionsArray.GetArrayLength() < 10);
        Assert.Contains("Results truncated", result.Message);
        Assert.Contains("of 10 subscriptions", result.Message);
        Assert.Contains("Increase --character-limit", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_EmptySubscriptionListWithCharacterLimit_ReturnsAppropriateMessage()
    {
        // Arrange
        var characterLimit = 5000;
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse($"--character-limit {characterLimit}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Results);
        Assert.Equal("No subscriptions found.", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultCharacterLimit_UsesDefaultValue()
    {
        // Arrange
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Test Subscription")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(subscriptions);

        var args = _commandDefinition.Parse(""); // No character-limit specified

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);
        // Should use default behavior (10000 characters is the default)
        Assert.Contains("1 subscriptions returned", result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_VerySmallCharacterLimit_HandlesGracefully()
    {
        // Arrange - Set a very small character limit that might not even fit one subscription
        var characterLimit = 50;
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Test")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(subscriptions);

        var args = _commandDefinition.Parse($"--character-limit {characterLimit}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);

        // Should either return empty results or truncated results, but handle it gracefully
        if (result.Results != null)
        {
            var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
            var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

            // Should have 0 or 1 subscription due to very small limit
            Assert.True(subscriptionsArray.GetArrayLength() <= 1);
        }

        Assert.NotNull(result.Message);
    }

}
