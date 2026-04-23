// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventHubs.Commands.EventHub;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.EventHub;

public class EventHubDeleteCommandTests : CommandUnitTestsBase<EventHubDeleteCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteEventHubAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(true);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(200, (int)response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(200, (int)response.Status);
            Assert.NotNull(response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        Service.DeleteEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Namespace 'test-namespace' not found in resource group 'test-rg'"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--eventhub", "test-hub",
            "--namespace", "test-namespace",
            "--resource-group", "test-rg");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        Service.DeleteEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Authentication failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "unauthorized-sub",
            "--eventhub", "test-hub",
            "--namespace", "test-namespace",
            "--resource-group", "test-rg");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfullyDeletesEventHub()
    {
        // Arrange
        Service.DeleteEventHubAsync(
            Arg.Is("test-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--eventhub", "test-hub",
            "--namespace", "test-namespace",
            "--resource-group", "test-rg");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_IdempotentWhenEventHubNotFound()
    {
        // Arrange
        Service.DeleteEventHubAsync(
            Arg.Is("nonexistent-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--eventhub", "nonexistent-hub",
            "--namespace", "test-namespace",
            "--resource-group", "test-rg");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }
}
