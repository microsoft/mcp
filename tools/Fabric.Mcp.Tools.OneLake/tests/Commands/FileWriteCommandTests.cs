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

public class FileWriteCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new FileWriteCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("file-write", command.Name);
        Assert.Equal("Write OneLake File", command.Title);
        Assert.Contains("Write content to a file in OneLake storage", command.Description);
        Assert.False(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Destructive);
        Assert.False(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("file-write", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

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
        Assert.Throws<ArgumentNullException>(() => new FileWriteCommand(null!, oneLakeService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileWriteCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        // Act
        var metadata = command.Metadata;

        // Assert
        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Fact]
    public async Task ExecuteAsync_WritesFileWithContentSuccessfully()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        oneLakeService.WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath} --content \"{content}\"");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            false, 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WritesFileWithOverwriteFlag()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        oneLakeService.WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath} --content \"{content}\" --overwrite");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            true, 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";
        var content = "Hello, OneLake!";

        oneLakeService.WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath} --content \"{content}\"");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).WriteFileAsync(
            workspaceId, 
            itemId, 
            filePath, 
            Arg.Any<Stream>(), 
            false, 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenNoContentProvided()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileWriteCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileWriteCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath}");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await oneLakeService.DidNotReceive().WriteFileAsync(
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<string>(), 
            Arg.Any<Stream>(), 
            Arg.Any<bool>(), 
            Arg.Any<CancellationToken>());
    }
}