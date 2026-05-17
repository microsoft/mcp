// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob;

public class BlobGetCommandTests : CommandUnitTestsBase<BlobGetCommand, IStorageService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsBlobs()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";
        var expectedBlobs = new List<BlobInfo>(
        [
            new("blob", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null),
            new("blob2", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null)
        ]);

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedBlobs);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--account", account,
            "--container", container);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result.Blobs);
        Assert.Equal(expectedBlobs.Count, result.Blobs.Count);
        Assert.Equal(expectedBlobs.Select(a => a.Name), result.Blobs.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBlobs()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--account", account,
            "--container", container);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Empty(result.Blobs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--account", account,
            "--container", container);

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
    [InlineData("--account mystorageaccount --subscription sub123 --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --blob blob", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --blob blob --prefix prefix", true)]
    [InlineData("--subscription sub123", false)] // Missing account and container
    [InlineData("--account mystorageaccount", false)] // Missing subscription and container
    [InlineData("--container container", false)] // Missing subscription and account
    [InlineData("--subscription sub123 --account mystorageaccount", false)] // Missing container
    [InlineData("--subscription sub123 --container container", false)] // Missing account
    [InlineData("--account mystorageaccount --container container", false)] // Missing subscription
    [InlineData("--blob blob", false)] // Missing subscription, account, and container
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedBlobs = new List<BlobInfo>(
            [
                new("blob", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null)
            ]);

            Service.GetBlobDetails(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedBlobs);
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
    public async Task ExecuteAsync_ReturnsBlobDetails_WhenBlobExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";
        var expected = new BlobInfo(blob, DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null,
            new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null);

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
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
            "--container", container,
            "--blob", blob);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Single(result.Blobs);

        Assert.Equal(expected.Name, result.Blobs[0].Name);
        Assert.Equal(expected.LastModified, result.Blobs[0].LastModified);
        Assert.Equal(expected.ContentType, result.Blobs[0].ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
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
            "--container", container,
            "--blob", blob);

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
        var container = "container123";
        var blob = "notfound";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Blob not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--subscription", subscription,
            "--container", container,
            "--blob", blob);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Blob not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
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
            "--container", container,
            "--blob", blob);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
