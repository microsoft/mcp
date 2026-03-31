// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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

    [Theory]
    [InlineData("../../dir")]
    [InlineData("Files/../../other-item")]
    [InlineData("../subdir")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<DirectoryCreateCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new DirectoryCreateCommand(logger, oneLakeService);

        oneLakeService
            .CreateDirectoryAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
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
