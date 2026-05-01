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

public class ContainerCreateCommandTests : CommandUnitTestsBase<ContainerCreateCommand, IStorageService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
    }

    [Theory]
    [InlineData("--account testaccount --subscription sub123 --container container123", true)]
    [InlineData("--account testaccount --tenant tenant123 --subscription sub123 --container container123 ", true)]
    [InlineData("--account testaccount", false)] // Missing subscription and container name
    [InlineData("--container container123", false)] // Missing subscription and account name
    [InlineData("--subscription sub123", false)] // Missing account name and container name
    [InlineData("--account testaccount --subscription sub123", false)] // Missing container name
    [InlineData("--container container123 --subscription sub123", false)] // Missing account name
    [InlineData("--account testaccount --container container123", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expected = new ContainerInfo("container123", DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
                "unlocked", "available", null, "private", false, false, null, null, false);

            Service.CreateContainer(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expected);
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.ContainerCreateCommandResult);

            Assert.NotNull(result.Container);
            Assert.Equal("container123", result.Container.Name);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesContainerAlreadyExists()
    {
        // Arrange
        Service.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Container already exists"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--subscription", "sub123",
             "--container", "existingcontainer");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        Service.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--subscription", "sub123",
            "--container", "invalidaccess");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--subscription", "sub123",
            "--container", "container123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        var expected = new ContainerInfo(container, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);

        Service.CreateContainer(
            account,
            container,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateContainer(
            account,
            container,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
