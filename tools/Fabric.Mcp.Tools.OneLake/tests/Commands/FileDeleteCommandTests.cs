// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class FileDeleteCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new FileDeleteCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("file-delete", command.Name);
        Assert.Equal("Delete OneLake File", command.Title);
        Assert.Contains("Delete a file from OneLake storage", command.Description);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("file-delete", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

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
        Assert.Throws<ArgumentNullException>(() => new FileDeleteCommand(null!, oneLakeService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileDeleteCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

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

    [Fact]
    public async Task ExecuteAsync_DeletesFileSuccessfully()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        oneLakeService.DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath}");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        oneLakeService.DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath}");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).DeleteFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyParameters_UsesDefaultValues()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileDeleteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileDeleteCommand(logger, oneLakeService);

        oneLakeService.DeleteFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse("");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        await oneLakeService.Received(1).DeleteFileAsync(string.Empty, string.Empty, string.Empty, Arg.Any<CancellationToken>());
    }
}