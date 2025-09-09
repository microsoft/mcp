// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly string _knownAccount = "account123";
    private static readonly string _knownContainer = "container123";
    private readonly string _knownSubscription = "sub123";

    public ContainerCreateCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<ContainerCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
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
    [InlineData("--account account123 --container container123 --subscription sub123", true)]
    [InlineData("--container container123 --subscription sub123", false)] // Missing account
    [InlineData("--account account123 --subscription sub123", false)] // Missing container
    [InlineData("--account account123 --container container123", false)] // Missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedProperties = CreateMockBlobContainerProperties();
            _storageService.CreateContainer(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedProperties);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.True(response.Status >= 400);
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesContainer()
    {
        // Arrange
        var expectedProperties = CreateMockBlobContainerProperties();
        _storageService.CreateContainer(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedProperties);

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
        var result = JsonSerializer.Deserialize<ContainerCreateCommand.ContainerCreateCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Container);
        Assert.Equal(expectedProperties.LastModified, result.Container.LastModified);
        Assert.Equal(expectedProperties.ETag, result.Container.ETag);
        Assert.Equal(expectedProperties.PublicAccess, result.Container.PublicAccess);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        _storageService.CreateContainer(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
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

    [Fact]
    public async Task ExecuteAsync_HandlesConflictException()
    {
        // Arrange
        var conflictException = new RequestFailedException(409, "Container already exists");
        _storageService.CreateContainer(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(conflictException);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--container", _knownContainer,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(409, response.Status);
        Assert.Contains("Container already exists", response.Message);
    }

    private static ContainerInfo CreateMockBlobContainerProperties()
        => new(_knownContainer, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string> { { "k", "v" } },
            "Leased", "Locked", "Infinite", "Blob", true, false, DateTimeOffset.UtcNow.AddDays(-1), 5, true);
}
