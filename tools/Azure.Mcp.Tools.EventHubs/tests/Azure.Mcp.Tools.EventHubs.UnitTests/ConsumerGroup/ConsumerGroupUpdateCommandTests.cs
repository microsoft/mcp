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

public class ConsumerGroupUpdateCommandTests : CommandUnitTestsBase<ConsumerGroupUpdateCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group --user-metadata test-metadata", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var consumerGroup = new Models.ConsumerGroup(
                "test-consumer-group",
                "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
                "test-rg",
                "test-namespace",
                "test-eventhub",
                "East US",
                args.Contains("--user-metadata") ? "test-metadata" : null,
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow);

            Service.CreateOrUpdateConsumerGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(consumerGroup);
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
    public async Task ExecuteAsync_CreatesConsumerGroupWithUserMetadata()
    {
        // Arrange
        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            "custom-metadata",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        Service.CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            "custom-metadata",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedConsumerGroup);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub",
            "--consumer-group", "test-consumer-group",
            "--user-metadata", "custom-metadata");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            "custom-metadata",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreatesConsumerGroupWithoutUserMetadata()
    {
        // Arrange
        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-eventhub/consumergroups/test-consumer-group",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            null,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        Service.CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedConsumerGroup);

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

        await Service.Received(1).CreateOrUpdateConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            null,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        Service.CreateOrUpdateConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Event Hub 'test-eventhub' could not be found"));

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
        Service.CreateOrUpdateConsumerGroupAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
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
}
