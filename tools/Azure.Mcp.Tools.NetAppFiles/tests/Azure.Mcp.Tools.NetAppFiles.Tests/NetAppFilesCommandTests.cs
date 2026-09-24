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
    public async Task VolumeGroupCreate_ReturnsCreatedVolumeGroup()
    {
        var (volumeGroup, volumeGroupName, location) = await CreateVolumeGroupAsync();

        Assert.Equal(JsonValueKind.Object, volumeGroup.ValueKind);
        Assert.Equal(volumeGroupName, volumeGroup.AssertProperty("name").GetString());
        volumeGroup.AssertProperty("id");
        Assert.Equal(location, volumeGroup.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volumeGroup.AssertProperty("provisioningState").GetString());
        Assert.Equal(3, volumeGroup.AssertProperty("volumes").GetArrayLength());
    }

    [Fact]
    public async Task VolumeGroupGet_ReturnsVolumeGroup()
    {
        var (_, volumeGroupName, location) = await CreateVolumeGroupAsync();
        var account = Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"];

        var result = await CallToolAsync(
            "netappfiles_volumegroup_get",
            new()
            {
                { "account", account },
                { "volume-group", volumeGroupName },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volumeGroup = result.AssertProperty("volumeGroup");
        Assert.Equal(JsonValueKind.Object, volumeGroup.ValueKind);
        Assert.Equal(volumeGroupName, volumeGroup.AssertProperty("name").GetString());
        volumeGroup.AssertProperty("id");
        Assert.Equal(location, volumeGroup.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volumeGroup.AssertProperty("provisioningState").GetString());
        Assert.Equal(3, volumeGroup.AssertProperty("volumes").GetArrayLength());
    }

    [Fact]
    public async Task VolumeGroupUpdate_ReturnsUpdatedVolumeGroup()
    {
        var (_, volumeGroupName, location) = await CreateVolumeGroupAsync();
        var account = Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"];

        var result = await CallToolAsync(
            "netappfiles_volumegroup_update",
            new()
            {
                { "account", account },
                { "volume-group", volumeGroupName },
                { "group-description", "Recorded update test" },
                { "resource-group", Settings.ResourceGroupName },
                { "subscription", Settings.SubscriptionId },
                { "tenant", Settings.TenantId }
            });

        var volumeGroup = result.AssertProperty("volumeGroup");
        Assert.Equal(JsonValueKind.Object, volumeGroup.ValueKind);
        Assert.Equal(volumeGroupName, volumeGroup.AssertProperty("name").GetString());
        volumeGroup.AssertProperty("id");
        Assert.Equal(location, volumeGroup.AssertProperty("location").GetString());
        Assert.Equal("Succeeded", volumeGroup.AssertProperty("provisioningState").GetString());
        Assert.Equal(3, volumeGroup.AssertProperty("volumes").GetArrayLength());
    }

    private async Task<(JsonElement VolumeGroup, string VolumeGroupName, string Location)> CreateVolumeGroupAsync()
    {
        var account = Settings.DeploymentOutputs["NETAPP_ACCOUNT_NAME"];
        var capacityPoolId = Settings.DeploymentOutputs["NETAPP_CAPACITY_POOL_ID"];
        var subnetId = Settings.DeploymentOutputs["NETAPP_SUBNET_ID"];
        var location = Settings.DeploymentOutputs["LOCATION"];
        var volumeGroupName = $"{Settings.ResourceBaseName}-vg";
        var volumes = $$"""
                        [
                            {
                                "name": "{{Settings.ResourceBaseName}}-data",
                                "creationToken": "{{Settings.ResourceBaseName}}-data",
                                "quotaGib": 100,
                                "subnetId": "{{subnetId}}",
                                "capacityPoolId": "{{capacityPoolId}}",
                                "volumeSpecName": "data",
                                "serviceLevel": "Premium",
                                "protocols": ["NFSv4.1"],
                                "allowedClients": "10.0.0.0/16"
                            },
                            {
                                "name": "{{Settings.ResourceBaseName}}-log",
                                "creationToken": "{{Settings.ResourceBaseName}}-log",
                                "quotaGib": 100,
                                "subnetId": "{{subnetId}}",
                                "capacityPoolId": "{{capacityPoolId}}",
                                "volumeSpecName": "log",
                                "serviceLevel": "Premium",
                                "protocols": ["NFSv4.1"],
                                "allowedClients": "10.0.0.0/16"
                            },
                            {
                                "name": "{{Settings.ResourceBaseName}}-shared",
                                "creationToken": "{{Settings.ResourceBaseName}}-shared",
                                "quotaGib": 100,
                                "subnetId": "{{subnetId}}",
                                "capacityPoolId": "{{capacityPoolId}}",
                                "volumeSpecName": "shared",
                                "serviceLevel": "Premium",
                                "protocols": ["NFSv4.1"],
                                "allowedClients": "10.0.0.0/16"
                            }
                        ]
                        """;

        var result = await CallToolAsync(
                "netappfiles_volumegroup_create",
                new()
                {
                                { "account", account },
                                { "volume-group", volumeGroupName },
                                { "location", location },
                                { "application-type", "SapHana" },
                                { "application-identifier", "SH1" },
                                { "volumes", volumes },
                                { "resource-group", Settings.ResourceGroupName },
                                { "subscription", Settings.SubscriptionId },
                                { "tenant", Settings.TenantId }
                });

        var volumeGroup = result.AssertProperty("volumeGroup");
        return (volumeGroup, volumeGroupName, location);
    }
}
