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

public class EventHubUpdateCommandTests : CommandUnitTestsBase<EventHubUpdateCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg --partition-count 4", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace --resource-group test-rg --message-retention-in-hours 168", true)]
    [InlineData("--subscription test-subscription --eventhub test-hub --namespace test-namespace", false)]
    [InlineData("--subscription test-subscription --eventhub test-hub", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var eventHub = new Models.EventHub(
                "test-hub",
                "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/test-hub",
                "test-rg",
                null,
                4,
                7,
                "Active",
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow,
                ["0", "1", "2", "3"]);

            Service.CreateOrUpdateEventHubAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<int?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(eventHub);
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
        Service.CreateOrUpdateEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
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
        Service.CreateOrUpdateEventHubAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
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
    public async Task ExecuteAsync_SuccessfullyCreatesEventHub()
    {
        // Arrange
        var eventHub = new Models.EventHub(
            "new-hub",
            "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/test-rg/providers/Microsoft.EventHub/namespaces/test-namespace/eventhubs/new-hub",
            "test-rg",
            null,
            8,
            14, // 336 hours = 14 days
            "Active",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            ["0", "1", "2", "3", "4", "5", "6", "7"]);

        Service.CreateOrUpdateEventHubAsync(
            Arg.Is("new-hub"),
            Arg.Is("test-namespace"),
            Arg.Is("test-rg"),
            Arg.Is("test-subscription"),
            Arg.Is(8),
            Arg.Is(336L),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(eventHub);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-subscription",
            "--eventhub", "new-hub",
            "--namespace", "test-namespace",
            "--resource-group", "test-rg",
            "--partition-count", "8",
            "--message-retention-in-hours", "336");

        // Assert
        Assert.Equal(200, (int)response.Status);
        Assert.NotNull(response.Results);
    }
}
