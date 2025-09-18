// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob;

public class BlobGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<BlobGetCommand> _logger;
    private readonly BlobGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownContainer = "container123";
    private readonly string _knownBlob = "test-blob.txt";
    private readonly string _knownSubscription = "sub123";

    public BlobGetCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BlobGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlobDetails()
    {
        // Arrange
        var expectedBlobInfo = new BlobInfo(_knownBlob, DateTimeOffset.UtcNow, "\"0x8D8B7C6E5B3A1B2\"", 1024,
            "text/plain", null, "BlockBlob", new Dictionary<string, string>(), "Unlocked", "Available", null, null,
            null, null, "Hot", null, false, DateTimeOffset.UtcNow.AddDays(-1), null, "2024-06-01T12:00:00.0000000Z");

        _storageService.GetBlobDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownBlob), Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns([expectedBlobInfo]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
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
    public async Task ExecuteAsync_BlobNotFound_HandlesException()
    {
        // Arrange
        var expectedError = "Blob not found";

        _storageService.GetBlobDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownBlob), Arg.Is(_knownSubscription), null, Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--blob", _knownBlob,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var expectedBlobInfos = new List<BlobInfo> {
            new("blob1", DateTimeOffset.UtcNow, "\"0x8D8B7C6E5B3A1B2\"", 1024, "text/plain", null, "BlockBlob",
                new Dictionary<string, string>(), "Unlocked", "Available", null, null, null, null, "Hot", null, false,
                DateTimeOffset.UtcNow.AddDays(-1), null, "2024-06-01T12:00:00.0000000Z"),
            new("blob2", DateTimeOffset.UtcNow, "\"0x8D8B7C6E5B3A1B2\"", 1024, "text/plain", null, "BlockBlob",
                new Dictionary<string, string>(), "Unlocked", "Available", null, null, null, null, "Hot", null, false,
                DateTimeOffset.UtcNow.AddDays(-1), null, "2024-06-01T12:00:00.0000000Z")
        };

        _storageService.GetBlobDetails(
            Arg.Is(_knownAccount),
            Arg.Is(_knownContainer),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedBlobInfos);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result);
        Assert.Equal(expectedBlobInfos.Select(b => b.Name), result.Blobs.Select(b => b.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBlobs()
    {
        // Arrange
        _storageService.GetBlobDetails(
            Arg.Is(_knownAccount),
            Arg.Is(_knownContainer),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Blobs);
    }

    [Fact]
    public async Task ExecuteAsync_GenericIssue_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.GetBlobDetails(
            Arg.Is(_knownAccount),
            Arg.Is(_knownContainer),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            null,
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
