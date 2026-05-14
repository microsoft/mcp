// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Storage.Commands.Account;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Subscription;

public class SubscriptionCommandTests : CommandUnitTestsBase<AccountGetCommand, IStorageService>
{
    [Fact]
    public void Validate_WithEnvironmentVariableOnly_PassesValidation()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");

        // Act
        var parseResult = CommandDefinition.Parse([]);

        // Assert
        Assert.Empty(parseResult.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_WithEnvironmentVariableOnly_CallsServiceWithCorrectSubscription()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var subscription = CommandHelper.GetDefaultSubscription()!;

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
        TestEnvironment.SetAzureSubscriptionId("env-subs");
        var ignoredSubscription = CommandHelper.GetDefaultSubscription()!;
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
