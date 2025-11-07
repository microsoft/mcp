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
using System.Text;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class FileReadCommandTests
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();

        // Act
        var command = new FileReadCommand(logger, oneLakeService);

        // Assert
        Assert.Equal("file-read", command.Name);
        Assert.Equal("Read OneLake File", command.Title);
        Assert.Contains("Read the contents of a file from OneLake storage", command.Description);
        Assert.True(command.Metadata.ReadOnly);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        // Act
        var systemCommand = command.GetCommand();

        // Assert
        Assert.NotNull(systemCommand);
        Assert.Equal("file-read", systemCommand.Name);
        Assert.NotNull(systemCommand.Description);
    }

    [Fact]
    public void CommandOptions_ContainsRequiredOptions()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

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
        Assert.Throws<ArgumentNullException>(() => new FileReadCommand(null!, oneLakeService));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileReadCommand(logger, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        // Act
        var metadata = command.Metadata;

        // Assert
        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("--workspace-id test-workspace --item-id test-item", "test-workspace", "test-item")]
    [InlineData("--workspace \"Analytics Workspace\" --item \"Sales Lakehouse\"", "Analytics Workspace", "Sales Lakehouse")]
    public async Task ExecuteAsync_ReadsFileSuccessfully(string identifierArgs, string expectedWorkspace, string expectedItem)
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        var filePath = "test/file.txt";
        var fileContent = "Hello, OneLake!";

        var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        oneLakeService.ReadFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<CancellationToken>())
            .Returns(contentStream);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"{identifierArgs} --file-path {filePath}");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).ReadFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceException()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        oneLakeService.ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath}");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await oneLakeService.Received(1).ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingIdentifiers_ReturnsValidationError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse("");
        var context = new CommandContext(serviceProvider);

        // Act
        var response = await command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await oneLakeService.DidNotReceive().ReadFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}