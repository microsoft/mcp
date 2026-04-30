// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobGetCommandTests : CommandUnitTestsBase<BlobGetCommand, IOneLakeService>
{
    private readonly IOptions<ServiceStartOptions> _serviceStartOptions = Substitute.For<IOptions<ServiceStartOptions>>();

    public BlobGetCommandTests()
    {
        _serviceStartOptions.Value.Returns(new ServiceStartOptions { Transport = TransportTypes.StdIo });
        Services.AddSingleton(_serviceStartOptions);
    }

    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("download_file", Command.Name);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.Destructive);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlobAndMetadata()
    {
        var workspaceId = "workspace";
        var itemId = "lakehouse";
        var blobPath = "Files/sample.txt";
        var contentBytes = Encoding.UTF8.GetBytes("hello");
        var encodedContent = Convert.ToBase64String(contentBytes);

        var result = new BlobGetResult(
            workspaceId,
            itemId,
            blobPath,
            contentBytes.Length,
            "text/plain",
            "utf-8",
            null,
            null,
            null,
            "md5",
            "crc64",
            encodedContent,
            "hello",
            "\"etag\"",
            DateTimeOffset.UtcNow,
            true,
            "scope",
            "keysha",
            "2023-11-03",
            "version-id",
            "request-id",
            "client-request-id",
            "root-activity-id");

        Service.GetBlobAsync(
            workspaceId,
            itemId,
            blobPath,
            Arg.Do<BlobDownloadOptions>(options =>
            {
                Assert.NotNull(options);
                Assert.True(options.IncludeInlineContent);
                Assert.True(options.InlineContentLimit.HasValue);
                Assert.Equal(1024 * 1024L, options.InlineContentLimit);
                Assert.Null(options.DestinationStream);
                Assert.Null(options.LocalFilePath);
            }),
            Arg.Any<CancellationToken>())
            .Returns(result);

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", blobPath);

        await Service.Received(1).GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());

        var commandResult = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.BlobGetCommandResult);

        Assert.Equal("File retrieved successfully.", commandResult.Message);
        Assert.Equal(encodedContent, commandResult.Blob.ContentBase64);
        Assert.Equal("hello", commandResult.Blob.ContentText);
        Assert.Equal("md5", commandResult.Blob.ContentMd5);
        Assert.Equal("crc64", commandResult.Blob.ContentCrc64);
    }

    [Theory]
    [InlineData("--workspace-id")]
    [InlineData("--item-id")]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenOptionMissing(string missingOption)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingOption,
            ("--workspace-id", "workspace"),
            ("--item-id", "lakehouse"),
            ("--file-path", "Files/sample.txt")
        ));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AdvisesDownload_WhenInlineContentTruncated()
    {
        var workspaceId = "workspace";
        var itemId = "lakehouse";
        var blobPath = "Files/large.bin";

        var result = new BlobGetResult(
            workspaceId,
            itemId,
            blobPath,
            10_000_000,
            "application/octet-stream",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
        {
            InlineContentTruncated = true
        };

        Service.GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>()).Returns(result);

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", blobPath);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("inline limit", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WritesToFile_WhenDownloadPathProvided()
    {
        var workspaceId = "workspace";
        var itemId = "lakehouse";
        var blobPath = "Files/sample.txt";

        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var result = new BlobGetResult(
            workspaceId,
            itemId,
            blobPath,
            100,
            "text/plain",
            "utf-8",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
        {
            ContentFilePath = tempFilePath
        };

        Service.GetBlobAsync(workspaceId, itemId, blobPath, Arg.Do<BlobDownloadOptions>(opts =>
            {
                Assert.NotNull(opts.DestinationStream);
                Assert.False(opts.IncludeInlineContent);
                Assert.Equal(tempFilePath, opts.LocalFilePath);
            }), Arg.Any<CancellationToken>())
            .Returns(result);
        try
        {
            var response = await ExecuteCommandAsync(
                "--workspace-id", workspaceId,
                "--item-id", itemId,
                "--file-path", blobPath,
                "--download-file-path", tempFilePath);

            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Contains(tempFilePath, response.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_RejectsDownloadPath_WhenTransportIsHttp()
    {
        _serviceStartOptions.Value.Returns(new ServiceStartOptions { Transport = TransportTypes.Http });

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "lakehouse",
            "--file-path", "Files/sample.txt",
            "--download-file-path", "c:/temp/file.bin");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.GetBlobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<BlobDownloadOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "blobPath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--file-path", traversalPath);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}

