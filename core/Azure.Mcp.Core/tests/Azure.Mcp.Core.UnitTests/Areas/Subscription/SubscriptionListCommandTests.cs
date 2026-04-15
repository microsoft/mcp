// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Areas.Subscription.Commands;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

public class SubscriptionListCommandTests : CommandUnitTestsBase<SubscriptionListCommand, ISubscriptionService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var expectedSubscriptions = new List<SubscriptionData>
        {
            SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Subscription 1"),
            SubscriptionTestHelpers.CreateSubscriptionData("sub2", "Subscription 2")
        };

        Service.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        Service.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await ExecuteCommandAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var subscriptions = ConvertResponse(result, SubscriptionJsonContext.Default.SubscriptionListCommandResult);
        Assert.NotNull(subscriptions);
        Assert.Equal(2, subscriptions.Subscriptions.Count);

        var first = subscriptions.Subscriptions[0];
        var second = subscriptions.Subscriptions[1];

        Assert.Equal("sub1", first.SubscriptionId);
        Assert.Equal("Subscription 1", first.DisplayName);
        Assert.False(first.IsDefault);
        Assert.Equal("sub2", second.SubscriptionId);
        Assert.Equal("Subscription 2", second.DisplayName);
        Assert.False(second.IsDefault);

        await Service.Received(1).GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithTenantId_PassesTenantToService()
    {
        // Arrange
        var tenantId = "test-tenant-id";

        Service.GetSubscriptions(Arg.Is(tenantId), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);
        Service.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await ExecuteCommandAsync("--tenant", tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await Service.Received(1).GetSubscriptions(
            Arg.Is(tenantId),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptySubscriptionList_ReturnsNotNullResults()
    {
        // Arrange
        Service.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([]);
        Service.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await ExecuteCommandAsync();

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
        Service.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await ExecuteCommandAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Contains(expectedError, result.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithAuthMethod_PassesAuthMethodToCommand()
    {
        // Arrange
        Service.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns([SubscriptionTestHelpers.CreateSubscriptionData("sub1", "Sub1")]);
        Service.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await ExecuteCommandAsync("--auth-method", AuthMethod.Credential.ToString().ToLowerInvariant());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        await Service.Received(1).GetSubscriptions(
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

        Service
            .GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        Service.GetDefaultSubscriptionId().Returns("sub2");

        var args = CommandDefinition.Parse("");

        // Act
        var result = await Command.ExecuteAsync(Context, args, TestContext.Current.CancellationToken);

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

        Service.GetSubscriptions(Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedSubscriptions);
        Service.GetDefaultSubscriptionId().Returns((string?)null);

        // Act
        var result = await ExecuteCommandAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Results);

        var subscriptions = ConvertResponse(result, SubscriptionJsonContext.Default.SubscriptionListCommandResult);
        Assert.NotNull(subscriptions);
        Assert.Equal(2, subscriptions.Subscriptions.Count);

        // No subscription should be marked as default
        Assert.All(subscriptions.Subscriptions, s => Assert.False(s.IsDefault));
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
