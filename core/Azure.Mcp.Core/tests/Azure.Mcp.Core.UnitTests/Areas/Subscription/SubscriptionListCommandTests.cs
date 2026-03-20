// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Subscription.Commands;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
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
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
        Assert.False(first.GetProperty("isDefault").GetBoolean());
        Assert.Equal("sub2", second.GetProperty("subscriptionId").GetString());
        Assert.Equal("Subscription 2", second.GetProperty("displayName").GetString());
        Assert.False(second.GetProperty("isDefault").GetBoolean());

        await _subscriptionService.Received(1).GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenantId_PassesTenantToService()
    {
        // Arrange
        var tenantId = "test-tenant-id";
        var args = _commandDefinition.Parse($"--tenant {tenantId}");

        _subscriptionService
            .GetSubscriptions(Arg.Is<string>(x => x == tenantId), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Is<string>(x => x == tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptySubscriptionList_ReturnsNotNullResults()
    {
        // Arrange
        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SubscriptionData>>(new Exception(expectedError)));

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

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
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await _subscriptionService.Received(1).GetSubscriptions(
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultSubscription_MarksDefaultSubscription()
    {
        // Arrange
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        _subscriptionService.GetDefaultSubscriptionId().Returns("sub2");

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(2, subscriptionsArray.GetArrayLength());

        // Default subscription should be first
        var first = subscriptionsArray[0];
        Assert.Equal("sub2", first.GetProperty("subscriptionId").GetString());
        Assert.True(first.GetProperty("isDefault").GetBoolean());

        var second = subscriptionsArray[1];
        Assert.Equal("sub1", second.GetProperty("subscriptionId").GetString());
        Assert.False(second.GetProperty("isDefault").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WithNoDefaultSubscription_AllSubscriptionsNotDefault()
    {
        // Arrange
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        _subscriptionService
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        var args = _commandDefinition.Parse("");

        // Act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(result.Results));
        var subscriptionsArray = jsonDoc.RootElement.GetProperty("subscriptions");

        Assert.Equal(2, subscriptionsArray.GetArrayLength());

        // No subscription should be marked as default
        for (int i = 0; i < subscriptionsArray.GetArrayLength(); i++)
        {
            Assert.False(subscriptionsArray[i].GetProperty("isDefault").GetBoolean());
        }
    }

    [Fact]
    public void MapToSubscriptionInfos_WithDefaultSubscriptionId_DefaultIsFirst()
    {
        // Arrange
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub3", "Subscription 3")
        };

        // Act
        var result = SubscriptionListCommand.MapToSubscriptionInfos(subscriptions, "sub2");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("sub2", result[0].SubscriptionId);
        Assert.True(result[0].IsDefault);
        Assert.False(result[1].IsDefault);
        Assert.False(result[2].IsDefault);
    }

    [Fact]
    public void MapToSubscriptionInfos_WithNoDefaultSubscriptionId_NoneMarkedDefault()
    {
        // Arrange
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        // Act
        var result = SubscriptionListCommand.MapToSubscriptionInfos(subscriptions, null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.False(s.IsDefault));
    }

    [Fact]
    public void MapToSubscriptionInfos_WithNonMatchingDefaultId_NoneMarkedDefault()
    {
        // Arrange
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        // Act
        var result = SubscriptionListCommand.MapToSubscriptionInfos(subscriptions, "non-existent");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.False(s.IsDefault));
    }

    [Fact]
    public void MapToSubscriptionInfos_IncludesStateAndTenantId()
    {
        // Arrange
        var subscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1")
        };

        // Act
        var result = SubscriptionListCommand.MapToSubscriptionInfos(subscriptions, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("sub1", result[0].SubscriptionId);
        Assert.Equal("Subscription 1", result[0].DisplayName);
        Assert.NotNull(result[0].State);
        Assert.NotNull(result[0].TenantId);
    }
}
