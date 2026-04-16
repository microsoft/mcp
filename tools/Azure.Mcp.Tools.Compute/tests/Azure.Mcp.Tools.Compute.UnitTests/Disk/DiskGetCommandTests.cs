// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskGetCommand.
/// </summary>
public class DiskGetCommandTests : CommandUnitTestsBase<DiskGetCommand, IComputeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(Command);
        Assert.Equal("get", Command.Name);
        Assert.Contains("disk", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), Command.Id.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_ListAllDisks_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";

        var mockDisks = new List<Models.DiskInfo>
        {
            new()
            {
                Name = "disk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/disk1",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128,
                DiskState = "Unattached"
            },
            new()
            {
                Name = "disk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/disk2",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256,
                DiskState = "Attached"
            }
        };

        Service.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisks);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "disk1");
        Assert.Contains(result.Disks, d => d.Name == "disk2");
    }

    [Fact]
    public async Task ExecuteAsync_ListDisksInResourceGroup_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";

        var mockDisks = new List<Models.DiskInfo>
        {
            new()
            {
                Name = "disk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/disk1",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128,
                DiskState = "Unattached"
            }
        };

        Service.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisks);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--resource-group", resourceGroup);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal("disk1", result.Disks[0].Name);
        Assert.Equal(resourceGroup, result.Disks[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_SpecificDisk_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var diskName = "testdisk";
        var resourceGroup = "testrg";

        var mockDisk = new Models.DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 128,
            DiskState = "Unattached",
            TimeCreated = DateTimeOffset.UtcNow
        };

        Service.GetDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--disk-name", diskName,
            "--resource-group", resourceGroup);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal(diskName, result.Disks[0].Name);
        Assert.Equal(resourceGroup, result.Disks[0].ResourceGroup);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "test-sub";
        var diskName = "testdisk";
        var resourceGroup = "testrg";

        var mockDisk = new Models.DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "westus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 64
        };

        Service.GetDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--disk-name", diskName,
            "--resource-group", resourceGroup);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal(mockDisk.Name, result.Disks[0].Name);
        Assert.Equal(mockDisk.Location, result.Disks[0].Location);
        Assert.Equal(mockDisk.SkuName, result.Disks[0].SkuName);
        Assert.Equal(mockDisk.DiskSizeGB, result.Disks[0].DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_WildcardPatternWithoutResourceGroup_ReturnsFilteredDisks()
    {
        // Arrange
        var subscription = "test-sub";
        var diskPattern = "win_OsDisk*";

        var mockDisks = new List<Models.DiskInfo>
        {
            new()
            {
                Name = "win_OsDisk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/win_OsDisk1",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "win_OsDisk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/win_OsDisk2",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            },
            new()
            {
                Name = "linux-disk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg3/providers/Microsoft.Compute/disks/linux-disk",
                ResourceGroup = "rg3",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 64
            }
        };

        Service.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisks);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--disk-name", diskPattern);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "win_OsDisk1");
        Assert.Contains(result.Disks, d => d.Name == "win_OsDisk2");
        Assert.DoesNotContain(result.Disks, d => d.Name == "linux-disk");
    }

    [Fact]
    public async Task ExecuteAsync_WildcardPatternWithResourceGroup_ReturnsFilteredDisksInResourceGroup()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskPattern = "data*";

        var mockDisks = new List<Models.DiskInfo>
        {
            new()
            {
                Name = "datadisk1",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/datadisk1",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "datadisk2",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/datadisk2",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            },
            new()
            {
                Name = "osdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/osdisk",
                ResourceGroup = resourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 64
            }
        };

        Service.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisks);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--disk-name", diskPattern,
            "--resource-group", resourceGroup);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Equal(2, result.Disks.Count);
        Assert.Contains(result.Disks, d => d.Name == "datadisk1");
        Assert.Contains(result.Disks, d => d.Name == "datadisk2");
        Assert.DoesNotContain(result.Disks, d => d.Name == "osdisk");
    }

    [Fact]
    public async Task ExecuteAsync_ExactDiskNameWithoutResourceGroup_ReturnsFilteredDisks()
    {
        // Arrange - When disk name is exact but no resource group, should list and filter
        var subscription = "test-sub";
        var diskName = "exactdisk";

        var mockDisks = new List<Models.DiskInfo>
        {
            new()
            {
                Name = "exactdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg1/providers/Microsoft.Compute/disks/exactdisk",
                ResourceGroup = "rg1",
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            },
            new()
            {
                Name = "otherdisk",
                Id = $"/subscriptions/{subscription}/resourceGroups/rg2/providers/Microsoft.Compute/disks/otherdisk",
                ResourceGroup = "rg2",
                Location = "westus",
                SkuName = "Standard_LRS",
                DiskSizeGB = 256
            }
        };

        Service.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisks);

        // Act
        var response = await ExecuteCommandAsync("--subscription", subscription, "--disk-name", diskName);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var result = ConvertResponse(response, ComputeJsonContext.Default.DiskGetCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disks);
        Assert.Single(result.Disks);
        Assert.Equal("exactdisk", result.Disks[0].Name);
    }
}

