// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tests;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskUpdateCommand.
/// </summary>
public class DiskUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<DiskUpdateCommand> _logger;
    private readonly DiskUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public DiskUpdateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<DiskUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_computeService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger, _computeService);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(_command);
        Assert.Equal("update", _command.Name);
        Assert.Contains("managed disk", _command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), _command.Id);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = _command.Metadata;

        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDiskSize_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var newSizeGb = 256;

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = newSizeGb,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            newSizeGb,
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--size-gb", newSizeGb.ToString()
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(newSizeGb, result.Disk.DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDiskSku_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var newSku = "StandardSSD_LRS";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = newSku,
            DiskSizeGB = 128,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            newSku,
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--sku", newSku
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(newSku, result.Disk.SkuName);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateMultipleProperties_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "UltraSSD_LRS",
            DiskSizeGB = 512,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--size-gb", "512",
            "--sku", "UltraSSD_LRS",
            "--disk-iops-read-write", "5000",
            "--disk-mbps-read-write", "200",
            "--max-shares", "2",
            "--network-access-policy", "AllowPrivate",
            "--enable-bursting", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(512, result.Disk.DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "westus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 64,
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--size-gb", "64"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(mockDisk.Name, result.Disk.Name);
        Assert.Equal(mockDisk.Location, result.Disk.Location);
        Assert.Equal(mockDisk.SkuName, result.Disk.SkuName);
        Assert.Equal(mockDisk.DiskSizeGB, result.Disk.DiskSizeGB);
        Assert.Equal(mockDisk.ProvisioningState, result.Disk.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ResolvesFromSubscription()
    {
        // Arrange - no resource-group specified, should search for disk by name
        var subscription = "test-sub";
        var diskName = "testdisk";
        var resolvedResourceGroup = "found-rg";

        var existingDisks = new List<DiskInfo>
        {
            new()
            {
                Name = diskName,
                ResourceGroup = resolvedResourceGroup,
                Location = "eastus",
                SkuName = "Premium_LRS",
                DiskSizeGB = 128
            }
        };

        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(existingDisks);

        var updatedDisk = new DiskInfo
        {
            Name = diskName,
            ResourceGroup = resolvedResourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 256,
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(updatedDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--disk", diskName,
            "--size-gb", "256"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(256, result.Disk.DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroupAndDiskNotFound_ReturnsBadRequest()
    {
        // Arrange - no resource-group, disk doesn't exist in subscription
        _computeService.ListDisksAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<DiskInfo>());

        var args = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--disk", "nonexistent",
            "--size-gb", "256"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        var args = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk", "testdisk",
            "--size-gb", "256"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DiskNotFound_Returns404()
    {
        // Arrange
        var notFoundEx = new Azure.RequestFailedException(404, "Disk not found");
        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundEx);

        var args = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk", "nonexistentdisk",
            "--size-gb", "256"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk", "testdisk",
            "--size-gb", "512",
            "--sku", "UltraSSD_LRS",
            "--disk-iops-read-write", "5000",
            "--disk-mbps-read-write", "200",
            "--max-shares", "3",
            "--network-access-policy", "AllowPrivate",
            "--enable-bursting", "true",
            "--tags", "env=staging team=dev",
            "--disk-encryption-set", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskEncryptionSets/myDes",
            "--encryption-type", "EncryptionAtRestWithPlatformAndCustomerKeys",
            "--disk-access", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskAccesses/myAccess",
            "--tier", "P50"
        ]);

        // Act - verify parse doesn't throw and has no errors
        Assert.NotNull(args);
        Assert.Empty(args.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDiskWithTags_ReturnsSuccess()
    {
        // Arrange - update disk with tags
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var tags = "env=prod team=infra";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 128,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            tags,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--tags", tags
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _computeService.Received(1).UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            tags,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDiskWithEncryptionAndTier_ReturnsSuccess()
    {
        // Arrange - update disk with encryption settings, disk access, and tier
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var diskEncryptionSet = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/diskEncryptionSets/myDes";
        var encryptionType = "EncryptionAtRestWithCustomerKey";
        var diskAccess = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/diskAccesses/myAccess";
        var tier = "P50";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 256,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            diskEncryptionSet,
            encryptionType,
            diskAccess,
            tier,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--disk-encryption-set", diskEncryptionSet,
            "--encryption-type", encryptionType,
            "--disk-access", diskAccess,
            "--tier", tier
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        await _computeService.Received(1).UpdateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            diskEncryptionSet,
            encryptionType,
            diskAccess,
            tier,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UpdateDiskWithAllTier1Parameters_ReturnsSuccess()
    {
        // Arrange - update disk with all new Tier 1 parameters at once
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 256,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        _computeService.UpdateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        var args = _commandDefinition.Parse([
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk", diskName,
            "--size-gb", "256",
            "--sku", "Premium_LRS",
            "--tags", "env=staging cost-center=123",
            "--disk-encryption-set", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskEncryptionSets/myDes",
            "--encryption-type", "EncryptionAtRestWithPlatformAndCustomerKeys",
            "--disk-access", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskAccesses/myAccess",
            "--tier", "P40",
            "--network-access-policy", "AllowPrivate",
            "--enable-bursting", "true"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.DiskUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
    }
}
