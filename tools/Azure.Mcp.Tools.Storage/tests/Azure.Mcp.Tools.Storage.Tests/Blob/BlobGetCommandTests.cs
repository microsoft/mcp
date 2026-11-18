// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Blob;

public class BlobGetCommandTests : CommandUnitTestsBase<BlobGetCommand, IStorageService>
{
    [Fact]
    public void Constructor_SelectDescription_AppendsSerializedBlobFieldsToDeclaredDescription()
    {
        var select = Assert.Single(CommandDefinition.Options, option => option.Name == "--select");
        var declaredSelect = Assert.Single(OptionDescriptor.FromType<BlobGetOptions>(), option => option.Name == "select");

        Assert.StartsWith($"{declaredSelect.Description} Choices are: ", select.Description);
        Assert.Equal(
            "Comma-separated list of response fields to include in the output. If not specified all fields will be included. " +
            "Choices are: name, lastModified, eTag, contentLength, contentType, contentHash, blobType, metadata, " +
            "leaseStatus, leaseState, leaseDuration, copyStatus, copySource, copyCompletedOn, accessTier, accessTierChangedOn, " +
            "hasLegalHold, createdOn, archiveStatus, versionId",
            select.Description);
    }

    [Theory]
    [InlineData("name,contentLength")]
    [InlineData(" NAME , contentLength,name ")]
    public async Task ExecuteAsync_Select_FiltersEveryBlobAndPreservesWrapper(string select)
    {
        Service.GetBlobDetails(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([CreateBlob("first"), CreateBlob("second")]);

        var response = await ExecuteCommandAsync("--account", "account", "--container", "container", "--select", select);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        var result = Assert.IsType<JsonObject>(JsonSerializer.SerializeToNode(response.Results));
        Assert.Single(result);
        var blobs = Assert.IsType<JsonArray>(result["blobs"]);
        Assert.Equal(2, blobs.Count);
        for (var i = 0; i < blobs.Count; i++)
        {
            var blob = Assert.IsType<JsonObject>(blobs[i]);
            Assert.Equal(["name", "contentLength"], blob.Select(property => property.Key));
            Assert.Equal(i == 0 ? "first" : "second", blob["name"]!.GetValue<string>());
            Assert.Equal(42, blob["contentLength"]!.GetValue<long>());
        }
    }

    [Fact]
    public async Task ExecuteAsync_Select_PreservesNestedMetadataAndDoesNotAffectLaterRequests()
    {
        Service.GetBlobDetails(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([CreateBlob("blob")]);

        var response = await ExecuteCommandAsync("--account", "account", "--container", "container", "--select", "metadata");
        var selected = JsonSerializer.SerializeToNode(response.Results);
        Assert.Equal("""{"blobs":[{"metadata":{"owner":"test","purpose":"selection"}}]}""", selected!.ToJsonString());

        var unfiltered = await ((IBaseCommand)Command).ExecuteAsync(
            new CommandContext(), CommandDefinition.Parse("--account account --container container"),
            TestContext.Current.CancellationToken);
        var blob = JsonSerializer.SerializeToNode(unfiltered.Results)!["blobs"]![0]!;
        Assert.Equal("blob", blob["name"]!.GetValue<string>());
        Assert.Equal("application/octet-stream", blob["contentType"]!.GetValue<string>());
        Assert.Equal(42, blob["contentLength"]!.GetValue<long>());
        Assert.Equal(selected.ToJsonString(), JsonSerializer.SerializeToNode(response.Results)!.ToJsonString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(",")]
    [InlineData("name,")]
    [InlineData("name,unknown")]
    [InlineData("metadata.owner")]
    [InlineData("blobs")]
    public async Task ExecuteAsync_InvalidSelect_ReturnsBadRequestWithoutCallingService(string select)
    {
        var response = await ExecuteCommandAsync("--account", "account", "--container", "container", "--select", select);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--select", response.Message);
        Assert.Null(response.Results);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_Select_PreservesEmptyCollection()
    {
        Service.GetBlobDetails(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--account", "account", "--container", "container", "--select", "name");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("""{"blobs":[]}""", JsonSerializer.SerializeToNode(response.Results)!.ToJsonString());
    }

    [Fact]
    public async Task ExecuteAsync_Select_DoesNotFilterErrorDetails()
    {
        Service.GetBlobDetails(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--account", "account", "--container", "container", "--select", "name");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        var error = JsonSerializer.SerializeToNode(response.Results);
        Assert.Equal("Test error", error!["message"]!.GetValue<string>());
        Assert.Equal("Exception", error["type"]!.GetValue<string>());
    }

    private static BlobInfo CreateBlob(string name) =>
        new(name, DateTimeOffset.UtcNow, null, 42, "application/octet-stream", null, null,
            new Dictionary<string, string> { ["owner"] = "test", ["purpose"] = "selection" },
            null, null, null, null, null, null, null, null, false, null, null, null);

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
}
