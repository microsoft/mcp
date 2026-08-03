// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Subscription;

public class SubscriptionCommandTests : SubscriptionCommandUnitTestsBase<AccountGetCommand, IStorageService>
{

    [Fact]
    public void Validate_WithEnvironmentVariableOnly_PassesValidation()
    {
        // Arrange
        SubscriptionResolver.ResolveSubscription(Arg.Any<string?>()).Returns("env-subs");

        // Act
        var parseResult = CommandDefinition.Parse([]);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnvironmentVariableOnly_CallsServiceWithCorrectSubscription()
    {
        // Arrange
        var subscription = "env-subs";
        SubscriptionResolver.ResolveSubscription(Arg.Any<string?>()).Returns(subscription);

        var expectedAccounts = new ResourceQueryResults<StorageAccountInfo>(
        [
            new("account1", null, null, null, null, null, null, null, null, null),
            new("account2", null, null, null, null, null, null, null, null, null)
        ], false);

        Service.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        // Act
        var response = await ExecuteCommandAsync();

        // Assert
        Assert.NotNull(response);

        // Verify the service was called with the environment variable subscription
        await Service.Received(1).GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            subscription,
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithBothOptionAndEnvironmentVariable_PrefersOption()
    {
        // Arrange
        var ignoredSubscription = "env-subs";
        SubscriptionResolver.ResolveSubscription(null).Returns(ignoredSubscription);
        var expectedSubscription = "option-subs";

        var expectedAccounts = new ResourceQueryResults<StorageAccountInfo>(
        [
            new("account1", null, null, null, null, null, null, null, null, null),
            new("account2", null, null, null, null, null, null, null, null, null)
        ], false);

        Service.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(expectedSubscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        // Act
        var response = await ExecuteCommandAsync("--subscription", expectedSubscription);

        // Assert
        Assert.NotNull(response);

        // Verify the service was called with the option subscription, not the environment variable
        await Service.Received(1).GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            expectedSubscription,
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
        await Service.DidNotReceive().GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            ignoredSubscription,
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
