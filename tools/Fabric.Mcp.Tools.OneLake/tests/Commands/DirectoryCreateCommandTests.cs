// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class DirectoryCreateCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new DirectoryCreateCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("directory-create", command.Name);
        Assert.Equal("Create OneLake Directory", command.Title);
        Assert.Contains("Create a directory in OneLake storage", command.Description);
        Assert.False(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryCreateCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("directory-create", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryCreateCommand(logger, oneLakeService);

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
        Assert.Throws<ArgumentNullException>(() => new DirectoryCreateCommand(null!, oneLakeService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DirectoryCreateCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryCreateCommand(logger, oneLakeService);

        // Act
        var metadata = command.Metadata;

        // Assert
        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }
}