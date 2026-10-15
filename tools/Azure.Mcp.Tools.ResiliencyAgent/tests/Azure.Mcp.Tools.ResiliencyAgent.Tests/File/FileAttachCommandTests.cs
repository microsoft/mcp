// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.File;
using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Tests.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Commands;

public sealed class FileAttachCommandTests
    : CommandUnitTestsBase<FileAttachCommand, ILocalFileSnapshotter>
{
    private const string FilePath = @"C:\workspace\main.bicep";
    private readonly IAttachmentCache _attachmentCache = Substitute.For<IAttachmentCache>();
    private readonly ValidatedLocalFile _validated = new(
        FilePath,
        "main.bicep",
        "text/plain",
        12,
        DateTime.UtcNow);
    private readonly AgentAttachment _snapshot = new(
        "attachment-1",
        "main.bicep",
        "text/plain",
        [1, 2, 3],
        "hash",
        DateTimeOffset.UtcNow.AddMinutes(5));

    public FileAttachCommandTests()
    {
        Services.AddSingleton(_attachmentCache);
        Service.Validate(FilePath).Returns(_validated);
        Service.SnapshotAsync(_validated, Arg.Any<CancellationToken>()).Returns(_snapshot);
        _attachmentCache.Add(_snapshot).Returns(_snapshot);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutElicitationSupport_DoesNotReadOrCacheFile()
    {
        CommandResponse response = await ExecuteAsync(server: null);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await Service.DidNotReceive().SnapshotAsync(
            Arg.Any<ValidatedLocalFile>(),
            Arg.Any<CancellationToken>());
        _attachmentCache.DidNotReceive().Add(Arg.Any<AgentAttachment>());
    }

    [Theory]
    [InlineData("decline")]
    [InlineData("cancel")]
    public async Task ExecuteAsync_WhenUserDoesNotAccept_DoesNotReadOrCacheFile(string action)
    {
        McpServer server = CreateServer(new ElicitResult { Action = action });

        CommandResponse response = await ExecuteAsync(server);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().SnapshotAsync(
            Arg.Any<ValidatedLocalFile>(),
            Arg.Any<CancellationToken>());
        _attachmentCache.DidNotReceive().Add(Arg.Any<AgentAttachment>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserSelectsFalse_DoesNotReadOrCacheFile()
    {
        McpServer server = CreateServer(new ElicitResult
        {
            Action = "accept",
            Content = new Dictionary<string, JsonElement>
            {
                ["approve"] = JsonSerializer.SerializeToElement(false)
            }
        });

        CommandResponse response = await ExecuteAsync(server);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().SnapshotAsync(
            Arg.Any<ValidatedLocalFile>(),
            Arg.Any<CancellationToken>());
        _attachmentCache.DidNotReceive().Add(Arg.Any<AgentAttachment>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserApproves_ReadsAndCachesFileOnce()
    {
        McpServer server = CreateServer(new ElicitResult
        {
            Action = "accept",
            Content = new Dictionary<string, JsonElement>
            {
                ["approve"] = JsonSerializer.SerializeToElement(true)
            }
        });

        CommandResponse response = await ExecuteAsync(server);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).SnapshotAsync(_validated, Arg.Any<CancellationToken>());
        _attachmentCache.Received(1).Add(_snapshot);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSnapshotIsCanceled_DoesNotCacheFile()
    {
        Service.SnapshotAsync(_validated, Arg.Any<CancellationToken>())
            .Returns<Task<AgentAttachment>>(_ => throw new OperationCanceledException());
        McpServer server = CreateServer(new ElicitResult
        {
            Action = "accept",
            Content = new Dictionary<string, JsonElement>
            {
                ["approve"] = JsonSerializer.SerializeToElement(true)
            }
        });

        CommandResponse response = await ExecuteAsync(server);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        _attachmentCache.DidNotReceive().Add(Arg.Any<AgentAttachment>());
    }

    private Task<CommandResponse> ExecuteAsync(McpServer? server)
    {
        var context = new CommandContext { McpServer = server };
        return ((IBaseCommand)Command).ExecuteAsync(
            context,
            CommandDefinition.Parse(["--file-path", FilePath]),
            TestContext.Current.CancellationToken);
    }

    private static McpServer CreateServer(ElicitResult result)
    {
        McpServer server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities
        {
            Elicitation = new ElicitationCapability { Form = new() }
        });
        server.SendRequestAsync(Arg.Any<JsonRpcRequest>(), Arg.Any<CancellationToken>())
            .Returns(new JsonRpcResponse
            {
                Id = new RequestId(1),
                Result = JsonSerializer.SerializeToNode(result)
            });
        return server;
    }
}
