// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobPutCommandTests : CommandUnitTestsBase<BlobPutCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("upload_file", Command.Name);
        Assert.Contains("Uploads a file to OneLake storage", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("upload_file", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Theory]
    [InlineData("--workspace-id test-workspace --item-id test-item", "test-workspace", "test-item")]
    [InlineData("--workspace \"Analytics Workspace\" --item \"Sales Lakehouse\"", "Analytics Workspace", "Sales Lakehouse")]
    public async Task ExecuteAsync_UploadsInlineContentSuccessfully(string identifierArgs, string expectedWorkspace, string expectedItem)
    {
        var blobPath = "Files/sample.txt";
        var content = "Hello OneLake";

        var blobResult = new BlobPutResult(
            expectedWorkspace,
            expectedItem,
            blobPath,
            content.Length,
            "application/octet-stream",
            "etag",
            DateTimeOffset.UtcNow,
            "request-id",
            "2023-11-03",
            true,
            "md5-value",
            "crc64-value",
            "scope",
            "key-sha256",
            "version-id",
            "client-request-id",
            "root-activity-id");

        Service.PutBlobAsync(
            expectedWorkspace,
            expectedItem,
            blobPath,
            Arg.Any<Stream>(),
            Arg.Any<long>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(blobResult);

        var response = await ExecuteCommandAsync($"{identifierArgs} --file-path {blobPath} --content \"{content}\"");

        await Service.Received(1).PutBlobAsync(
            expectedWorkspace,
            expectedItem,
            blobPath,
            Arg.Any<Stream>(),
            content.Length,
            null,
            false,
            Arg.Any<CancellationToken>());

        var result = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.BlobPutCommandResult, HttpStatusCode.Created);

        Assert.Equal("2023-11-03", result.Version);
        Assert.True(result.RequestServerEncrypted);
        Assert.Equal("md5-value", result.ContentMd5);
        Assert.Equal("crc64-value", result.ContentCrc64);
        Assert.Equal("scope", result.EncryptionScope);
        Assert.Equal("key-sha256", result.EncryptionKeySha256);
        Assert.Equal("version-id", result.VersionId);
        Assert.Equal("client-request-id", result.ClientRequestId);
        Assert.Equal("root-activity-id", result.RootActivityId);
    }

    [Fact]
    public async Task ExecuteAsync_UploadsFromLocalFile()
    {
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var blobPath = "Files/data.json";
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "{\"hello\":\"world\"}", TestContext.Current.CancellationToken);

            Service.PutBlobAsync(
                workspaceId,
                itemId,
                blobPath,
                Arg.Any<Stream>(),
                Arg.Any<long>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .Returns(new BlobPutResult(workspaceId, itemId, blobPath, new FileInfo(tempFile).Length, "application/json", "etag", DateTimeOffset.UtcNow, "request-id"));

            var response = await ExecuteCommandAsync(
                "--workspace-id", workspaceId,
                "--item-id", itemId,
                "--file-path", blobPath,
                "--local-file-path", tempFile,
                "--content-type", "application/json",
                "--overwrite");

            Assert.Equal(HttpStatusCode.Created, response.Status);
            var expectedLength = new FileInfo(tempFile).Length;

            await Service.Received(1).PutBlobAsync(
                workspaceId,
                itemId,
                blobPath,
                Arg.Any<Stream>(),
                expectedLength,
                "application/json",
                true,
                Arg.Any<CancellationToken>());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenNoContentProvided()
    {
        var response = await ExecuteCommandAsync(
            "--workspace-id", "test-workspace",
            "--item-id", "test-item",
            "--file-path", "Files/empty.txt");

        Assert.NotEqual(HttpStatusCode.Created, response.Status);
        await Service.DidNotReceive().PutBlobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<long>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.PutBlobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<Stream>(),
            Arg.Any<long>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "blobPath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--file-path", traversalPath,
            "--content", "data");

        Assert.NotEqual(HttpStatusCode.Created, response.Status);
    }
}
