// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.ConsumerGroup;

public class ConsumerGroupDeleteCommandTests : CommandUnitTestsBase<ConsumerGroupDeleteCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteConsumerGroupAsync(
                Arg.Any<string>(),
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
    public async Task ExecuteAsync_DeletesConsumerGroupSuccessfully()
    {
        // Arrange
        Service.DeleteConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub",
            "--consumer-group", "test-consumer-group");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).DeleteConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        Service.DeleteConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Consumer group 'test-consumer-group' could not be found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub",
            "--consumer-group", "test-consumer-group");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        Service.DeleteConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "unauthorized-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub",
            "--consumer-group", "test-consumer-group");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParameters()
    {
        // Arrange
        const string subscriptionId = "12345678-1234-1234-1234-123456789012";
        const string resourceGroup = "my-resource-group";
        const string namespaceName = "my-namespace";
        const string eventHubName = "my-eventhub";
        const string consumerGroupName = "my-consumer-group";

        Service.DeleteConsumerGroupAsync(
            consumerGroupName,
            eventHubName,
            namespaceName,
            resourceGroup,
            subscriptionId,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscriptionId,
            "--resource-group", resourceGroup,
            "--namespace", namespaceName,
            "--eventhub", eventHubName,
            "--consumer-group", consumerGroupName);

        // Assert
        Assert.Equal(200, (int)response.Status);

        await Service.Received(1).DeleteConsumerGroupAsync(
            consumerGroupName,
            eventHubName,
            namespaceName,
            resourceGroup,
            subscriptionId,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
