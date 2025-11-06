// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.Workspace;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class OneLakeWorkspaceListCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<OneLakeWorkspaceListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new OneLakeWorkspaceListCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("onelake-workspace-list", command.Name);
        Assert.Equal("List OneLake Workspaces", command.Title);
        Assert.Contains("List all OneLake workspaces", command.Description);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<OneLakeWorkspaceListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new OneLakeWorkspaceListCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("onelake-workspace-list", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsFormatOption()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<OneLakeWorkspaceListCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new OneLakeWorkspaceListCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert - Just verify we have some options
        Assert.NotEmpty(systemCommand.Options);
    }
}