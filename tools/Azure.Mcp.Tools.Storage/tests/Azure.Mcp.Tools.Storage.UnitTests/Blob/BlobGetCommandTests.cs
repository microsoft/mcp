// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
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

    public BlobGetCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<BlobGetCommand>>();

        _serviceProvider = new ServiceCollection().BuildServiceProvider();
        _command = new(_logger, _storageService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsBlobs()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";
        var expectedBlobs = new List<BlobInfo>(
        [
            new("blob", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null),
            new("blob2", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null)
        ]);

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedBlobs);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Blobs);
        Assert.Equal(expectedBlobs.Count, result.Blobs.Count);
        Assert.Equal(expectedBlobs.Select(a => a.Name), result.Blobs.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoBlobs()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Blobs);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account mystorageaccount --subscription sub123 --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --blob blob", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --blob blob --prefix prefix", true)]
    [InlineData("--subscription sub123", false)] // Missing account and container
    [InlineData("--account mystorageaccount", false)] // Missing subscription and container
    [InlineData("--container container", false)] // Missing subscription and account
    [InlineData("--subscription sub123 --account mystorageaccount", false)] // Missing container
    [InlineData("--subscription sub123 --container container", false)] // Missing account
    [InlineData("--account mystorageaccount --container container", false)] // Missing subscription
    [InlineData("--blob blob", false)] // Missing subscription, account, and container
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedBlobs = new List<BlobInfo>(
            [
                new("blob", DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null, new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null)
            ]);

            _storageService.GetBlobDetails(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedBlobs);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBlobDetails_WhenBlobExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";
        var expected = new BlobInfo(blob, DateTimeOffset.UtcNow, null, null, "application/octet-stream", null, null,
            new Dictionary<string, string>(), null, null, null, null, null, null, null, null, false, null, null, null);

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expected]);

        var args = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container, "--blob", blob]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.BlobGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Blobs);

        Assert.Equal(expected.Name, result.Blobs[0].Name);
        Assert.Equal(expected.LastModified, result.Blobs[0].LastModified);
        Assert.Equal(expected.ContentType, result.Blobs[0].ContentType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container, "--blob", blob]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "notfound";

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Blob not found"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container, "--blob", blob]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Blob not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var blob = "blob123";

        _storageService.GetBlobDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(blob),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container, "--blob", blob]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
