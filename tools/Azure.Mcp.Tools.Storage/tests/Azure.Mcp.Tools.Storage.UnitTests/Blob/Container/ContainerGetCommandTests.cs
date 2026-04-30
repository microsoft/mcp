// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob.Container;

public class ContainerGetCommandTests : CommandUnitTestsBase<ContainerGetCommand, IStorageService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsContainers()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var expectedContainers = new List<ContainerInfo>(
        [
            new("container", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false),
            new("container2", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false)
        ]);

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedContainers);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--account", account);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.NotNull(result.Containers);
        Assert.Equal(expectedContainers.Count, result.Containers.Count);
        Assert.Equal(expectedContainers.Select(a => a.Name), result.Containers.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoContainers()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--account", account);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";
        var account = "testaccount";

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--account", account);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--account mystorageaccount --subscription sub123", true)]
    [InlineData("--subscription sub123 --account mystorageaccount", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --prefix prefix", true)]
    [InlineData("--subscription sub123", false)] // Missing account
    [InlineData("--account mystorageaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedContainers = new List<ContainerInfo>(
            [
                new("container", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false)
            ]);

            Service.GetContainerDetails(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedContainers);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsContainerDetails_WhenContainerExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var expected = new ContainerInfo(container, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expected]);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.Single(result.Containers);

        Assert.Equal(expected.Name, result.Containers[0].Name);
        Assert.Equal(expected.LastModified, result.Containers[0].LastModified);
        Assert.Equal(expected.ETag, result.Containers[0].ETag);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "notfound";

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Container not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";

        Service.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
