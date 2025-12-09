// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobPutCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobPutCommand(NullLogger<BlobPutCommand>.Instance, oneLakeService);

        Assert.Equal("file", command.Name);
        Assert.Contains("Upload content to OneLake", command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobPutCommand(NullLogger<BlobPutCommand>.Instance, oneLakeService);

        var systemCommand = command.GetCommand();

        Assert.NotNull(systemCommand);
        Assert.Equal("file", systemCommand.Name);
        Assert.NotEmpty(systemCommand.Options);
    }

    [Theory]
    [InlineData("--workspace-id test-workspace --item-id test-item", "test-workspace", "test-item")]
    [InlineData("--workspace \"Analytics Workspace\" --item \"Sales Lakehouse\"", "Analytics Workspace", "Sales Lakehouse")]
    public async Task ExecuteAsync_UploadsInlineContentSuccessfully(string identifierArgs, string expectedWorkspace, string expectedItem)
    {
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobPutCommand(NullLogger<BlobPutCommand>.Instance, oneLakeService);

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

        oneLakeService.PutBlobAsync(
            expectedWorkspace,
            expectedItem,
            blobPath,
            Arg.Any<Stream>(),
            Arg.Any<long>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(blobResult);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var parseResult = command.GetCommand().Parse($"{identifierArgs} --file-path {blobPath} --content \"{content}\"");
        var context = new CommandContext(serviceProvider);

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.Status);
        await oneLakeService.Received(1).PutBlobAsync(
            expectedWorkspace,
            expectedItem,
            blobPath,
            Arg.Any<Stream>(),
            content.Length,
            null,
            false,
            Arg.Any<CancellationToken>());

        var resultJson = SerializeResult(context.Response.Results);
        using var document = JsonDocument.Parse(resultJson);
        var root = document.RootElement;
        Assert.Equal("2023-11-03", root.GetProperty("version").GetString());
        Assert.True(root.GetProperty("requestServerEncrypted").GetBoolean());
        Assert.Equal("md5-value", root.GetProperty("contentMd5").GetString());
        Assert.Equal("crc64-value", root.GetProperty("contentCrc64").GetString());
        Assert.Equal("scope", root.GetProperty("encryptionScope").GetString());
        Assert.Equal("key-sha256", root.GetProperty("encryptionKeySha256").GetString());
        Assert.Equal("version-id", root.GetProperty("versionId").GetString());
        Assert.Equal("client-request-id", root.GetProperty("clientRequestId").GetString());
        Assert.Equal("root-activity-id", root.GetProperty("rootActivityId").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_UploadsFromLocalFile()
    {
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobPutCommand(NullLogger<BlobPutCommand>.Instance, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var blobPath = "Files/data.json";
        var tempFile = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFile, "{\"hello\":\"world\"}", TestContext.Current.CancellationToken);

            oneLakeService.PutBlobAsync(
                workspaceId,
                itemId,
                blobPath,
                Arg.Any<Stream>(),
                Arg.Any<long>(),
                Arg.Any<string?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .Returns(new BlobPutResult(workspaceId, itemId, blobPath, new FileInfo(tempFile).Length, "application/json", "etag", DateTimeOffset.UtcNow, "request-id"));

            var serviceProvider = Substitute.For<IServiceProvider>();
            var parseResult = command.GetCommand().Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {blobPath} --local-file-path {tempFile} --content-type application/json --overwrite");
            var context = new CommandContext(serviceProvider);

            var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

            Assert.Equal(HttpStatusCode.Created, response.Status);
            var expectedLength = new FileInfo(tempFile).Length;

            await oneLakeService.Received(1).PutBlobAsync(
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
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobPutCommand(NullLogger<BlobPutCommand>.Instance, oneLakeService);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var parseResult = command.GetCommand().Parse("--workspace-id test-workspace --item-id test-item --file-path Files/empty.txt");
        var context = new CommandContext(serviceProvider);

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.Created, response.Status);
        await oneLakeService.DidNotReceive().PutBlobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<long>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
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
