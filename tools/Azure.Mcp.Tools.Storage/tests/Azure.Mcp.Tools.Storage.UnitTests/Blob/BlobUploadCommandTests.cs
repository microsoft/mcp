// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob;

public class BlobUploadCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<BlobUploadCommand> _logger;
    private readonly BlobUploadCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownContainer = "container123";
    private readonly string _knownBlob = "test-blob.txt";
    private readonly string _knownLocalFilePath = "C:\\temp\\test-file.txt";
    private readonly string _knownSubscription = "sub123";

    public BlobUploadCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BlobUploadCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_UploadsBlobSuccessfully()
    {
        // Arrange
        var uploadResult = new BlobUploadResult(
            Blob: _knownBlob,
            Container: _knownContainer,
            UploadedFile: Path.GetFileName(_knownLocalFilePath),
            LastModified: DateTimeOffset.UtcNow,
            ETag: "\"0x8D123456789ABCD\"",
            MD5Hash: "abc123def456"
        );

        _storageService.UploadBlob(
            _knownAccount,
            _knownContainer,
            _knownBlob,
            _knownLocalFilePath,
            _knownSubscription,
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>())
            .Returns(uploadResult);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
            "--local-file-path", _knownLocalFilePath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_FileNotFoundError_ReturnsError()
    {
        // Arrange
        _storageService.UploadBlob(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>())
            .ThrowsAsync(new FileNotFoundException($"Local file not found: {_knownLocalFilePath}"));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
            "--local-file-path", _knownLocalFilePath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("Local file not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_BlobAlreadyExistsError_ReturnsError()
    {
        // Arrange
        _storageService.UploadBlob(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>())
            .ThrowsAsync(new InvalidOperationException("Blob 'test-blob.txt' already exists in container 'container123'."));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
            "--local-file-path", _knownLocalFilePath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--account account123")]
    [InlineData("--account account123 --container container123")]
    [InlineData("--account account123 --container container123 --blob test-blob.txt")]
    [InlineData("--account account123 --container container123 --blob test-blob.txt --local-file-path /path/to/file.txt")]
    public async Task ExecuteAsync_MissingRequiredOptions_ReturnsValidationError(string argString)
    {
        // Arrange
        var args = string.IsNullOrEmpty(argString)
            ? _commandDefinition.Parse([])
            : _commandDefinition.Parse(argString.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(200, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_AccessDeniedError_ReturnsError()
    {
        // Arrange
        var requestFailedException = new RequestFailedException(403, "Access denied");
        _storageService.UploadBlob(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>())
            .ThrowsAsync(requestFailedException);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
            "--local-file-path", _knownLocalFilePath,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("Access denied", response.Message);
    }
}
