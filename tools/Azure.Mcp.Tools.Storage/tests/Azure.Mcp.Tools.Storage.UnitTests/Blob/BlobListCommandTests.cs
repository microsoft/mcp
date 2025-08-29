// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob;

public class BlobListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<BlobListCommand> _logger;
    private readonly BlobListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownContainer = "container123";
    private readonly string _knownSubscription = "sub123";

    public BlobListCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BlobListCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsSubscriptions()
    {
        // Arrange
        var expectedBlobs = new List<string> { "blob1", "blob2" };

        _storageService.ListBlobs(Arg.Is(_knownAccount), Arg.Is(_knownContainer), Arg.Is(_knownSubscription),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>()).Returns(expectedBlobs);

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
        var result = JsonSerializer.Deserialize<BlobListResult>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedBlobs, result.Blobs);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoBlobs()
    {
        // Arrange
        _storageService.ListBlobs(Arg.Is(_knownAccount), Arg.Is(_knownContainer), Arg.Is(_knownSubscription),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>()).Returns([]);

        var args = _commandDefinition.Parse([
                "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
            ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.ListBlobs(Arg.Is(_knownAccount), Arg.Is(_knownContainer), Arg.Is(_knownSubscription),
            null, Arg.Any<RetryPolicyOptions>()).ThrowsAsync(new Exception(expectedError));

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

    private class BlobListResult
    {
        [JsonPropertyName("blobs")]
        public List<string> Blobs { get; set; } = [];
    }
}
