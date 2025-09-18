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

public class ContainerGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStorageService _storageService;
    private readonly ILogger<ContainerGetCommand> _logger;
    private readonly ContainerGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownAccount = "account123";
    private readonly string _knownContainer = "container123";
    private readonly string _knownSubscription = "sub123";

    public ContainerGetCommandTests()
    {
        _storageService = Substitute.For<IStorageService>();
        _logger = Substitute.For<ILogger<ContainerGetCommand>>();

        var collection = new ServiceCollection().AddSingleton(_storageService);
        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsContainers()
    {
        // Arrange
        var expectedContainers = new List<ContainerInfo>
        {
            new ("container1", DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string> { { "k", "v" } },
                "Leased", "Locked", "Infinite", "Blob", true, false, DateTimeOffset.UtcNow.AddDays(-1), 5, true),
            new ("container2", DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string> { { "k", "v" } },
                "Leased", "Locked", "Infinite", "Blob", true, false, DateTimeOffset.UtcNow.AddDays(-1), 5, true)
        };

        _storageService.GetContainerDetails(
            Arg.Is(_knownAccount),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns(expectedContainers);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ContainerGetCommand.ContainerGetCommandResult>(json);

        Assert.NotNull(result);
        Assert.NotNull(result.Containers);
        Assert.Equal(2, result.Containers.Count);
        Assert.Equal(expectedContainers[0].Name, result.Containers[0].Name);
        Assert.Equal(expectedContainers[1].Name, result.Containers[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoContainers()
    {
        // Arrange
        _storageService.GetContainerDetails(
            Arg.Is(_knownAccount),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .Returns([]);

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
            "--subscription", _knownSubscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ContainerGetCommand.ContainerGetCommandResult>(json);

        Assert.NotNull(result);
        Assert.Empty(result.Containers);
    }

    [Fact]
    public async Task ExecuteAsync_GenericError_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _storageService.GetContainerDetails(
            Arg.Is(_knownAccount),
            Arg.Is<string?>(s => string.IsNullOrEmpty(s)),
            Arg.Is(_knownSubscription),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", _knownAccount,
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
    public async Task ExecuteAsync_ReturnsContainerDetails()
    {
        // Arrange
        var expectedContainers = new List<ContainerInfo>
        {
            new (_knownContainer, DateTimeOffset.UtcNow, "etag123", new Dictionary<string, string> { { "k", "v" } },
                "Leased", "Locked", "Infinite", "Blob", true, false, DateTimeOffset.UtcNow.AddDays(-1), 5, true)
        };

        _storageService.GetContainerDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
            Arg.Is(_knownSubscription), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>())
            .Returns(expectedContainers);

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
        var result = JsonSerializer.Deserialize<ContainerGetCommand.ContainerGetCommandResult>(json);

        Assert.NotNull(result);
        Assert.Single(result.Containers);
        Assert.Equal(expectedContainers[0].LastModified, result.Containers[0].LastModified);
        Assert.Equal(expectedContainers[0].LeaseStatus, result.Containers[0].LeaseStatus);
        Assert.Equal(expectedContainers[0].LeaseState, result.Containers[0].LeaseState);
        Assert.Equal(expectedContainers[0].LeaseDuration, result.Containers[0].LeaseDuration);
        Assert.Equal(expectedContainers[0].PublicAccess, result.Containers[0].PublicAccess);
        Assert.Equal(expectedContainers[0].HasImmutabilityPolicy, result.Containers[0].HasImmutabilityPolicy);
        Assert.Equal(expectedContainers[0].HasLegalHold, result.Containers[0].HasLegalHold);
        Assert.Equal(expectedContainers[0].DeletedOn, result.Containers[0].DeletedOn);
        Assert.Equal(expectedContainers[0].RemainingRetentionDays, result.Containers[0].RemainingRetentionDays);
        Assert.Equal(expectedContainers[0].Metadata, result.Containers[0].Metadata);
        Assert.Equal(expectedContainers[0].HasImmutableStorageWithVersioning, result.Containers[0].HasImmutableStorageWithVersioning);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";
        _storageService.GetContainerDetails(Arg.Is(_knownAccount), Arg.Is(_knownContainer),
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
}
