// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.OneLake.Commands.File;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.OneLake.Tests.Commands;

public class FileReadCommandTests : CommandUnitTestsBase<FileReadCommand, IOneLakeService>
{
    private readonly IOptions<ServiceStartOptions> _serviceStartOptions;

    public FileReadCommandTests()
    {
        _serviceStartOptions = Substitute.For<IOptions<ServiceStartOptions>>();
        _serviceStartOptions.Value.Returns(new ServiceStartOptions { Transport = TransportTypes.StdIo });
        Services.AddSingleton(_serviceStartOptions);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("read", Command.Name);
        Assert.Equal("Read OneLake File", Command.Title);
        Assert.Contains("Read the contents of a file from OneLake storage", Command.Description);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public void GetCommand_ReturnsValidCommand()
    {
        Assert.Equal("read", CommandDefinition.Name);
        Assert.NotNull(CommandDefinition.Description);
        Assert.NotEmpty(CommandDefinition.Options);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileReadCommand(null!, Service, _serviceStartOptions));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOneLakeServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileReadCommand(Logger, null!, _serviceStartOptions));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceStartOptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FileReadCommand(Logger, Service, null!));
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

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

        Service.ReadFileAsync(
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
            .Returns(blobResult);

        // Act
        var response = await ExecuteCommandAsync($"{identifierArgs} --file-path {filePath}");

        // Assert
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1)
            .ReadFileAsync(expectedWorkspace, expectedItem, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WritesToFile_WhenDownloadPathProvided()
    {
        // Arrange
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

        Service.ReadFileAsync(
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
            .Returns(blobResult);

        try
        {
            // Act
            var response = await ExecuteCommandAsync(
                "--workspace-id", workspaceId,
                "--item-id", itemId,
                "--file-path", filePath,
                "--download-file-path", downloadPath);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.Contains(downloadPath, response.Message, StringComparison.OrdinalIgnoreCase);
            await Service.Received(1)
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
        var workspaceId = "test-workspace";
        var itemId = "test-item";
        var filePath = "test/file.txt";

        Service.ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var response = await ExecuteCommandAsync(
            "--workspace-id", workspaceId,
            "--item-id", itemId,
            "--file-path", filePath);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        await Service.Received(1)
            .ReadFileAsync(workspaceId, itemId, filePath, Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingIdentifiers_ReturnsValidationError()
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync("");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        await Service.DidNotReceive()
            .ReadFileAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>());

    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("Files/../../other-item/data")]
    [InlineData("../credentials.env")]
    public async Task ExecuteAsync_RejectsTraversalPath_ReturnsErrorResponse(string traversalPath)
    {
        Service.ReadFileAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is<string>(p => p.Contains("..", StringComparison.Ordinal)),
            Arg.Any<BlobDownloadOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new ArgumentException("Path cannot contain directory traversal sequences.", "filePath"));

        var response = await ExecuteCommandAsync(
            "--workspace-id", "workspace",
            "--item-id", "item",
            "--file-path", traversalPath);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }
}
