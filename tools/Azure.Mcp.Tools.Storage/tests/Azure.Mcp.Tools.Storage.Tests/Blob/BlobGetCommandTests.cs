// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Blob;

public class BlobGetCommandTests : CommandUnitTestsBase<BlobGetCommand, IStorageService>
{
    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsBlobs()
    {
        // Arrange
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
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((expectedBlobs, null));

        // Act
        var response = await ExecuteCommandAsync("--account", account, "--container", container);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result.Blobs);
        Assert.Equal(expectedBlobs.Count, result.Blobs.Count);
        Assert.Equal(expectedBlobs.Select(a => a.Name), result.Blobs.Select(a => a.Name));
        var json = JsonSerializer.SerializeToElement(result, StorageJsonContext.Default.BlobGetCommandResult);
        Assert.False(json.TryGetProperty("prefixes", out _));
        Assert.False(json.TryGetProperty("continuationToken", out _));
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IStorageService.ListBlobs));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBlobs()
    {
        // Arrange
        var account = "testaccount";
        var container = "container123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(([], null));

        // Act
        var response = await ExecuteCommandAsync("--account", account, "--container", container);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Empty(result.Blobs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var account = "testaccount";
        var container = "container123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--account", account, "--container", container);

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
        Assert.DoesNotContain("--subscription", CommandDefinition.Options.Select(option => option.Name));
        var cursor = Assert.Single(CommandDefinition.Options, option => option.Name == "--cursor");
        Assert.Equal(ICursorOption.CursorDescription, cursor.Description);
    }

    [Theory]
    [InlineData("--account mystorageaccount --container container", true)]
    [InlineData("--container container --account mystorageaccount", true)]
    [InlineData("--account mystorageaccount --container container --blob blob", true)]
    [InlineData("--account mystorageaccount --container container --blob blob --prefix prefix", true)]
    [InlineData("--account mystorageaccount", false)] // Missing container
    [InlineData("--container container", false)] // Missing account
    [InlineData("--blob blob", false)] // Missing account and container
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
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns((expectedBlobs, null));
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
        var container = "container123";
        var blob = "blob123";
        var expected = new BlobInfo(blob, DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null,
            new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null);

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(([expected], null));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
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
        var container = "container123";
        var blob = "blob123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
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
        var container = "container123";
        var blob = "notfound";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Blob not found"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--container", container,
            "--blob", blob);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Container or blob not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var container = "container123";
        var blob = "blob123";

        Service.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--container", container,
            "--blob", blob);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_BindsCursorToBlobListingArguments()
    {
        const string marker = " next+/=% token ";
        Service.GetBlobDetails("testaccount", "container", null, "prefix", null, "tenant", Arg.Any<CancellationToken>())
            .Returns(([], marker));
        Service.GetBlobDetails("testaccount", "container", null, "prefix", marker, "tenant", Arg.Any<CancellationToken>())
            .Returns(([], null));
        string[] args = ["--account", "testaccount", "--container", "container", "--tenant", "tenant"];

        var first = await ExecuteCommandAsync([.. args, "--prefix", "prefix"]);
        var result = ValidateAndDeserializeResponse(first, StorageJsonContext.Default.BlobGetCommandResult);
        Assert.Empty(result.Blobs);
        Assert.NotNull(result.NextCursor);
        Assert.NotEqual(marker, result.NextCursor);

        var final = await ExecuteCommandAsync([.. args, "--prefix", "prefix", "--cursor", result.NextCursor]);
        var finalResult = ValidateAndDeserializeResponse(final, StorageJsonContext.Default.BlobGetCommandResult);
        var payload = JsonSerializer.SerializeToElement(finalResult, StorageJsonContext.Default.BlobGetCommandResult);
        Assert.False(payload.TryGetProperty("nextCursor", out _));
        await Service.Received(1).GetBlobDetails(
            "testaccount", "container", null, "prefix", marker, "tenant", Arg.Any<CancellationToken>());

        Service.ClearReceivedCalls();
        var mismatch = await ExecuteCommandAsync([.. args, "--prefix", "other", "--cursor", result.NextCursor]);
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.Status);
        Assert.Contains("does not match the original request", mismatch.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsCursorWithSpecificBlob()
    {
        var response = await ExecuteCommandAsync(
            "--account", "testaccount", "--container", "container", "--blob", "blob", "--cursor", "opaque");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--cursor cannot be used with --blob", response.Message);
        await Service.DidNotReceiveWithAnyArgs().GetBlobDetails(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDelimiter_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container", "--delimiter", string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--delimiter must not be empty.", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_HierarchicalListing_ReturnsAndResumesCursor()
    {
        const string marker = "opaque +/=&token";
        Service.ListBlobs("testaccount", "container", "/", "folder/", null, "tenant", Arg.Any<CancellationToken>())
            .Returns(new BlobListResult([], ["folder/subfolder/"], marker));
        Service.ListBlobs("testaccount", "container", "/", "folder/", marker, "tenant", Arg.Any<CancellationToken>())
            .Returns(new BlobListResult([], [], null));
        string[] args = ["--account", "testaccount", "--container", "container", "--prefix", "folder/", "--delimiter", "/", "--tenant", "tenant"];

        var first = ValidateAndDeserializeResponse(await ExecuteCommandAsync(args), StorageJsonContext.Default.BlobGetCommandResult);
        Assert.Empty(first.Blobs);
        Assert.Equal(["folder/subfolder/"], first.Prefixes);
        Assert.NotNull(first.NextCursor);
        Assert.NotEqual(marker, first.NextCursor);

        var last = ValidateAndDeserializeResponse(await ExecuteCommandAsync([.. args, "--cursor", first.NextCursor]), StorageJsonContext.Default.BlobGetCommandResult);
        Assert.Empty(last.Blobs);
        Assert.Empty(last.Prefixes!);
        var json = JsonSerializer.SerializeToElement(last, StorageJsonContext.Default.BlobGetCommandResult);
        Assert.False(json.TryGetProperty("nextCursor", out _));
        await Service.Received(1).ListBlobs("testaccount", "container", "/", "folder/", marker, "tenant", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--account other --container container --prefix folder/ --delimiter / --tenant tenant")]
    [InlineData("--account testaccount --container other --prefix folder/ --delimiter / --tenant tenant")]
    [InlineData("--account testaccount --container container --prefix other --delimiter / --tenant tenant")]
    [InlineData("--account testaccount --container container --prefix folder/ --delimiter :: --tenant tenant")]
    [InlineData("--account testaccount --container container --prefix folder/ --tenant tenant")]
    [InlineData("--account testaccount --container container --prefix folder/ --delimiter / --tenant other")]
    public async Task ExecuteAsync_HierarchicalCursorRejectsChangedArguments(string args)
    {
        var cursor = new PaginationCursor("d6bdc190-e68f-49af-82e7-9cf6ec9b8183", "testaccount", "container", "folder/", "tenant", "/").Encode("marker");
        var response = await ExecuteCommandAsync($"{args} --cursor {cursor}");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("does not match the original request", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDelimiterWithSpecificBlob()
    {
        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container", "--blob", "blob", "--delimiter", "/");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--delimiter cannot be used with --blob", response.Message);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task ExecuteAsync_RejectsBlankHierarchicalCursor(string cursor)
    {
        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container", "--delimiter", "/", "--cursor", cursor);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_HierarchicalListing_HandlesServiceErrors()
    {
        Service.ListBlobs("testaccount", "container", "/", null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed", "AuthorizationPermissionMismatch", null));

        var response = await ExecuteCommandAsync("--account", "testaccount", "--container", "container", "--delimiter", "/");
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Storage Blob Data Reader", response.Message);
    }
}
