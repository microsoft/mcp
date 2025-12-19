// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Azure.Mcp.Core.Areas.Server.Options;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobGetCommandTests
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

        Assert.Equal("file", command.Name);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
        Assert.False(command.Metadata.Destructive);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlobAndMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);
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

        service.GetBlobAsync(
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

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath}");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());

        using var document = JsonDocument.Parse(SerializeResult(context.Response.Results));
        var root = document.RootElement;
        Assert.Equal("File retrieved successfully.", root.GetProperty("message").GetString());
        var blob = root.GetProperty("blob");
        Assert.Equal(encodedContent, blob.GetProperty("contentBase64").GetString());
        Assert.Equal("hello", blob.GetProperty("contentText").GetString());
        Assert.Equal("md5", blob.GetProperty("contentMd5").GetString());
        Assert.Equal("crc64", blob.GetProperty("contentCrc64").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenWorkspaceMissing()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--item-id lakehouse --file-path Files/sample.txt");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenItemMissing()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --file-path Files/sample.txt");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AdvisesDownload_WhenInlineContentTruncated()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);
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

        service.GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>()).Returns(result);

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath}");
        var context = CreateContext();

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Contains("inline limit", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WritesToFile_WhenDownloadPathProvided()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);
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

        service
            .GetBlobAsync(workspaceId, itemId, blobPath, Arg.Do<BlobDownloadOptions>(opts =>
            {
                Assert.NotNull(opts.DestinationStream);
                Assert.False(opts.IncludeInlineContent);
                Assert.Equal(tempFilePath, opts.LocalFilePath);
            }), Arg.Any<CancellationToken>())
            .Returns(result);
        try
        {
            var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath} --download-file-path {tempFilePath}");
            var context = CreateContext();

            var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

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
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --item-id lakehouse --file-path Files/sample.txt --download-file-path c:/temp/file.bin");
        var context = CreateContext("http");

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions>(), Arg.Any<CancellationToken>());
    }

    private static string SerializeResult(ResponseResult? result)
    {
        if (result is null)
        {
            return string.Empty;
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            result.Write(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static CommandContext CreateContext(string transport = "stdio")
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            Transport = transport
        });

        serviceProvider.GetService(typeof(IOptions<ServiceStartOptions>)).Returns(serviceOptions);
        return new CommandContext(serviceProvider);
    }
}
