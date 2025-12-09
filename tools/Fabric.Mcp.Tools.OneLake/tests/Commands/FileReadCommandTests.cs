// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text;
using System.Threading;
using Azure.Mcp.Core.Areas.Server.Options;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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
        Assert.Equal("read", command.Name);
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
        Assert.Equal("read", systemCommand.Name);
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

        var blobResult = new BlobGetResult(
            expectedWorkspace,
            expectedItem,
            filePath,
            fileContent.Length,
            "text/plain",
            "utf-8",
            null,
            null,
            null,
            null,
            null,
            null,
            fileContent,
            "etag",
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
        {
            InlineContentTruncated = false
        };

        oneLakeService
            .ReadFileAsync(
                expectedWorkspace,
                expectedItem,
                filePath,
                Arg.Do<BlobDownloadOptions?>(options =>
                {
                    Assert.NotNull(options);
                    Assert.True(options!.IncludeInlineContent);
                    Assert.True(options.InlineContentLimit.HasValue);
                    Assert.Equal(1024 * 1024L, options.InlineContentLimit);
                    Assert.Null(options.DestinationStream);
                    Assert.Null(options.LocalFilePath);
                }),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(blobResult));

        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"{identifierArgs} --file-path {filePath}");
        var context = CreateContext();

        // Act
        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await oneLakeService
            .Received(1)
            .ReadFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WritesToFile_WhenDownloadPathProvided()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        var workspaceId = "workspace";
        var itemId = "item";
        var filePath = "Files/sample.txt";
        var downloadPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var blobResult = new BlobGetResult(
            workspaceId,
            itemId,
            filePath,
            512,
            "text/plain",
            "utf-8",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "etag-value",
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
        {
            ContentFilePath = downloadPath
        };

        oneLakeService
            .ReadFileAsync(
                workspaceId,
                itemId,
                filePath,
                Arg.Do<BlobDownloadOptions?>(options =>
                {
                    Assert.NotNull(options);
                    Assert.False(options!.IncludeInlineContent);
                    Assert.Equal(downloadPath, options.LocalFilePath);
                    Assert.NotNull(options.DestinationStream);
                }),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(blobResult));

        try
        {
            var systemCommand = command.GetCommand();
            var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath} --download-file-path {downloadPath}");
            var context = CreateContext();

            // Act
            var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Contains(downloadPath, response.Message, StringComparison.OrdinalIgnoreCase);
            await oneLakeService
                .Received(1)
                .ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());
        }
        finally
        {
            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
        }
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

        oneLakeService
            .ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse($"--workspace-id {workspaceId} --item-id {itemId} --file-path {filePath}");
        var context = CreateContext();

        // Act
        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await oneLakeService
            .Received(1)
            .ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingIdentifiers_ReturnsValidationError()
    {
        // Arrange
        var logger = LoggerFactory.Create(builder => { }).CreateLogger<FileReadCommand>();
        var oneLakeService = Substitute.For<IOneLakeService>();
        var command = new FileReadCommand(logger, oneLakeService);

        var systemCommand = command.GetCommand();
        var parseResult = systemCommand.Parse("");
        var context = CreateContext();

        // Act
        var response = await command.ExecuteAsync(context, parseResult, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await oneLakeService
            .DidNotReceive()
            .ReadFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());

    }

    private static CommandContext CreateContext(string transport = "stdio")
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceOptions = Microsoft.Extensions.Options.Options.Create(new ServiceStartOptions
        {
            Transport = transport
        });

        serviceProvider.GetService(typeof(IOptions<ServiceStartOptions>)).Returns(serviceOptions);
        return new CommandContext(serviceProvider);
    }
}
