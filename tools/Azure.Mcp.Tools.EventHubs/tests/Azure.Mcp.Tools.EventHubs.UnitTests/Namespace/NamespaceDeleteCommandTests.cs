// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.EventHubs.Commands.Namespace;
using Azure.Mcp.Tools.EventHubs.Options.Namespace;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.EventHubs.UnitTests.Namespace;

public class NamespaceDeleteCommandTests : CommandUnitTestsBase<NamespaceDeleteCommand, IEventHubsService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete", Command.Name);
        Assert.Equal("Delete Event Hubs Namespace", Command.Title);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("", false, "Missing Required")]
    [InlineData("--subscription test-sub", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg", false, "Missing Required")]
    [InlineData("--subscription test-sub --resource-group test-rg --namespace test-ns", true, "")]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedErrorFragment)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.DeleteNamespaceAsync(
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
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.NotEqual(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Message);
            if (!string.IsNullOrEmpty(expectedErrorFragment))
            {
                Assert.Contains(expectedErrorFragment, response.Message);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeletesNamespaceSuccessfully()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesNamespaceWithTenant()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "test-tenant-123",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tenant", "test-tenant-123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        await Service.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            "test-tenant-123",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNamespaceNotFound()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new KeyNotFoundException("Namespace not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "nonexistent-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAccessDenied()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "protected-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "Conflict: The namespace cannot be deleted in its current state"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "conflicted-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Conflict", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthenticationFailure()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Identity.AuthenticationFailedException("Authentication failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
        Assert.Contains("Authentication failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesResourceNotFoundError()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(404, "Resource group not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "nonexistent-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesGenericServiceError()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsSuccessMessage()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        // Verify the response contains a result
        Assert.NotNull(response.Results);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace",
            "--tenant", "test-tenant"
        ]);

        // Act
        var options = Command.GetType()
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(Command, [parseResult]) as NamespaceDeleteOptions;

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-sub", options.Subscription);
        Assert.Equal("test-rg", options.ResourceGroup);
        Assert.Equal("test-namespace", options.Namespace);
        Assert.Equal("test-tenant", options.Tenant);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectParametersToService()
    {
        // Arrange
        var namespaceName = "my-test-namespace";
        var resourceGroup = "my-resource-group";
        var subscription = "my-subscription";
        var tenant = "my-tenant";

        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--namespace", namespaceName,
            "--tenant", tenant);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteNamespaceAsync(
            namespaceName,
            resourceGroup,
            subscription,
            tenant,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutTenant_PassesNullTenant()
    {
        // Arrange
        Service.DeleteNamespaceAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "test-rg",
            "--namespace", "test-namespace");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteNamespaceAsync(
            "test-namespace",
            "test-rg",
            "test-sub",
            null, // tenant should be null
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
