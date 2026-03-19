// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Commands;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.Commands;

public class ItemCreateCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ItemCreateCommand>();
        var fabricCoreService = Substitute.For<IFabricCoreService>();

        // Act
        var command = new ItemCreateCommand(logger, fabricCoreService);

        // Assert
        Assert.Equal("create_item", command.Name);
        Assert.Equal("Create Fabric Item", command.Title);
        Assert.Contains("Creates a new item in a Fabric workspace", command.Description);
        Assert.False(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.False(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ItemCreateCommand>();
        var fabricCoreService = Substitute.For<IFabricCoreService>();
        var command = new ItemCreateCommand(logger, fabricCoreService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("create_item", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ItemCreateCommand>();
        var fabricCoreService = Substitute.For<IFabricCoreService>();
        var command = new ItemCreateCommand(logger, fabricCoreService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert - Just verify we have some options
        Assert.NotEmpty(systemCommand.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Arrange
        var fabricCoreService = Substitute.For<IFabricCoreService>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(null!, fabricCoreService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFabricCoreServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ItemCreateCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ItemCreateCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<ItemCreateCommand>();
        var fabricCoreService = Substitute.For<IFabricCoreService>();
        var command = new ItemCreateCommand(logger, fabricCoreService);

        // Act
        var metadata = command.Metadata;

        // Assert
        Assert.False(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }
}
