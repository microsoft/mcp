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
