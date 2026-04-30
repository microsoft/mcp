// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Disk;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Disk;

/// <summary>
/// Unit tests for the DiskCreateCommand.
/// </summary>
public class DiskCreateCommandTests : CommandUnitTestsBase<DiskCreateCommand, IComputeService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(Command);
        Assert.Equal("create", Command.Name);
        Assert.Contains("managed disk", Command.Description, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(Guid.Empty.ToString(), Command.Id);
    }

    [Fact]
    public void Metadata_HasCorrectProperties()
    {
        var metadata = Command.Metadata;

        Assert.False(metadata.OpenWorld);
        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDisk_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var location = "eastus";
        var sizeGb = 128;

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = location,
            SkuName = "Premium_LRS",
            DiskSizeGB = sizeGb,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            location,
            sizeGb,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--location", location,
            "--size-gb", sizeGb.ToString());

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(resourceGroup, result.Disk.ResourceGroup);
        Assert.Equal(location, result.Disk.Location);
        Assert.Equal(sizeGb, result.Disk.DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskWithAllOptions_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var location = "eastus";
        var sizeGb = 256;
        var sku = "StandardSSD_LRS";
        var osType = "Linux";
        var zone = "1";
        var hyperVGeneration = "V2";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = location,
            SkuName = sku,
            DiskSizeGB = sizeGb,
            OSType = osType,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            location,
            sizeGb,
            sku,
            osType,
            zone,
            hyperVGeneration,
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--location", location,
            "--size-gb", sizeGb.ToString(),
            "--sku", sku,
            "--os-type", osType,
            "--zone", zone,
            "--hyper-v-generation", hyperVGeneration);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(sku, result.Disk.SkuName);
        Assert.Equal(sizeGb, result.Disk.DiskSizeGB);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var location = "westus";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = location,
            SkuName = "Premium_LRS",
            DiskSizeGB = 64,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded",
            TimeCreated = DateTimeOffset.UtcNow
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--location", location,
            "--size-gb", "64");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(mockDisk.Name, result.Disk.Name);
        Assert.Equal(mockDisk.Location, result.Disk.Location);
        Assert.Equal(mockDisk.SkuName, result.Disk.SkuName);
        Assert.Equal(mockDisk.DiskSizeGB, result.Disk.DiskSizeGB);
        Assert.Equal(mockDisk.ProvisioningState, result.Disk.ProvisioningState);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskWithoutLocation_ReturnsSuccess()
    {
        // Arrange - location not specified, should resolve from resource group
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
            DiskSizeGB = 128,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--size-gb", "128");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
    }

    [Fact]
    public async Task ExecuteAsync_MissingResourceGroup_ReturnsBadRequest()
    {
        // Arrange & Act - no resource-group specified
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--disk-name", "testdisk",
            "--location", "eastus",
            "--size-gb", "128");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceError()
    {
        // Arrange
        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk",
            "--location", "eastus",
            "--size-gb", "128");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var args = CommandDefinition.Parse([
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk",
            "--location", "eastus",
            "--size-gb", "256",
            "--sku", "Standard_LRS",
            "--os-type", "Linux",
            "--zone", "2",
            "--hyper-v-generation", "V2",
            "--source", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/snapshots/snap1",
            "--tags", "env=prod team=infra",
            "--disk-encryption-set", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskEncryptionSets/myDes",
            "--encryption-type", "EncryptionAtRestWithCustomerKey",
            "--disk-access", "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/diskAccesses/myAccess",
            "--tier", "P30",
            "--max-shares", "2",
            "--network-access-policy", "AllowPrivate",
            "--enable-bursting", "true",
            "--disk-iops-read-write", "5000",
            "--disk-mbps-read-write", "200",
            "--upload-type", "Upload",
            "--upload-size-bytes", "20972032",
            "--security-type", "TrustedLaunch"
        ]);

        // Act - use reflection or just verify parse doesn't throw
        // The BindOptions is called internally by ExecuteAsync
        Assert.NotNull(args);
        Assert.Empty(args.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromSourceResourceId_ReturnsSuccess()
    {
        // Arrange - create disk from a snapshot resource ID
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var source = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/snapshots/mysnapshot";

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

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            source,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--source", source);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromBlobUri_ReturnsSuccess()
    {
        // Arrange - create disk from a VHD blob URI
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var source = "https://mystorageaccount.blob.core.windows.net/vhds/mydisk.vhd";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "westus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 256,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            source,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--source", source);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskWithTier1Parameters_ReturnsSuccess()
    {
        // Arrange - create disk with tags, encryption, tier, and performance options
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var location = "eastus";
        var sizeGb = 256;
        var tags = "env=prod team=infra";
        var diskEncryptionSet = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/diskEncryptionSets/myDes";
        var encryptionType = "EncryptionAtRestWithCustomerKey";
        var diskAccess = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/diskAccesses/myAccess";
        var tier = "P30";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = location,
            SkuName = "Premium_LRS",
            DiskSizeGB = sizeGb,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            location,
            sizeGb,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            tags,
            diskEncryptionSet,
            encryptionType,
            diskAccess,
            tier,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--location", location,
            "--size-gb", sizeGb.ToString(),
            "--tags", tags,
            "--disk-encryption-set", diskEncryptionSet,
            "--encryption-type", encryptionType,
            "--disk-access", diskAccess,
            "--tier", tier);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(sizeGb, result.Disk.DiskSizeGB);

        await Service.Received(1).CreateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            location,
            sizeGb,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            tags,
            diskEncryptionSet,
            encryptionType,
            diskAccess,
            tier,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskWithPerformanceOptions_ReturnsSuccess()
    {
        // Arrange - create disk with IOPS, throughput, shared disk, network, and bursting options
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "ultraDisk";
        var location = "eastus";
        var sizeGb = 512;
        var sku = "UltraSSD_LRS";

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = location,
            SkuName = sku,
            DiskSizeGB = sizeGb,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--location", location,
            "--size-gb", sizeGb.ToString(),
            "--sku", sku,
            "--max-shares", "3",
            "--network-access-policy", "AllowPrivate",
            "--enable-bursting", "true");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal(sku, result.Disk.SkuName);
    }

    [Fact]
    public async Task ExecuteAsync_MissingSourceAndSizeGbAndGalleryRefAndUpload_ReturnsBadRequest()
    {
        // Arrange & Act - neither --source, --size-gb, --gallery-image-reference, nor --upload-type specified
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskForUpload_ReturnsSuccess()
    {
        // Arrange - create disk for upload
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "uploaddisk";
        var uploadSizeBytes = 20972032L;

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 20,
            DiskState = "ReadyToUpload",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            "Upload",
            uploadSizeBytes,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--upload-type", "Upload",
            "--upload-size-bytes", uploadSizeBytes.ToString());

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal("ReadyToUpload", result.Disk.DiskState);
    }

    [Fact]
    public async Task ExecuteAsync_UploadTypeMissingUploadSizeBytes_ReturnsBadRequest()
    {
        // Arrange & Act - --upload-type specified but --upload-size-bytes missing
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk",
            "--upload-type", "Upload");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskForUploadWithSecurityData_ReturnsSuccess()
    {
        // Arrange - create disk for UploadWithSecurityData with security-type
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "securedisk";
        var uploadSizeBytes = 20972032L;

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Standard_LRS",
            DiskSizeGB = 20,
            DiskState = "ReadyToUpload",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            "UploadWithSecurityData",
            uploadSizeBytes,
            "TrustedLaunch",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--upload-type", "UploadWithSecurityData",
            "--upload-size-bytes", uploadSizeBytes.ToString(),
            "--security-type", "TrustedLaunch",
            "--hyper-v-generation", "V2");

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
        Assert.Equal("ReadyToUpload", result.Disk.DiskState);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromGalleryImage_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdisk";
        var galleryImageRef = "/subscriptions/test-sub/resourceGroups/testrg/providers/Microsoft.Compute/galleries/myGallery/images/myImage/versions/1.0.0";

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

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--gallery-image-reference", galleryImageRef);

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromGalleryImageWithLun_ReturnsSuccess()
    {
        // Arrange
        var subscription = "test-sub";
        var resourceGroup = "testrg";
        var diskName = "testdatadisk";
        var galleryImageRef = "/subscriptions/test-sub/resourceGroups/testrg/providers/Microsoft.Compute/galleries/myGallery/images/myImage/versions/1.0.0";
        var lun = 1;

        var mockDisk = new DiskInfo
        {
            Name = diskName,
            Id = $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.Compute/disks/{diskName}",
            ResourceGroup = resourceGroup,
            Location = "eastus",
            SkuName = "Premium_LRS",
            DiskSizeGB = 64,
            DiskState = "Unattached",
            ProvisioningState = "Succeeded"
        };

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            galleryImageRef,
            lun,
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(mockDisk);

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--resource-group", resourceGroup,
            "--disk-name", diskName,
            "--gallery-image-reference", galleryImageRef,
            "--gallery-image-reference-lun", lun.ToString());

        // Assert
        var result = ValidateAndDeserializeResponse(response, ComputeJsonContext.Default.DiskCreateCommandResult);

        Assert.NotNull(result.Disk);
        Assert.Equal(diskName, result.Disk.Name);

        await Service.Received(1).CreateDiskAsync(
            diskName,
            resourceGroup,
            subscription,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            galleryImageRef,
            lun,
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CreateDiskFromHttpSource_ThrowsSecurityException()
    {
        // Arrange - HTTP source should be rejected by EndpointValidator
        var source = "http://mystorageaccount.blob.core.windows.net/vhds/mydisk.vhd";

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzurePublicCloud));
        Assert.Contains("HTTPS", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://evil-server.com/vhds/mydisk.vhd")]
    [InlineData("https://internal-host:8080/secret")]
    [InlineData("https://169.254.169.254/latest/meta-data")]
    [InlineData("https://myaccount.blob.core.windows.net.evil.com/vhds/disk.vhd")]
    [InlineData("https://notblob.core.windows.net/vhds/disk.vhd")]
    [InlineData("https://myaccount.table.core.windows.net/vhds/disk.vhd")]
    [InlineData("https://myaccount.file.core.windows.net/vhds/disk.vhd")]
    [InlineData("https://attacker.com#storageacc.blob.core.windows.net")]
    [InlineData("https://attacker.com#.blob.core.windows.net/vhds/disk.vhd")]
    [InlineData("https://storageacc.blob.core.windows.net@attacker.com/vhds/disk.vhd")]
    public void CreateDiskFromNonBlobUri_ThrowsSecurityException(string source)
    {
        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzurePublicCloud));
        Assert.Contains("storage-blob", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://mystorageaccount.blob.core.windows.net/vhds/mydisk.vhd")]
    [InlineData("https://account123.blob.core.windows.net/container/path/to/disk.vhd")]
    [InlineData("https://acct.blob.core.windows.net/vhds/disk.vhd?sv=2021-06-08&ss=b&srt=o&sp=r")]
    [InlineData("https://account.blob.core.windows.net:4443/vhds/disk.vhd")]
    [InlineData("https://user:pass@account.blob.core.windows.net/vhds/disk.vhd")]
    public void CreateDiskFromValidPublicCloudBlobUri_DoesNotThrow(string source)
    {
        // Act & Assert - should not throw for public cloud
        EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzurePublicCloud);
    }

    [Theory]
    [InlineData("https://myaccount.blob.core.chinacloudapi.cn/vhds/mydisk.vhd")]
    public void CreateDiskFromValidChinaCloudBlobUri_DoesNotThrow(string source)
    {
        // Act & Assert - should not throw for China cloud
        EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzureChina);
    }

    [Theory]
    [InlineData("https://myaccount.blob.core.usgovcloudapi.net/vhds/mydisk.vhd")]
    public void CreateDiskFromValidGovCloudBlobUri_DoesNotThrow(string source)
    {
        // Act & Assert - should not throw for US Government cloud
        EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzureGovernment);
    }

    [Fact]
    public void CreateDiskFromChinaCloudUri_RejectedInPublicCloud()
    {
        // Arrange - China cloud URI should be rejected when using public cloud environment
        var source = "https://myaccount.blob.core.chinacloudapi.cn/vhds/mydisk.vhd";

        // Act & Assert
        var ex = Assert.Throws<SecurityException>(() =>
            EndpointValidator.ValidateAzureServiceEndpoint(source, "storage-blob", ArmEnvironment.AzurePublicCloud));
        Assert.Contains("storage-blob", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromHttpSource_ReturnsError()
    {
        // Arrange - service throws ArgumentException for HTTP source
        var source = "http://mystorageaccount.blob.core.windows.net/vhds/mydisk.vhd";

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            source,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new SecurityException("Endpoint must use HTTPS protocol. Got: http"));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk",
            "--source", source);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("HTTPS", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_CreateDiskFromNonAzureBlobUri_ReturnsError()
    {
        // Arrange - service throws ArgumentException for non-Azure blob URI
        var source = "https://evil-server.com/vhds/mydisk.vhd";

        Service.CreateDiskAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            source,
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int?>(),
            Arg.Any<long?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new SecurityException("Endpoint host 'evil-server.com' is not a valid storage-blob domain for Azure Public Cloud."));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "test-sub",
            "--resource-group", "testrg",
            "--disk-name", "testdisk",
            "--source", source);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("storage-blob", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
