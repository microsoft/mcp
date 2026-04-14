// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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
        Assert.Equal("delete_directory", command.Name);
        Assert.Equal("Delete OneLake Directory", command.Title);
        Assert.Contains("Deletes a directory from OneLake storage", command.Description);
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
        Assert.Equal("delete_directory", systemCommand.Name);
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

    [Theory]
    [InlineData("../../dir")]
    [InlineData("Files/../../other-item")]
    [InlineData("../subdir")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryDeleteCommand(logger, oneLakeService);

        oneLakeService
            .DeleteDirectoryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "directoryPath"));

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id workspace --item-id item --directory-path {traversalPath}");
        var context = new CommandContext(serviceProvider);

        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        Assert.NotEqual(System.Net.HttpStatusCode.OK, response.Status);
    }
}
