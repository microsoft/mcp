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

public class ConsumerGroupGetCommandTests : CommandUnitTestsBase<ConsumerGroupGetCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg --namespace test-namespace --eventhub test-eventhub --consumer-group test-consumer-group", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var consumerGroups = new List<Models.ConsumerGroup>
            {
                new("test-group", "test-id", "test-rg", "test-namespace", "test-eventhub")
            };

            Service.GetConsumerGroupsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(consumerGroups);

            Service.GetConsumerGroupAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(consumerGroups.First());
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
    public async Task ExecuteAsync_ListsConsumerGroupsSuccessfully()
    {
        // Arrange
        var expectedConsumerGroups = new List<Models.ConsumerGroup>
        {
            new("group1", "id1", "test-rg", "test-namespace", "test-eventhub"),
            new("group2", "id2", "test-rg", "test-namespace", "test-eventhub")
        };

        Service.GetConsumerGroupsAsync(
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedConsumerGroups);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);

        await Service.Received(1).GetConsumerGroupsAsync(
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_GetsSingleConsumerGroupSuccessfully()
    {
        // Arrange
        var expectedConsumerGroup = new Models.ConsumerGroup(
            "test-consumer-group",
            "test-id",
            "test-rg",
            "test-namespace",
            "test-eventhub",
            "East US",
            "test metadata",
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow);

        Service.GetConsumerGroupAsync(
            "test-consumer-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
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

        await Service.Received(1).GetConsumerGroupAsync(
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
    public async Task ExecuteAsync_ReturnsEmptyListWhenConsumerGroupNotFound()
    {
        // Arrange
        Service.GetConsumerGroupAsync(
            "nonexistent-group",
            "test-eventhub",
            "test-namespace",
            "test-rg",
            "test-subscription",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns((Models.ConsumerGroup?)null);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub",
            "--consumer-group", "nonexistent-group");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        Service.GetConsumerGroupsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Event Hub 'test-eventhub' could not be found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--eventhub", "test-eventhub");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        Service.GetConsumerGroupsAsync(
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
            "--eventhub", "test-eventhub");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }



    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.False(string.IsNullOrEmpty(CommandDefinition.Description));
        Assert.NotEmpty(Command.Title);
    }
}
