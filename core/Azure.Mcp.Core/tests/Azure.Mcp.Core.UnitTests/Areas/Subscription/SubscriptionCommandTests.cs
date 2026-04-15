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

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

public class SubscriptionCommandTests : CommandUnitTestsBase<AccountGetCommand, IStorageService>
{
    [Fact]
    public void Validate_WithEnvironmentVariableOnly_PassesValidation()
    {
        // Arrange
        TestEnvironment.SetAzureSubscriptionId("env-subs");

        // Act
        var parseResult = _commandDefinition.Parse([]);

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

        _service.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        var parseResult = _commandDefinition.Parse([]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);

        // Verify the service was called with the environment variable subscription
        await _service.Received(1).GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            subscription,
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

        _service.GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(expectedSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedAccounts);

        var parseResult = _commandDefinition.Parse(["--subscription", expectedSubscription]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);

        // Verify the service was called with the option subscription, not the environment variable
        await _service.Received(1).GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            expectedSubscription,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
        await _service.DidNotReceive().GetAccountDetails(
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            ignoredSubscription,
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
