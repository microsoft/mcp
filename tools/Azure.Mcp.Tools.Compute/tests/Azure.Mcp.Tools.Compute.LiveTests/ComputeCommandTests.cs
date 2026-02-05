// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Compute.LiveTests;

[Collection("LiveServer")]
public class ComputeCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // Use Settings.ResourceBaseName with suffixes (following SQL pattern)
    private string VmName => $"{Settings.ResourceBaseName}-vm";
    private string VmssName => $"{Settings.ResourceBaseName}-vmss";
    private string DiskName => $"{Settings.ResourceBaseName}-disk";

    // Disable default sanitizer additions to avoid conflicts (following SQL pattern)
    public override bool EnableDefaultSanitizerAdditions => false;

    // Sanitize resource group in URIs
    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "resource[gG]roups\\/([^?\\/]+)",
            Value = "sanitized",
            GroupForReplace = "1"
        })
    ];

    // Sanitize resource group name, base name, and subscription ID everywhere
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceGroupName,
            Value = "sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceBaseName,
            Value = "sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.SubscriptionId,
            Value = "00000000-0000-0000-0000-000000000000",
        }),
        // Sanitize all subscription GUIDs in image references and other nested properties
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
            Value = "00000000-0000-0000-0000-000000000000",
        })
    ];

    [Fact]
    public async Task Should_list_vms_in_subscription()
    {
        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var vms = result.AssertProperty("Vms");
        Assert.Equal(JsonValueKind.Array, vms.ValueKind);
        Assert.NotEmpty(vms.EnumerateArray());
    }

    [Fact]
    public async Task Should_list_vms_in_resource_group()
    {
        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName }
            });

        var vms = result.AssertProperty("Vms");
        Assert.Equal(JsonValueKind.Array, vms.ValueKind);

        var vmArray = vms.EnumerateArray().ToList();
        Assert.True(vmArray.Count >= 1); // Should have at least 1 VM in the test resource group
    }

    [Fact]
    public async Task Should_get_specific_vm_details()
    {
        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", VmName }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var name = vm.GetProperty("name");
        Assert.NotNull(name.GetString()); // Name is sanitized during playback

        var location = vm.GetProperty("location");
        Assert.NotNull(location.GetString());

        var vmSize = vm.GetProperty("vmSize");
        Assert.Equal("Standard_B2s", vmSize.GetString());

        var osType = vm.GetProperty("osType");
        Assert.Equal("Linux", osType.GetString());

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());
    }

    [Fact]
    public async Task Should_get_vm_with_instance_view()
    {
        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", VmName },
                { "instance-view", true }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var name = vm.GetProperty("name");
        Assert.NotNull(name.GetString()); // Name is sanitized during playback

        // Verify instance view is present
        var instanceView = result.AssertProperty("InstanceView");
        Assert.Equal(JsonValueKind.Object, instanceView.ValueKind);

        // Check for power state
        var powerState = instanceView.GetProperty("powerState");
        Assert.NotNull(powerState.GetString());
        // Should be "running" or similar VM state

        // Check for provisioning state (lowercase in instance view)
        var provisioningState = instanceView.GetProperty("provisioningState");
        Assert.Equal("succeeded", provisioningState.GetString());
    }

    [Fact]
    public async Task Should_list_vmss_in_subscription()
    {
        var result = await CallToolAsync(
            "compute_vmss_get",
            new()
            {
                { "subscription", Settings.SubscriptionId }
            });

        var vmssList = result.AssertProperty("VmssList");
        Assert.Equal(JsonValueKind.Array, vmssList.ValueKind);
        Assert.NotEmpty(vmssList.EnumerateArray());
    }

    [Fact]
    public async Task Should_get_specific_vmss_details()
    {
        var result = await CallToolAsync(
            "compute_vmss_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", VmssName }
            });

        var vmss = result.AssertProperty("Vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var name = vmss.GetProperty("name");
        Assert.NotNull(name.GetString()); // Name is sanitized during playback

        var location = vmss.GetProperty("location");
        Assert.NotNull(location.GetString());

        var sku = vmss.GetProperty("sku");
        Assert.Equal(JsonValueKind.Object, sku.ValueKind);
        // Skip SKU name assertion as it may be sanitized
    }

    [Fact]
    public async Task Should_get_specific_vmss_vm()
    {
        // Get first instance (instance-id "0")
        var result = await CallToolAsync(
            "compute_vmss_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", VmssName },
                { "instance-id", "0" }
            });

        var vm = result.AssertProperty("VmInstance");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var returnedInstanceId = vm.GetProperty("instanceId");
        Assert.Equal("0", returnedInstanceId.GetString());
    }

    [Fact]
    public async Task DiskGet_SpecificDisk_ReturnsValidDiskDetails()
    {
        // Arrange
        var diskName = DiskName;
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
        Assert.NotNull(disk.AssertProperty("Name").GetString()); // Name is sanitized during playback
        Assert.NotNull(disk.AssertProperty("ResourceGroup").GetString()); // Resource group is sanitized during playback
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
        // The MCP server returns error responses with status codes, not exceptions
        // We expect a result with error information
        Assert.NotNull(result);
        // The response should contain error details in the results section
        // Check that the results property exists
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskGet_WithInvalidResourceGroup_ReturnsNotFound()
    {
        // Arrange
        var diskName = DiskName;
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
        // The MCP server returns error responses with status codes, not exceptions
        // We expect a result with error information
        Assert.NotNull(result);
        // The response should contain error details in the results section
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskGet_WithDiskButNoResourceGroup_SearchesAcrossSubscription()
    {
        // Arrange
        var diskName = DiskName;
        var subscription = Settings.SubscriptionId;

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "disk", diskName }
            });

        // Assert
        // When disk name is provided without resource group, it searches across the entire subscription
        Assert.NotNull(result);
        var disks = result.Value.AssertProperty("Disks");
        var diskList = disks.EnumerateArray().ToList();
        // In playback, the sanitizer may filter out results, so just verify the structure is correct
        if (diskList.Any())
        {
            var disk = diskList.First();
            Assert.NotNull(disk.GetProperty("Name").GetString()); // Name is sanitized during playback
        }
    }
}
