// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
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
            Arg.Any<CancellationToken>())
            .Returns(expectedBlobs);

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
            Arg.Any<CancellationToken>())
            .Returns([]);

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
            Arg.Any<CancellationToken>())
            .Returns([expected]);

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

    [Theory]
    [InlineData("--delimiter /", null, "/", 100, null)]
    [InlineData("--max-results 2 --prefix folder/", "folder/", null, 2, null)]
    [InlineData("--max-results 5000", null, null, 5000, null)]
    [InlineData("--delimiter :: --prefix folder:: --max-results 2 --continuation-token next-token", "folder::", "::", 2, "next-token")]
    [InlineData("--continuation-token next-token", null, null, 100, "next-token")]
    public async Task ExecuteAsync_PagedListing_ForwardsOptionsAndReturnsPage(
        string listingArgs, string? prefix, string? delimiter, int maxResults, string? continuationToken)
    {
        var expectedBlob = new BlobInfo("folder/file.txt", DateTimeOffset.UtcNow, null, 42, "text/plain", null, null,
            new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null);
        List<string> prefixes = delimiter is null ? [] : ["folder/subfolder/"];

        Service.ListBlobs("testaccount", "container", maxResults, prefix, delimiter, continuationToken, "test-tenant", Arg.Any<CancellationToken>())
            .Returns(new BlobListResult([expectedBlob], prefixes, "next-page"));

        var response = await ExecuteCommandAsync($"--account testaccount --container container --tenant test-tenant {listingArgs}");
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Equal(expectedBlob.Name, Assert.Single(result.Blobs).Name);
        Assert.Equal(prefixes, result.Prefixes);
        Assert.Equal("next-page", result.ContinuationToken);
        await Service.Received(1).ListBlobs(
            "testaccount", "container", maxResults, prefix, delimiter, continuationToken, "test-tenant", Arg.Any<CancellationToken>());
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IStorageService.GetBlobDetails));
    }

    [Fact]
    public async Task ExecuteAsync_PrefixOnly_PreservesUnpagedListing()
    {
        Service.GetBlobDetails("testaccount", "container", null, "folder/", null, Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--account testaccount --container container --prefix folder/");
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Empty(result.Blobs);
        Assert.Null(result.Prefixes);
        Assert.Null(result.ContinuationToken);
        await Service.Received(1).GetBlobDetails("testaccount", "container", null, "folder/", null, Arg.Any<CancellationToken>());
        Assert.DoesNotContain(Service.ReceivedCalls(), call => call.GetMethodInfo().Name == nameof(IStorageService.ListBlobs));
    }

    [Fact]
    public async Task ExecuteAsync_PagedListing_ReturnsDirectoriesWithoutBlobs()
    {
        Service.ListBlobs("testaccount", "container", 1, null, "/", null, null, Arg.Any<CancellationToken>())
            .Returns(new BlobListResult([], ["folder/"], "opaque+/=token"));

        var response = await ExecuteCommandAsync("--account testaccount --container container --delimiter / --max-results 1");
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Empty(result.Blobs);
        Assert.Equal(["folder/"], result.Prefixes);
        Assert.Equal("opaque+/=token", result.ContinuationToken);
    }

    [Fact]
    public async Task ExecuteAsync_PagedListing_ReturnsEmptyPageWithoutContinuationToken()
    {
        Service.ListBlobs("testaccount", "container", 1, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new BlobListResult([], [], null));

        var response = await ExecuteCommandAsync("--account testaccount --container container --max-results 1");
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.Empty(result.Blobs);
        Assert.NotNull(result.Prefixes);
        Assert.Empty(result.Prefixes);
        Assert.Null(result.ContinuationToken);
        var json = JsonSerializer.SerializeToElement(result, StorageJsonContext.Default.BlobGetCommandResult);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("blobs").ValueKind);
        Assert.Equal(JsonValueKind.Array, json.GetProperty("prefixes").ValueKind);
        Assert.False(json.TryGetProperty("continuationToken", out _));
    }

    [Theory]
    [InlineData("--max-results 0", "--max-results must be between 1 and 5000.")]
    [InlineData("--max-results -1", "--max-results must be between 1 and 5000.")]
    [InlineData("--max-results 5001", "--max-results must be between 1 and 5000.")]
    [InlineData("--blob file.txt --delimiter /", "cannot be used with --blob")]
    [InlineData("--blob file.txt --max-results 1", "cannot be used with --blob")]
    [InlineData("--blob file.txt --continuation-token token", "cannot be used with --blob")]
    public async Task ExecuteAsync_InvalidListingOptions_ReturnsBadRequest(string listingArgs, string expectedError)
    {
        var response = await ExecuteCommandAsync($"--account testaccount --container container {listingArgs}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Empty(Service.ReceivedCalls());
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
    public async Task ExecuteAsync_PagedListing_HandlesServiceErrors()
    {
        Service.ListBlobs("testaccount", "container", 1, null, null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed", "AuthorizationPermissionMismatch", null));

        var response = await ExecuteCommandAsync("--account testaccount --container container --max-results 1");

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Storage Blob Data Reader", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }
}
