// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.Compute.LiveTests;

public class ComputeCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    // Use Settings.ResourceBaseName with suffixes (following SQL pattern)
    private string VmName => $"{Settings.ResourceBaseName}-vm";
    private string VmssName => $"{Settings.ResourceBaseName}-vmss";

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
        Assert.Equal("Standard_D2s_v6", vmSize.GetString());

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
}
