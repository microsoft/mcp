// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob.Container;

public class ContainerGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<ContainerGetCommand> _logger;
    private readonly ContainerGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ContainerGetCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<ContainerGetCommand>>();

        _serviceProvider = new ServiceCollection().BuildServiceProvider();
        _command = new(_logger, _storageService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_NoParameters_ReturnsContainers()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var expectedContainers = new List<ContainerInfo>(
        [
            new("container", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false),
            new("container2", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false)
        ]);

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedContainers);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Containers);
        Assert.Equal(expectedContainers.Count, result.Containers.Count);
        Assert.Equal(expectedContainers.Select(a => a.Name), result.Containers.Select(a => a.Name));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoContainers()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        var subscription = "sub123";
        var account = "testaccount";

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--account", account]);

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
    [InlineData("--account mystorageaccount --subscription sub123", true)]
    [InlineData("--subscription sub123 --account mystorageaccount", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container", true)]
    [InlineData("--subscription sub123 --account mystorageaccount --container container --prefix prefix", true)]
    [InlineData("--subscription sub123", false)] // Missing account
    [InlineData("--account mystorageaccount", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedContainers = new List<ContainerInfo>(
            [
                new("container", DateTimeOffset.UtcNow, null, new Dictionary<string, string>(), null, null, null, null, false, false, null, null, false)
            ]);

            _storageService.GetContainerDetails(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedContainers);
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
    public async Task ExecuteAsync_ReturnsContainerDetails_WhenContainerExists()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";
        var expected = new ContainerInfo(container, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([expected]);

        var args = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.ContainerGetCommandResult);

        Assert.NotNull(result);
        Assert.Single(result.Containers);

        Assert.Equal(expected.Name, result.Containers[0].Name);
        Assert.Equal(expected.LastModified, result.Containers[0].LastModified);
        Assert.Equal(expected.ETag, result.Containers[0].ETag);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container]);

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
        var container = "notfound";

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Container not found"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("Container not found", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        var account = "mystorageaccount";
        var subscription = "sub123";
        var container = "container123";

        _storageService.GetContainerDetails(
            Arg.Is(account),
            Arg.Is(container),
            Arg.Is(subscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", account, "--subscription", subscription, "--container", container]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }
}
