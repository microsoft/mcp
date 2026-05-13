// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceGetCommandTests : CommandUnitTestsBase<NamespaceGetCommand, IEventHubsService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription test-subscription", true)]
    [InlineData("--subscription test-subscription --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --namespace test-namespace --resource-group test-rg", true)]
    [InlineData("--subscription test-subscription --namespace test-namespace", false)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            // Set up appropriate service method based on arguments
            if (args.Contains($"{EventHubsOptionDefinitions.NamespaceOption.Name}") && args.Contains($"{OptionDefinitions.Common.ResourceGroup.Name}"))
            {
                // Single namespace request
                var namespaceDetails = new Models.Namespace(
                    "eh-namespace-prod-001",
                    "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-001",
                    "rg-eventhubs-prod",
                    "East US",
                    new Models.EventHubsSku("Standard", "Standard", null),
                    "Active",
                    "Succeeded",
                    DateTimeOffset.UtcNow.AddDays(-30),
                    DateTimeOffset.UtcNow.AddDays(-1),
                    "https://eh-namespace-prod-001.servicebus.windows.net:443/",
                    "12345678-1234-1234-1234-123456789012:eh-namespace-prod-001",
                    false,
                    null,
                    true,
                    true,
                    new Dictionary<string, string> { { "env", "prod" } });

                Service.GetNamespaceAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                    .Returns(namespaceDetails);
            }
            else
            {
                // List request
                var namespaces = new List<Models.Namespace>
                {
                    new("eh-namespace-prod-001",
                        "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-001",
                        "rg-eventhubs-prod",
                        "East US",
                        new Models.EventHubsSku("Standard", "Standard", null)),
                    new("eh-namespace-prod-002",
                        "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-namespace-prod-002",
                        "rg-eventhubs-prod",
                        "East US",
                        new Models.EventHubsSku("Standard", "Standard", null)),
                    new("eh-shared-services",
                        "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg-eventhubs-prod/providers/Microsoft.EventHub/namespaces/eh-shared-services",
                        "rg-eventhubs-prod",
                        "East US",
                        new Models.EventHubsSku("Standard", "Standard", null)),
                };

                Service.GetNamespacesAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<RetryPolicyOptions?>(),
                    Arg.Any<CancellationToken>())
                    .Returns(namespaces);
            }
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
        Service.GetNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Resource Group 'rg-eventhubs-test' could not be found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "12345678-1234-1234-1234-123456789012",
            "--resource-group", "rg-eventhubs-test");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationError()
    {
        // Arrange
        Service.GetNamespacesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("The current user does not have access to subscription 'unauthorized-sub'"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "unauthorized-sub",
            "--resource-group", "rg-eventhubs-prod");

        // Assert
        Assert.NotEqual(200, (int)response.Status);
        Assert.NotNull(response.Message);
    }
}
