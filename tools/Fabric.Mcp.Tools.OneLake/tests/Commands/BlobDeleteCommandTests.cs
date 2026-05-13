// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobDeleteCommandTests : CommandUnitTestsBase<BlobDeleteCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesMetadata()
    {
        Assert.Equal("delete", Command.Name);
        Assert.True(Command.Metadata.Destructive);
        Assert.False(Command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesBlobSuccessfully()
    {
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

        Service.DeleteBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>()).Returns(result);

        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", blobPath);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteBlobAsync(workspaceId, itemId, blobPath, Arg.Any<CancellationToken>());

        var commandResult = ValidateAndDeserializeResponse(response, OneLakeJsonContext.Default.BlobDeleteCommandResult);

        Assert.Equal("Blob deleted successfully.", commandResult.Message);
        Assert.Equal("request-id", commandResult.Result.RequestId);
        Assert.Equal("client-request-id", commandResult.Result.ClientRequestId);
        Assert.Equal("root-activity-id", commandResult.Result.RootActivityId);
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
        await Service.DidNotReceive().DeleteBlobAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
