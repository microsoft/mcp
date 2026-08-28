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

namespace Azure.Mcp.Tools.Storage.Tests.Blob;

public class BlobUploadCommandTests : CommandUnitTestsBase<BlobUploadCommand, IStorageService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("upload", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Description);
        Assert.DoesNotContain(CommandDefinition.Options, option => option.Name == "--subscription");
    }

    [Fact]
    public async Task ExecuteAsync_UploadsBlob_WhenValidParametersProvided()
    {
        // Arrange
        var account = "testaccount";
        var container = "testcontainer";
        var blob = "testblob.txt";
        var localFilePath = "/tmp/file.txt";
        var expectedResult = new BlobUploadResult(blob, container, localFilePath,
            DateTimeOffset.UtcNow, "\"etag\"", "md5hash");

        Service.UploadBlob(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(localFilePath),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", account,
            "--container", container,
            "--blob", blob,
            "--local-file-path", localFilePath);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.BlobUploadResult);
        Assert.Equal(blob, result.Blob);
        Assert.Equal(container, result.Container);
        Assert.Equal(localFilePath, result.UploadedFile);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Upload failed";

        Service.UploadBlob(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "testaccount",
            "--container", "testcontainer",
            "--blob", "testblob",
            "--local-file-path", "/tmp/file.txt");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--account acct --container cont --blob b --local-file-path /f", true)]
    [InlineData("--container cont --blob b --local-file-path /f", false)] // Missing account
    [InlineData("--account acct --blob b --local-file-path /f", false)] // Missing container
    [InlineData("--account acct --container cont --local-file-path /f", false)] // Missing blob
    [InlineData("--account acct --container cont --blob b", false)] // Missing local-file-path
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedResult = new BlobUploadResult("b", "cont", "/f",
                DateTimeOffset.UtcNow, "\"etag\"", "md5hash");

            Service.UploadBlob(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedResult);
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
}
