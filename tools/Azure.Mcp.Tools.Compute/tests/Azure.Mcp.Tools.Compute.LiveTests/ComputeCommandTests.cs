// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Compute.LiveTests;

/// <summary>
/// Live integration tests for Compute commands.
/// </summary>
public class ComputeCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task DiskGet_SpecificDisk_ReturnsValidDiskDetails()
    {
        // Arrange
        var diskName = Settings.DeploymentOutputs["DISKNAME"];
        var resourceGroup = Settings.ResourceGroupName;
        var subscription = Settings.SubscriptionId;

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroup },
                { "disk", diskName }
            });

        // Assert
        Assert.NotNull(result);
        JsonElement disks = result.Value.AssertProperty("Disks");
        Assert.Equal(JsonValueKind.Array, disks.ValueKind);

        List<JsonElement> diskList = disks.EnumerateArray().ToList();
        Assert.Single(diskList);

        JsonElement disk = diskList[0];
        Assert.Equal(diskName, disk.AssertProperty("Name").GetString());
        Assert.Equal(resourceGroup, disk.AssertProperty("ResourceGroup").GetString());
        Assert.NotNull(disk.AssertProperty("Location").GetString());
        Assert.NotNull(disk.AssertProperty("SkuName").GetString());
        Assert.True(disk.AssertProperty("DiskSizeGB").GetInt32() > 0);
    }

    [Fact]
    public async Task DiskGet_ListAllDisksInSubscription_ReturnsDisks()
    {
        // Arrange
        var subscription = Settings.SubscriptionId;

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription }
            });

        // Assert
        Assert.NotNull(result);
        JsonElement disks = result.Value.AssertProperty("Disks");
        Assert.Equal(JsonValueKind.Array, disks.ValueKind);

        List<JsonElement> diskList = disks.EnumerateArray().ToList();
        Assert.NotEmpty(diskList);
    }

    [Fact]
    public async Task DiskGet_ListDisksInResourceGroup_ReturnsDisks()
    {
        // Arrange
        var resourceGroup = Settings.ResourceGroupName;
        var subscription = Settings.SubscriptionId;

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroup }
            });

        // Assert
        Assert.NotNull(result);
        JsonElement disks = result.Value.AssertProperty("Disks");
        Assert.Equal(JsonValueKind.Array, disks.ValueKind);

        List<JsonElement> diskList = disks.EnumerateArray().ToList();
        Assert.NotEmpty(diskList);
        Assert.All(diskList, d => Assert.Equal(resourceGroup, d.AssertProperty("ResourceGroup").GetString()));
    }

    [Fact]
    public async Task DiskGet_WithInvalidDiskName_ReturnsNotFound()
    {
        // Arrange
        var resourceGroup = Settings.ResourceGroupName;
        var subscription = Settings.SubscriptionId;
        var invalidDiskName = "nonexistent-disk-" + Guid.NewGuid().ToString("N")[..8];

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroup },
                { "disk", invalidDiskName }
            });

        // Assert
        // When disk is not found, the response should be null or contain an error
        // For now, we'll check that we either get null or an empty disk list
        if (result != null)
        {
            JsonElement disks = result.Value.AssertProperty("Disks");
            List<JsonElement> diskList = disks.EnumerateArray().ToList();
            Assert.Empty(diskList);
        }
    }

    [Fact]
    public async Task DiskGet_WithInvalidResourceGroup_ReturnsNotFound()
    {
        // Arrange
        var diskName = Settings.DeploymentOutputs["DISKNAME"];
        var subscription = Settings.SubscriptionId;
        var invalidResourceGroup = "nonexistent-rg-" + Guid.NewGuid().ToString("N")[..8];

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", invalidResourceGroup },
                { "disk", diskName }
            });

        // Assert
        // When resource group is invalid, the response should be null or contain an error
        if (result != null)
        {
            JsonElement disks = result.Value.AssertProperty("Disks");
            List<JsonElement> diskList = disks.EnumerateArray().ToList();
            Assert.Empty(diskList);
        }
    }

    [Fact]
    public async Task DiskGet_WithDiskButNoResourceGroup_ReturnsBadRequest()
    {
        // Arrange
        var diskName = Settings.DeploymentOutputs["DISKNAME"];
        var subscription = Settings.SubscriptionId;

        // Act & Assert
        // This should throw or return null because resource group is required when disk is specified
        // The validation happens in the command, so we expect this to fail
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await CallToolAsync(
                "compute_disk_get",
                new()
                {
                    { "subscription", subscription },
                    { "disk", diskName }
                });
        });
    }
}
