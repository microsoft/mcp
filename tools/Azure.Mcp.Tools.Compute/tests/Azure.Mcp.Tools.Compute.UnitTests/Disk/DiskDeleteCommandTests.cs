// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskDeleteCommand.
/// </summary>
public class DiskDeleteCommandTests : CommandUnitTestsBase<DiskDeleteCommand, IComputeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(Command);
        Assert.Equal("delete", Command.Name);
        Assert.Contains("disk", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), Command.Id.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_DeletesDisk_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        Service.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskDeleteCommandResult);

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

        Service.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskDeleteCommandResult);

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

        Service.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskDeleteCommandResult);

        Assert.NotNull(result);
        Assert.True(result.Deleted);
        Assert.Equal(diskName, result.DiskName);
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredDiskName_ReturnsError()
    {
        // Arrange & Act - missing --disk-name
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg");

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MissingRequiredResourceGroup_ReturnsError()
    {
        // Arrange & Act - missing --resource-group
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--disk-name", "testdisk");

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

        Service.DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException("Conflict"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName);

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

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName);

        // Assert - if the command reached the service call, options were bound correctly
        await Service.Received().DeleteDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
