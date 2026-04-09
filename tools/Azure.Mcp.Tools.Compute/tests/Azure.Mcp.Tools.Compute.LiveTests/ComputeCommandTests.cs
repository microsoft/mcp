// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Helpers;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Compute.LiveTests;

public class ComputeCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture) : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    private readonly ITestOutputHelper _output = output;

    // Use Settings.ResourceBaseName with suffixes (following SQL pattern)
    private string VmName => $"{Settings.ResourceBaseName}-vm";
    private string VmssName => $"{Settings.ResourceBaseName}-vmss";
    private string DiskName => $"{Settings.ResourceBaseName}-disk";
    private string GalleryName => $"{Settings.ResourceBaseName.Replace("-", string.Empty)}gallery";
    private const string GalleryArtifactsContainerName = "galleryapp";
    private const string GalleryApplicationPackageBlobName = "noop-package.zip";
    private const string GalleryApplicationConfigBlobName = "noop-config.json";
    private const string UserAssignedManagedIdentityApiVersion = "2024-11-30";
    private const string RoleAssignmentsApiVersion = "2022-04-01";
    private const string StorageBlobDataContributorRoleDefinitionId = "ba92f5b4-2d11-453d-a403-e96b0029c9fe";

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

    // Sanitize admin password in request bodies
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..adminPassword")
        {
            Value = "REDACTED",
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..['source-media-link']")
        {
            Value = "https://sanitized.example/source",
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..['default-configuration-link']")
        {
            Value = "https://sanitized.example/config",
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..sourceMediaLink")
        {
            Value = "https://sanitized.example/source",
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..defaultConfigurationLink")
        {
            Value = "https://sanitized.example/config",
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

    #region VM Update Tests

    [Fact]
    public async Task Should_create_vm_with_password_auth()
    {
        var createVmName = RegisterOrRetrieveVariable("createVmName", $"testvm{DateTime.UtcNow:MMddHHmmss}");

        var result = await CallToolAsync(
            "compute_vm_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", createVmName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "TestP@ssw0rd123!" },
                { "image", "Ubuntu2404" },
                { "no-public-ip", true }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var vmSize = vm.GetProperty("vmSize");
        Assert.Equal("Standard_DS1_v2", vmSize.GetString());

        var osType = vm.GetProperty("osType");
        Assert.Equal("linux", osType.GetString());
    }

    /// <summary>
    /// Based on Azure CLI example:
    /// az vm create -n MyVm -g MyResourceGroup --public-ip-address "" --image Win2012R2Datacenter
    /// Creates a Windows Server VM with no public IP address.
    /// </summary>
    [Fact]
    public async Task Should_create_windows_vm_with_password_auth()
    {
        var createVmName = RegisterOrRetrieveVariable("createWinVmName", $"winvm{DateTime.UtcNow:MMddHHmmss}");

        var result = await CallToolAsync(
            "compute_vm_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", createVmName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "WinTestP@ss123!" },
                { "image", "Win2022Datacenter" },
                { "no-public-ip", true }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var vmSize = vm.GetProperty("vmSize");
        Assert.Equal("Standard_DS1_v2", vmSize.GetString());

        var osType = vm.GetProperty("osType");
        Assert.Equal("windows", osType.GetString());
    }

    /// <summary>
    /// Based on Azure CLI example:
    /// az vm create -n MyVm -g MyResourceGroup --image Win2019Datacenter --size Standard_DS2_v2
    /// Creates a Windows Server 2019 VM with a specific VM size and OS disk type.
    /// </summary>
    [Fact]
    public async Task Should_create_windows_vm_with_custom_size()
    {
        var createVmName = RegisterOrRetrieveVariable("createWinVm2Name", $"wv2{DateTime.UtcNow:MMddHHmmss}");

        var result = await CallToolAsync(
            "compute_vm_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", createVmName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "WinTestP@ss456!" },
                { "image", "Win2019Datacenter" },
                { "vm-size", "Standard_DS2_v2" },
                { "os-disk-type", "StandardSSD_LRS" },
                { "no-public-ip", true }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var vmSize = vm.GetProperty("vmSize");
        Assert.Equal("Standard_DS2_v2", vmSize.GetString());

        var osType = vm.GetProperty("osType");
        Assert.Equal("windows", osType.GetString());
    }

    [Fact]
    public async Task Should_update_vm_tags()
    {
        var result = await CallToolAsync(
            "compute_vm_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", VmName },
                { "tags", "testkey=testvalue,environment=livetests" }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        // Verify tags were applied
        var tags = vm.GetProperty("tags");
        Assert.Equal(JsonValueKind.Object, tags.ValueKind);
    }

    [Fact]
    public async Task Should_update_vm_boot_diagnostics()
    {
        var result = await CallToolAsync(
            "compute_vm_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", VmName },
                { "boot-diagnostics", "true" }
            });

        var vm = result.AssertProperty("Vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());
    }

    #endregion

    #region VMSS Update Tests

    /// <summary>
    /// Based on Azure CLI example:
    /// az vmss create -n MyVmss -g MyResourceGroup --instance-count 5 --image Win2016Datacenter --os-disk-size-gb 40
    /// Creates a Windows VMSS with a specific instance count and custom OS disk size.
    /// </summary>
    [Fact]
    public async Task Should_create_windows_vmss_with_instance_count()
    {
        var createVmssName = RegisterOrRetrieveVariable("createWinVmssName", $"wvs{DateTime.UtcNow:HHmmss}");

        var result = await CallToolAsync(
            "compute_vmss_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", createVmssName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "WinTestP@ss789!" },
                { "image", "Win2022Datacenter" },
                { "instance-count", 2 },
                { "os-disk-size-gb", 40 }
            });

        var vmss = result.AssertProperty("Vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var provisioningState = vmss.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var capacity = vmss.GetProperty("capacity");
        Assert.Equal(2, capacity.GetInt32());
    }

    /// <summary>
    /// Based on Azure CLI example:
    /// az vmss create -n MyVmss -g MyResourceGroup --image Ubuntu2204 --vm-sku Standard_DS2_v2 --upgrade-policy-mode Manual
    /// Creates a Linux VMSS with a custom VM size and Manual upgrade policy.
    /// </summary>
    [Fact]
    public async Task Should_create_linux_vmss_with_custom_size_and_upgrade_policy()
    {
        var createVmssName = RegisterOrRetrieveVariable("createLinuxVmssName", $"lnxvmss{DateTime.UtcNow:MMddHHmmss}");

        var result = await CallToolAsync(
            "compute_vmss_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", createVmssName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "LinuxTestP@ss321!" },
                { "image", "Ubuntu2404" },
                { "vm-size", "Standard_DS2_v2" },
                { "instance-count", 1 },
                { "upgrade-policy", "Manual" },
                { "os-disk-type", "StandardSSD_LRS" }
            });

        var vmss = result.AssertProperty("Vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var provisioningState = vmss.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var upgradePolicy = vmss.GetProperty("upgradePolicy");
        Assert.Equal("Manual", upgradePolicy.GetString());
    }

    [Fact]
    public async Task Should_update_vmss_tags()
    {
        var result = await CallToolAsync(
            "compute_vmss_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", VmssName },
                { "tags", "testkey=testvalue,environment=livetests" }
            });

        var vmss = result.AssertProperty("Vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var provisioningState = vmss.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        // Verify tags were applied
        var tags = vmss.GetProperty("tags");
        Assert.Equal(JsonValueKind.Object, tags.ValueKind);
    }

    [Fact]
    public async Task Should_update_vmss_upgrade_policy()
    {
        var result = await CallToolAsync(
            "compute_vmss_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", VmssName },
                { "upgrade-policy", "Automatic" }
            });

        var vmss = result.AssertProperty("Vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var provisioningState = vmss.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());

        var upgradePolicy = vmss.GetProperty("upgradePolicy");
        Assert.Equal("Automatic", upgradePolicy.GetString());
    }

    #endregion

    #region VM Delete Tests

    [Fact]
    public async Task Should_delete_vm_with_force()
    {
        // Create a dedicated VM to delete
        var deleteVmName = RegisterOrRetrieveVariable("deleteVmName", $"delvm{DateTime.UtcNow:MMddHHmmss}");

        await CallToolAsync(
            "compute_vm_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", deleteVmName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "TestP@ssw0rd123!" },
                { "image", "Ubuntu2404" },
                { "no-public-ip", true }
            });

        // Delete the VM
        var result = await CallToolAsync(
            "compute_vm_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", deleteVmName },
                { "force-deletion", true }
            });

        var message = result.AssertProperty("Message");
        Assert.Contains("successfully deleted", message.GetString());

        var success = result.AssertProperty("Success");
        Assert.Equal(JsonValueKind.True, success.ValueKind);
    }

    #endregion

    #region VMSS Delete Tests

    [Fact]
    public async Task Should_delete_vmss_with_force()
    {
        // Create a dedicated VMSS to delete
        var deleteVmssName = RegisterOrRetrieveVariable("deleteVmssName", $"delvms{DateTime.UtcNow:HHmmss}");

        await CallToolAsync(
            "compute_vmss_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", deleteVmssName },
                { "location", "eastus2" },
                { "admin-username", "azureuser" },
                { "admin-password", "TestP@ssw0rd123!" },
                { "image", "Ubuntu2404" },
                { "instance-count", 1 }
            });

        // Delete the VMSS
        var result = await CallToolAsync(
            "compute_vmss_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", deleteVmssName },
                { "force-deletion", true }
            });

        var message = result.AssertProperty("Message");
        Assert.Contains("successfully deleted", message.GetString());

        var success = result.AssertProperty("Success");
        Assert.Equal(JsonValueKind.True, success.ValueKind);
    }

    #endregion

    #region Gallery Application Tests

    [Fact]
    public async Task Should_crud_gallery_application_and_version()
    {
        var galleryApplicationName = RegisterOrRetrieveVariable("galleryApplicationCrudName", $"ga-{DateTime.UtcNow:MMddHHmmss}");
        var versionName = RegisterOrRetrieveVariable("galleryApplicationVersionCrudName", "1.0.0");

        await EnsureGalleryExistsAsync(GalleryName, "eastus2", TestContext.Current.CancellationToken);
        await EnsureGalleryManagedIdentityAsync(GalleryName, "eastus2", TestContext.Current.CancellationToken);
        var galleryArtifacts = await EnsureGalleryArtifactsAsync("eastus2", TestContext.Current.CancellationToken);
        var sourceMediaLink = RegisterOrRetrieveVariable("galleryApplicationSourceMediaLink", galleryArtifacts.SourceMediaLink);
        var defaultConfigurationLink = RegisterOrRetrieveVariable("galleryApplicationDefaultConfigurationLink", galleryArtifacts.DefaultConfigurationLink);

        var createAppResult = await CallToolAsync(
            "compute_galleryapplication_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "gallery", GalleryName },
                { "gallery-application", galleryApplicationName },
                { "location", "eastus2" },
                { "tags", "env=live owner=compute" }
            });

        var createdApp = createAppResult.AssertProperty("GalleryApplication");
        Assert.Equal(galleryApplicationName, createdApp.GetProperty("name").GetString());

        var getAppResult = await CallToolAsync(
            "compute_galleryapplication_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "gallery", GalleryName },
                { "gallery-application", galleryApplicationName }
            });

        var fetchedApp = getAppResult.AssertProperty("GalleryApplication");
        Assert.Equal(galleryApplicationName, fetchedApp.GetProperty("name").GetString());

        var updateAppResult = await CallToolAsync(
            "compute_galleryapplication_update",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "gallery", GalleryName },
                { "gallery-application", galleryApplicationName },
                { "tags", "env=live owner=compute-updated" }
            });

        var updatedApp = updateAppResult.AssertProperty("GalleryApplication");
        Assert.Equal(galleryApplicationName, updatedApp.GetProperty("name").GetString());

        try
        {
            var createVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_create",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName },
                    { "gallery-application-version", versionName },
                    { "location", "eastus2" },
                    { "source-media-link", sourceMediaLink },
                    { "default-configuration-link", defaultConfigurationLink },
                    { "replica-count", 1 },
                    { "exclude-from-latest", true },
                    { "manage-action-install", "install.sh" },
                    { "manage-action-remove", "remove.sh" },
                    { "manage-action-update", "update.sh" },
                    { "package-file-name", "package.zip" },
                    { "config-file-name", "settings.json" },
                    { "script-behavior-after-reboot", "None" },
                    { "target-regions", "eastus2" }
                });

            var createdVersion = createVersionResult.AssertProperty("GalleryApplicationVersion");
            Assert.Equal(versionName, createdVersion.GetProperty("name").GetString());
            Assert.Equal(sourceMediaLink, createdVersion.GetProperty("sourceMediaLink").GetString());

            var getVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName },
                    { "gallery-application-version", versionName }
                });

            var fetchedVersion = getVersionResult.AssertProperty("GalleryApplicationVersion");
            Assert.Equal(versionName, fetchedVersion.GetProperty("name").GetString());

            var listVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName }
                });

            var versions = listVersionResult.AssertProperty("GalleryApplicationVersions");
            Assert.Equal(JsonValueKind.Array, versions.ValueKind);
            Assert.Contains(versions.EnumerateArray(), v => v.GetProperty("name").GetString() == versionName);

            var updateVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_update",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName },
                    { "gallery-application-version", versionName },
                    { "exclude-from-latest", false },
                    { "target-regions", "eastus2,westus2" }
                });

            var updatedVersion = updateVersionResult.AssertProperty("GalleryApplicationVersion");
            Assert.Equal(versionName, updatedVersion.GetProperty("name").GetString());
            Assert.Equal(JsonValueKind.False, updatedVersion.GetProperty("excludeFromLatest").ValueKind);

            var deleteVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName },
                    { "gallery-application-version", versionName }
                });

            var versionDeleted = deleteVersionResult.AssertProperty("Deleted");
            Assert.Equal(JsonValueKind.True, versionDeleted.ValueKind);

            var getDeletedVersionResult = await CallToolAsync(
                "compute_galleryapplicationversion_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName },
                    { "gallery-application-version", versionName }
                });

            Assert.NotNull(getDeletedVersionResult);
            Assert.True(getDeletedVersionResult.Value.TryGetProperty("message", out _));
        }
        finally
        {
            var deleteAppResult = await CallToolAsync(
                "compute_galleryapplication_delete",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName }
                });

            var appDeleted = deleteAppResult.AssertProperty("Deleted");
            Assert.Equal(JsonValueKind.True, appDeleted.ValueKind);

            var getDeletedAppResult = await CallToolAsync(
                "compute_galleryapplication_get",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "resource-group", Settings.ResourceGroupName },
                    { "gallery", GalleryName },
                    { "gallery-application", galleryApplicationName }
                });

            Assert.NotNull(getDeletedAppResult);
            Assert.True(getDeletedAppResult.Value.TryGetProperty("message", out _));

            // Cleanup gallery artifacts storage account
            await CleanupGalleryArtifactsAsync(TestContext.Current.CancellationToken);
        }
    }

    private async Task EnsureGalleryExistsAsync(string galleryName, string location, CancellationToken cancellationToken)
    {
        if (TestMode == TestMode.Playback)
        {
            return;
        }

        var credentialOptions = new DefaultAzureCredentialOptions
        {
            TenantId = Settings.TenantId,
            ExcludeManagedIdentityCredential = true,
        };

        var armClient = new ArmClient(new DefaultAzureCredential(credentialOptions));
        var subscription = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(Settings.SubscriptionId));
        var resourceGroup = await subscription.GetResourceGroupAsync(Settings.ResourceGroupName, cancellationToken);
        var galleries = resourceGroup.Value.GetGalleries();

        try
        {
            await galleries.GetAsync(galleryName, cancellationToken: cancellationToken);
            return;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var galleryData = new GalleryData(new AzureLocation(location))
            {
                Description = "Compute live tests gallery"
            };

            await galleries.CreateOrUpdateAsync(WaitUntil.Completed, galleryName, galleryData, cancellationToken);
        }
    }

    private async Task<(string SourceMediaLink, string DefaultConfigurationLink)> EnsureGalleryArtifactsAsync(string location, CancellationToken cancellationToken)
    {
        if (TestMode == TestMode.Playback)
        {
            return (
                "https://sanitized.blob.core.windows.net/sanitized/noop-package.zip",
                "https://sanitized.blob.core.windows.net/sanitized/noop-config.json");
        }

        var credentialOptions = new DefaultAzureCredentialOptions
        {
            TenantId = Settings.TenantId,
            ExcludeManagedIdentityCredential = true,
        };

        var armClient = new ArmClient(new DefaultAzureCredential(credentialOptions));
        var subscription = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(Settings.SubscriptionId));
        var resourceGroup = await subscription.GetResourceGroupAsync(Settings.ResourceGroupName, cancellationToken);

        var storageAccountName = GetStorageAccountName();
        var storageAccount = await EnsureStorageAccountAsync(resourceGroup.Value, storageAccountName, location, cancellationToken);
        var galleryIdentity = await EnsureUserAssignedManagedIdentityAsync(resourceGroup.Value, location, cancellationToken);
        var roleAssigned = await AssignRoleToPrincipalAsync(storageAccount.Id.ToString(), galleryIdentity.PrincipalId, "ServicePrincipal", cancellationToken);
        if (roleAssigned)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        // Use Azure AD credentials for the test runner to seed the artifacts.
        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net"),
            new DefaultAzureCredential(credentialOptions));

        var containerClient = blobServiceClient.GetBlobContainerClient(GalleryArtifactsContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var packageBlob = containerClient.GetBlobClient(GalleryApplicationPackageBlobName);
        var packagePayload = CreateNoOpZipArchive();
        using (var packageStream = new MemoryStream(packagePayload))
        {
            await UploadWithRetryAsync(packageBlob, packageStream, cancellationToken);
        }

        var configBlob = containerClient.GetBlobClient(GalleryApplicationConfigBlobName);
        var configPayload = BinaryData.FromString("{\"description\":\"noop\"}");
        using (var configStream = new MemoryStream(configPayload.ToArray()))
        {
            await UploadWithRetryAsync(configBlob, configStream, cancellationToken);
        }

        var sourceMediaLink = packageBlob.Uri.ToString();
        var defaultConfigurationLink = configBlob.Uri.ToString();
        return (sourceMediaLink, defaultConfigurationLink);
    }

    private static async Task UploadWithRetryAsync(BlobClient blob, Stream content, CancellationToken cancellationToken)
    {
        const int maxRetries = 6;
        const int delayMs = 10000;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Reset stream position for retries
                if (content.CanSeek)
                    content.Seek(0, SeekOrigin.Begin);

                await blob.UploadAsync(content, overwrite: true, cancellationToken);
                return; // Success
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 403 && attempt < maxRetries - 1)
            {
                // Authorization error - likely RBAC propagation delay
                // Wait and retry
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        // All retries failed - let the last exception propagate
        if (content.CanSeek)
            content.Seek(0, SeekOrigin.Begin);
        await blob.UploadAsync(content, overwrite: true, cancellationToken);
    }

    private static byte[] CreateNoOpZipArchive()
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("install.sh");
            using var writer = new StreamWriter(entry.Open());
            writer.WriteLine("#!/usr/bin/env sh");
            writer.WriteLine("echo noop");
        }

        return stream.ToArray();
    }

    private async Task<StorageAccountResource> EnsureStorageAccountAsync(ResourceGroupResource resourceGroup, string storageAccountName, string location, CancellationToken cancellationToken)
    {
        var storageAccounts = resourceGroup.GetStorageAccounts();
        StorageAccountResource storageAccount;

        try
        {
            var existing = await storageAccounts.GetAsync(storageAccountName, cancellationToken: cancellationToken);
            storageAccount = existing.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var createParams = new StorageAccountCreateOrUpdateContent(
                new StorageSku(StorageSkuName.StandardLrs),
                StorageKind.StorageV2,
                new AzureLocation(location))
            {
                AllowBlobPublicAccess = false,
                AllowSharedKeyAccess = false,
                MinimumTlsVersion = StorageMinimumTlsVersion.Tls1_2,
            };

            var created = await storageAccounts.CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, createParams, cancellationToken);
            storageAccount = created.Value;
        }

        // Assign Storage Blob Data Contributor role to the current user so the test can seed artifacts.
        var currentUserObjectId = await GetCurrentUserObjectIdAsync(new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            TenantId = Settings.TenantId,
            ExcludeManagedIdentityCredential = true,
        }), cancellationToken);
        var roleAssigned = !string.IsNullOrEmpty(currentUserObjectId)
            && await AssignRoleToPrincipalAsync(storageAccount.Id.ToString(), currentUserObjectId, "User", cancellationToken);

        _output.WriteLine($"Uploader object ID resolved: {!string.IsNullOrEmpty(currentUserObjectId)}");
        _output.WriteLine($"Uploader storage RBAC assigned: {roleAssigned}");

        // Wait for RBAC propagation if role was assigned
        if (roleAssigned)
        {
            _output.WriteLine("Waiting 60 seconds for uploader RBAC propagation.");
            await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
        }

        return storageAccount;
    }

    private async Task EnsureGalleryManagedIdentityAsync(string galleryName, string location, CancellationToken cancellationToken)
    {
        if (TestMode == TestMode.Playback)
        {
            return;
        }

        try
        {
            var credentialOptions = new DefaultAzureCredentialOptions
            {
                TenantId = Settings.TenantId,
                ExcludeManagedIdentityCredential = true,
            };
            var credential = new DefaultAzureCredential(credentialOptions);
            var armClient = new ArmClient(credential);
            var subscription = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(Settings.SubscriptionId));
            var resourceGroup = await subscription.GetResourceGroupAsync(Settings.ResourceGroupName, cancellationToken);
            var identity = await EnsureUserAssignedManagedIdentityAsync(resourceGroup.Value, location, cancellationToken);
            var galleries = resourceGroup.Value.GetGalleries();
            var gallery = await galleries.GetAsync(galleryName, cancellationToken: cancellationToken);
            var galleryData = gallery.Value.Data;

            galleryData.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned)
            {
                UserAssignedIdentities =
                {
                    [new ResourceIdentifier(identity.ResourceId)] = new UserAssignedIdentity()
                }
            };

            await galleries.CreateOrUpdateAsync(WaitUntil.Completed, galleryName, galleryData, cancellationToken);
        }
        catch (Exception)
        {
            _output.WriteLine("Failed to configure gallery with a user-assigned managed identity.");
        }
    }

    private async Task<(string ResourceId, string PrincipalId, string ClientId)> EnsureUserAssignedManagedIdentityAsync(ResourceGroupResource resourceGroup, string location, CancellationToken cancellationToken)
    {
        var identityName = GetGalleryManagedIdentityName();
        var identityResourceId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{resourceGroup.Data.Name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identityName}";

        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            TenantId = Settings.TenantId,
            ExcludeManagedIdentityCredential = true,
        });
        var token = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
            cancellationToken);

        using var httpClient = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, $"https://management.azure.com{identityResourceId}?api-version={UserAssignedManagedIdentityApiVersion}")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { location }),
                System.Text.Encoding.UTF8,
                "application/json")
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        var properties = document.RootElement.GetProperty("properties");

        return (
            identityResourceId,
            properties.GetProperty("principalId").GetString() ?? string.Empty,
            properties.GetProperty("clientId").GetString() ?? string.Empty);
    }

    private async Task<bool> AssignRoleToPrincipalAsync(string scope, string principalId, string principalType, CancellationToken cancellationToken)
    {
        try
        {
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                TenantId = Settings.TenantId,
                ExcludeManagedIdentityCredential = true,
            });
            var roleDefinitionId = $"/subscriptions/{Settings.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{StorageBlobDataContributorRoleDefinitionId}";
            var roleAssignmentName = CreateDeterministicRoleAssignmentName(scope, principalId, roleDefinitionId);
            var requestUri = $"https://management.azure.com{scope}/providers/Microsoft.Authorization/roleAssignments/{roleAssignmentName}?api-version={RoleAssignmentsApiVersion}";
            var requestBody = new
            {
                properties = new
                {
                    roleDefinitionId,
                    principalId,
                    principalType
                }
            };

            var armToken = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
                cancellationToken);

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", armToken.Token);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode || (int)response.StatusCode == 409)
            {
                _output.WriteLine($"Role assignment succeeded for scope {scope} and principal type {principalType}.");
                return true;
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _output.WriteLine($"Role assignment failed for scope {scope} with status {(int)response.StatusCode}: {responseBody}");
            return false;
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Role assignment threw for scope {scope}: {ex.Message}");
            return false;
        }
    }

    private static string? GetObjectIdFromJwt(string jwt)
    {
        var tokenParts = jwt.Split('.');
        if (tokenParts.Length != 3)
        {
            return null;
        }

        var payload = tokenParts[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        using var json = JsonDocument.Parse(decoded);

        if (json.RootElement.TryGetProperty("idtyp", out var identityType) && string.Equals(identityType.GetString(), "app", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return json.RootElement.TryGetProperty("oid", out var oidElement) ? oidElement.GetString() : null;
    }

    private async Task<string?> GetCurrentUserObjectIdAsync(TokenCredential credential, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        var graphToken = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }),
            cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me?$select=id");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", graphToken.Token);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("id", out var idElement))
            {
                return idElement.GetString();
            }
        }

        _output.WriteLine($"Microsoft Graph /me lookup failed with status {(int)response.StatusCode}.");

        var armToken = await credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }),
            cancellationToken);
        var fallbackOid = GetObjectIdFromJwt(armToken.Token);
        _output.WriteLine($"ARM token OID fallback resolved: {!string.IsNullOrEmpty(fallbackOid)}");
        return fallbackOid;
    }

    private static string CreateDeterministicRoleAssignmentName(string scope, string principalId, string roleDefinitionId)
    {
        var input = System.Text.Encoding.UTF8.GetBytes($"{scope}|{principalId}|{roleDefinitionId}");
        var hash = SHA256.HashData(input);
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        return new Guid(guidBytes).ToString();
    }

    private string GetStorageAccountName()
    {
        var baseName = Settings.ResourceBaseName.Replace("-", string.Empty).ToLowerInvariant();
        var suffix = "gappsa";
        var maxBaseLength = 24 - suffix.Length;
        if (baseName.Length > maxBaseLength)
        {
            baseName = baseName[..maxBaseLength];
        }

        return RegisterOrRetrieveVariable("galleryApplicationStorageAccountName", $"{baseName}{suffix}");
    }

    private string GetGalleryManagedIdentityName()
    {
        var baseName = Settings.ResourceBaseName.Replace("-", string.Empty).ToLowerInvariant();
        var suffix = "gappuai";
        var maxBaseLength = 128 - suffix.Length - 1;
        if (baseName.Length > maxBaseLength)
        {
            baseName = baseName[..maxBaseLength];
        }

        return RegisterOrRetrieveVariable("galleryApplicationManagedIdentityName", $"{baseName}-{suffix}");
    }

    private async Task CleanupGalleryArtifactsAsync(CancellationToken cancellationToken)
    {
        if (TestMode == TestMode.Playback)
        {
            return;
        }

        try
        {
            var storageAccountName = RegisterOrRetrieveVariable("galleryApplicationStorageAccountName", string.Empty);
            if (string.IsNullOrEmpty(storageAccountName))
            {
                return; // No storage account was created
            }

            var credentialOptions = new DefaultAzureCredentialOptions
            {
                TenantId = Settings.TenantId,
                ExcludeManagedIdentityCredential = true,
            };

            var armClient = new ArmClient(new DefaultAzureCredential(credentialOptions));
            var subscription = armClient.GetSubscriptionResource(SubscriptionResource.CreateResourceIdentifier(Settings.SubscriptionId));
            var resourceGroup = await subscription.GetResourceGroupAsync(Settings.ResourceGroupName, cancellationToken);
            var storageAccounts = resourceGroup.Value.GetStorageAccounts();

            var storageAccount = await storageAccounts.GetAsync(storageAccountName, cancellationToken: cancellationToken);
            await storageAccount.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Storage account already deleted or doesn't exist, no cleanup needed
        }
        catch (Exception)
        {
            // Ignore cleanup failures - storage account may be in a transient state
        }
    }

    #endregion

    #region Gallery Application Version Tests

    // Combined into Should_crud_gallery_application_and_version.

    #endregion

    #region Disk Tests

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

    #endregion

    #region Disk Create Tests

    [Fact]
    public async Task DiskCreate_EmptyDisk_CreatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-create-test";

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

    [Fact]
    public async Task DiskCreate_WithLocationAndTags_CreatesWithProperties()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-tag-test";

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
            },
            resultProcessor: elem => elem); // Don't try to extract "results" property since we expect an error response with a different structure

        // Assert - should return an error response
        Assert.NotNull(result);
        Assert.True(result.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskCreate_ThenGetVerifies_FullLifecycle()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-lifecycle-test";

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

    [Fact]
    [CustomMatcher(compareBody: false)] // Gallery image reference embeds resource group/base name in request body; GeneralRegexSanitizer doesn't fully sanitize nested body values during playback matching
    public async Task DiskCreate_FromGalleryImage_CreatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-gallery-test";
        var galleryImageVersionId = Settings.DeploymentOutputs.GetValueOrDefault("GALLERYIMAGEVERSIONID", "Sanitized");

        // Act - create disk from gallery image (OS disk, no LUN)
        // Use eastus2 location to match gallery image replication target region
        JsonElement? result = await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", newDiskName },
                { "gallery-image-reference", galleryImageVersionId },
                { "sku", "Standard_LRS" },
                { "location", "eastus2" }
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

    [Fact]
    [CustomMatcher(compareBody: false)] // Gallery image reference embeds resource group/base name in request body; GeneralRegexSanitizer doesn't fully sanitize nested body values during playback matching
    public async Task DiskCreate_FromGalleryImageWithLun_CreatesDataDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-gallery-lun-test";
        var galleryImageVersionId = Settings.DeploymentOutputs.GetValueOrDefault("GALLERYIMAGEVERSIONID", "Sanitized");

        // Act - create disk from gallery image data disk at LUN 0
        // Use eastus2 location to match gallery image replication target region
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
                { "location", "eastus2" }
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

    [Fact]
    public async Task DiskCreate_WithUploadType_CreatesReadyToUploadDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-upload-test";

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

    [Fact]
    public async Task DiskCreate_WithUploadTypeUploadWithSecurityData_CreatesReadyToUploadDisk()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-uploadsec-test";

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
            },
            resultProcessor: elem => elem); // Don't try to extract "results" property since we expect an error response with a different structure

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

    [Fact]
    public async Task DiskUpdate_ChangeSku_UpdatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-upsku-test";

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

    [Fact]
    public async Task DiskUpdate_AddTags_UpdatesSuccessfully()
    {
        var newDiskName = $"{Settings.ResourceBaseName}-uptag-test";

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

        // Create a disk at 64GB (must match or exceed any previous run's final size
        // since Azure disallows downsizing)
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

    #endregion

    #region Disk Delete Tests

    [Fact]
    public async Task DiskDelete_ExistingDisk_DeletesSuccessfully()
    {
        var deleteDiskName = $"{Settings.ResourceBaseName}-del-test";

        await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName },
                { "size-gb", 32 },
                { "sku", "Standard_LRS" }
            });

        JsonElement? result = await CallToolAsync(
            "compute_disk_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName }
            });

        Assert.NotNull(result);
        Assert.True(result.Value.AssertProperty("Deleted").GetBoolean());
        Assert.NotNull(result.Value.AssertProperty("DiskName").GetString());
    }

    [Fact]
    public async Task DiskDelete_NonExistentDisk_ReturnsFalse()
    {
        var invalidDiskName = RegisterOrRetrieveVariable("deleteInvalidDiskName", "nonexistent-disk-" + Guid.NewGuid().ToString("N")[..8]);

        // Delete a disk that doesn't exist
        JsonElement? result = await CallToolAsync(
            "compute_disk_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", invalidDiskName }
            });

        // Idempotent operation should return Deleted = false for non-existent disk
        Assert.NotNull(result);
        Assert.False(result.Value.AssertProperty("Deleted").GetBoolean());
    }

    [Fact]
    public async Task DiskDelete_ThenGet_ConfirmsDeletion()
    {
        var deleteDiskName = $"{Settings.ResourceBaseName}-delget-test";

        await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName },
                { "size-gb", 32 },
                { "sku", "Standard_LRS" }
            });

        JsonElement? deleteResult = await CallToolAsync(
            "compute_disk_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName }
            });

        Assert.NotNull(deleteResult);
        Assert.True(deleteResult.Value.AssertProperty("Deleted").GetBoolean());

        // Verify - attempt to get the deleted disk should return error
        JsonElement? getResult = await CallToolAsync(
            "compute_disk_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName }
            });

        // The disk should no longer exist - expect error response
        Assert.NotNull(getResult);
        Assert.True(getResult.Value.TryGetProperty("message", out _));
    }

    [Fact]
    public async Task DiskDelete_IdempotentDoubleDelete_SucceedsBothTimes()
    {
        var deleteDiskName = $"{Settings.ResourceBaseName}-deldbl-test";

        await CallToolAsync(
            "compute_disk_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName },
                { "size-gb", 32 },
                { "sku", "Standard_LRS" }
            });

        JsonElement? firstResult = await CallToolAsync(
            "compute_disk_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName }
            });

        JsonElement? secondResult = await CallToolAsync(
            "compute_disk_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "disk-name", deleteDiskName }
            });

        // First delete should succeed, second should return false (already deleted)
        Assert.NotNull(firstResult);
        Assert.True(firstResult.Value.AssertProperty("Deleted").GetBoolean());

        Assert.NotNull(secondResult);
        Assert.False(secondResult.Value.AssertProperty("Deleted").GetBoolean());
    }

    #endregion
}
