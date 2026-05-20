// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class FileWriteCommandTests : CommandUnitTestsBase<FileWriteCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Contains("Write content to a file in OneLake storage", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("write", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileWriteCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileWriteCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("--workspace-id test-workspace --item-id test-item", "test-workspace", "test-item")]
    [InlineData("--workspace \"Analytics Workspace\" --item \"Sales Lakehouse\"", "Analytics Workspace", "Sales Lakehouse")]
    public async Task ExecuteAsync_WritesFileWithContentSuccessfully(string identifierArgs, string expectedWorkspace, string expectedItem)
    {
        // Arrange
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        Service.WriteFileAsync(
            expectedWorkspace,
            expectedItem,
            filePath,
            Arg.Any<Stream>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync($"{identifierArgs} --file-path {filePath} --content \"{content}\"");

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).WriteFileAsync(
            expectedWorkspace,
            expectedItem,
            filePath,
            Arg.Any<Stream>(),
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WritesFileWithOverwriteFlag()
    {
        // Arrange
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        Service.WriteFileAsync(
            workspaceId,
            itemId,
            filePath,
            Arg.Any<Stream>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", filePath,
            "--content", content,
            "--overwrite");

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).WriteFileAsync(
            workspaceId,
            itemId,
            filePath,
            Arg.Any<Stream>(),
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        Service.WriteFileAsync(
            workspaceId,
            itemId,
            filePath,
            Arg.Any<Stream>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", filePath,
            "--content", content);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await Service.Received(1).WriteFileAsync(
            workspaceId,
            itemId,
            filePath,
            Arg.Any<Stream>(),
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenNoContentProvided()
    {
        // Arrange
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        // Act
        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", filePath);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await Service.DidNotReceive().WriteFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Stream>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.WriteFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<Stream>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "filePath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--file-path", traversalPath,
            "--content", "data");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
