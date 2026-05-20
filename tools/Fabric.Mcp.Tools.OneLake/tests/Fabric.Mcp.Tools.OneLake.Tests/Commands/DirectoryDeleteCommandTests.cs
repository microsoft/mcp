// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class DirectoryDeleteCommandTests : CommandUnitTestsBase<DirectoryDeleteCommand, IOneLakeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("delete_directory", Command.Name);
        Assert.Equal("Delete OneLake Directory", Command.Title);
        Assert.Contains("Deletes a directory from OneLake storage", Command.Description);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("delete_directory", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectoryDeleteCommand(null!, Service));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DirectoryDeleteCommand(Logger, null!));
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

    [Theory]
    [InlineData("../../dir")]
    [InlineData("Files/../../other-item")]
    [InlineData("../subdir")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.DeleteDirectoryAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "directoryPath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--directory-path", traversalPath);

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
