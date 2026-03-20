// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Blob.Container;

public class ContainerCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<ContainerCreateCommand> _logger;
    private readonly ContainerCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ContainerCreateCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<ContainerCreateCommand>>();

        _serviceProvider = new ServiceCollection().BuildServiceProvider();
        _command = new(_logger, _storageService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--account testaccount --subscription sub123 --container container123", true)]
    [InlineData("--account testaccount --tenant tenant123 --subscription sub123 --container container123 ", true)]
    [InlineData("--account testaccount", false)] // Missing subscription and container name
    [InlineData("--container container123", false)] // Missing subscription and account name
    [InlineData("--subscription sub123", false)] // Missing account name and container name
    [InlineData("--account testaccount --subscription sub123", false)] // Missing container name
    [InlineData("--container container123 --subscription sub123", false)] // Missing account name
    [InlineData("--account testaccount --container container123", false)] // Missing subscription
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expected = new ContainerInfo("container123", DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
                "unlocked", "available", null, "private", false, false, null, null, false);

            _storageService.CreateContainer(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expected);
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

            var json = JsonSerializer.Serialize(response.Results);
            var result = JsonSerializer.Deserialize(json, StorageJsonContext.Default.ContainerCreateCommandResult);
            Assert.NotNull(result);
            Assert.NotNull(result.Container);
            Assert.Equal("container123", result.Container.Name);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesContainerAlreadyExists()
    {
        // Arrange
        _storageService.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Container already exists"));

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--subscription", "sub123", "--container", "existingcontainer"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _storageService.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--subscription", "sub123", "--container", "invalidaccess"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _storageService.CreateContainer(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var parseResult = _commandDefinition.Parse(["--account", "testaccount", "--subscription", "sub123", "--container", "container123"]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var subscription = "sub123";
        var account = "testaccount";
        var container = "container123";

        var expected = new ContainerInfo(container, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string>(),
            "unlocked", "available", null, "private", false, false, null, null, false);

        _storageService.CreateContainer(
            account,
            container,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var parseResult = _commandDefinition.Parse([
            "--account", account,
            "--subscription", subscription,
            "--container", container
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _storageService.Received(1).CreateContainer(
            account,
            container,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>());
    }
}
