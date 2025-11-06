// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class BlobListCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<BlobListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new BlobListCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("list-blobs", command.Name);
        Assert.Equal("List OneLake Blobs", command.Title);
        Assert.Contains("List files and directories in OneLake storage as blobs", command.Description);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<BlobListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobListCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("list-blobs", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<BlobListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new BlobListCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert - Just verify we have some options
        Assert.NotEmpty(systemCommand.Options);
    }
}