// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
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
    private readonly IMcpServer _mcpServer;
    private readonly ILogger<SubscriptionListCommand> _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly SubscriptionListCommand _command;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public SubscriptionListCommandTests()
    {
        _mcpServer = Substitute.For<IMcpServer>();
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _logger = Substitute.For<ILogger<SubscriptionListCommand>>();
        var collection = new ServiceCollection()
            .AddSingleton(_mcpServer)
            .AddSingleton(_subscriptionService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _parser = new(_command.GetCommand());
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

        var args = _parser.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
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
        var args = _parser.Parse($"--tenant {tenantId}");

        _subscriptionService
            .GetSubscriptions(Arg.Is<string>(x => x == tenantId), Arg.Any<RetryPolicyOptions>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptySubscriptionList_ReturnsNullResults()
    {
        // Arrange
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _parser.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.Null(result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorInResponse()
    {
        // Arrange
        var expectedError = "Test error message";
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromException<List<SubscriptionData>>(new Exception(expectedError)));

        var args = _parser.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.Status);
        Assert.Contains(expectedError, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToCommand()
    {
        // Arrange
        var authMethod = AuthMethod.Credential.ToString().ToLowerInvariant();
        var args = _parser.Parse($"--auth-method {authMethod}");

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>());
    }
    [Fact]
    public async Task ExecuteAsync_GetBySubscriptionId_ReturnsMatchingSubscription()
    {
        // Arrange
        var expectedSubscriptionId = "test-subscription-id";
        var expectedDisplayName = "Test Subscription";
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData(expectedSubscriptionId, expectedDisplayName)
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSubscriptions);

        var args = _parser.Parse($"--subscription {expectedSubscriptionId}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(1, subscriptionsArray.GetArrayLength());
        var subscription = subscriptionsArray[0];
        Assert.Equal(expectedSubscriptionId, subscription.GetProperty("subscriptionId").GetString());
        Assert.Equal(expectedDisplayName, subscription.GetProperty("displayName").GetString());
    }
    [Fact]
    public async Task ExecuteAsync_GetBySubscriptionName_ReturnsMatchingSubscription()
    {
        // Arrange
        var expectedSubscriptionId = "test-subscription-id";
        var expectedDisplayName = "Test Subscription";
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData(expectedSubscriptionId, expectedDisplayName)
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedSubscriptions);

        var args = _parser.Parse($" --subscription {expectedDisplayName}");

        // Act
        var result = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(1, subscriptionsArray.GetArrayLength());
        var subscription = subscriptionsArray[0];
        Assert.Equal(expectedSubscriptionId, subscription.GetProperty("subscriptionId").GetString());
        Assert.Equal(expectedDisplayName, subscription.GetProperty("displayName").GetString());
    }
}
