// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskDeleteCommand.
/// </summary>
public class DiskDeleteCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<DiskDeleteCommand> _logger;
    private readonly DiskDeleteCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DiskDeleteCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<DiskDeleteCommand>>();

        var collection = new ServiceCollection().AddSingleton(_computeService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        // Command already created in constructor

        // Assert
        Assert.NotNull(_command);
        Assert.Equal("delete", _command.Name);
        Assert.Contains("disk", _command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), _command.Id.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesDisk_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        _computeService.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--disk-name", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskDeleteCommandResult);

        Assert.NotNull(result);
        Assert.True(result.Deleted);
        Assert.Equal(diskName, result.DiskName);
    }

    [Fact]
    public async Task ExecuteAsync_DiskNotFound_ReturnsFalse()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "nonexistent";

        _computeService.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--disk-name", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskDeleteCommandResult);

        Assert.NotNull(result);
        Assert.False(result.Deleted);
        Assert.Equal(diskName, result.DiskName);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        _computeService.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--disk-name", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskDeleteCommandResult);

        Assert.NotNull(result);
        Assert.True(result.Deleted);
        Assert.Equal(diskName, result.DiskName);
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredDiskName_ReturnsError()
    {
        // Arrange - missing --disk-name
        var args = _commandDefinition.Parse(["--subscription", "test-sub", "--resource-group", "testrg"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredResourceGroup_ReturnsError()
    {
        // Arrange - missing --resource-group
        var args = _commandDefinition.Parse(["--subscription", "test-sub", "--disk-name", "testdisk"]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        _computeService.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException("Conflict"));

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--disk-name", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        var args = _commandDefinition.Parse(["--subscription", subscription, "--resource-group", resourceGroup, "--disk-name", diskName]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert - if the command reached the service call, options were bound correctly
        await _computeService.Received().DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
