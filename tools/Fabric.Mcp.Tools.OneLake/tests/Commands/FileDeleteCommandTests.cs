// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class FileDeleteCommandTests : CommandUnitTestsBase<FileDeleteCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Theory]
    [InlineData("--workspace-id test-workspace --item-id test-item", "test-workspace", "test-item")]
    [InlineData("--workspace \"Analytics Workspace\" --item \"Sales Lakehouse\"", "Analytics Workspace", "Sales Lakehouse")]
    public async Task ExecuteAsync_DeletesFileSuccessfully(string identifierArgs, string expectedWorkspace, string expectedItem)
    {
        // Arrange
        var filePath = "test/file.txt";

        Service.DeleteFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await ExecuteCommandAsync($"{identifierArgs} --file-path {filePath}");

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileDeleteCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileDeleteCommand(Logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.True(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        Service.DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", filePath);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await Service.Received(1).DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingIdentifiers_ReturnsValidationError()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.DeleteFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "filePath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--file-path", traversalPath);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
