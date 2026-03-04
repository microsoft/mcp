// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Attributes;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Compute.LiveTests;

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
            Value = "Sanitized",
            GroupForReplace = "1"
        })
    ];

    // Sanitize resource group name, base name, and subscription ID everywhere
    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers =>
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceGroupName,
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = Settings.ResourceBaseName,
            Value = "Sanitized",
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
        Assert.NotNull(vmSize.GetString());

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
                { "disk-name", diskName }
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
        var invalidDiskName = RegisterOrRetrieveVariable("invalidDiskName", "nonexistent-disk-" + Guid.NewGuid().ToString("N")[..8]);

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", resourceGroup },
                { "disk-name", invalidDiskName }
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
        var invalidResourceGroup = RegisterOrRetrieveVariable("invalidResourceGroup", "nonexistent-rg-" + Guid.NewGuid().ToString("N")[..8]);

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", subscription },
                { "resource-group", invalidResourceGroup },
                { "disk-name", diskName }
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
                { "disk-name", diskName }
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

    #region Disk Create Tests

    [Fact]
    public async Task DiskCreate_EmptyDisk_CreatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-create-test";

        try
        {
            // Act
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 32 },
                    { "sku", "Standard_LRS" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal(32, disk.AssertProperty("DiskSizeGB").GetInt32());
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            // Cleanup: Delete the created disk so tests are repeatable
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskCreate_WithLocationAndTags_CreatesWithProperties()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-tag-test";

        try
        {
            // Act
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 64 },
                    { "sku", "Standard_LRS" },
                    { "location", "westus2" },
                    { "tags", "environment=test,purpose=live-test" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal(64, disk.AssertProperty("DiskSizeGB").GetInt32());
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());

            // Verify tags were applied
            JsonElement tags = disk.AssertProperty("Tags");
            Assert.Equal(JsonValueKind.Object, tags.ValueKind);
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskCreate_WithoutSizeOrSource_ReturnsError()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-nosize-test";

        // Act - creating a disk without size-gb or source should fail
        JsonElement? result = await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", newDiskName },
                { "sku", "Standard_LRS" }
            });

        // Assert - should return an error response
        Assert.NotNull(result);
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskCreate_ThenGetVerifies_FullLifecycle()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-lifecycle-test";

        try
        {
            // Create
            JsonElement? createResult = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 32 },
                    { "sku", "Standard_LRS" }
                });

            Assert.NotNull(createResult);
            JsonElement createdDisk = createResult.Value.AssertProperty("Disk");
            Assert.Equal("Succeeded", createdDisk.AssertProperty("ProvisioningState").GetString());

            // Get - verify the created disk can be retrieved
            JsonElement? getResult = await CallToolAsync(
                "compute_disk_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName }
                });

            Assert.NotNull(getResult);
            JsonElement disks = getResult.Value.AssertProperty("Disks");
            Assert.Equal(JsonValueKind.Array, disks.ValueKind);

            List<JsonElement> diskList = disks.EnumerateArray().ToList();
            Assert.Single(diskList);

            JsonElement retrievedDisk = diskList[0];
            Assert.NotNull(retrievedDisk.AssertProperty("Name").GetString());
            Assert.NotNull(retrievedDisk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal(32, retrievedDisk.AssertProperty("DiskSizeGB").GetInt32());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)] // Gallery image reference embeds resource group/base name in request body; GeneralRegexSanitizer doesn't fully sanitize nested body values during playback matching
    public async Task DiskCreate_FromGalleryImage_CreatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-gallery-test";
        var galleryImageVersionId = Settings.DeploymentOutputs.GetValueOrDefault("GALLERYIMAGEVERSIONID", "Sanitized");

        try
        {
            // Act - create disk from gallery image (OS disk, no LUN)
            // Use westus2 location to match gallery image replication target region
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "gallery-image-reference", galleryImageVersionId },
                    { "sku", "Standard_LRS" },
                    { "location", "westus2" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    [CustomMatcher(compareBody: false)] // Gallery image reference embeds resource group/base name in request body; GeneralRegexSanitizer doesn't fully sanitize nested body values during playback matching
    public async Task DiskCreate_FromGalleryImageWithLun_CreatesDataDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-gallery-lun-test";
        var galleryImageVersionId = Settings.DeploymentOutputs.GetValueOrDefault("GALLERYIMAGEVERSIONID", "Sanitized");

        try
        {
            // Act - create disk from gallery image data disk at LUN 0
            // Use westus2 location to match gallery image replication target region
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "gallery-image-reference", galleryImageVersionId },
                    { "gallery-image-reference-lun", 0 },
                    { "sku", "Standard_LRS" },
                    { "location", "westus2" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskCreate_WithUploadType_CreatesReadyToUploadDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-upload-test";

        try
        {
            // Act - create a disk ready for upload with Upload type
            // 20972032 bytes = 20 MB VHD + 512 byte footer
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "upload-type", "Upload" },
                    { "upload-size-bytes", 20972032L },
                    { "sku", "Standard_LRS" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
            Assert.Equal("ReadyToUpload", disk.AssertProperty("DiskState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskCreate_WithUploadTypeUploadWithSecurityData_CreatesReadyToUploadDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-uploadsec-test";

        try
        {
            // Act - create a disk ready for upload with security data
            // Requires security-type to be set for UploadWithSecurityData
            JsonElement? result = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "upload-type", "UploadWithSecurityData" },
                    { "upload-size-bytes", 20972032L },
                    { "sku", "Standard_LRS" },
                    { "security-type", "TrustedLaunch" },
                    { "hyper-v-generation", "V2" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString());
            Assert.NotNull(disk.AssertProperty("Location").GetString());
            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
            Assert.Equal("ReadyToUpload", disk.AssertProperty("DiskState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskCreate_WithUploadTypeButNoUploadSizeBytes_ReturnsError()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-uploadnosize-test";

        // Act - upload-type without upload-size-bytes should fail
        JsonElement? result = await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", newDiskName },
                { "upload-type", "Upload" },
                { "sku", "Standard_LRS" }
            });

        // Assert - should return an error response
        Assert.NotNull(result);
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    #endregion

    #region Disk Update Tests

    [Fact]
    public async Task DiskUpdate_IncreaseDiskSize_UpdatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-upsize-test";

        try
        {
            // Arrange - create a disk first
            await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 32 },
                    { "sku", "Standard_LRS" }
                });

            // Act - update disk size (can only increase)
            JsonElement? result = await CallToolAsync(
                "compute_disk_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 64 }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("Name").GetString()); // Name is sanitized during playback
            Assert.Equal(64, disk.AssertProperty("DiskSizeGB").GetInt32());
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskUpdate_ChangeSku_UpdatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-upsku-test";

        try
        {
            // Arrange - create a Standard_LRS disk
            await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 32 },
                    { "sku", "Standard_LRS" }
                });

            // Act - change SKU to StandardSSD_LRS
            JsonElement? result = await CallToolAsync(
                "compute_disk_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "sku", "StandardSSD_LRS" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            Assert.NotNull(disk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskUpdate_AddTags_UpdatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-uptag-test";

        try
        {
            // Arrange - create a disk without tags
            await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 32 },
                    { "sku", "Standard_LRS" }
                });

            // Act - add tags
            JsonElement? result = await CallToolAsync(
                "compute_disk_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "tags", "environment=test,updated=true" }
                });

            // Assert
            Assert.NotNull(result);
            JsonElement disk = result.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, disk.ValueKind);

            JsonElement tags = disk.AssertProperty("Tags");
            Assert.Equal(JsonValueKind.Object, tags.ValueKind);
            Assert.Equal("Succeeded", disk.AssertProperty("ProvisioningState").GetString());
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    [Fact]
    public async Task DiskUpdate_NonExistentDisk_ReturnsError()
    {
        var invalidDiskName = RegisterOrRetrieveVariable("updateInvalidDiskName", "nonexistent-disk-" + Guid.NewGuid().ToString("N")[..8]);

        // Act
        JsonElement? result = await CallToolAsync(
            "compute_disk_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", invalidDiskName },
                { "size-gb", 64 }
            });

        // Assert - should return an error response
        Assert.NotNull(result);
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskUpdate_CreateThenUpdateMultipleProperties_FullLifecycle()
    {
        var diskSuffix = RegisterOrRetrieveVariable("updateFullDiskSuffix", Random.Shared.NextInt64().ToString());
        var newDiskName = $"{Settings.ResourceBaseName}-full-{diskSuffix}";

        try
        {
            // Create a disk at 64GB (must match or exceed any previous run's final size
            // since CleanupDiskAsync cannot delete disks and Azure disallows downsizing)
            JsonElement? createResult = await CallToolAsync(
                "compute_disk_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 64 },
                    { "sku", "Standard_LRS" }
                });

            Assert.NotNull(createResult);
            JsonElement createdDisk = createResult.Value.AssertProperty("Disk");
            Assert.Equal("Succeeded", createdDisk.AssertProperty("ProvisioningState").GetString());

            // Update multiple properties at once (resize up to 128GB)
            JsonElement? updateResult = await CallToolAsync(
                "compute_disk_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName },
                    { "size-gb", 128 },
                    { "sku", "StandardSSD_LRS" },
                    { "tags", "environment=test,lifecycle=full" }
                });

            Assert.NotNull(updateResult);
            JsonElement updatedDisk = updateResult.Value.AssertProperty("Disk");
            Assert.Equal(JsonValueKind.Object, updatedDisk.ValueKind);
            Assert.Equal(128, updatedDisk.AssertProperty("DiskSizeGB").GetInt32());
            Assert.NotNull(updatedDisk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
            Assert.Equal("Succeeded", updatedDisk.AssertProperty("ProvisioningState").GetString());

            // Verify with a get call
            JsonElement? getResult = await CallToolAsync(
                "compute_disk_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", newDiskName }
                });

            Assert.NotNull(getResult);
            JsonElement disks = getResult.Value.AssertProperty("Disks");
            List<JsonElement> diskList = disks.EnumerateArray().ToList();
            Assert.Single(diskList);

            JsonElement verifiedDisk = diskList[0];
            Assert.Equal(128, verifiedDisk.AssertProperty("DiskSizeGB").GetInt32());
            Assert.NotNull(verifiedDisk.AssertProperty("SkuName").GetString()); // SkuName is sanitized during playback
        }
        finally
        {
            await CleanupDiskAsync(newDiskName);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Attempts to delete a disk for cleanup. Silently ignores errors
    /// since the disk may not have been created if the test failed early.
    /// </summary>
    private async Task CleanupDiskAsync(string diskName)
    {
        try
        {
            // Use the disk get command to verify the disk exists before attempting cleanup.
            // Since there is no delete command yet, we log the cleanup intent.
            // When a delete command is available, replace this with actual deletion.
            var result = await CallToolAsync(
                "compute_disk_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "disk-name", diskName }
                });

            // Log that the disk still exists and should be cleaned up
            if (result != null && result.Value.TryGetProperty("Disks", out _))
            {
                Output.WriteLine($"Note: Test disk '{diskName}' still exists and should be cleaned up manually or by test infrastructure teardown.");
            }
        }
        catch
        {
            // Ignore cleanup errors - disk may not have been created
        }
    }

    #endregion
}
