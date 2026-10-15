// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Blob.Container;

public class ContainerCreateCommandTests : CommandUnitTestsBase<ContainerCreateCommand, IStorageService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("create", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.DoesNotContain("--subscription", CommandDefinition.Options.Select(option => option.Name));
    }

    [Theory]
    [InlineData("--account testaccount --container container123", true)]
    [InlineData("--account testaccount --tenant tenant123 --container container123 ", true)]
    [InlineData("--account testaccount", false)] // Missing container name
    [InlineData("--container container123", false)] // Missing account name
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
                Arg.Any<string?>(),
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
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Container already exists"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
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
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--container", "invalidaccess");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesStorageAccountNotFound()
    {
        // Arrange
        Service.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Storage account not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "nonexistentaccount",
            "--container", "container123");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
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
        var account = "testaccount";
        var container = "container123";

        var expected = new ContainerInfo(container, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);

        Service.CreateContainer(
            account,
            container,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--container", container);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateContainer(
            account,
            container,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsTenantAndCancellationToken()
    {
        var token = TestContext.Current.CancellationToken;
        var expected = new ContainerInfo("container123", DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);
        Service.CreateContainer("testaccount", "container123", "specific-tenant", token).Returns(expected);

        var response = await ExecuteCommandAsync(
            "--account", "testaccount", "--container", "container123", "--tenant", "specific-tenant");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).CreateContainer("testaccount", "container123", "specific-tenant", token);
    }

    [Theory]
    [InlineData("AuthorizationPermissionMismatch", "Storage Blob Data Contributor")]
    [InlineData("InsufficientAccountPermissions", "Storage Blob Data Contributor")]
    [InlineData("UnknownForbidden", "network/firewall")]
    public async Task ExecuteAsync_KeepsHumanRbacOverrides(string errorCode, string expectedMessage)
    {
        Service.CreateContainer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "denied", errorCode, null));

        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container123");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains(expectedMessage, response.Message);
        Assert.DoesNotContain("Storage request failed (HTTP", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_KeepsHumanAlreadyExistsOverride()
    {
        Service.CreateContainer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(409, "already exists", "ContainerAlreadyExists", null));
        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container123");
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("use 'storage blob container get' first", response.Message);
        Assert.DoesNotContain("Storage request failed (HTTP", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_UsesSelectedGenericErrorDefault()
    {
        Service.CreateContainer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(429, "retry later"));
        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container123");
        Assert.Equal(HttpStatusCode.TooManyRequests, response.Status);
        Assert.Contains("retry later", response.Message);
#if STORAGE_GENERATED_MCP
        Assert.Contains("HTTP 429", response.Message);
#else
        Assert.DoesNotContain("Storage request failed (HTTP", response.Message);
#endif
    }
}
