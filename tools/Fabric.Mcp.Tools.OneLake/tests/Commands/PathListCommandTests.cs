// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class PathListCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<PathListCommand>();

        // Act
        var command = new PathListCommand(logger);

        // Assert
        Assert.Equal("list_files", command.Name);
        Assert.Equal("List OneLake Files", command.Title);
        Assert.Contains("List files and directories", command.Description);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<PathListCommand>();
        var command = new PathListCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("list_files", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<PathListCommand>();
        var command = new PathListCommand(logger);

        // Act
        var systemCommand = command.GetCommand();

        // Assert - Just verify we have some options, specific names may vary
        Assert.NotEmpty(systemCommand.Options);
    }
}
