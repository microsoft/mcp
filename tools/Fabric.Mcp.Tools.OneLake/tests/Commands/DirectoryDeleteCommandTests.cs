// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class DirectoryDeleteCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new DirectoryDeleteCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("directory-delete", command.Name);
        Assert.Equal("Delete OneLake Directory", command.Title);
        Assert.Contains("Delete a directory from OneLake storage", command.Description);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryDeleteCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("directory-delete", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryDeleteCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert - Just verify we have some options
        Assert.NotEmpty(systemCommand.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DirectoryDeleteCommand(null!, oneLakeService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DirectoryDeleteCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryDeleteCommand(logger, oneLakeService);

        // Act
        var metadata = command.Metadata;

        // Assert
        Assert.True(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }
}