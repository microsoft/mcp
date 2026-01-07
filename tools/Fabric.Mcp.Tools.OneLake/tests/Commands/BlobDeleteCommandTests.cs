// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobDeleteCommandTests
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobDeleteCommand(NullLogger<BlobDeleteCommand>.Instance, service);

        Assert.Equal("delete", command.Name);
        Assert.True(command.Metadata.Destructive);
        Assert.False(command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesBlobSuccessfully()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobDeleteCommand(NullLogger<BlobDeleteCommand>.Instance, service);

        var workspaceId = "workspace";
        var itemId = "lakehouse";
        var blobPath = "Files/sample.txt";

        var result = new BlobDeleteResult(
            workspaceId,
            itemId,
            blobPath,
            "2023-11-03",
            "version-id",
            "request-id",
            "client-request-id",
            "root-activity-id");

        service.DeleteBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>()).Returns(result);

        var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath}");
        var context = new CommandContext(Substitute.For<IServiceProvider>());

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await service.Received(1).DeleteBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>());

        using var document = JsonDocument.Parse(SerializeResult(context.Response.Results));
        var root = document.RootElement;
        Assert.Equal("Blob deleted successfully.", root.GetProperty("message").GetString());
        var metadata = root.GetProperty("result");
        Assert.Equal("request-id", metadata.GetProperty("requestId").GetString());
        Assert.Equal("client-request-id", metadata.GetProperty("clientRequestId").GetString());
        Assert.Equal("root-activity-id", metadata.GetProperty("rootActivityId").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenWorkspaceMissing()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobDeleteCommand(NullLogger<BlobDeleteCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--item-id lakehouse --file-path Files/sample.txt");
        var context = new CommandContext(Substitute.For<IServiceProvider>());

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().DeleteBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenItemMissing()
    {
        var service = Substitute.For<IOneLakeService>();
        var command = new BlobDeleteCommand(NullLogger<BlobDeleteCommand>.Instance, service);

        var parseResult = command.GetCommand().Parse("--workspace-id workspace --file-path Files/sample.txt");
        var context = new CommandContext(Substitute.For<IServiceProvider>());

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await service.DidNotReceive().DeleteBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
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
