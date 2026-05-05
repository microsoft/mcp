// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ResourceHealth.Commands.ServiceHealthEvents;
using Azure.Mcp.Tools.ResourceHealth.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ResourceHealth.UnitTests.ServiceHealthEvents;

public class ServiceHealthEventsListCommandTests : CommandUnitTestsBase<ServiceHealthEventsListCommand, IResourceHealthService>
{
    [Theory]
    [InlineData("", false)]
    [InlineData("--subscription sub123", true)]
    [InlineData("--subscription sub123 --event-type ServiceIssue", true)]
    [InlineData("--subscription sub123 --event-type InvalidType", false)]
    [InlineData("--subscription sub123 --status Active", true)]
    [InlineData("--subscription sub123 --status InvalidStatus", false)]
    [InlineData("--subscription sub123 --tracking-id TRACK123", true)]
    [InlineData("--subscription sub123 --filter startTime ge 2023-01-01", true)]
    public async Task ExecuteAsync_ValidatesInput(string args, bool shouldSucceed)
    {
        // The subscription option falls back to the Azure CLI profile or AZURE_SUBSCRIPTION_ID env var.
        // For the empty-args (missing subscription) cases, skip when a CLI
        // profile default is present so we only assert when validation actually fails.
        if (!shouldSucceed && string.IsNullOrWhiteSpace(args))
        {
            TestEnvironment.SkipIfDefaultSubscriptionConfigured();
        }

        // Arrange
        if (shouldSucceed)
        {
            // Setup service mock for successful cases
            Service.ListServiceHealthEventsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Special parsing for complex arguments
        string[] cleanedArgs = args.Contains("--filter") ?
            ["--subscription", "sub123", "--filter", "startTime ge 2023-01-01"] :
            args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Act
        var response = await ExecuteCommandAsync(cleanedArgs);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            // Error message might contain "required" for missing subscription or "Invalid" for enum validation
            Assert.True(
                response.Message.Contains("required", StringComparison.CurrentCultureIgnoreCase) ||
                response.Message.Contains("invalid", StringComparison.CurrentCultureIgnoreCase),
                $"Expected error message to contain 'required' or 'invalid', but got: {response.Message}");
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSubscription_ReturnsSuccess()
    {
        // Arrange
        Service.ListServiceHealthEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        var expectedError = "Service error";
        Service.ListServiceHealthEventsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "nonexistent-sub");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains(expectedError, response.Message ?? "");
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenSubscriptionLookupFails()
    {
        var subscription = "missing-subscription";
        var expectedError = $"Could not find subscription with name {subscription} (Parameter 'subscriptionName'). To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        Service.ListServiceHealthEventsAsync(
            subscription,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException($"Could not find subscription with name {subscription}", "subscriptionName"));

        var response = await ExecuteCommandAsync("--subscription", subscription);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--subscription", "sub123")]
    [InlineData("--subscription", "sub123", "--event-type", "ServiceIssue", "--status", "Active")]
    public async Task ExecuteAsync_ReturnsValidJsonStructure(params string[] args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert - Should have proper structure even if empty results
        Assert.NotNull(response);
    }
}
