// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobGetCommandTests
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

    Assert.Equal("download", command.Name);
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

        service.GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>()).Returns(result);

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath}");
        var context = new CommandContext(Substitute.For<IServiceProvider>());

    var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).GetBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>());

        using var document = JsonDocument.Parse(SerializeResult(context.Response.Results));
        var root = document.RootElement;
        Assert.Equal("Blob retrieved successfully.", root.GetProperty("message").GetString());
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
        var context = new CommandContext(Substitute.For<IServiceProvider>());

    var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenItemMissing()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobGetCommand(NullLogger<BlobGetCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --file-path Files/sample.txt");
        var context = new CommandContext(Substitute.For<IServiceProvider>());

    var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().GetBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
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
}
