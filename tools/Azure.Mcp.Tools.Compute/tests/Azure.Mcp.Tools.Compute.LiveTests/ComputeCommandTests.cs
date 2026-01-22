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
    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..vmId")
        {
            Value = "Sanitized"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..id")
        {
            Value = "Sanitized"
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

        var vms = result.AssertProperty("vms");
        Assert.Equal(JsonValueKind.Array, vms.ValueKind);
        Assert.NotEmpty(vms.EnumerateArray());

        // Verify we have at least the test VMs
        var vmNames = vms.EnumerateArray()
            .Select(vm => vm.GetProperty("name").GetString())
            .ToList();

        Assert.Contains(Settings.DeploymentOutputs["vmName"], vmNames);
        Assert.Contains(Settings.DeploymentOutputs["vm2Name"], vmNames);
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

        var vms = result.AssertProperty("vms");
        Assert.Equal(JsonValueKind.Array, vms.ValueKind);

        var vmArray = vms.EnumerateArray().ToList();
        Assert.Equal(2, vmArray.Count); // Should have exactly 2 VMs in the test resource group

        var vmNames = vmArray.Select(vm => vm.GetProperty("name").GetString()).ToList();
        Assert.Contains(Settings.DeploymentOutputs["vmName"], vmNames);
        Assert.Contains(Settings.DeploymentOutputs["vm2Name"], vmNames);
    }

    [Fact]
    public async Task Should_get_specific_vm_details()
    {
        var vmName = Settings.DeploymentOutputs["vmName"];

        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", vmName }
            });

        var vm = result.AssertProperty("vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var name = vm.GetProperty("name");
        Assert.Equal(vmName, name.GetString());

        var location = vm.GetProperty("location");
        Assert.NotNull(location.GetString());

        var vmSize = vm.GetProperty("vmSize");
        Assert.Equal("Standard_B1s", vmSize.GetString());

        var osType = vm.GetProperty("osType");
        Assert.Equal("Linux", osType.GetString());

        var provisioningState = vm.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());
    }

    [Fact]
    public async Task Should_get_vm_with_instance_view()
    {
        var vmName = Settings.DeploymentOutputs["vmName"];

        var result = await CallToolAsync(
            "compute_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vm-name", vmName },
                { "instance-view", true }
            });

        var vm = result.AssertProperty("vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var name = vm.GetProperty("name");
        Assert.Equal(vmName, name.GetString());

        // Verify instance view is present
        var instanceView = result.AssertProperty("instanceView");
        Assert.Equal(JsonValueKind.Object, instanceView.ValueKind);

        // Check for power state
        var powerState = instanceView.GetProperty("powerState");
        Assert.NotNull(powerState.GetString());
        // Should be "running" or similar VM state

        // Check for provisioning state
        var provisioningState = instanceView.GetProperty("provisioningState");
        Assert.Equal("Succeeded", provisioningState.GetString());
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

        var vmssList = result.AssertProperty("vmssList");
        Assert.Equal(JsonValueKind.Array, vmssList.ValueKind);
        Assert.NotEmpty(vmssList.EnumerateArray());

        var vmssNames = vmssList.EnumerateArray()
            .Select(vmss => vmss.GetProperty("name").GetString())
            .ToList();

        Assert.Contains(Settings.DeploymentOutputs["vmssName"], vmssNames);
    }

    [Fact]
    public async Task Should_get_specific_vmss_details()
    {
        var vmssName = Settings.DeploymentOutputs["vmssName"];

        var result = await CallToolAsync(
            "compute_vmss_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", vmssName }
            });

        var vmss = result.AssertProperty("vmss");
        Assert.Equal(JsonValueKind.Object, vmss.ValueKind);

        var name = vmss.GetProperty("name");
        Assert.Equal(vmssName, name.GetString());

        var location = vmss.GetProperty("location");
        Assert.NotNull(location.GetString());

        var skuName = vmss.GetProperty("skuName");
        Assert.Equal("Standard_B1s", skuName.GetString());
    }

    [Fact]
    public async Task Should_list_vmss_vms()
    {
        var vmssName = Settings.DeploymentOutputs["vmssName"];

        var result = await CallToolAsync(
            "compute_vmss_vms_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", vmssName }
            });

        var vms = result.AssertProperty("vms");
        Assert.Equal(JsonValueKind.Array, vms.ValueKind);

        // Should have 2 instances based on capacity in Bicep
        var vmArray = vms.EnumerateArray().ToList();
        Assert.Equal(2, vmArray.Count);

        // Verify each VM has required properties
        foreach (var vm in vmArray)
        {
            var instanceId = vm.GetProperty("instanceId");
            Assert.NotNull(instanceId.GetString());

            var vmId = vm.GetProperty("vmId");
            Assert.NotNull(vmId.GetString());

            var provisioningState = vm.GetProperty("provisioningState");
            Assert.Equal("Succeeded", provisioningState.GetString());
        }
    }

    [Fact]
    public async Task Should_get_specific_vmss_vm()
    {
        var vmssName = Settings.DeploymentOutputs["vmssName"];

        // First get the list to find an instance ID
        var listResult = await CallToolAsync(
            "compute_vmss_vms_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", vmssName }
            });

        var vms = listResult.AssertProperty("vms");
        var firstVm = vms.EnumerateArray().First();
        var instanceId = firstVm.GetProperty("instanceId").GetString();
        Assert.NotNull(instanceId);

        // Now get that specific instance
        var result = await CallToolAsync(
            "compute_vmss_vm_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vmss-name", vmssName },
                { "instance-id", instanceId }
            });

        var vm = result.AssertProperty("vm");
        Assert.Equal(JsonValueKind.Object, vm.ValueKind);

        var returnedInstanceId = vm.GetProperty("instanceId");
        Assert.Equal(instanceId, returnedInstanceId.GetString());
    }
}
