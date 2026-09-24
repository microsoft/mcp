// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.Tests;

public class NetAppFilesCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task AccountGet_ReturnsAccount()
    {
        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_get",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountCreate_ReturnsCreatedAccount()
    {
        var result = await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task AccountUpdate_ReturnsUpdatedAccount()
    {
        await CallToolAsync(
            "netappfiles_account_create",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "location", "eastus" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_account_update",
            new()
            {
                { "account", Settings.ResourceBaseName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId },
                { "tags", "{\"recorded-test\":\"account-update\"}" }
            });

        var account = result.AssertProperty("account");
        Assert.Equal(JsonValueKind.Object, account.ValueKind);
        account.AssertProperty("name");
        account.AssertProperty("id");
        Assert.Equal("eastus", account.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", account.AssertProperty("provisioningState").GetString());
    }

    [Fact]
    public async Task VolumeCreate_ReturnsCreatedVolume()
    {
        var result = await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", $"{Settings.ResourceBaseName}-volume" },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal($"{Settings.ResourceBaseName}-volume", volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(100, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }

    [Fact]
    public async Task VolumeGet_ReturnsVolume()
    {
        var volumeName = $"{Settings.ResourceBaseName}-get-volume";
        await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_volume_get",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal(volumeName, volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(100, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }

    [Fact]
    public async Task VolumeUpdate_ReturnsUpdatedVolume()
    {
        var volumeName = $"{Settings.ResourceBaseName}-update-volume";
        await CallToolAsync(
            "netappfiles_volume_create",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "location", Settings.DeploymentOutputs["LOCATION"] },
                { "subnet-id", Settings.DeploymentOutputs["NETAPP_SUBNET_ID"] },
                { "quota-gib", 100 },
                { "service-level", "Standard" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var result = await CallToolAsync(
            "netappfiles_volume_update",
            new()
            {
                { "account", Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"] },
                { "pool", Settings.DeploymentOutputs["NETAPP_POOL_NAME"] },
                { "volume", volumeName },
                { "quota-gib", 200 },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volume = result.AssertProperty("volume");
        Assert.Equal(JsonValueKind.Object, volume.ValueKind);
        Assert.Equal(volumeName, volume.AssertProperty("name").GetString());
        volume.AssertProperty("id");
        Assert.Equal(Settings.DeploymentOutputs["LOCATION"], volume.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volume.AssertProperty("provisioningState").GetString());
        Assert.Equal(200, volume.AssertProperty("quotaGib").GetInt64());
        Assert.Equal("Standard", volume.AssertProperty("serviceLevel").GetString());
    }
}